namespace PublicStatusIndicator
{
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
}