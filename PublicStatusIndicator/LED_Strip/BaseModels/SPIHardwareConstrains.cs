using Windows.Devices.Spi;

namespace PublicStatusIndicator
{
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