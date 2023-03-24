using CtrlBluetoothLE.Core;
using CtrlBluetoothLE.Service.Common;
using CtrlBluetoothLE.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CtrlBluetoothLE.Service
{
    public class NavigationService : ObservableObject, INavigationService
    {
        private ViewModelBase _currentView;
        private ViewModelBase _currentContent;

        private readonly Func<Type, ViewModelBase> _viewModelFactory;

        public ViewModelBase CurrentView
        {
            get { return _currentView; }
            set { _currentView = value; OnPropertyChanged(); }
        }

        public ViewModelBase CurrentContent
        {
            get { return _currentContent; }
            set { _currentContent = value; OnPropertyChanged(); }
        }

        public NavigationService(Func<Type, ViewModelBase> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public void NavigateTo<TViewModel>() where TViewModel : ViewModelBase
        {
            CurrentView = _viewModelFactory.Invoke(typeof(TViewModel));
            CurrentContent = null;
        }

        public void NavigateToContent<TViewModel>() where TViewModel : ViewModelBase
        {
            CurrentContent = _viewModelFactory.Invoke(typeof(TViewModel));
        }

    }
}
