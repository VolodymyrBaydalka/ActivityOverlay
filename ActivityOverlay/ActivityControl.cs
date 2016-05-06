using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ActivityOverlay
{
    [TemplatePart(Name = "PART_Activity", Type = typeof(ContentPresenter))]
    public class ActivityControl : ContentControl
    {
        public static readonly DependencyProperty CurrentActivityProperty = DependencyProperty.Register("CurrentActivity", typeof(Activity), typeof(ActivityControl), new PropertyMetadata(null));
        public static readonly DependencyProperty LoadingTemplateProperty = DependencyProperty.Register("LoadingTemplate", typeof(DataTemplate), typeof(ActivityControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SuccessTemplateProperty = DependencyProperty.Register("SuccessTemplate", typeof(DataTemplate), typeof(ActivityControl), new PropertyMetadata(null));
        public static readonly DependencyProperty ErrorTemplateProperty = DependencyProperty.Register("ErrorTemplate", typeof(DataTemplate), typeof(ActivityControl), new PropertyMetadata(null));

        public static readonly RoutedEvent StartedEvent = EventManager.RegisterRoutedEvent("Started", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ActivityControl));
        public static readonly RoutedEvent FinishedEvent = EventManager.RegisterRoutedEvent("Finished", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ActivityControl));
        public static readonly RoutedEvent SucceedEvent = EventManager.RegisterRoutedEvent("Succeed", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ActivityControl));
        public static readonly RoutedEvent ErrorEvent = EventManager.RegisterRoutedEvent("Error", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(ActivityControl));

        private readonly ObservableCollection<Activity> _activities = new ObservableCollection<Activity>();
        private ContentPresenter _activityPresenter;

        public Activity CurrentActivity
        {
            get { return (Activity)GetValue(CurrentActivityProperty); }
            set { SetValue(CurrentActivityProperty, value); }
        }

        public DataTemplate LoadingTemplate
        {
            get { return (DataTemplate)GetValue(LoadingTemplateProperty); }
            set { SetValue(LoadingTemplateProperty, value); }
        }

        public DataTemplate ErrorTemplate
        {
            get { return (DataTemplate)GetValue(ErrorTemplateProperty); }
            set { SetValue(ErrorTemplateProperty, value); }
        }

        public DataTemplate SuccessTemplate
        {
            get { return (DataTemplate)GetValue(SuccessTemplateProperty); }
            set { SetValue(SuccessTemplateProperty, value); }
        }

        public ReadOnlyObservableCollection<Activity> Activities { get; private set; }

        public event RoutedEventHandler Started
        {
            add { AddHandler(StartedEvent, value); }
            remove { RemoveHandler(StartedEvent, value); }
        }

        public event RoutedEventHandler Error
        {
            add { AddHandler(ErrorEvent, value); }
            remove { RemoveHandler(ErrorEvent, value); }
        }

        public event RoutedEventHandler Succeed
        {
            add { AddHandler(SucceedEvent, value); }
            remove { RemoveHandler(SucceedEvent, value); }
        }

        public event RoutedEventHandler Finished
        {
            add { AddHandler(FinishedEvent, value); }
            remove { RemoveHandler(FinishedEvent, value); }
        }

        static ActivityControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ActivityControl), new FrameworkPropertyMetadata(typeof(ActivityControl)));
        }

        public ActivityControl()
        {
            this.Activities = new ReadOnlyObservableCollection<Activity>(_activities);

            this.CommandBindings.Add(new CommandBinding(ActivityCommands.RestartCommand, (s, e) =>
            {
                if (this.CurrentActivity != null)
                {
                    this.CurrentActivity.Status = ActivityStatus.NotStarted;
                    CheckAndRun();
                }
            }, (s, e) =>
            {
                e.CanExecute = this.CurrentActivity != null && this.CurrentActivity.Restartable;
            }));

            this.CommandBindings.Add(new CommandBinding(ActivityCommands.ContinueCommand, (s, e) =>
            {
                if (this.CurrentActivity != null)
                {
                    _activities.Remove(this.CurrentActivity);
                    this.CurrentActivity = null;
                    CheckAndRun();
                }
            }, (s, e) =>
            {
                e.CanExecute = this.CurrentActivity != null;
            }));

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _activityPresenter = Template.FindName("PART_Activity", this) as ContentPresenter;
        }

        public Activity EnqueueActivity(Func<Task> action, string name = null, string message = null, bool showErrors = true, bool showSuccess = false, bool restartable = true)
        {
            var activity = new Activity
            {
                Name = name,
                Message = message ?? name,
                Action = action,
                ShowSuccess = showSuccess,
                ShowErrors = showErrors,
                Restartable = restartable
            };

            EnqueueActivity(activity);

            return activity;
        }

        public void EnqueueActivity(Activity activity)
        {
            _activities.Add(activity);
            CheckAndRun();
        }

        private async void CheckAndRun()
        {
            var activity = _activities.FirstOrDefault();

            if (this.CurrentActivity != activity)
                this.CurrentActivity = activity;

            if (_activityPresenter != null)
            {
                _activityPresenter.Visibility = activity == null ? Visibility.Collapsed : Visibility.Visible;
                _activityPresenter.Content = activity;
            }

            if (activity != null && activity.Status == ActivityStatus.NotStarted)
            {
                this.CurrentActivity = activity;

                RaiseEvent(new RoutedEventArgs(StartedEvent));

                activity.Status = ActivityStatus.Running;

                if (_activityPresenter != null)
                    _activityPresenter.ContentTemplate = LoadingTemplate;

                try
                {
                    await activity.Action();
                    activity.Status = ActivityStatus.Finished;

                    if (_activityPresenter != null)
                        _activityPresenter.ContentTemplate = SuccessTemplate;

                    RaiseEvent(new RoutedEventArgs(SucceedEvent));

                    if (!activity.ShowSuccess)
                    {
                        _activities.Remove(activity);
                    }
                }
                catch (Exception e)
                {
                    activity.Error = e;
                    activity.Status = ActivityStatus.Failed;

                    if (_activityPresenter != null)
                        _activityPresenter.ContentTemplate = ErrorTemplate;

                    RaiseEvent(new RoutedEventArgs(ErrorEvent));

                    if (!activity.ShowErrors)
                    {
                        _activities.Remove(activity);
                    }
                }

                RaiseEvent(new RoutedEventArgs(FinishedEvent));

                this.Dispatcher.Invoke(CheckAndRun);
            }
        }
    }
}
