using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using PublicStatusIndicator.IndicatorEngine;
using PublicStatusIndicator.GUI_Elements;
using System.Collections.Generic;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PublicStatusIndicator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region HardCodedSettings
        private const int VIRTUAL_LEN = 12;
        private const int ROTATE_SMOOTHNESS = 3;
        private const int PULSE_VALUES = 72;
        #endregion

        StatusIndicator.IndicatorConfig PreviewConfig;

        UserControl UC_ModePreview;
        UserControl UC_FormSettings;


        private UserControl _activePage;
        public UserControl ActivePage
        {
            get { return _activePage; }
            set { _activePage = value; NotifyPropertyChanged(); }
        }

        internal App ParentApp;

        public Dictionary<string, List<ProfileElement>> DefinedProfiles { get; } = new Dictionary<string, List<ProfileElement>>(); 

        public MainPage()
        {
            PreviewConfig = new StatusIndicator.IndicatorConfig(VIRTUAL_LEN, ROTATE_SMOOTHNESS, PULSE_VALUES);

            UC_ModePreview = new Preview(this, PreviewConfig);
            UC_FormSettings = new Settings(this, PreviewConfig);
            ActivePage = UC_ModePreview;

            DefinedProfiles.Add("Summon Sauron", EngineDefines.SummonSauron);
            DefinedProfiles.Add("Dismiss Sauron", EngineDefines.DismissSauron);

            DefinedProfiles.Add("StepRight", EngineDefines.MoveHimRight);
            DefinedProfiles.Add("StepLeft", EngineDefines.MoveHimLeft);

            DefinedProfiles.Add("Appear/Disappear", EngineDefines.SauronAppearAndDisappear);
            DefinedProfiles.Add("Blame", EngineDefines.SauronBlame);
            
            InitializeComponent();
            DataContext = this;

        }

        private string _statusOutput = "Press Button";
        public string StatusOutput
        {
            get { return _statusOutput; }
            set { _statusOutput = value; NotifyPropertyChanged(); }
        }

        private SolidColorBrush _centerColor = new SolidColorBrush(Colors.DarkGray);
        public SolidColorBrush CenterColor
        {
            get { return _centerColor; }
            set { _centerColor = value; NotifyPropertyChanged(); }
        }


        public void RefreshEvent()
        {
            (UC_ModePreview as Preview).RefreshPage();
        }

        internal event SetNewState SetNewStateByGui;
        internal event SetNewProfile SetNewProfileByGui;

        /// <summary>
        /// Display-Mode
        /// </summary>
        DisplayMode dMode = DisplayMode.Preview;

        /// <summary>
        /// translates button action to particular command
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Action(object sender, RoutedEventArgs e)
        {
            var cmdButton = (Button) sender;
            EngineState newState = EngineState.Blank;
            switch (cmdButton.Name)
            {
                case "Blank":
                    newState = EngineState.Blank;
                    break;

                case "Idle":
                    newState = EngineState.Idle;
                    break;

                case "InProgress":
                    newState = EngineState.Progress;
                    break;

                case "StateBad":
                    newState = EngineState.Bad;
                    break;

                case "Unstable":
                    newState = EngineState.Unstable;
                    break;

                case "Stable":
                    newState = EngineState.Stable;
                    break;

                case "testSauron":
                    newState = EngineState.Sauron;
                    break;

                case "RunProfile":
                    newState = EngineState.Sauron;
                    SetNewProfileByGui?.Invoke(   (List<ProfileElement>)SauronProfileSelect.SelectedValue );
                    break;

                case "showProfiles":
                    if (dMode == DisplayMode.Preview)
                    {
                        dMode = DisplayMode.ShowProfiles;
                        ActivePage = UC_FormSettings;
                    }
                    else
                    {
                        dMode = DisplayMode.Preview;
                        ActivePage = UC_ModePreview;
                    }
                    break;

                default:
                    break;
            }

            StatusOutput = cmdButton.Name;
            CenterColor = new SolidColorBrush(EngineDefines.StateColors[newState]);

            // Tell GUI that state is changed
            (UC_ModePreview as Preview).ChangeState(newState);

            // Tell App that stet is changed
            SetNewStateByGui?.Invoke(newState);
        }

        /// <summary>
        /// Display contend
        /// </summary>
        enum DisplayMode
        {
            /// <summary>
            /// Simulates LED strip on display
            /// </summary>
            Preview,

            /// <summary>
            /// Shows gray-scale waveforms to visualize different profiles
            /// </summary>
            ShowProfiles,

            /// <summary>
            /// Shows settings dialogue
            /// </summary>
            Settings
        }
        #region PropChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}