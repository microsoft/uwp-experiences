using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace SpacecraftExplorer
{
    public sealed partial class RadialOnscreenMenu : UserControl
    {
        public RadialOnscreenMenu()
        {
            this.InitializeComponent();
        }

        // rotates the circle with delta dt
        public void Rotate(double dt)
        {
            RotationTransform.Rotation += dt;
            InspectorTransform.Rotation += dt * 5.0f;
        }

        // Sets the display of the gauge
        public void SetDisplayContent(string disp)
        {
            CounterText.Text = disp;
            InspectorText.Text = disp;
        }

        public void SetInspectorDialMode()
        {
            InspectorDial.Visibility = Visibility.Visible;
            GenericDial.Visibility = Visibility.Collapsed;
        }

        public void SetGenericDialMode()
        {
            InspectorDial.Visibility = Visibility.Collapsed;
            GenericDial.Visibility = Visibility.Visible;
        }
    }
}
