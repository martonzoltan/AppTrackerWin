using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTrackerWin.Models
{
    public class MainWindowViewModel
    {
        public int SwitchView
        {
            get;
            set;
        }

        public MainWindowViewModel()
        {
            SwitchView = 0;
        }
    }
}
