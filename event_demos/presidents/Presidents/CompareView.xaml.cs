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


// TODO: 4.0 - Note that we set the focus engagement manually to ensure we capture the input see the XAML file:  IsFocusEngagementEnabled="True"

namespace Presidents
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompareView : Page
    {
        public CompareView()
        {
            this.InitializeComponent();
            gv.ItemsSource = President.AllPresidents;
            gv.SelectionMode = ListViewSelectionMode.Multiple;
            //gv.IsFocusEngagementEnabled = true;
        }

    }
}
