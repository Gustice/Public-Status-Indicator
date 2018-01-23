using Windows.Devices.Spi;

namespace PublicStatusIndicator
{
    /// <summary>
    /// Generic class for SPI-controlled LED-driver or integrated LEDs
    /// This class is designed for unsigned 16 bit values
    /// </summary>
    abstract public class GenreicLEDslave : RaspiMultiSlave
    {
        /// <summary>
        /// Fullscale value regardless actual LED-Driver implementation.
        /// </summary>
        public int NormValueWith { get; set; }
        /// <summary>
        /// Fullscale value Regardless actual LED-Driver implementation.
        /// </summary>
        public int MaxLEDportValue { get; set; }

        /// <summary>
        /// Place holder for particular LED-Driver-implementation
        /// </summary>
        protected LEDdriverSpecificDefinitions LEDSpecifigdefines = new LEDdriverSpecificDefinitions { };

        /// <summary>
        /// Container for last message to restore blanked LED-values
        /// </summary>
        byte[] LastMessage;

        /// <summary>
        /// Constructor for LED-driver
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on RasPi board</param>
        /// <param name="spiAdr">Defines the CS-address combination for addressing the slave</param>
        /// <param name="givenConstrains">Restriction for using the respective device</param>
        /// <param name="givenSpecs">Specifications of given LED-Slave</param>
        public GenreicLEDslave(SpiDevice spiInterface, SPIHardwareConstrains givenConstrains, LEDdriverSpecificDefinitions givenSpecs)
            : base(spiInterface, givenConstrains)
        {
            LEDSpecifigdefines = givenSpecs;
        }

        /// <summary>
        /// Shows if LED-driver have already been configured
        /// </summary>
        public bool ConfigRunDone { get; set; } = false;

        /// <summary>
        /// Sends new port values to LED-driver 
        /// </summary>
        public void UpdateLEDs()
        {
            byte[] Send;

            /// Execute configuration run (if necessary)
            if (ConfigRunDone == false)
            {
                ConfigRun();
                ConfigRunDone = true;
            }

            /// Generate send-stream
            GenLEDStram(out Send);

            /// send data
            base.SendByteStram(Send);
            /// Latch data (if necessary)
            LatchData();

            LastMessage = Send;
        }

        /// <summary>
        /// Prepare send-byte array for transmission
        /// </summary>
        /// <param name="Send"></param>
        abstract protected void GenLEDStram(out byte[] Send);

        /// <summary>
        /// Execute device-specific operation to latch port data
        /// </summary>
        virtual protected void LatchData() { }

        /// <summary>
        /// Write device configuration
        /// </summary>
        virtual protected void ConfigRun() { }

        /// <summary>
        /// Blank LEDs (switch all Ports off)
        /// </summary>
        /// <param name="disalbeLEDs"></param>
        abstract public void BlankLEDs();

        /// <summary>
        /// Restore latest Values bevore blank operation
        /// </summary>
        abstract protected void RestoreLEDs();

        /// <summary>
        /// Resets all stored gray values and sets the LED-driver in default reset configuration
        /// </summary>
        abstract public void ResetLEDs();
    }
}