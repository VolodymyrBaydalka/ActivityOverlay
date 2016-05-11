using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ActivityOverlay
{
    public static class ActivityCommands
    {
        public static ICommand RestartCommand { get; private set; }
        public static ICommand ContinueCommand { get; private set; }
        public static ICommand CancelCommand { get; private set; }

        static ActivityCommands()
        {
            RestartCommand = new RoutedUICommand("Restart", "Restart", typeof(ActivityCommands));
            ContinueCommand = new RoutedUICommand("Continue", "Continue", typeof(ActivityCommands));
            CancelCommand = new RoutedUICommand("Cancel", "Cancel", typeof(ActivityCommands));
        }
    }
}
