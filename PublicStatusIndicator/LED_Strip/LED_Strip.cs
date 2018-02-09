using System;
using System.Collections.Generic;
using Windows.Devices.Spi;
using Windows.UI;
using System.Threading.Tasks;

using PublicStatusIndicator.IndicatorEngine;
using Windows.Devices.Enumeration;

namespace PublicStatusIndicator
{
    internal class LED_Strip
    {
        // Hardcoded configuration
        // Make shure that the perepheral device is connected to the appropriate ports
        #region HardCoded_Configuration
        private const int HW_SPI_CS_LINE = 0;                   
        private const string HW_SPI_LED_CONTROLLER = "SPI0";    
        const int SPI_FREQU = 4000000;
        const SpiMode SPI_MODE = SpiMode.Mode0;
        #endregion

        private LED_APA102 _leDstrip;
        private Color[] _rgBring;
        private SpiDevice _statusLedInterface;
        private readonly StatusIndicator _ledIndicator;

        private EngineState _state = EngineState.Blank;
        int _numPixels;
        int _smoothness;
        int _pulsePeriode;

        //@todo hier bereinigen
        Color[] EyePrototype = new Color[] {
            Color.FromArgb(0xFF, 0x00, 0x00, 0x00),
            Color.FromArgb(0xFF, 0x40, 0x00, 0x00),
            Color.FromArgb(0xFF, 0x80, 0x00, 0x00),
            Color.FromArgb(0xFF, 0x80, 0x60, 0x00),
            Color.FromArgb(0xFF, 0x40, 0x00, 0x00),
            Color.FromArgb(0xFF, 0x80, 0x80, 0x00),
            Color.FromArgb(0xFF, 0x40, 0x00, 0x00),
            Color.FromArgb(0xFF, 0x80, 0x60, 0x00),
            Color.FromArgb(0xFF, 0x80, 0x00, 0x00),
            Color.FromArgb(0xFF, 0x40, 0x00, 0x00),
            Color.FromArgb(0xFF, 0x00, 0x00, 0x00),
        };
        Color[] EyeTemplate; 


        public LED_Strip(int numPixels, int smothness, int pulsePeriode)
        {
            _numPixels = numPixels;
            _smoothness = smothness;
            _pulsePeriode = pulsePeriode;

            StatusIndicator.IndicatorConfig config = new StatusIndicator.IndicatorConfig(numPixels, smothness, pulsePeriode);

            InitHardware();
            _ledIndicator = new StatusIndicator(config);
            _ledIndicator.MaxBrightnes = 0xF0;
            _rgBring = new Color[_numPixels];

            EyeTemplate = new Color[_numPixels * _smoothness*2];

            int len = EyePrototype.Length;
            for (int i = 0; i < len-1; i++)
            {
                for (int j = 0; j < _smoothness-1; j++)
                {
                    Color temp = Color.FromArgb(0xFF,
                        (byte)((int)EyePrototype[i].R * (_smoothness - j) / _smoothness + (int)EyePrototype[i + 1].R * (j) / _smoothness),
                        (byte)((int)EyePrototype[i].G * (_smoothness - j) / _smoothness + (int)EyePrototype[i + 1].G * (j) / _smoothness),
                        (byte)((int)EyePrototype[i].B * (_smoothness - j) / _smoothness + (int)EyePrototype[i + 1].B * (j) / _smoothness));
                    EyeTemplate[i * _smoothness + j] = temp;
                }
            }

            for (int i = 0; i < EyeTemplate.Length/2; i++)
            {
                EyeTemplate[i + EyeTemplate.Length / 2] = EyeTemplate[i];
            }
        }

        /// <summary>
        /// Event method to trigger next image of local indicator object
        /// </summary>
        public void RefreshEvent()
        {
            _rgBring = _ledIndicator.EffectAccordingToState(_state);
            _leDstrip.SetAllLEDs(_rgBring);
            _leDstrip.UpdateLEDs();
        }

        /// <summary>
        /// Change state of local indicator object
        /// </summary>
        /// <param name="newState"></param>
        public void ChangeState(EngineState newState)
        {
            _state = newState;
        }


        public void SetEyePosition(int num) // @todo ggf. löschen
        {
            _leDstrip.ResetLEDs();
            for (int i = 0; i < _numPixels; i++)
            {
                _leDstrip.SetLED(i, EyeTemplate[num+i*_smoothness]);
            }
            _leDstrip.UpdateLEDs();
        }

        /// <summary>
        /// Abstraction method ot blank LEDs of peripheral object
        /// </summary>
        public void BlankAllLEDs()
        {
            _leDstrip.BlankLEDs();
        }

        /// <summary>
        /// Initializes Hardware and peripheral data objects
        /// </summary>
        private async void InitHardware()
        {
            await InitSpi();
            _leDstrip = new LED_APA102(_statusLedInterface, _numPixels);
            _leDstrip.BlankLEDs();
        }


        /// <summary>
        /// Initializes SPI-Port with hard-coded configuration
        /// </summary>
        /// <returns></returns>
        private async Task InitSpi()
        {
            try
            {
                var settings = new SpiConnectionSettings(HW_SPI_CS_LINE);
                settings.ClockFrequency = SPI_FREQU;
                settings.Mode = SPI_MODE; // CLK-Idle ist low, Dataset on Falling Edge, Sample on Rising Edge
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
    }


}