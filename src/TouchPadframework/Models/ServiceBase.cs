using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Newtonsoft.Json;
using System.Windows.Markup;
using System.Runtime.InteropServices.ComTypes;
using Newtonsoft.Json.Bson;
namespace TouchPadframework.Models
{
    public class Settings
    {
        public bool EnableFeatureX { get; set; }
        public int MaxItems { get; set; }
    
    }
    public class ServiceBase
    {
         public string AppName { get; set; }
        public string Version { get; set; }
        public Settings settings { get; set; } = new Settings();
    }
    public interface IConfigurationService
    {
        ServiceBase LoadConfig();

        void RegisterForChanges(Action<ServiceBase> onChange);

        Task<ServiceBase> LoadConfigAsync();
        Task SaveConfigAsync(string filePath, ServiceBase config);
    }

    public class ConfigurationService : IConfigurationService
    {
        private ServiceBase _currentConfig;

        public ServiceBase LoadConfig()
        {
            // 從文件或其他來源加載配置
            return _currentConfig;
        }

        public void RegisterForChanges(Action<ServiceBase> onChange)
        {
            // 假設有個事件或機制來通知配置變化
            // 例如：FileSystemWatcher 監聽配置文件的更改
        }
        public ConfigurationService()
        {
           
        
        }
        public readonly string filePath = $"{System.Windows.Forms.Application.StartupPath}\\config.json";

        public async Task<ServiceBase> LoadConfigAsync()
        {
            // 判斷檔案是否存在
            if (!File.Exists(filePath))
            {
                ServiceBase serviceBase = GetDefaultConfig();
                SaveConfigAsync(filePath, serviceBase);
                // 返回帶有預設值的配置
                return serviceBase;
            }
            else
            {

                string jsonString = System.IO.File.ReadAllText(filePath);
                return JsonConvert.DeserializeObject<ServiceBase>(jsonString);
            }
          
        }
        private ServiceBase GetDefaultConfig()
        {
            // 設置預設配置
            return new ServiceBase
            {
                AppName = "Default Application",
                Version = "1.0.0",
                settings = new Settings
                {
                    EnableFeatureX = false,
                    MaxItems = 10
                }
            };
        }
      

        public async Task SaveConfigAsync(string filePath, ServiceBase config)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            string jsonString = System.Text.Json.JsonSerializer.Serialize(config, options);
            File.WriteAllText(filePath, jsonString);
        }
    }
}
