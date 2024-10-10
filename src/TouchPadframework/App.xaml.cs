using Splat;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using ReactiveUI;
using Serilog;
using System.Reactive;
using Microsoft.Extensions.Logging;
using TouchPadframework.Models;
using TouchPadframework.ViewModels;

namespace TouchPadframework
{
    /// <summary>
    /// App.xaml 的互動邏輯
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // 設定 Serilog 日誌記錄
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();
            // 初始化 Splat 和 ReactiveUI
            Locator.CurrentMutable.InitializeSplat();
            Locator.CurrentMutable.InitializeReactiveUI();

            // 自動註冊 View 和 ViewModel 關聯
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetExecutingAssembly());
            ConfigureLogging(); // 配置日誌

#if DEBUG
#endif
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // 啟動主視窗
            var mainWindow = new MainWindow();
      

            var configurationService = new ConfigurationService();
            AppViewModel ViewModel = new AppViewModel(configurationService);
            mainWindow.DataContext = ViewModel;
            mainWindow.Show();
        }

        private void ConfigureLogging()
        {
#if DEBUG
            // 註冊日誌服務
            Locator.CurrentMutable.RegisterConstant(new LoggingService { Level = Microsoft.Extensions.Logging.LogLevel.Debug }, typeof(Microsoft.Extensions.Logging.ILogger));
#endif
        }


    }
}
