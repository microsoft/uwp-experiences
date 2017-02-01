
// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace SpacecraftExplorer
{
    /// <summary>
    /// Helper class that provides attached properties to enable any TextBox with the Surface Dial. Rotate to change the value by StepValue between MinValue and MaxValue, and tap to go to the Next focus element from a TextBox
    /// </summary>
    public static class SurfaceDialStepTextboxHelper
    {
        /// <summary>
        /// The Surface Dial controller instance itself
        /// </summary>
        private static RadialController s_controller;

        /// <summary>
        /// A default menu item that will be used for this to function. It will automatically be cleaned up when you move away from the TextBox, and created on Focus.
        /// </summary>
        private static RadialControllerMenuItem s_stepTextMenuItem;

        /// <summary>
        /// The textbox itself needed to refernece the current TextBox that is being modified
        /// </summary>
        private static TextBox s_textBox;


        /// <summary>
        /// The RadialController can be set from your app logic in case you use Surface Dial in other custom cases than on a TextBox. 
        /// This helper class will do everything for you, but if you want to control the Menu Items and/or wish to use the same Surface Dial insta
        /// This is the property for the static controller so you can access it if needed.
        /// </summary>
        public static RadialController Controller
        {
            get
            {
                return s_controller;
            }

            set
            {
                s_controller = value;
            }
        }



        /// <summary>
        /// If you provide the Controller yourself, set this to true so you won't add new menu items.
        /// </summary>
        public static readonly DependencyProperty ForceMenuItemProperty =
            DependencyProperty.RegisterAttached("ForceMenuItem", typeof(bool), typeof(SurfaceDialStepTextboxHelper), new PropertyMetadata(false));
        
        /// <summary>
        /// Set the default icon of the menu item that gets added. A user will most likely not see this. Defaults to the Ruler icon.
        /// </summary>
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.RegisterAttached("Icon", typeof(RadialControllerMenuKnownIcon), typeof(SurfaceDialStepTextboxHelper), new PropertyMetadata(RadialControllerMenuKnownIcon.Ruler));

        /// <summary>
        /// The amount the TextBox will be modified for each rotation step on the Surface Dial. This can be any double value. 
        /// </summary>
        public static readonly DependencyProperty StepValueProperty =
            DependencyProperty.RegisterAttached("StepValue", typeof(double), typeof(SurfaceDialStepTextboxHelper), new PropertyMetadata(0d, new PropertyChangedCallback(StepValueChanged)));

        /// <summary>
        /// A flag to enable or disable haptic feedback when rotating the dial for the give TextBox. This is enabled by default.
        /// </summary>
        public static readonly DependencyProperty EnableHapticFeedbackProperty =
            DependencyProperty.RegisterAttached("EnableHapticFeedback", typeof(bool), typeof(SurfaceDialStepTextboxHelper), new PropertyMetadata(true));
        
        /// <summary>
        /// Sets the minimum value the TextBox can have when modifying it using a Surface Dial. Default is -100.0
        /// </summary>
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.RegisterAttached("MinValue", typeof(double), typeof(SurfaceDialStepTextboxHelper), new PropertyMetadata(-100d));

        /// <summary>
        /// Sets the maxium value the TextBox can have when modifying it using a Surface Dial. Default is 100.0
        /// </summary>
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.RegisterAttached("MaxValue", typeof(double), typeof(SurfaceDialStepTextboxHelper), new PropertyMetadata(100d));
        
        /// <summary>
        /// TapToNext is a feature you can set to automatically try to focus the next focusable element from the Surface Dial enabled TextBox. This is on dy default.
        /// </summary>
        public static readonly DependencyProperty EnableTapToNextControlProperty =
            DependencyProperty.RegisterAttached("EnableTapToNextControl", typeof(bool), typeof(SurfaceDialStepTextboxHelper), new PropertyMetadata(true));

        /// <summary>
        /// EnableMinMax limits the value in the textbox to your spesificed Min and Max values, see the other properties.
        /// </summary>
        public static readonly DependencyProperty EnableMinMaxValueProperty =
            DependencyProperty.RegisterAttached("EnableMinMaxValue", typeof(bool), typeof(SurfaceDialStepTextboxHelper), new PropertyMetadata(false));




        /// <summary>
        /// Getter of the EnableMinMax property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetEnableMinMaxValue(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableMinMaxValueProperty);
        }

        /// <summary>
        /// Setter of the EnableMinMax property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetEnableMinMaxValue(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableMinMaxValueProperty, value);
        }
        
        /// <summary>
        /// Getter of the TapToNext flag.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetEnableTapToNextControl(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableTapToNextControlProperty);
        }

        /// <summary>
        /// Setter of the TapToNext flag.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetEnableTapToNextControl(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableTapToNextControlProperty, value);
        }

        /// <summary>
        /// Getter of the MaxValue
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static double GetMaxValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MaxValueProperty);
        }

        /// <summary>
        /// Setter of the MaxValue
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetMaxValue(DependencyObject obj, double value)
        {
            obj.SetValue(MaxValueProperty, value);
        }
        
        /// <summary>
        /// Getter of the MinValue
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static double GetMinValue(DependencyObject obj)
        {
            return (double)obj.GetValue(MinValueProperty);
        }

        /// <summary>
        /// Setter of the MinValue
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetMinValue(DependencyObject obj, double value)
        {
            obj.SetValue(MinValueProperty, value);
        }

        /// <summary>
        /// Setter of the StepValue.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static double GetStepValue(DependencyObject obj)
        {
            return (double)obj.GetValue(StepValueProperty);
        }

        /// <summary>
        /// Getter of the StepValue
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetStepValue(DependencyObject obj, double value)
        {
            obj.SetValue(StepValueProperty, value);
        }

        /// <summary>
        /// Getter of the Icon
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static RadialControllerMenuKnownIcon GetIcon(DependencyObject obj)
        {
            return (RadialControllerMenuKnownIcon)obj.GetValue(IconProperty);
        }

        /// <summary>
        /// Setter of the Icon
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetIcon(DependencyObject obj, RadialControllerMenuKnownIcon value)
        {
            obj.SetValue(IconProperty, value);
        }

        /// <summary>
        /// Setter of the Haptic Feedback property
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetEnableHapticFeedback(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableHapticFeedbackProperty);
        }

        /// <summary>
        /// Getter of the Haptic Feedback property
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetEnableHapticFeedback(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableHapticFeedbackProperty, value);
        }

        /// <summary>
        /// Getter of the Force Menu Item flag
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool GetForceMenuItem(DependencyObject obj)
        {
            return (bool)obj.GetValue(ForceMenuItemProperty);
        }
        /// <summary>
        /// Setter of the Force Menu Item flag
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="value"></param>
        public static void SetForceMenuItem(DependencyObject obj, bool value)
        {
            obj.SetValue(ForceMenuItemProperty, value);
        }


        /// <summary>
        /// This function gets called every time there is a rotational change on the connected Surface Dial while a Surface Dial enabled TextBox is in focus.
        /// This function ensures that the TextBox stays within the set range between MinValue and MaxValue while rotating the Surface Dial.
        /// It defaults the content of the TextBox to 0.0 if a non-numerical value is detected.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void Controller_RotationChanged(RadialController sender, RadialControllerRotationChangedEventArgs args)
        {
            if (s_textBox == null)
            {
                return;
            }

            string t = s_textBox.Text; double nr;

            if (double.TryParse(t, out nr))
            {
                nr += args.RotationDeltaInDegrees * GetStepValue(s_textBox);
                if(GetEnableMinMaxValue(s_textBox))
                { 
                    if (nr < GetMinValue(s_textBox))
                        nr = GetMinValue(s_textBox);
                    if (nr > GetMaxValue(s_textBox))
                        nr = GetMaxValue(s_textBox);
                }
            }
            else // default to zero if content is not a number
            {
                nr = 0.0d;
            }

            s_textBox.Text = nr.ToString("0.00");
        }

        /// <summary>
        /// Sets up the events needed for the current TextBox so it can trigger on GotFocus and LostFocus
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void StepValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textBox = d as TextBox;

            if (textBox == null)
            {
                return;
            }

            textBox.GotFocus += TextBox_GotFocus;
            textBox.LostFocus += TextBox_LostFocus;
        }

        /// <summary>
        /// When the focus of the TextBox is lost, ensure we clean up the events and Surface Dial menu item.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (GetForceMenuItem(s_textBox))
            {
                s_controller.Menu.Items.Remove(s_stepTextMenuItem);
            }
            s_controller.RotationChanged -= Controller_RotationChanged;
            if (GetEnableTapToNextControl(s_textBox))
            {
                s_controller.ButtonClicked -= Controller_ButtonClicked;
            }

            s_textBox = null;
        }

        /// <summary>
        /// When a Surface Dial TextBox gets focus, ensure the proper events are setup, and connect the Surface Dial itself.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            s_textBox = sender as TextBox;

            if (s_textBox == null)
            {
                return;
            }

            s_controller = s_controller ?? RadialController.CreateForCurrentView();

            if (GetForceMenuItem(s_textBox))
            { 
                s_stepTextMenuItem = RadialControllerMenuItem.CreateFromKnownIcon("Step Text Box", GetIcon(s_textBox));
                s_controller.Menu.Items.Add(s_stepTextMenuItem);
                s_controller.Menu.SelectMenuItem(s_stepTextMenuItem);
            }

            s_controller.UseAutomaticHapticFeedback = GetEnableHapticFeedback(s_textBox);
            s_controller.RotationResolutionInDegrees = 1;
            s_controller.RotationChanged += Controller_RotationChanged;
            if(GetEnableTapToNextControl(s_textBox))
            {
                s_controller.ButtonClicked += Controller_ButtonClicked;
            }

        }

        /// <summary>
        /// If the TapToNext flag is enabled, this function will try to take the focus to the next focusable element.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void Controller_ButtonClicked(RadialController sender, RadialControllerButtonClickedEventArgs args)
        {
            FocusManager.TryMoveFocus(FocusNavigationDirection.Next);
        }
    }
}
