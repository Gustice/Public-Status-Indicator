using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;

namespace PublicStatusIndicator
{
    public delegate void IncrementPassed(int pos);
    public delegate void SwitchPressed();

    internal class IncrementalEncoder
    {
        // Hardcoded configuration
        // Make shure that the perepheral device is connected to the appropriate ports
        #region HardCoded               
        const int SAMPLE_TIME = 10;
        const int APIN = 2;
        const int BPIN = 3;
        const int SWPIN = 4;
        #endregion

        private int _value;

        /// <summary>
        /// Output-value either adjusted to encoder resolution or to desired maximum output value.
        /// </summary>
        public int Value
        {
            get { return _value; }
            set
            {
                if (value >= Resolution)
                {
                    _value = 0;
                }
                else if (value < 0)
                {
                    _value = Resolution - 1;
                }
                else
                {
                    _value = value;
                }
            }
        }

        public int LostIncrements_Cnt { get; set; }

        public int Resolution { get; set; }

        GpioController _pinController;

        GpioPin _AChannel; 
        GpioPin _BChannel;
        GpioPin _Switch;

        private DispatcherTimer Sampler;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="resolution">Resolution of used encoder or desired output signal</param>
        public IncrementalEncoder(int resolution)
        {
            Resolution = resolution;
            InitGPIO();

            Sampler = new DispatcherTimer();
            Sampler.Interval = TimeSpan.FromMilliseconds(SAMPLE_TIME);
            Sampler.Tick += Timer_Tick;
        }

        internal event IncrementPassed OnIncrement;
        internal event SwitchPressed OnSwitchPressed;

        /// <summary>
        /// Abstraction method to start sampling
        /// </summary>
        public void StartSampling()
        {
            Sampler.Start();
        }

        /// <summary>
        /// Abstraction method to stop sampling
        /// </summary>
        public void StopSampling()
        {
            Sampler.Stop();
        }

        int lastSwitch;
        /// <summary>
        /// Sample timer to evaluate encoder-signal and switch
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, object e)
        {
            EvalEncoderIncrement();

            int sw = (int)_Switch.Read();

            if ((sw == 0) && (lastSwitch == 1)) // detect falling flag
            {
                OnSwitchPressed?.Invoke();
            }
            lastSwitch = sw;
        }

        int lastStep;
        /// <summary>
        /// Evaluates assoicated ports in order to find whether the encoder was moved or not in comparison to last invoke.
        /// If both signals where changed at once the counter for lost increments is incremented.
        /// </summary>
        /// <remarks> Note thate this method has to be invoked periodically</remarks>
        private void EvalEncoderIncrement()
        {
            int thisStep = ((int)_AChannel.Read()) | ((int)_BChannel.Read() << 1);
            thisStep |= ( (lastStep << 2) & 0xF);

            switch (thisStep)
            {
                case 0x0: // 00 00  
                case 0x5: // 01 01 
                case 0xA: // 10 10 
                case 0xF: // 11 11 
                    // No Changes
                    break;

                case 0x1: // 00 01
                case 0x7: // 01 11
                case 0xE: // 11 10
                case 0x8: // 10 00
                    Value++;
                    OnIncrement?.Invoke(Value);
                    break;

                case 0x2: // 00 10
                case 0xB: // 10 11
                case 0xD: // 11 01
                case 0x4: // 01 00
                    Value--;
                    OnIncrement?.Invoke(Value);
                    break;

                case 0x3: // 00 11
                case 0x6: // 01 10
                case 0x9: // 10 01
                case 0xC: // 11 00
                    LostIncrements_Cnt++;
                    break;

                default:
                    break;
            }

            lastStep = thisStep & (0x3);
        }

        /// <summary>
        /// Initializes digital ports according to hard-coded configuration
        /// </summary>
        private void InitGPIO()
        {
            _pinController = GpioController.GetDefault();
            try
            {
                _AChannel = _pinController.OpenPin(APIN);
                _AChannel.SetDriveMode(GpioPinDriveMode.Input);
                _BChannel = _pinController.OpenPin(BPIN);
                _BChannel.SetDriveMode(GpioPinDriveMode.Input);
                _Switch = _pinController.OpenPin(SWPIN);
                _Switch.SetDriveMode(GpioPinDriveMode.Input);
            }
            catch (System.Exception)
            {
                throw new System.Exception("There appearts to be a GPIO-Port missing or is already used");
            }
        }
    }
}