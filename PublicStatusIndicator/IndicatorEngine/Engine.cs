using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Spi;
using Windows.UI;
using Windows.UI.Xaml;

namespace PublicStatusIndicator.IndicatorEngine
{
    public class Engine
    {
        private const int VIRTUAL_LEN = 12;
        private const int LEDSTRIP_LEN = 20;
        private const int HW_SPI_CS_LINE = 0;
        private const string HW_SPI_LED_CONTROLLER = "SPI0";

        public readonly Dictionary<EngineState, string> StateOutputs = new Dictionary<EngineState, string>
        {
            {EngineState.Idle, "Idle"},
            {EngineState.Progress, "Progress..."},
            {EngineState.Good, "GOOD"},
            {EngineState.Bad, "BAD"}
        };

        private readonly StatusIndicator _ledIndicator;

        private LED_APA102 _leDstrip;
        private Color[] _rgBring;
        private SpiDevice _statusLedInterface;
        private readonly StatusIndicator _virtualIndicator;
        private Color[] _virtualRing;
        
        public Engine()
        {
            _virtualIndicator = new StatusIndicator(12, 4);
            _ledIndicator = new StatusIndicator(20, 2);
            _virtualRing = new Color[VIRTUAL_LEN];
            _rgBring = new Color[LEDSTRIP_LEN];

            LEDRing = new ObersableBrushes();

            InitHardware();

            RefreshTimer = new DispatcherTimer();
            RefreshTimer.Interval = TimeSpan.FromMilliseconds(100);
            RefreshTimer.Tick += LED_Refresh_Tick;
        }

        public EngineState State { get; set; }

        public ObersableBrushes LEDRing { get; }

        public DispatcherTimer RefreshTimer { get; set; }

        private async void InitHardware()
        {
            await InitSpi();
            _leDstrip = new LED_APA102(_statusLedInterface, LEDSTRIP_LEN);
            _leDstrip.BlankLEDs();
        }

        private async Task InitSpi()
        {
            try
            {
                var settings = new SpiConnectionSettings(HW_SPI_CS_LINE);
                settings.ClockFrequency = 4000000;
                settings.Mode = SpiMode.Mode0; // CLK-Idle ist low, Dataset on Falling Edge, Sample on Rising Edge
                var spiAqs = SpiDevice.GetDeviceSelector(HW_SPI_LED_CONTROLLER);
                var devicesInfo = await DeviceInformation.FindAllAsync(spiAqs);
                _statusLedInterface = await SpiDevice.FromIdAsync(devicesInfo[0].Id, settings);
            }
            /* If initialization fails, display the exception and stop running */
            catch (Exception ex)
            {
                throw new Exception("SPI Initialization Failed", ex);
            }
        }


        private void LED_Refresh_Tick(object sender, object e)
        {
            _virtualRing = _virtualIndicator.EffectAccordingToState(State);
            _rgBring = _ledIndicator.EffectAccordingToState(State);
            LEDRing.SetAllVaules(_virtualRing);
            _leDstrip.SetAllLEDs(_rgBring);
            _leDstrip.UpdateLEDs();
        }
    }

    public static class InidicatorEngine
    {
        private static Engine _engine;

        public static Engine Instance
        {
            get
            {
                if (_engine == null)
                    _engine = new Engine();
                return _engine;
            }
        }
    }
}