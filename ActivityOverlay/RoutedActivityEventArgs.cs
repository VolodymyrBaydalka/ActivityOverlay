using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ActivityOverlay
{
    public delegate void RoutedActivityEventHandler(object sender, RoutedActivityEventArgs e);

    public class RoutedActivityEventArgs : RoutedEventArgs
    {
        public Activity Activity { get; set; }

        public RoutedActivityEventArgs(RoutedEvent routedEvent, Activity activity) : base(routedEvent)
        {
            this.Activity = activity;
        }
    }
}
