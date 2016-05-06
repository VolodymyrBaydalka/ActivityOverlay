using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ActivityOverlay
{
    public class SystemSoundsAction : TriggerAction<FrameworkElement>
    {
        public static readonly DependencyProperty SoundProperty = DependencyProperty.Register("Sound", typeof(SystemSound),
            typeof(SystemSoundsAction), new PropertyMetadata(SystemSounds.Beep));

        public SystemSound Sound
        {
            get { return (SystemSound)GetValue(SoundProperty); }
            set { SetValue(SoundProperty, value); }
        }

        protected override void Invoke(object parameter)
        {
            var s = this.Sound;

            if (s != null)
                s.Play();
        }
    }
}
