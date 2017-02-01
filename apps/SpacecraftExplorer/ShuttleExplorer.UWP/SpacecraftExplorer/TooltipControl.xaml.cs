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
    public sealed partial class TooltipControl : UserControl
    {
        public TooltipControl()
        {
            this.InitializeComponent();
        }

        public void SetTooltip(string title, string rotateLeft, string click, string rotateRight)
        {
            TitleText.Text = title;
            RotateLeftText.Text = rotateLeft;
            RotateRightText.Text = rotateRight;
            ClickText.Text = click;
        }
    }
}
