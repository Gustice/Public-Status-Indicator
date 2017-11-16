using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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

using Windows.UI;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using PublicStatusIndicator.IndicatorEngine;
using Windows.Devices.Spi;
using Windows.Devices.Enumeration;
using System.Threading.Tasks;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PublicStatusIndicator
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        #region PropChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion


        private string _statusOutput = "Status";
        public string StatusOutput
        {
            get { return _statusOutput; }
            set { _statusOutput = value; NotifyPropertyChanged(); }
        }

        public ObersableBrushes LEDring { get; } = new ObersableBrushes();

        private SolidColorBrush _centerColor = new SolidColorBrush(Colors.DarkGray);
        public SolidColorBrush CenterColor
        {
            get { return _centerColor; }
            set
            {
                _centerColor = value;
                NotifyPropertyChanged();
            }
        }
        DispatcherTimer RefreshTimer;

        StatusIndicator VirtualIndicator;
        StatusIndicator LEDIndicator;
        LED_APA102 LEDstrip;

        public MainPage()
        {
            InitHardware();

            RefreshTimer = new DispatcherTimer();
            RefreshTimer.Interval = TimeSpan.FromMilliseconds(100);
            RefreshTimer.Tick += LED_Refresh_Tick;

            this.InitializeComponent();
            DataContext = this;

            VirtualIndicator = new StatusIndicator(12, 4);
            LEDIndicator = new StatusIndicator(20, 2);
        }

        const int VIRTUAL_LEN = 12;
        const int LEDSTRIP_LEN = 20;

        Color[] VirtualRing = new Color[VIRTUAL_LEN];
        Color[] RGBring = new Color[LEDSTRIP_LEN];


        private async void InitHardware()
        {
            await InitSpi();
            LEDstrip = new LED_APA102(StatusLEDInterface, LEDSTRIP_LEN);
            LEDstrip.BlankLEDs();
        }

        SpiDevice StatusLEDInterface;
        private const int HW_SPI_CS_Line = 0;
        private const string HW_SPI_LED_Controller = "SPI0";

        private async Task InitSpi()
        {
            try
            {
                var settings = new SpiConnectionSettings(HW_SPI_CS_Line);
                settings.ClockFrequency = 4000000;
                settings.Mode = SpiMode.Mode0; // CLK-Idle ist low, Dataset on Falling Edge, Sample on Rising Edge
                string spiAqs = SpiDevice.GetDeviceSelector(HW_SPI_LED_Controller);
                var devicesInfo = await DeviceInformation.FindAllAsync(spiAqs);
                StatusLEDInterface = await SpiDevice.FromIdAsync(devicesInfo[0].Id, settings);
            }
            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }



        void LED_Refresh_Tick(object sender, object e)
        {
            VirtualRing = VirtualIndicator.EffectAccordingToState(DisplayState);
            RGBring = LEDIndicator.EffectAccordingToState(DisplayState);
            LEDring.SetAllVaules(VirtualRing);
            LEDstrip.SetAllLEDs(RGBring);
            LEDstrip.UpdateLEDs();
        }


        E_States DisplayState = E_States.Idle;

        Dictionary<E_States, string> StateOutputs = new Dictionary<E_States, string>
        {
            {E_States.Idle, "Idle" },
            {E_States.Progress, "Progress..." },
            {E_States.Good, "GOOD" },
            {E_States.Bad, "BAD" },
        };


        private void Button_Action(object sender, RoutedEventArgs e)
        {
            Button cmdButton = (Button)sender;

            Color newColor = Colors.DarkGray;
            switch (cmdButton.Name)
            {
                case "Blank":
                    DisplayState = E_States.Idle;
                    newColor = Color.FromArgb(0xFF, 0x20, 0x20, 0x20);
                    break;
                case "InProgress":
                    DisplayState = E_States.Progress;
                    newColor = Color.FromArgb(0xFF, 0x40, 0x40, 0x00);

                    break;
                case "StateGood":
                    DisplayState = E_States.Good;
                    newColor = Color.FromArgb(0xFF, 0x00, 0x80, 0x00);
                    break;
                case "StateBad":
                    DisplayState = E_States.Bad;
                    newColor = Color.FromArgb(0xFF, 0x80, 0x00, 0x00);
                    break;

                case "showProfiles":
                    {
                        

                    }
                    
                    break;
                default:
                    break;
            }
            StatusOutput = StateOutputs[DisplayState];
            CenterColor = new SolidColorBrush(newColor);

            RefreshTimer.Start();
        }

        public class ObersableBrushes : INotifyPropertyChanged
        {
            private SolidColorBrush _color1;
            public SolidColorBrush Color1
            {
                get { return _color1; }
                set { _color1 = value; NotifyPropertyChanged(); }
            }

            private SolidColorBrush _color2;
            public SolidColorBrush Color2
            {
                get { return _color2; }
                set { _color2 = value; NotifyPropertyChanged(); }
            }

            private SolidColorBrush _color3;
            public SolidColorBrush Color3
            {
                get { return _color3; }
                set { _color3 = value; NotifyPropertyChanged(); }
            }

            private SolidColorBrush _color4;
            public SolidColorBrush Color4
            {
                get { return _color4; }
                set { _color4 = value; NotifyPropertyChanged(); }
            }

            private SolidColorBrush _color5;
            public SolidColorBrush Color5
            {
                get { return _color5; }
                set { _color5 = value; NotifyPropertyChanged(); }
            }

            private SolidColorBrush _color6;
            public SolidColorBrush Color6
            {
                get { return _color6; }
                set { _color6 = value; NotifyPropertyChanged(); }
            }

            private SolidColorBrush _color7;
            public SolidColorBrush Color7
            {
                get { return _color7; }
                set { _color7 = value; NotifyPropertyChanged(); }
            }

            private SolidColorBrush _color8;
            public SolidColorBrush Color8
            {
                get { return _color8; }
                set { _color8 = value; NotifyPropertyChanged(); }
            }

            private SolidColorBrush _color9;
            public SolidColorBrush Color9
            {
                get { return _color9; }
                set { _color9 = value; NotifyPropertyChanged(); }
            }

            private SolidColorBrush _color10;
            public SolidColorBrush Color10
            {
                get { return _color10; }
                set { _color10 = value; NotifyPropertyChanged(); }
            }

            private SolidColorBrush _color11;
            public SolidColorBrush Color11
            {
                get { return _color11; }
                set { _color11 = value; NotifyPropertyChanged(); }
            }

            private SolidColorBrush _color12;
            public SolidColorBrush Color12
            {
                get { return _color12; }
                set { _color12 = value; NotifyPropertyChanged(); }
            }

            SolidColorBrush[] BrushReferences = new SolidColorBrush[12];

            public ObersableBrushes()
            {
                Color1 = new SolidColorBrush(Colors.DarkGray);
                Color2 = new SolidColorBrush(Colors.DarkGray);
                Color3 = new SolidColorBrush(Colors.DarkGray);
                Color4 = new SolidColorBrush(Colors.DarkGray);
                Color5 = new SolidColorBrush(Colors.DarkGray);
                Color6 = new SolidColorBrush(Colors.DarkGray);
                Color7 = new SolidColorBrush(Colors.DarkGray);
                Color8 = new SolidColorBrush(Colors.DarkGray);
                Color9 = new SolidColorBrush(Colors.DarkGray);
                Color10 = new SolidColorBrush(Colors.DarkGray);
                Color11 = new SolidColorBrush(Colors.DarkGray);
                Color12 = new SolidColorBrush(Colors.DarkGray);
            }

            public void SetAllVaules(Color[] newColors)
            {
                Color1 = new SolidColorBrush(newColors[0]);
                Color2 = new SolidColorBrush(newColors[1]);
                Color3 = new SolidColorBrush(newColors[2]);
                Color4 = new SolidColorBrush(newColors[3]);
                Color5 = new SolidColorBrush(newColors[4]);
                Color6 = new SolidColorBrush(newColors[5]);
                Color7 = new SolidColorBrush(newColors[6]);
                Color8 = new SolidColorBrush(newColors[7]);
                Color9 = new SolidColorBrush(newColors[8]);
                Color10 = new SolidColorBrush(newColors[9]);
                Color11 = new SolidColorBrush(newColors[10]);
                Color12 = new SolidColorBrush(newColors[11]);
            }

            #region PropChanged
            public event PropertyChangedEventHandler PropertyChanged;

            private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
            #endregion
        }
    }
}






