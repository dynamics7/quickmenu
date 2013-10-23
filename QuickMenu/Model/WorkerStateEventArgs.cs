using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace QuickMenu
{
    public class WorkerStateEventArgs : EventArgs
    {
        public bool IsBusy
        {
            get;
            set;
        }
    }
}
