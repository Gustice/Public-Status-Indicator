using System;
using Windows.Devices.Spi;

namespace PublicStatusIndicator
{
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
}