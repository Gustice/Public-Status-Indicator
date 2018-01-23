using System;
using System.Collections.Generic;
using Windows.UI;

namespace PublicStatusIndicator
{
    /// <summary>
    /// RGB-LED-Settings for color channels and drive current for one LED
    /// </summary>
    public struct RGBValue
    {
        /// <summary>
        /// Defines the Maximum Value that can be set
        /// </summary>
        public const int MaxValue = Byte.MaxValue;

        /// <summary>
        /// Red Value
        /// </summary>
        public Byte R { get; set; }

        /// <summary>
        /// Green Value
        /// </summary>
        public Byte G { get; set; }

        /// <summary>
        /// Blue Value
        /// </summary>
        public Byte B { get; set; }

        /// <summary>
        /// Intensity Value
        /// </summary>
        public Byte I { get; set; }

        /// <summary>
        /// Method to preset all color values
        /// </summary>
        /// <param name="r">Red value</param>
        /// <param name="g">Green value</param>
        /// <param name="b">Blue value</param>
        /// <param name="i">Intensity value (md</param>
        public RGBValue(byte r = 0, byte g = 0, byte b = 0, byte i = 0)
        {
            R = r;
            G = g;
            B = b;
            I = i;
        }

        public static RGBValue operator *(RGBValue value, float factor)
        {
            value.R = (byte)((float)value.R * factor);
            value.G = (byte)((float)value.G * factor);
            value.B = (byte)((float)value.B * factor);
            return (value);
        }

        public static RGBValue operator +(RGBValue value1, RGBValue value2)
        {
            value1.R += value2.R;
            value1.G += value2.G;
            value1.B += value2.B;
            return (value1);
        }

        public static RGBValue operator -(RGBValue value1, RGBValue value2)
        {
            value1.R -= value2.R;
            value1.G -= value2.G;
            value1.B -= value2.B;
            return (value1);
        }
    }

    /// <summary>
    /// Generic RGB-Color-Definitions
    /// </summary>
    public static class RGBDefines
    {
        const Byte MaxValue = RGBValue.MaxValue;
        public readonly static RGBValue Black = new RGBValue { I = MaxValue, R = 0, G = 0, B = 0 };
        public readonly static RGBValue Red = new RGBValue { I = MaxValue, R = MaxValue, G = 0, B = 0 };
        public readonly static RGBValue Green = new RGBValue { I = MaxValue, R = 0, G = MaxValue, B = 0 };
        public readonly static RGBValue Blue = new RGBValue { I = MaxValue, R = 0, G = 0, B = MaxValue };
        public readonly static RGBValue Yellow = new RGBValue { I = MaxValue, R = MaxValue, G = MaxValue, B = 0 };
        public readonly static RGBValue Cyan = new RGBValue { I = MaxValue, R = 0, G = MaxValue, B = MaxValue };
        public readonly static RGBValue Magenta = new RGBValue { I = MaxValue, R = MaxValue, G = 0, B = MaxValue };
        public readonly static RGBValue White = new RGBValue { I = MaxValue, R = MaxValue, G = MaxValue, B = MaxValue };
    }
}