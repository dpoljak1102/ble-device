using CtrlBluetoothLE.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CtrlBluetoothLE.Service.Common
{
    public interface INavigationService
    {
        ViewModelBase CurrentView { get; }
        ViewModelBase CurrentContent { get; set; }

        void NavigateTo<T>() where T : ViewModelBase;

        void NavigateToContent<T>() where T : ViewModelBase;
    }
}
