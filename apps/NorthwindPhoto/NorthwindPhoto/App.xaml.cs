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

using System;
using System.Collections.ObjectModel;
using System.Reactive.Subjects;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using NorthwindPhoto.Model;

namespace NorthwindPhoto
{
    public class ProtocolDataEventArgs : EventArgs
    {
        public string Image { get; set; }
    }

    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private static RadialController _radialController;

        /// <summary>
        /// An Rx feed for the protocol acitvation. Anything listening to this will know that Cortana has been invoked.
        /// </summary>
        public static Subject<ProtocolDataEventArgs> ProtocolSubject = new Subject<ProtocolDataEventArgs>();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
        }

        /// <summary>
        /// The storage of all the Photos which can be accessed application wide.
        /// </summary>
        public static ObservableCollection<Photo> PhotoCollection { get; set; } = new ObservableCollection<Photo>();

        /// <summary>
        /// The main frame of the application
        /// </summary>
        public static Frame MainFrame { get; set; }

        /// <summary>
        /// Singleton instance of the Radial Controller for the app.
        /// </summary>
        public static RadialController RadialController
        {
            get
            {
                if (_radialController == null)
                {
                    if (RadialController.IsSupported())
                    {
                        _radialController = RadialController.CreateForCurrentView();
                    }
                }

                return _radialController;
            }
        }

        /// <summary>
        /// Called when activated via Cortana
        /// </summary>
        protected override void OnActivated(IActivatedEventArgs args)
        {
            if (args.Kind == ActivationKind.Protocol)
            {
                var eventArgs = args as ProtocolActivatedEventArgs;

                if (args.PreviousExecutionState != ApplicationExecutionState.Running)
                {
                    LoadData();

                    var rootFrame = Window.Current.Content as Frame;

                    // Do not repeat app initialization when the Window already has content,
                    // just ensure that the window is active
                    if (rootFrame == null)
                    {
                        // Create a Frame to act as the navigation context and navigate to the first page
                        rootFrame = new Frame();

                        if (rootFrame.Content == null)
                            rootFrame.Navigate(typeof(MainPage));

                        // Place the frame in the current Window
                        Window.Current.Content = rootFrame;
                    }
                    Window.Current.Activate();
                }

                /// TODO: 6. Cortana Activation
                ProtocolSubject.OnNext(new ProtocolDataEventArgs {Image = eventArgs.Uri.Host});
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            var rootFrame = Window.Current.Content as Frame;
            LoadData();

            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);

                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        private void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }

        /// <summary>
        /// Loading data from a private store.
        /// </summary>
        private void LoadData()
        {
            for (var i = 1; i < 62; i++)
                PhotoCollection.Add(new Photo
                {
                    Path =
                        $"http://adx.metulev.com/video/Images/Watermark/Large/FeaturedImage_2x1_Image{i.ToString("00")}.jpg"
                });
        }
    }
}
