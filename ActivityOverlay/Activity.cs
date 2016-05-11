using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace ActivityOverlay
{
    public class Activity : DependencyObject
    {
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(Activity), new PropertyMetadata(null));
        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register("Message", typeof(string), typeof(Activity), new PropertyMetadata(null));
        public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register("Error", typeof(Exception), typeof(Activity), new PropertyMetadata(null));
        public static readonly DependencyProperty StatusProperty = DependencyProperty.Register("Status", typeof(ActivityStatus), typeof(Activity), new PropertyMetadata(ActivityStatus.NotStarted));

        public static readonly DependencyProperty ShowErrorsProperty = DependencyProperty.Register("ShowErrors", typeof(bool), typeof(Activity), new PropertyMetadata(true));
        public static readonly DependencyProperty ShowSuccessProperty = DependencyProperty.Register("ShowSuccess", typeof(bool), typeof(Activity), new PropertyMetadata(true));
        public static readonly DependencyProperty RestartableProperty = DependencyProperty.Register("Restartable", typeof(bool), typeof(Activity), new PropertyMetadata(true));
        public static readonly DependencyProperty CancellableProperty = DependencyProperty.Register("Cancellable", typeof(bool), typeof(Activity), new PropertyMetadata(false));

        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        public string Message
        {
            get { return (string)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public Exception Error
        {
            get { return (Exception)GetValue(ErrorProperty); }
            set { SetValue(ErrorProperty, value); }
        }

        public ActivityStatus Status
        {
            get { return (ActivityStatus)GetValue(StatusProperty); }
            set { SetValue(StatusProperty, value); }
        }

        public bool ShowErrors
        {
            get { return (bool)GetValue(ShowErrorsProperty); }
            set { SetValue(ShowErrorsProperty, value); }
        }

        public bool ShowSuccess
        {
            get { return (bool)GetValue(ShowSuccessProperty); }
            set { SetValue(ShowSuccessProperty, value); }
        }


        public bool Restartable
        {
            get { return (bool)GetValue(RestartableProperty); }
            set { SetValue(RestartableProperty, value); }
        }

        public bool Cancellable
        {
            get { return (bool)GetValue(CancellableProperty); }
            set { SetValue(CancellableProperty, value); }
        }


        public Activity() {
        }

        public Activity(Func<CancellationToken, Task> action)
        {
            this.Action = action;
        }

        internal Func<CancellationToken, Task> Action { get; set; }
    }
}
