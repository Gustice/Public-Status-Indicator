using PublicStatusIndicator.IndicatorEngine;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.System.Threading;
using PublicStatusIndicator.Controller;

// To use the Web-Server
using PublicStatusIndicator.Webserver;

// Open Tasks
// - Settings have to be made avilable on construction event to set up all objects dinamically to particular needs
// - Settings have to be saved as settings file and made accesible to manipulate and save settings in order to customize the appearance of all effects

namespace PublicStatusIndicator
{
    public delegate void SetNewState(EngineState newState);
    public delegate void SetNewProfile(List<ProfileElement> newProfile);

    public delegate void SetPropotionalValue(float pos);
    public delegate float GetProportianalValue();


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

        private Frame _rootFrame = Window.Current.Content as Frame;
        readonly LED_Strip _ledStrip;

        private EngineState _state { get; set; }
        public DispatcherTimer RefreshTimer { get; set; }
        
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;

            _ledStrip = new LED_Strip(LEDSTRIP_LEN, LED_ROTATE_SMOOTHNESS, LED_PULSE_VALUES);
            _state = EngineState.Blank;

            InitWebserver();

            var dirPreset = new IncrementalEncoder(LEDSTRIP_LEN, DECODER_SAMPLE_TIME, 
                new IncrementalEncoder.PinConfig(DECODER_APIN, DECODER_BPIN, DECODER_SWPIN));
            dirPreset.OnIncrement += DirPreset_OnEncoderStepCB;
            dirPreset.OnSwitchPressed += DirPreset_OnSwitchPressedCB;

            RefreshTimer = new DispatcherTimer();
            RefreshTimer.Interval = TimeSpan.FromMilliseconds(MS_TICK);
            RefreshTimer.Tick += LED_Refresh_Tick;
            RefreshTimer.Start();

            dirPreset.StartSampling();
        }
            
        int _showOffStep = 0;
        private void DirPreset_OnSwitchPressedCB() // @todo hier ausbessern
        {
            switch (_ledStrip.State)
            {
                case EngineState.Blank:
                    _ledStrip.SetState(EngineState.Sauron);
                    break;
                case EngineState.Sauron:
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
                    break;
                case EngineState.Idle:
                    break;
                case EngineState.Progress:
                    break;
                case EngineState.Bad:
                    break;
                case EngineState.Unstable:
                    break;
                case EngineState.Stable:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
            webStatusCtrl.SetNewStateByHost += ChangeState_CB;
            webStatusCtrl.SetNewProfileByGui += ChangeProfile_CB;
            webStatusCtrl.SetBlamePosition += SetBlamePosition_CB;
            webStatusCtrl.GetFixPointPosition += ReturnCurrentFixPointPosition;

            var asyncAction = ThreadPool.RunAsync(workItem =>
            {
                var server = new HttpServer();
                server.RegisterController(webStatusCtrl);
                
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

        public void SetBlamePosition_CB(float blamePos)
        {
            _ledStrip.SetBlameProfile(blamePos);
        }

        public float ReturnCurrentFixPointPosition()
        {
            return _ledStrip.GetFixPointPosition();
        }


        private void LED_Refresh_Tick(object sender, object e)
        {
            _ledStrip.RefreshEvent();

            if (_rootFrame.Content is MainPage)
            {
                // Event an die GUI verdrahten.
                (_rootFrame.Content as MainPage).RefreshEvent();
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
                DebugSettings.EnableFrameRateCounter = true;
            }
#endif
            
            // Do not repeat app initialization when the Window already has content,
            // just ensure that the window is active
            if (_rootFrame == null)
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                _rootFrame = new Frame();

                _rootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: Load state from previously suspended application
                }

                // Place the frame in the current Window
                Window.Current.Content = _rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (_rootFrame.Content == null)
                {
                    // When the navigation stack isn't restored navigate to the first page,
                    // configuring the new page by passing required information as a navigation
                    // parameter
                    _rootFrame.Navigate(typeof(MainPage), e.Arguments);

                    if (_rootFrame.Content is MainPage)
                    {
                        // Event an die GUI verdrahten.
                        (_rootFrame.Content as MainPage).SetNewStateByGui += ChangeState_CB;
                        (_rootFrame.Content as MainPage).SetNewProfileByGui += ChangeProfile_CB;
                        (_rootFrame.Content as MainPage).SetBlamePosition += SetBlamePosition_CB;
                        (_rootFrame.Content as MainPage).GetFixPointPosition += ReturnCurrentFixPointPosition;
                        (_rootFrame.Content as MainPage).OnIncrement += DirPreset_OnEncoderStepCB;


                        (_rootFrame.Content as MainPage).ParentApp = this;
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
