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

        public EngineState State
        {
            get { return _ledIndicator.State; }
        }

        int _numPixels;
        int _smoothness;
        int _pulsePeriode;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="numPixels"></param>
        /// <param name="smothness"></param>
        /// <param name="pulsePeriode"></param>
        public LED_Strip(int numPixels, int smothness, int pulsePeriode)
        {
            _numPixels = numPixels;
            _smoothness = smothness;
            _pulsePeriode = pulsePeriode;

            StatusIndicator.IndicatorConfig config = new StatusIndicator.IndicatorConfig(numPixels, smothness, pulsePeriode);

            InitHardware();
            _ledIndicator = new StatusIndicator(config);
            _ledIndicator.MaxBrightness = 0xF0;
            _rgBring = new Color[_numPixels];
        }

        /// <summary>
        /// Change Saurons fix point on purpose
        /// </summary>
        /// <param name="num"></param>
        /// <param name="rel"></param>
        public void SetEyePosition(int num, int rel)
        {
            _ledIndicator.DeviateSauronsFixPoint(rel);
        }

        /// <summary>
        /// Event method to trigger next image of local indicator object
        /// </summary>
        public void RefreshEvent()
        {// @todo has to transfered to some kind of static function which refreshes all Indicator-Objects
            _rgBring = _ledIndicator.EffectAccordingToProfile();

            _leDstrip.SetAllLEDs(_rgBring);
            _leDstrip.UpdateLEDs();
        }

        /// <summary>
        /// Change state of local indicator object
        /// </summary>
        /// <param name="nextState"></param>
        public void SetState(EngineState nextState)
        {
            _ledIndicator.State = nextState;
            if (nextState == EngineState.Sauron)
            {
                if (_ledIndicator.Profile == null)
                {
                    _ledIndicator.Profile = EngineDefines.SummonSauron;
                }
            }
            else
            {
                _ledIndicator.Profile = null;
            }
        }

        /// <summary>
        /// Change profile (Set of states) of local indicator object
        /// </summary>
        /// <param name="profile"></param>
        public void SetProfile(List<ProfileElement> profile)
        {
            _ledIndicator.Profile = profile;
        }

        /// <summary>
        /// Set a Sauron-BlameProfile
        /// </summary>
        /// <param name="newPos"></param>
        public void SetBlameProfile(float newPos)
        {
            int pos;
            int len;
            _ledIndicator.GetSauronsFixPointToLength(out pos, out len);

            // Determine Blame position from relativ position
            int BalemPos = (int)(newPos * len);

            // Start allways about 140° away from demanded blame position (for sake of effect)
            int PosAtStart = BalemPos - (int)((float)len * 0.4);

            // First look nervously around demanded blame position (for sake of effect)
            int NevDelta = (int)((float)len * 0.05);

            _ledIndicator.Profile = EngineDefines.PrepareSauronBlameScript(PosAtStart, NevDelta, BalemPos);
        }

        /// <summary>
        /// Returns relative Value of current fixpoint position of sauron (Teach-Mode)
        /// 100 % = 1 represents the avilable ranges
        /// </summary>
        /// <returns></returns>
        public float GetFixPointPosition()
        {
            int pos;
            int len;
            _ledIndicator.GetSauronsFixPointToLength(out pos, out len);

            return (float)pos /len;
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