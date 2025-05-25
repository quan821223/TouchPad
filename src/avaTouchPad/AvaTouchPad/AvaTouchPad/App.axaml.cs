using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using AvaTouchPad.Model.Configurations;
using AvaTouchPad.ViewModels;
using AvaTouchPad.Views;
using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;



namespace AvaTouchPad;


public partial class App : Application
{
    private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

    private static string CONFIG_FILE_NAME = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "root", "appsettings.json");
    private IServiceProvider _serviceProvider;
    private AvaTouchPad.Model.Services.IConfigurationManager _configManager;
    public static TouchPadParameters Parameters { get; private set; }
    private static AppConfig _config;
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
            // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel()
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            singleViewPlatform.MainView = new MainView
            {
                DataContext = new MainViewModel()
            };
        }
        var logger = LogManager.GetCurrentClassLogger();
        logger.Info("App 啟動完成");
        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
    private TouchPadParameters ParseCommandLineArgs(string[] args)
    {
        var parameters = new TouchPadParameters();
        var arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var arg in args)
        {
            if (arg.Contains("="))
            {
                var parts = arg.Split(new[] { '=' }, 2);
                if (parts.Length == 2)
                {
                    arguments[parts[0].Trim()] = parts[1].Trim();
                }
            }
        }

        // 解析各個參數
        if (arguments.TryGetValue("PixelWidth", out string pixelWidth))
        {
            parameters.PixelWidth = int.Parse(pixelWidth);
        }

        if (arguments.TryGetValue("PixelHeight", out string pixelHeight))
        {
            parameters.PixelHeight = int.Parse(pixelHeight);
        }

        if (arguments.TryGetValue("XSensorPad", out string xSensorPad))
        {
            parameters.XSensorPad = int.Parse(xSensorPad);
        }

        if (arguments.TryGetValue("YSensorPad", out string ySensorPad))
        {
            parameters.YSensorPad = int.Parse(ySensorPad);
        }

        return parameters;
    }
    public class TouchPadParameters
    {
        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }
        public int XSensorPad { get; set; }
        public int YSensorPad { get; set; }

        // 可以添加參數驗證方法
        public bool Validate()
        {
            return PixelWidth > 0 &&
                   PixelHeight > 0 &&
                   XSensorPad > 0 &&
                   YSensorPad > 0;
        }
    }
    //public static string DirLicenseData
    //{
    //    get
    //    {
    //        string dirPath = Path.Combine(
    //            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    //            ScriptEditor.Company);

    //        try
    //        {
    //            if (!Directory.Exists(dirPath))
    //                Directory.CreateDirectory(dirPath);
    //        }
    //        catch (Exception ex)
    //        {
    //            // ✅ 寫進 NLog 的 log（會依你設定寫進 ErrorLog 或 GeneralLog）
    //            logger.Error(ex, $"[DirLicenseData] 建立資料夾失敗: {dirPath}");
    //        }

    //        return dirPath;
    //    }
    //}
    //public static string DirLicenseData
    //{
    //    get
    //    {
    //        string dirPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
    //                                         System.IO.Path.DirectorySeparatorChar + ScriptEditor.Company;

    //        try
    //        {
    //            if (System.IO.Directory.Exists(dirPath) == false)
    //                System.IO.Directory.CreateDirectory(dirPath);
    //        }
    //        catch (Exception ex)
    //        {
    //            System.Windows.MessageBox.Show(ex.Message);
    //            System.Windows.MessageBox.Show(ex.StackTrace);

    //        }

    //        return dirPath;
    //    }
    //}
}