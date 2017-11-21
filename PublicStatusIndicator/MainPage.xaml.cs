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


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PublicStatusIndicator
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private SolidColorBrush _centerColor = new SolidColorBrush(Colors.DarkGray);

        private string _statusOutput = "Status";


        public MainPage()
        {
            Engine = InidicatorEngine.Instance;
            InitializeComponent();
            DataContext = this;

            InitWebserver();
        }

        public Engine Engine { get; }

        public string StatusOutput
        {
            get { return _statusOutput; }
            set
            {
                _statusOutput = value;
                NotifyPropertyChanged();
            }
        }


        public SolidColorBrush CenterColor
        {
            get { return _centerColor; }
            set
            {
                _centerColor = value;
                NotifyPropertyChanged();
            }
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


        private void Button_Action(object sender, RoutedEventArgs e)
        {
            var cmdButton = (Button) sender;

            var newColor = Colors.DarkGray;
            switch (cmdButton.Name)
            {
                case "Blank":
                    Engine.State = EngineState.Idle;
                    newColor = Color.FromArgb(0xFF, 0x20, 0x20, 0x20);
                    break;
                case "InProgress":
                    Engine.State = EngineState.Progress;
                    newColor = Color.FromArgb(0xFF, 0x40, 0x40, 0x00);

                    break;
                case "StateGood":
                    Engine.State = EngineState.Good;
                    newColor = Color.FromArgb(0xFF, 0x00, 0x80, 0x00);
                    break;
                case "StateBad":
                    Engine.State = EngineState.Bad;
                    newColor = Color.FromArgb(0xFF, 0x80, 0x00, 0x00);
                    break;

                case "showProfiles":
                {
                }
                    break;
                default:
                    break;
            }

            StatusOutput = Engine.State.ToString();
            CenterColor = new SolidColorBrush(newColor);
            Engine.RefreshTimer.Start();
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