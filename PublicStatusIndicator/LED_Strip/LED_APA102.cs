using System;
using System.Collections.Generic;
using Windows.Devices.Spi;
using Windows.UI;

namespace PublicStatusIndicator
{
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
        //  Stop: 0xFFFFFFFF (This seems to be just an inacurate example in the datasheet.
        //      The correct stop sequence depends on count of linked LEDs due to phase-shift after every LED.
        //      It is   N_StopBits = N_LEDs/2
        //              N_StopBytes = (N_LEDs/2)/8+1 (represented as integer)

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
            MinSPIclock = 0 // No constraint in this case
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

        public void SetLED(int index, Color color)
        {
            if (index < LEDs.Count)
            {
                RGBset tLED = new RGBset();
                tLED.SetRGBvalue(color.A, color.R, color.G, color.B);
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
            int streamLen = RGBset.LEDValByteWidth * LEDs.Count + StartSeq.Length + StopSeq.Length;
            byte[] part;
            int i;
            int lastI;
            Send = new byte[streamLen];

            for (i = 0; i < StartSeq.Length; i++)
                Send[i] = StartSeq[i];
            lastI = i;

            for (int idx = 0; idx < LEDs.Count; idx++)
            {
                LEDs[idx].GenValueStram(out part);

                for (i=0; i < RGBset.LEDValByteWidth; i++)
                    Send[i+ lastI] = part[i];
                lastI += RGBset.LEDValByteWidth;
            }
            for (i = 0; i < StopSeq.Length; i++)
                Send[i + lastI] = StopSeq[i];
        }

        /// <summary>
        /// Resets LED values
        /// </summary>
        public override void ResetLEDs()
        {
            for (int idx = 0; idx < LEDs.Count; idx++)
            {
                RGBset temp = new RGBset();
                temp.SetRGBvalue(0xFF, 0, 0, 0);
                LEDs[idx] = temp;
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
                byte intens = (byte)(color.I >> 3);
                byte red = color.R;
                byte green = color.G;
                byte blue = color.B;
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

                tVal.I = (Byte)((Byte)intens << 3);
                tVal.R = red;
                tVal.G = green;
                tVal.B = blue;
                color = tVal;
            }
        }
    }
}