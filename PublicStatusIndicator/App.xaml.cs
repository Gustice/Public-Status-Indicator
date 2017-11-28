using PublicStatusIndicator.IndicatorEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;


// @todo VonBlank auf Idle
namespace PublicStatusIndicator
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        private const int LEDSTRIP_LEN = 20;
        private const int MS_TICK = 50;

        Frame rootFrame = Window.Current.Content as Frame;
        LED_Strip _ledStrip;

        public EngineState _state { get; set; }
        public DispatcherTimer RefreshTimer { get; set; }

        public delegate void SetNewState(EngineState newState);

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            _ledStrip = new LED_Strip(LEDSTRIP_LEN);
            _state = EngineState.Blank;
            
            RefreshTimer = new DispatcherTimer();
            RefreshTimer.Interval = TimeSpan.FromMilliseconds(MS_TICK);
            RefreshTimer.Tick += LED_Refresh_Tick;
        }

        public void ChangeState_CB(EngineState toState)
        {
            _ledStrip.ChangeState(toState);
            if (RefreshTimer.IsEnabled != true)
            {
                RefreshTimer.Start();
            }
        }

        private void LED_Refresh_Tick(object sender, object e)
        {
            _ledStrip.RefreshEvent();

            if (rootFrame.Content is MainPage)
            {
                // Event an die GUI verdrahten.
                (rootFrame.Content as MainPage).RefreshEvent();
            }
        }


        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootFrame = new Frame();

                rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);

                    if (rootFrame.Content is MainPage)
                    {
                        // Event an die GUI verdrahten.
                        (rootFrame.Content as MainPage).SetNewStateByGui += new SetNewState(ChangeState_CB);
                    }
                }
                // Ensure the current window is active
                Window.Current.Activate();
            }
        }

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
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
            //TODO: Save application state and stop any background activity
            deferral.Complete();
        }
    }
}
