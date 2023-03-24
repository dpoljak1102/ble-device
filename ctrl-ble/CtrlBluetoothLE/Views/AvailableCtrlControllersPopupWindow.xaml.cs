using CtrlBluetoothLE.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CtrlBluetoothLE.Views
{
    /// <summary>
    /// Interaction logic for AvailableCtrlControllersPopupWindow.xaml
    /// </summary>
    public partial class AvailableCtrlControllersPopupWindow : Window
    {
        public string DeviceId{ get; set; }
        public string DeviceName{ get; set; }

        readonly AvailableCtrlControllersViewModel availableCtrlControllersViewModel;
        public AvailableCtrlControllersPopupWindow()
        {
            availableCtrlControllersViewModel = new AvailableCtrlControllersViewModel();
            InitializeComponent();
            DataContext = availableCtrlControllersViewModel;
        }

        private async void BluetoothDevicePairedButton_Click(object sender, RoutedEventArgs e)
        {
            var device = await availableCtrlControllersViewModel.PairToBluetoothLeDeviceAsync();
            if(device != null)
            {
                DeviceId = device.Id;
                DeviceName = device.Name;
                DialogResult = true;
            }
        }

        private void WindowCloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            var viewModel = (AvailableCtrlControllersViewModel)DataContext;
            if (viewModel.WindowsOnCloseCommand.CanExecute(null))
                viewModel.WindowsOnCloseCommand.Execute(null);
        }

    }
}
