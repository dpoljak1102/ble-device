using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CtrlBluetoothLE.ViewModels
{
    public class LoadingViewModel
    {
        private LoadingViewModel() { }
        public static LoadingViewModel CreateView()
        {
            var myClass = new LoadingViewModel();
            return myClass;
        }
    }
}
