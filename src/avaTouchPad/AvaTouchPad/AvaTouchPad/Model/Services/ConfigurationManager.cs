using AvaTouchPad.Model.Configurations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AvaTouchPad.Model.Services
{
    public interface IConfigurationManager
    {
        AppConfig CurrentConfig { get; }
        event EventHandler<AppConfig> ConfigurationChanged;
        Task LoadConfigurationAsync();
        Task SaveConfigurationAsync();
        Task ResetToDefaultsAsync();
        void ValidateConfiguration();
    }
    public interface IEncryptionService
    {
        string Encrypt(string plainText);
        string Decrypt(string cipherText);
    }
    public class ConfigurationManager : IConfigurationManager
    {
        private const string CONFIG_FILE_NAME = "appsettings.json";
        private const string CONFIG_BACKUP_FILE_NAME = "appsettings.backup.json";
        private const string CONFIG_DIRECTORY = "Configurations";
        private const string ENCRYPTION_PREFIX = "ENC:";

        //private readonly Logger<ConfigurationManager> _logger;
        private readonly IEncryptionService _encryptionService;
        private readonly JsonSerializerSettings _jsonSettings;
        private readonly object _lockObject = new object();

        private AppConfig _currentConfig;
        public AppConfig CurrentConfig
        {
            get => _currentConfig;
            private set
            {
                _currentConfig = value;
                OnConfigurationChanged(_currentConfig);
            }
        }

        public event EventHandler<AppConfig> ConfigurationChanged;

        public ConfigurationManager(
            // ILogger<ConfigurationManager> logger,
            IEncryptionService encryptionService)
        {
            // _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _encryptionService = encryptionService ?? throw new ArgumentNullException(nameof(encryptionService));

            _jsonSettings = new JsonSerializerSettings
            {
                Formatting = Newtonsoft.Json.Formatting.Indented,
                NullValueHandling = NullValueHandling.Ignore,
                DefaultValueHandling = DefaultValueHandling.Include,
            };

            InitializeConfiguration();
        }

        private void InitializeConfiguration()
        {
            try
            {
                EnsureConfigurationDirectory();
                LoadConfigurationAsync().Wait();
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Configuration initialization failed");
                throw new InvalidOperationException("Failed to initialize configuration system", ex);
            }
        }
        private void ValidateConfiguration(AppConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "Configuration cannot be null");
            }

            // 檢查 System 配置  
            if (config.System == null)
            {
                throw new InvalidOperationException("System configuration section is required");
            }

            // 檢查 ApplicationName 是否被更改  
            const string defaultApplicationName = "SuperCarter";
            if (config.System.ApplicationName != defaultApplicationName)
            {
                // _logger.LogWarning("ApplicationName has been modified. Restoring to default value.");
                config.System.ApplicationName = defaultApplicationName;
            }
        }
        private void EnsureConfigurationDirectory()
        {
            string configDir = GetConfigurationDirectory();
            if (!Directory.Exists(configDir))
            {
                try
                {
                    Directory.CreateDirectory(configDir);
                    // _logger.LogInformation("Created configuration directory: {ConfigDir}", configDir);
                }
                catch (Exception ex)
                {
                    // _logger.LogError(ex, "Failed to create configuration directory");
                    throw new InvalidOperationException("Unable to create configuration directory", ex);
                }
            }
        }

        public async Task LoadConfigurationAsync()
        {
            string configPath = GetConfigFilePath();

            try
            {
                if (!File.Exists(configPath))
                {
                    // _logger.LogWarning("Configuration file not found. Creating with defaults.");
                    CurrentConfig = CreateDefaultConfig();
                    await SaveConfigurationAsync();
                    return;
                }

                //string jsonContent = await File.ReadAllTextAsync(configPath, Encoding.UTF8);
                string jsonContent = File.ReadAllText(configPath, Encoding.UTF8);

                if (IsEncrypted(jsonContent))
                {
                    jsonContent = _encryptionService.Decrypt(jsonContent.Substring(ENCRYPTION_PREFIX.Length));
                }

                var loadedConfig = JsonConvert.DeserializeObject<AppConfig>(jsonContent, _jsonSettings);
                var defaultConfig = CreateDefaultConfig();

                // 合併配置，保留現有值，缺少的使用默認值  
                var mergedConfig = MergeConfigurations(loadedConfig, defaultConfig);

                ValidateConfiguration(mergedConfig);
                CurrentConfig = mergedConfig;

                // 如果有新增的配置項，保存更新後的配置  
                if (HasNewProperties(loadedConfig, mergedConfig))
                {
                    await SaveConfigurationAsync();
                    // _logger.LogInformation("Configuration updated with new default properties");
                }

                // _logger.LogInformation("Configuration loaded successfully");
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error loading configuration");
                await LoadBackupConfigurationAsync();
            }
        }
        private AppConfig MergeConfigurations(AppConfig current, AppConfig defaultConfig)
        {
            if (current == null) return defaultConfig;

            // 系統配置合併  
            if (current.System == null)
            {
                current.System = defaultConfig.System;
            }
            else
            {
                current.System.ApplicationName ??= defaultConfig.System.ApplicationName;
                current.System.DefaultCulture ??= defaultConfig.System.DefaultCulture;
                // 其他系統配置項的合併...  
            }
            if (current.Security == null)
            {
                current.Security = defaultConfig.Security;
            }
            else
            {
                current.Security.SecurityLevel = (current.Security.SecurityLevel == 0) ?
                        defaultConfig.Security.SecurityLevel :
                        current.Security.SecurityLevel;

                // 其他系統配置項的合併...  
            }
            // 通訊配置合併  
            if (current.Communication == null)
            {
                current.Communication = defaultConfig.Communication;
            }
            else
            {
                if (current.Communication.SerialPort == null)
                {
                    current.Communication.SerialPort = defaultConfig.Communication.SerialPort;
                }
                else
                {
                    // SerialPort 配置的合併  
                    if (current.Communication.SerialPort.DefaultBaudRate <= 0)
                    {
                        current.Communication.SerialPort.DefaultBaudRate = defaultConfig.Communication.SerialPort.DefaultBaudRate;
                    }
                    if (current.Communication.SerialPort.DefaultDataBits is < 5 or > 8)
                    {
                        current.Communication.SerialPort.DefaultDataBits = defaultConfig.Communication.SerialPort.DefaultDataBits;
                    }
                    current.Communication.SerialPort.ReadTimeout =
                        current.Communication.SerialPort.ReadTimeout <= 0 ?
                        defaultConfig.Communication.SerialPort.ReadTimeout :
                        current.Communication.SerialPort.ReadTimeout;
                    // 其他 SerialPort 配置項的合併...  
                }

                if (current.Communication.Network == null)
                {
                    current.Communication.Network = defaultConfig.Communication.Network;
                }
                else
                {
                    // Network 配置的合併  
                    current.Communication.Network.ApiBaseUrl ??= defaultConfig.Communication.Network.ApiBaseUrl;
                    current.Communication.Network.ConnectionTimeout =
                        current.Communication.Network.ConnectionTimeout <= 0 ?
                        defaultConfig.Communication.Network.ConnectionTimeout :
                        current.Communication.Network.ConnectionTimeout;
                    // 其他 Network 配置項的合併...  
                }
            }

            // 日誌配置合併  
            if (current.Logging == null)
            {
                current.Logging = defaultConfig.Logging;
            }
            else
            {
                current.Logging.LogLevel ??= defaultConfig.Logging.LogLevel;
                current.Logging.LogPath ??= defaultConfig.Logging.LogPath;
                current.Logging.MaxFileSize =
                    current.Logging.MaxFileSize <= 0 ?
                    defaultConfig.Logging.MaxFileSize :
                    current.Logging.MaxFileSize;
                // 其他日誌配置項的合併...  
            }

            // 安全配置合併  
            if (current.Security == null)
            {
                current.Security = defaultConfig.Security;
            }
            else
            {
                current.Security.EncryptionKey ??= defaultConfig.Security.EncryptionKey;
                // 其他安全配置項的合併...  
            }

            return current;
        }

        private bool HasNewProperties(AppConfig original, AppConfig merged)
        {
            // 將兩個配置序列化為 JSON 並比較，檢查是否有新增的屬性  
            var originalJson = JsonConvert.SerializeObject(original, _jsonSettings);
            var mergedJson = JsonConvert.SerializeObject(merged, _jsonSettings);
            return originalJson != mergedJson;
        }

        public async Task SaveConfigurationAsync()
        {
            string configPath = GetConfigFilePath();
            string backupPath = GetBackupConfigPath();

            try
            {
                // 建立備份
                if (File.Exists(configPath))
                {
                    File.Copy(configPath, backupPath, true);
                }

                string jsonContent = JsonConvert.SerializeObject(CurrentConfig, _jsonSettings);

                if (CurrentConfig.Security.EnableEncryption)
                {
                    jsonContent = ENCRYPTION_PREFIX + _encryptionService.Encrypt(jsonContent);
                }

                //await File.WriteAllTextAsync(configPath, jsonContent, Encoding.UTF8);
                File.WriteAllText(configPath, jsonContent, Encoding.UTF8);

                // _logger.LogInformation("Configuration saved successfully");

                // 觸發配置變更事件
                OnConfigurationChanged(CurrentConfig);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error saving configuration");
                throw new InvalidOperationException("Failed to save configuration", ex);
            }
        }

        private async Task LoadBackupConfigurationAsync()
        {
            string backupPath = GetBackupConfigPath();

            try
            {
                if (File.Exists(backupPath))
                {
                    // string jsonContent = await File.ReadAllTextAsync(backupPath, Encoding.UTF8);
                    string jsonContent = File.ReadAllText(backupPath, Encoding.UTF8);
                    if (IsEncrypted(jsonContent))
                    {
                        jsonContent = _encryptionService.Decrypt(jsonContent.Substring(ENCRYPTION_PREFIX.Length));
                    }

                    CurrentConfig = JsonConvert.DeserializeObject<AppConfig>(jsonContent, _jsonSettings);
                    // _logger.LogInformation("Backup configuration loaded successfully");
                }
                else
                {
                    CurrentConfig = CreateDefaultConfig();
                    // _logger.LogWarning("No backup found. Using default configuration");
                }
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Failed to load backup configuration");
                CurrentConfig = CreateDefaultConfig();
            }
        }

        public void ValidateConfiguration()
        {
            ValidateConfig(CurrentConfig);
        }

        private void ValidateConfig(AppConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), "Configuration cannot be null");
            }

            // 系統配置驗證
            if (config.System == null)
            {
                throw new InvalidOperationException("System configuration section is required");
            }

            if (string.IsNullOrEmpty(config.System.ApplicationName))
            {
                throw new InvalidOperationException("Application name is required");
            }

            // 通訊配置驗證
            if (config.Communication?.SerialPort != null)
            {
                if (config.Communication.SerialPort.DefaultBaudRate <= 0)
                {
                    throw new InvalidOperationException("Invalid baud rate");
                }

                if (config.Communication.SerialPort.DefaultDataBits is < 5 or > 8)
                {
                    throw new InvalidOperationException("Invalid data bits");
                }
            }

            // 日誌配置驗證
            if (config.Logging != null)
            {
                if (config.Logging.MaxFileSize <= 0)
                {
                    throw new InvalidOperationException("Invalid log file size");
                }

                if (string.IsNullOrEmpty(config.Logging.LogPath))
                {
                    throw new InvalidOperationException("Log path is required");
                }
            }
        }

        public async Task ResetToDefaultsAsync()
        {
            try
            {
                CurrentConfig = CreateDefaultConfig();
                await SaveConfigurationAsync();
                // _logger.LogInformation("Configuration reset to defaults");
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Failed to reset configuration");
                throw new InvalidOperationException("Failed to reset configuration to defaults", ex);
            }
        }

        public AppConfig CreateDefaultConfig()
        {
            return new AppConfig
            {
                System = new SystemConfig
                {
                    Environment = "Development",
                    ApplicationName = "SuperCarter",
                    DefaultCulture = "zh-TW",
                    EnableDebugMode = false
                },
                Communication = new CommunicationConfig
                {
                    SerialPort = new SerialPortConfig
                    {
                        DefaultBaudRate = 115200,
                        DefaultDataBits = 8,
                        ReadTimeout = 1000,
                        WriteTimeout = 1000,
                        AllowedPorts = new List<string>()
                    },
                    Network = new NetworkConfig
                    {
                        ApiBaseUrl = "http://localhost:5000",
                        ConnectionTimeout = 30000,
                        UseSSL = false,
                        Headers = new Dictionary<string, string>()
                    }
                },
                Logging = new LoggingConfig
                {
                    LogLevel = "Information",
                    LogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"),
                    EnableFileLogging = true,
                    EnableConsoleLogging = true,
                    MaxFileSize = 10485760, // 10MB
                    MaxFileCount = 10
                },
                Security = new SecurityConfig
                {
                    EnableEncryption = false,
                    EncryptionKey = Guid.NewGuid().ToString()
                }
            };
        }

        private bool IsEncrypted(string content)
        {
            return !string.IsNullOrEmpty(content) && content.StartsWith(ENCRYPTION_PREFIX);
        }

        private string GetConfigurationDirectory()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CONFIG_DIRECTORY);
        }

        private string GetConfigFilePath()
        {
            return Path.Combine(GetConfigurationDirectory(), CONFIG_FILE_NAME);
        }

        private string GetBackupConfigPath()
        {
            return Path.Combine(GetConfigurationDirectory(), CONFIG_BACKUP_FILE_NAME);
        }

        protected virtual void OnConfigurationChanged(AppConfig config)
        {
            try
            {
                ConfigurationChanged?.Invoke(this, config);
            }
            catch (Exception ex)
            {
                // _logger.LogError(ex, "Error in configuration changed event handler");
            }
        }
    }
}
