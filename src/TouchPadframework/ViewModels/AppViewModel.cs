using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TouchPadframework.Models;

namespace TouchPadframework.ViewModels
{
    public class AppViewModel : ReactiveObject, IActivatableViewModel
    {
        private readonly IConfigurationService _configurationService;

        public ViewModelActivator Activator { get; }

        private string _name;
        private int _age;
        private string _infoResult; // 用於顯示信息結果


        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public int Age
        {
            get => _age;
            set => this.RaiseAndSetIfChanged(ref _age, value);
        }
        public string InfoResult // 新增屬性
        {
            get => _infoResult;
            set => this.RaiseAndSetIfChanged(ref _infoResult, value);
        }

        // 這個命令會用來顯示信息
        public ReactiveCommand<Unit, string> ShowInfoCommand { get; }

        public ReactiveCommand<Unit, ServiceBase> UpdateConfigCommand { get; }


        private string _configFilePath;
        public string ConfigFilePath
        {
            get => _configFilePath;
            set => this.RaiseAndSetIfChanged(ref _configFilePath, value);
        }
        public AppViewModel(IConfigurationService configurationService)
        {
            _configurationService = configurationService;
            Activator = new ViewModelActivator();
            ShowInfoCommand = ReactiveCommand.Create(ShowInfo);
            UpdateConfigCommand = ReactiveCommand.CreateFromTask(LoadConfigAsync);


        }
        private string ShowInfo()
        {
       
            InfoResult = $"Name: {Name}, Age: {Age}"; // 更新結果
            MessageBox.Show(InfoResult);
            return InfoResult; // 返回結果（如果需要）
        }
        private async Task<ServiceBase> LoadConfigAsync()
        {
            try
            {
                var service = await _configurationService.LoadConfigAsync();
                serviceBase = service; // 更新屬性
                return serviceBase; // 確保返回 ServiceBase
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return null; // 如果返回 null，您可能需要考慮如何處理
            }
        }
        private ServiceBase _serviceBase;
        public ServiceBase serviceBase
        {
            get => _serviceBase;
            set => this.RaiseAndSetIfChanged(ref _serviceBase, value);
        }

       
    }
}
