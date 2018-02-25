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
using Windows.System.Threading;

// To use the Web-Server
using PublicStatusIndicator.Webserver;
using PublicStatusIndicator.ApiController;

// Open Tasks
// - Settings have to be made avilable on construction event to set up all objects dinamically to particular needs
// - Settings have to be saved as settings file and made accesible to manipulate and save settings in order to customize the appearance of all effects

namespace PublicStatusIndicator
{
    public delegate void SetNewState(EngineState newState);
    public delegate void SetNewProfile(List<ProfileElement> newProfile);


    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        #region HardCoded
        private const int MS_TICK = 40;                 // -> Refresh-Frequency 25 per second
        private const int LEDSTRIP_LEN = 24;
        private const int LED_ROTATE_SMOOTHNESS  = 2;           // Rounds per second = SF * 24*40/1000 <= Hier ca. 2
        private const int LED_PULSE_VALUES = 3 * 1000/MS_TICK;  // 

        // Make shure that the perepheral device is connected to the appropriate ports
        const int DECODER_SAMPLE_TIME = 10;
        const int DECODER_APIN = 2;
        const int DECODER_BPIN = 3;
        const int DECODER_SWPIN = 4;
        #endregion

        Frame rootFrame = Window.Current.Content as Frame;
        LED_Strip _ledStrip;
        IncrementalEncoder DirPreset;

        public EngineState _state { get; set; }
        public DispatcherTimer RefreshTimer { get; set; }
        
        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            _ledStrip = new LED_Strip(LEDSTRIP_LEN, LED_ROTATE_SMOOTHNESS, LED_PULSE_VALUES);
            _state = EngineState.Blank;

            InitWebserver();

            DirPreset = new IncrementalEncoder(LEDSTRIP_LEN, DECODER_SAMPLE_TIME, 
                new IncrementalEncoder.PinConfig(DECODER_APIN, DECODER_BPIN, DECODER_SWPIN));
            DirPreset.OnIncrement += DirPreset_OnEncoderStepCB;
            DirPreset.OnSwitchPressed += DirPreset_OnSwitchPressedCB;

            RefreshTimer = new DispatcherTimer();
            RefreshTimer.Interval = TimeSpan.FromMilliseconds(MS_TICK);
            RefreshTimer.Tick += LED_Refresh_Tick;
            RefreshTimer.Start();

            DirPreset.StartSampling();
        }
            
        int _showOffStep = 0;
        private void DirPreset_OnSwitchPressedCB() // @todo hier ausbessern
        {
            if (_ledStrip.State == EngineState.Blank)
            {
                _ledStrip.SetState(EngineState.Sauron);
            }
            else if (_ledStrip.State == EngineState.Sauron)
            {
                switch (_showOffStep)
                {
                    case 0:
                        _showOffStep++;
                        //@todo hier den Mad-Aufruf bringen.
                        break;

                    default:
                        _showOffStep = 0;
                        _ledStrip.SetState(EngineState.Blank);
                        break;
                }
            }
        }

        private void DirPreset_OnEncoderStepCB(int abs, int rel)
        {
            if (_ledStrip.State == EngineState.Sauron)
            {
                _ledStrip.SetEyePosition(abs, rel);
            }
        }

        private void InitWebserver()
        {
            StatusController webStatusCtrl = new StatusController(this);
            webStatusCtrl.SetNewStateByHost += new SetNewState(ChangeState_CB);
            webStatusCtrl.SetNewProfileByGui += new SetNewProfile(ChangeProfile_CB);

            RouteManager.CurrentRouteManager.Controllers.Add(webStatusCtrl);
            RouteManager.CurrentRouteManager.InitRoutes();
            var asyncAction = ThreadPool.RunAsync(workItem =>
            {
                var server = new HttpServer(80);
            });
        }


        public void ChangeState_CB(EngineState toState)
        {
            _ledStrip.SetState(toState);
        }

        public void ChangeProfile_CB(List<ProfileElement> toProfile)
        {
            _ledStrip.SetProfile(toProfile);
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
                        (rootFrame.Content as MainPage).SetNewProfileByGui += new SetNewProfile(ChangeProfile_CB);
                        (rootFrame.Content as MainPage).ParentApp = this;
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
