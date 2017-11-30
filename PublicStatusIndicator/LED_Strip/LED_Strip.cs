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

        public LED_Strip(int numPixels)
        {
            _numPixels = numPixels;
            InitHardware();
            _ledIndicator = new StatusIndicator(_numPixels, _numPixels * 6);
            _ledIndicator.MaxBrighness = 0xF0;
            _rgBring = new Color[_numPixels];
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

    enum E_SPIinterfaces
    {
        SPI0,
        SPI1
    }

    /// <summary>
    /// REG-LED with integrated synchronous serial interface
    /// </summary>
    public class LED_APA102 : GenreicLEDslave
    {
        /* Device info
        // SCLK-Idle-State is 0
        // Driver samples serial input at rising edge / MOSI has to be set on falling edge

        // The LEDs have a serial-data-input and a clock-Input for incoming LED-data
        // The LEDs have a serial-data-output and a cLock output for following LEDs (daisy-chain)
        // Each LED-value has a width of 32-Bit
        //  Start: 0x00000000
        //  Color:   8bit:      [111iiiii] Drive current (5 bit value)
        //              8bit:   [BBBBBBBB] Blue gray Value
        //              8bit:   [GGGGGGGG] Green gray Value
        //              8bit:   [RRRRRRRR] Red gray value
        //  Stop: 0xFFFFFFFF

        // Protocol:
        // Send start sequence.
        // Send color data to first LED in daisy-chain
        // Send color data for the following LEDs
        // Send end sequence (End Sequence is 
        //      Byte-Count = Number of LEDs /2
        // because of the inverted clock and data logic on the outputs compared to the inputs

        // LED can be used in daisy-chain. LED data for first LED is transmitted first.
        */

        /// <summary>
        /// SPI-Specs for LED-driver
        /// </summary>
        readonly static SPIHardwareConstrains InterfaceConstrains = new SPIHardwareConstrains
        {
            DemandedSPIMode = SpiMode.Mode0,
            MaxSPIclock = int.MaxValue, // Value is unknown
            MinSPIclock = 0
        };

        /// <summary>
        /// Specs for LED-driver
        /// </summary>
        readonly static LEDdriverSpecificDefinitions DriverDefines = new LEDdriverSpecificDefinitions
        {
            BlankOptions = LEDdriverSpecificDefinitions.eLEDBlankOptions.OnlyByNewDataSet,
            GrayValWidth = 8,
            GainValueWidth = 5,
            NumChannels = 3,
            IsDaisyChainSupported = true,
        };

        private readonly byte[] StartSeq = new byte[4] { 0, 0, 0, 0 };
        private byte[] StopSeq = new byte[4] { 0xFF, 0xFF, 0xFF, 0xFF };

        List<RGBset> LEDs;

        /// <summary>
        /// Constructor for LED_APA102
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on Raspi board</param>
        /// <param name="spiAdr">Defines the CS-address combination for addressing the slave</param>
        public LED_APA102(SpiDevice spiInterface, int stripLen)
            : base(spiInterface, InterfaceConstrains, DriverDefines)
        {
            LEDs = new List<RGBset>(); //@todo hier ggf. auf etwas Ressourcenschonenderes umsteigen

            for (int i = 0; i < stripLen; i++)
            {
                AddLED(RGBDefines.Black);
            }
        }

        /// <summary>
        /// Adds new RGB-LED to daisy-chain
        /// </summary>
        /// <param name="color"></param>
        public void AddLED(RGBValue startColor)
        {
            RGBset tLED = new RGBset();
            tLED.SetRGBvalue(startColor);
            LEDs.Add(tLED);

            // Definition der Stop-Squenz: 
            // Pro zwei LEDs ein zusätzliches Bit wegen der Clock-Verschiebung um 180°

            int StopSeqLenght = LEDs.Count / 2 / 8 + 1; // 
            StopSeqLenght = Math.Min(4, StopSeqLenght); // Aufgrund des Datenblattes jedoch immer minimum 1 Byte.
            StopSeq = new byte[StopSeqLenght];

            for (int i = 0; i < StopSeqLenght; i++)
            {
                StopSeq[i] = 0xFF;
            }
        }

        /// <summary>
        /// Sets LED intensity and color by generic RGBvalue of a certain LED in the daisy-chain
        /// </summary>
        /// <param name="index">Index of LED (starting with 0)</param>
        /// <param name="color">Color value to be set on chosen Index</param>
        public void SetLED(int index, RGBValue color)
        {
            if (index < LEDs.Count)
            {
                RGBset tLED = new RGBset();
                tLED.SetRGBvalue(color);
                LEDs[index] = tLED;
            }
        }

        /// <summary>
        /// Sets all LEDs with according to given Array.
        /// </summary>
        /// <param name="colors">Defines the new Colors for the array. 
        /// The array-length has to meet actual number of added LEDs</param>
        public void SetAllLEDs(RGBValue[] colors)
        {
            if (colors.Length == LEDs.Count)
            {
                RGBset tLED = new RGBset();
                for (int idx = 0; idx < LEDs.Count; idx++)
                {
                    tLED.SetRGBvalue(colors[idx]);
                    LEDs[idx] = tLED;
                }

            }
        }

        /// <summary>
        /// Sets all LEDs with according to given Array.
        /// </summary>
        /// <param name="colors">Defines the new Colors for the array. 
        /// The array-length has to meet actual number of added LEDs</param>
        public void SetAllLEDs(Color[] colors)
        {
            if (colors.Length == LEDs.Count)
            {
                RGBset tLED = new RGBset();
                for (int idx = 0; idx < LEDs.Count; idx++)
                {
                    tLED.SetRGBvalue(0xFF, colors[idx].R, colors[idx].G, colors[idx].B);
                    LEDs[idx] = tLED;
                }
            }
        }

        /// <summary>
        /// Sets LED intensity and color by local LED-Set of a certain LED in the daisy-chain
        /// </summary>
        /// <param name="index"></param>
        /// <param name="led"></param>
        public void SetLED(int index, RGBset led)
        {
            if (index < LEDs.Count)
            {
                LEDs[index] = led;
            }
        }

        /// <summary>
        /// Get LED Object
        /// </summary>
        /// <param name="index"></param>
        /// <param name="led"></param>
        public void GetLED(int index, out RGBset led)
        {
            if (index < LEDs.Count)
            {
                led = LEDs[index];
            }
            else
            {
                led = new RGBset();
            }
        }

        /// <summary>
        /// Get color of LED object
        /// </summary>
        /// <param name="index"></param>
        /// <param name="color"></param>
        public void GetLEDvalue(int index, out RGBValue color)
        {
            if (index < LEDs.Count)
            {
                LEDs[index].GetRGBvalue(out color);
            }
            else
            {
                color = new RGBValue();
            }
        }

        /// <summary>
        /// Generates LED-Driver data
        /// </summary>
        /// <param name="Send"></param>
        protected override void GenLEDStram(out byte[] Send)
        {
            int streamLen = RGBset.LEDValByteWidth * (LEDs.Count + 2);
            byte[] part;
            int idx;
            Send = new byte[streamLen];

            StartSeq.CopyTo(Send, 0);

            for (idx = 0; idx < LEDs.Count; idx++)
            {
                LEDs[idx].GenValueStram(out part);
                part.CopyTo(Send, (1 + idx) * RGBset.LEDValByteWidth);
            }

            StopSeq.CopyTo(Send, (1 + idx) * RGBset.LEDValByteWidth);
        }

        /// <summary>
        /// Resets LED values
        /// </summary>
        public override void ResetLEDs()
        {
            for (int idx = 0; idx < LEDs.Count; idx++)
            {
                LEDs[idx].SetRGBvalue(0, 0, 0, 0);
            }
        }

        /// <summary>
        /// Method to blank LED ports
        /// </summary>
        /// <param name="disalbeLEDs"></param>
        public override void BlankLEDs()
        {
            ResetLEDs();
            base.UpdateLEDs();
        }

        protected override void RestoreLEDs()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// RGB-LED-settings for color channels and drive current for single RGB-LED
        /// </summary>
        public struct RGBset
        {
            // Each Color Value is 32 bit Value with following structure:
            //  II BB GG RR
            //  Color:  MS 8bit:   [111iiiii] Driver current (with a Width of 5 bit)
            //             8bit:   [BBBBBBBB] Blue gray value
            //             8bit:   [GGGGGGGG] Green gray value
            //          LS 8bit:   [RRRRRRRR] Red gray value

            public const int ColorChannelWidth = 8;
            public const int IntensitiyWidth = 5;
            public const int MinRGBStepValue = 256;
            public const int MinIntensStepValue = 2048;
            public const int LEDValueWidt = 32;
            public const int LEDValByteWidth = 4;
            // @todo diese Definitionen in LED-Beschreibung einführen

            /// <summary>
            /// Binary LED-value of RGB-LED
            /// </summary>
            private UInt32 LEDValue { get; set; }

            /// <summary>
            /// Generates byte stream to set one single RGB-LED
            /// </summary>
            /// <param name="stream"></param>
            public void GenValueStram(out byte[] stream)
            {
                stream = new byte[4];
                stream[0] = (byte)(LEDValue >> 24);
                stream[1] = (byte)(LEDValue >> 16);
                stream[2] = (byte)(LEDValue >> 8);
                stream[3] = (byte)LEDValue;
            }

            /// <summary>
            /// Setter-method to set color and intensity of RGB-LED
            /// </summary>
            /// <param name="intens">Driving current for each LED color channel</param>
            /// <param name="red">Red channel</param>
            /// <param name="green">Green channel</param>
            /// <param name="blue">Blue channel</param>
            public void SetRGBvalue(byte intens, byte red, byte green, byte blue)
            {
                LEDValue = ((UInt32)(intens | 0xE0) << 24) | (UInt32)blue << 16 | (UInt32)green << 8 | (UInt32)red;
            }

            /// <summary>
            /// Setter-method to set color and intensity of RGB-LED
            /// </summary>
            /// <param name="color"></param>
            public void SetRGBvalue(RGBValue color)
            {
                byte intens = (byte)(color.Intensity >> 3);
                byte red = color.Red;
                byte green = color.Green;
                byte blue = color.Blue;
                this.SetRGBvalue(intens, red, green, blue);
            }

            /// <summary>
            /// Getter-method for brightness information for each channel
            /// </summary>
            /// <param name="intens"></param>
            /// <param name="red"></param>
            /// <param name="green"></param>
            /// <param name="blue"></param>
            public void GetRGBValue(out byte intens, out byte red, out byte green, out byte blue)
            {
                red = (byte)LEDValue;
                green = (byte)(LEDValue >> 8);
                blue = (byte)(LEDValue >> 16);
                intens = (byte)(LEDValue >> 24);
            }

            /// <summary>
            /// Getter-method for RGB-Color value
            /// </summary>
            /// <param name="startColor"></param>
            public void GetRGBvalue(out RGBValue color)
            {
                byte intens;
                byte red;
                byte green;
                byte blue;

                RGBValue tVal = new RGBValue();
                this.GetRGBValue(out intens, out red, out green, out blue);

                tVal.Intensity = (Byte)((Byte)intens << 3);
                tVal.Red = red;
                tVal.Green = green;
                tVal.Blue = blue;
                color = tVal;
            }
        }
    }

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
                //@todo ConfigRun immer Voraussetzen wenn neue LED hinzugefügt wird
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


    /// <summary>
    /// Defeinitions for LED-Driver devices
    /// </summary>
    public struct LEDdriverSpecificDefinitions
    {
        /// <summary>
        /// Width of on gray- or color value
        /// </summary>
        public int GrayValWidth { get; set; }

        /// <summary>
        ///  Defines the maximum gray-value
        /// </summary>
        public int MaxGrayVal { get; set; }

        /// <summary>
        /// Number of LED or color channels
        /// </summary>
        public int NumChannels { get; set; }

        /// <summary>
        /// Definition for global LED-gain value for all channels of driver
        /// </summary>
        public int GainValueWidth { get; set; }

        /// <summary>
        ///  Defines the maximum gain-value
        /// </summary>
        public int MaxGainVal { get; set; }

        /// <summary>
        /// Defines whether the driver supports daisychain configuration or not
        /// </summary>
        public bool IsDaisyChainSupported { get; set; }

        /// <summary>
        /// LED-shutdown or blank options
        /// </summary>
        public eLEDBlankOptions BlankOptions { get; set; }

        /// <summary>
        /// Defines options to disable LED ports
        /// </summary>
        public enum eLEDBlankOptions
        {
            /// LED outputs can be only schut down bo Zweo Set of Output Data
            OnlyByNewDataSet,
            /// LEDs can additianally be blanked by dedicated Input
            BlankInput,
        }
    }

    /// <summary>
    /// Base-Class for SPI-addressed peripheral devices.
    /// This base class is designed for the RasPi IO-module which has different SPI-controlled slaves populated.
    /// All IO-Slaves are controlled by the the same SP-interface the chip selection is achieved either by 
    /// a 3 bit CS-demultiplexer and/or dedicated control lines.
    /// </summary>
    abstract public class RaspiMultiSlave
    {
        /// <summary>
        /// Assigned SP-interface for SPI-device
        /// </summary>
        public SpiDevice SPIhandle { get; set; }


        /// <summary>
        /// Hardware constrains defines the restriction for using the respective device.
        /// The given restrictions are verified with the actual SPI configuration of given SPI handle during initialization.
        /// </summary>
        protected SPIHardwareConstrains SPIconstrains = new SPIHardwareConstrains
        {
            //DemandedSPIMode
            //MaxSPIclock
            //MinSPIclock
        };

        /// <summary>
        /// Constructor for RasPiMultiSlave
        /// </summary>
        /// <param name="spiInterface"> Defines the SP-interface on RasPi board</param>
        /// <param name="givenConstrains">Restriction for the respective device</param>
        public RaspiMultiSlave(SpiDevice spiInterface, SPIHardwareConstrains givenConstrains)
        {
            /// check if the SP-interface is defined
            if (spiInterface != null)
            {
                SpiConnectionSettings tempSet = spiInterface.ConnectionSettings;
                SPIconstrains = givenConstrains;

                /// Check whether the SPI-configuration meets demanded configuration
                if (tempSet.ClockFrequency < SPIconstrains.MinSPIclock)
                {
                    spiInterface = null;
                    throw new Exception("SPI-Clock doesn't meet specification: Clock was set to low");
                }
                if (tempSet.ClockFrequency > SPIconstrains.MaxSPIclock)
                {
                    spiInterface = null;
                    throw new Exception("SPI-Clock doesn't meet specification: Clock was set to high");
                }
                if (tempSet.Mode != SPIconstrains.DemandedSPIMode)
                {
                    spiInterface = null;
                    throw new Exception("SPI-Mode doesned meet specification");
                }

                /// Memorize SPI-Handle for further usage
                SPIhandle = spiInterface;
            }
            else
            {
                throw new Exception("Missing SP-interface-definition");
            }
        }

        /// <summary>
        /// Send data to SPI-slave
        /// </summary>
        /// <param name="sendData">Byte-Array for transmission</param>
        protected void SendByteStram(byte[] sendData)
        {
            if (SPIhandle != null)
            {
                SPIhandle.Write(sendData);
            }
        }

        /// <summary>
        /// Get data from SPI-slave
        /// </summary>
        /// <param name="recData">ByteArray for polled data</param>
        protected void GetByteStream(byte[] recData)
        {
            if (SPIhandle != null)
            {
                // Activate CS-signal and CS-address, if necessary
                SPIhandle.Read(recData);
                // Resets CS-signal and CS-address
            }
        }

        /// <summary>
        /// Transceive data to/from SPI-slave
        /// </summary>
        /// <param name="sendData">Byte-Array for data transmission</param>
        /// <param name="recData">ByteArray for polled data</param>
        protected void TranceiveByteStram(byte[] sendData, byte[] recData)
        {
            if (SPIhandle != null)
            {
                SPIhandle.TransferFullDuplex(sendData, recData);
            }
        }
    }

    /// <summary>
    /// Hardware constrains SPI-Slaves
    /// </summary>
    public struct SPIHardwareConstrains
    {
        /// <summary>
        /// SPI-clock-restrictions for using the device
        /// </summary>
        public int MinSPIclock { get; set; }
        /// <summary>
        /// SPI-clock-restrictions for using the device 
        /// </summary>
        public int MaxSPIclock { get; set; }
        /// <summary>
        /// SPI-mode for transmission
        /// </summary>
        public SpiMode DemandedSPIMode { get; set; }
    }
}