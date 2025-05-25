using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.IO;
namespace AvaTouchPad.Model.Configurations
{
    public class AppConfig
    {
        public SystemInformation SystemInfo { get; set; } = new SystemInformation();
        public SystemConfig System { get; set; }
        public CommunicationConfig Communication { get; set; }
        public LoggingConfig Logging { get; set; }
        public SecurityConfig Security { get; set; }
        public CanvasSetting canvasSetting { get; set; }
        public CanvasSetting_CMD canvasSetting_CMD { get; set; }
        // 新增只讀參數區域  

    }
    public class CanvasSetting_CMD
    {
        public string FIDM1_StartCMD { get; set; } = "FA57010801";
        public string FIDM2_StartCMD { get; set; } = "FA57020801";
        public string FIDM3_StartCMD { get; set; } = "FA57030801";
        public string FIDM1_EndCMD { get; set; } = "FA57010800";
        public string FIDM2_EndCMD { get; set; } = "FA57010800";
        public string FIDM3_EndCMD { get; set; } = "FA57010800";
    }
    public class CanvasSetting
    {
        public int? SetHeight { get; set; } = 1080;
        public int? SetWidth { get; set; } = 2348;
        public int? XSensorpad_count { get; set; } = 32;
        public int? YSensorpad_count { get; set; } = 32;
        public int? Auto_ctrl { get; set; } = 1;
        public int? TouchClearTimeout { get; set; } = 200;
        public int? PointSize { get; set; } = 15;
        public int? TOUCH_CLEAR { get; set; } = 1;
        public string? Rateadjustmentcmd { get; set; } = "500";

    }
    public class SystemInformation
    {
        public string SoftwareVersion { get; set; } = "1.6.5";
        public static string ConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "AvaTouchPad");

        //public string ROOT_DIR_Path_access { get; set; } = System.Windows.Forms.Application.StartupPath + @"a\ccess";
        //public string ROOT_DIR_Path_util { get; set; } = System.Windows.Forms.Application.StartupPath + @"\util";
        //public string ROOT_DIR_Path_img { get; set; } = System.Windows.Forms.Application.StartupPath + @"\img";
        //public string ROOT_DIR_Path_logs { get; set; } = System.Windows.Forms.Application.StartupPath + @"\logs";
        //public string ROOT_DIR_Path_config { get; set; } = System.Windows.Forms.Application.StartupPath + @"\config";
        //public string ROOT_DIR_Path_config_root { get; set; } = System.Windows.Forms.Application.StartupPath + @"\config\root";
        //public string ROOT_DIR_Path_src { get; set; } = System.Windows.Forms.Application.StartupPath + @"\src";
        //public string ROOT_DIR_Path_lib { get; set; } = System.Windows.Forms.Application.StartupPath + @"\lib";
        //public string ROOT_DIR_Path_scripts { get; set; } = System.Windows.Forms.Application.StartupPath + @"\scripts";
        //public string ROOT_DIR_Path_result { get; set; } = System.Windows.Forms.Application.StartupPath + @"\result";
        //public string ROOT_DIR_Path_DataSrc { get; set; } = System.Windows.Forms.Application.StartupPath + @"\DataSrc";

    }

    public class SystemConfig
    {
        public string Environment { get; set; }
        public string ApplicationName { get; set; }
        public string DefaultCulture { get; set; }
        public bool EnableDebugMode { get; set; }
        public int License { get; set; } = 0;

    }

    public class CommunicationConfig
    {
        public SerialPortConfig SerialPort { get; set; }
        public NetworkConfig Network { get; set; }
    }

    public class SerialPortConfig
    {
        public int DefaultBaudRate { get; set; }
        public int DefaultDataBits { get; set; }
        public int ReadTimeout { get; set; }
        public int WriteTimeout { get; set; }
        public List<string> AllowedPorts { get; set; }
    }

    public class NetworkConfig
    {
        public string ApiBaseUrl { get; set; }
        public int ConnectionTimeout { get; set; }
        public bool UseSSL { get; set; }
        public Dictionary<string, string> Headers { get; set; }
    }

    public class LoggingConfig
    {
        public string LogLevel { get; set; }
        public string LogPath { get; set; }
        public bool EnableFileLogging { get; set; }
        public bool EnableConsoleLogging { get; set; }
        public int MaxFileSize { get; set; }
        public int MaxFileCount { get; set; }
    }

    public class SecurityConfig
    {
        public string ApiKey { get; set; }
        public string SecretKey { get; set; }
        public bool EnableEncryption { get; set; } = false;
        public string EncryptionKey { get; set; }

        private int _SecurityLevel = 1;
        public int SecurityLevel
        {
            get => _SecurityLevel;
            set => _SecurityLevel = (value > 0) ? 1 : value;
        }
    }
}
