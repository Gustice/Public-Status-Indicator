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
        private const int HW_SPI_CS_LINE = 0;
        private const string HW_SPI_LED_CONTROLLER = "SPI0";

        private LED_APA102 _leDstrip;
        private Color[] _rgBring;
        private SpiDevice _statusLedInterface;
        private readonly StatusIndicator _ledIndicator;

        private EngineState _state = EngineState.Blank;
        int _numPixels;
        int _smoothness;
        int _pulsePeriode;


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

            InitHardware();
            _ledIndicator = new StatusIndicator(_numPixels, _smoothness, _pulsePeriode);
            _ledIndicator.MaxBrighness = 0xF0;
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

        public void RefreshEvent()
        {
            _rgBring = _ledIndicator.EffectAccordingToState(_state);
            _leDstrip.SetAllLEDs(_rgBring);
            _leDstrip.UpdateLEDs();
        }

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

        public void BlankAllLEDs()
        {
            _leDstrip.BlankLEDs();
        }

        private async void InitHardware()
        {
            await InitSpi();
            _leDstrip = new LED_APA102(_statusLedInterface, _numPixels);
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
    }


}