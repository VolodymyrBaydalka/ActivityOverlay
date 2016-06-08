#region License
/*******************************************************************************
 * Copyright 2016 Volodymyr Baydalka.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *******************************************************************************/
#endregion
 
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ActivityOverlay
{
    [TemplatePart(Name = "PART_Activity", Type = typeof(ContentPresenter))]
    public class ActivityControl : ContentControl
    {
        private static readonly DependencyPropertyKey CurrentActivityPropertyKey = DependencyProperty.RegisterReadOnly("CurrentActivity", typeof(Activity), typeof(ActivityControl), new PropertyMetadata(null));
        public static readonly DependencyProperty CurrentActivityProperty = CurrentActivityPropertyKey.DependencyProperty;

        public static readonly DependencyProperty LoadingTemplateProperty = DependencyProperty.Register("LoadingTemplate", typeof(DataTemplate), typeof(ActivityControl), new PropertyMetadata(null));
        public static readonly DependencyProperty SuccessTemplateProperty = DependencyProperty.Register("SuccessTemplate", typeof(DataTemplate), typeof(ActivityControl), new PropertyMetadata(null));
        public static readonly DependencyProperty ErrorTemplateProperty = DependencyProperty.Register("ErrorTemplate", typeof(DataTemplate), typeof(ActivityControl), new PropertyMetadata(null));

        public static readonly RoutedEvent StartedEvent = EventManager.RegisterRoutedEvent("Started", RoutingStrategy.Bubble, typeof(RoutedActivityEventHandler), typeof(ActivityControl));
        public static readonly RoutedEvent FinishedEvent = EventManager.RegisterRoutedEvent("Finished", RoutingStrategy.Bubble, typeof(RoutedActivityEventHandler), typeof(ActivityControl));
        public static readonly RoutedEvent SucceedEvent = EventManager.RegisterRoutedEvent("Succeed", RoutingStrategy.Bubble, typeof(RoutedActivityEventHandler), typeof(ActivityControl));
        public static readonly RoutedEvent ErrorEvent = EventManager.RegisterRoutedEvent("Error", RoutingStrategy.Bubble, typeof(RoutedActivityEventHandler), typeof(ActivityControl));
        public static readonly RoutedEvent ContinueEvent = EventManager.RegisterRoutedEvent("Continue", RoutingStrategy.Bubble, typeof(RoutedActivityEventHandler), typeof(ActivityControl));

        private readonly ObservableCollection<Activity> _activities = new ObservableCollection<Activity>();
        private ContentPresenter _activityPresenter;
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        public Activity CurrentActivity
        {
            get { return (Activity)GetValue(CurrentActivityProperty); }
            private set { SetValue(CurrentActivityPropertyKey, value); }
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

        public event RoutedActivityEventHandler Started
        {
            add { AddHandler(StartedEvent, value); }
            remove { RemoveHandler(StartedEvent, value); }
        }

        public event RoutedActivityEventHandler Error
        {
            add { AddHandler(ErrorEvent, value); }
            remove { RemoveHandler(ErrorEvent, value); }
        }

        public event RoutedActivityEventHandler Succeed
        {
            add { AddHandler(SucceedEvent, value); }
            remove { RemoveHandler(SucceedEvent, value); }
        }

        public event RoutedActivityEventHandler Finished
        {
            add { AddHandler(FinishedEvent, value); }
            remove { RemoveHandler(FinishedEvent, value); }
        }

        public event RoutedActivityEventHandler Continue
        {
            add { AddHandler(ContinueEvent, value); }
            remove { RemoveHandler(ContinueEvent, value); }
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
                    RaiseEvent(new RoutedActivityEventArgs(ContinueEvent, this.CurrentActivity));

                    _activities.Remove(this.CurrentActivity);
                    this.CurrentActivity = null;
                    CheckAndRun();
                }
            }, (s, e) =>
            {
                e.CanExecute = this.CurrentActivity != null;
            }));

            this.CommandBindings.Add(new CommandBinding(ActivityCommands.CancelCommand, (s, e) =>
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel();
                }
            }, (s, e) =>
            {
                e.CanExecute = this.CurrentActivity != null && this.CurrentActivity.Cancellable;
            }));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _activityPresenter = Template.FindName("PART_Activity", this) as ContentPresenter;
            UpdateActivityPresenter(CurrentActivity);
        }

        public Activity EnqueueActivity(Func<CancellationToken, Task> action, string name = null, string message = null, bool showErrors = true, bool showSuccess = false,
            bool restartable = true, bool cancellable = false)
        {
            var activity = new Activity
            {
                Name = name,
                Message = message ?? name,
                Action = action,
                ShowSuccess = showSuccess,
                ShowErrors = showErrors,
                Restartable = restartable,
                Cancellable = cancellable
            };

            EnqueueActivity(activity);

            return activity;
        }

        public void EnqueueActivity(Activity activity)
        {
            _activities.Add(activity);
            CheckAndRun();
        }

        private void UpdateActivityPresenter(Activity activity)
        {
            if (_activityPresenter == null)
                return;

            if (activity == null)
            {
                _activityPresenter.Visibility = Visibility.Collapsed;
                _activityPresenter.Content = null;
            }
            else
            {
                _activityPresenter.Visibility = Visibility.Visible;
                _activityPresenter.Content = activity;

                switch (activity.Status)
                {
                    case ActivityStatus.NotStarted:
                        break;

                    case ActivityStatus.Running:
                        _activityPresenter.ContentTemplate = LoadingTemplate;
                        break;

                    case ActivityStatus.Finished:
                        _activityPresenter.ContentTemplate = SuccessTemplate;
                        break;

                    case ActivityStatus.Failed:
                        _activityPresenter.ContentTemplate = ErrorTemplate;
                        break;
                }
            }
        }

        /// <summary>
        /// Updates UI and starts pending activityies if no one runs
        /// </summary>
        private async void CheckAndRun()
        {
            var activity = _activities.FirstOrDefault(); // if empty it returns null and hide presenter

            if (this.CurrentActivity != activity)
                this.CurrentActivity = activity;

            if (activity == null || activity.Status != ActivityStatus.NotStarted)
            {
                UpdateActivityPresenter(activity);
            }
            else if (activity.Status == ActivityStatus.NotStarted)
            {
                activity.Status = ActivityStatus.Running;
                RaiseEvent(new RoutedActivityEventArgs(StartedEvent, activity));
                UpdateActivityPresenter(activity);

                try
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    await activity.Action(_cancellationTokenSource.Token);
                    activity.Status = ActivityStatus.Finished;

                    RaiseEvent(new RoutedActivityEventArgs(SucceedEvent, activity));

                    if (!activity.ShowSuccess) //remove from queue and go next
                    {
                        _activities.Remove(activity);
                    }

                    UpdateActivityPresenter(activity.ShowSuccess ? activity : null);
                }
                catch (Exception e)
                {
                    activity.Error = e;
                    activity.Status = ActivityStatus.Failed;

                    RaiseEvent(new RoutedActivityEventArgs(ErrorEvent, activity));

                    if (!activity.ShowErrors) //remove from queue and go next
                    {
                        _activities.Remove(activity);
                    }

                    UpdateActivityPresenter(activity.ShowErrors ? activity : null);
                }

                RaiseEvent(new RoutedActivityEventArgs(FinishedEvent, activity));

                this.Dispatcher.Invoke(CheckAndRun); //rerun method to process the next activity
            }
        }
    }
}
