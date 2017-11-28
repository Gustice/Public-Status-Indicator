using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.System.Threading;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using PublicStatusIndicator.ApiController;
using PublicStatusIndicator.IndicatorEngine;
using PublicStatusIndicator.Webserver;
using PublicStatusIndicator.GUI_Elements;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PublicStatusIndicator
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        UserControl UC_ModePreview;
        UserControl UC_FormSettings;

        private UserControl _activePage;
        public UserControl ActivePage
        {
            get { return _activePage; }
            set { _activePage = value; NotifyPropertyChanged(); }
        }


        public MainPage()
        {
            UC_ModePreview = new Preview(this);
            UC_FormSettings = new Settings(this);
            ActivePage = UC_ModePreview;

            InitializeComponent();
            DataContext = this;

            InitWebserver();
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

        private void InitWebserver()
        {
            RouteManager.CurrentRouteManager.Controllers.Add(new StatusController());
            RouteManager.CurrentRouteManager.InitRoutes();
            var asyncAction = ThreadPool.RunAsync(workItem =>
            {
                var server = new HttpServer(80);
            });
        }

        public void RefreshEvent()
        {
            (UC_ModePreview as Preview).RefreshPage();
        }

        internal event App.SetNewState SetNewStateByGui;


        EngineState _cState = EngineState.Idle;
        DisplayMode dMode = DisplayMode.Preview;

        private void Button_Action(object sender, RoutedEventArgs e)
        {
            var cmdButton = (Button) sender;
            var newColor = Colors.DarkGray;
            EngineState newState = EngineState.Blank;
            switch (cmdButton.Name)
            {
                case "Blank":
                    newState = EngineState.Blank;
                    newColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
                    break;

                case "Idle":
                    newState = EngineState.Idle;
                    newColor = Color.FromArgb(0xFF, 0x20, 0x20, 0x20);
                    break;

                case "InProgress":
                    newState = EngineState.Progress;
                    newColor = Color.FromArgb(0xFF, 0x40, 0x40, 0x00);
                    break;

                case "StateBad":
                    newState = EngineState.Bad;
                    newColor = Color.FromArgb(0xFF, 0x80, 0x00, 0x00);
                    break;

                case "Unstable":
                    newState = EngineState.Stable;
                    newColor = Color.FromArgb(0xFF, 0x00, 0x80, 0x00);
                    break;

                case "Stable":
                    newState = EngineState.Stable;
                    newColor = Color.FromArgb(0xFF, 0x00, 0x80, 0x00);
                    break;

                case "showProfiles":
                    if (dMode == DisplayMode.Preview)
                    {
                        dMode = DisplayMode.Settings;
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
            CenterColor = new SolidColorBrush(newColor);

            // Tell GUI that state is changed
            (UC_ModePreview as Preview).ChangeState(newState);

            // Tell App that stet is changed
            SetNewStateByGui?.Invoke(newState);
        }

        enum DisplayMode
        {
            Preview,
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