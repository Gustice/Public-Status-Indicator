using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    /// <summary>
    /// Effekt uses display-data with intermediate data-points to gain a smoothly appearing pulse effekt.
    /// See Effect_Memory for more detailed description
    /// </summary>
    internal class PulseEffect : Effect_Memory
    {
        /// <summary>
        ///  Constructor with smoothed dsiplay source and current target image
        ///  See base class for more detailed desctiption
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public PulseEffect(Color[] source, Color[] target)
        {
            RingIndex = 0;

            TargetLenght = target.Length;
            MaxIndex = source.Length;

            Source = source;
            Temp = new Color[target.Length];
        }

        /// <summary>
        /// Generates on temporary image to display one point out of smoothed source
        /// </summary>
        /// <returns></returns>
        public Color[] PulseStep()
        {
            for (var i = 0; i < TargetLenght; i++)
            {
                Temp[i] = Source[RingIndex];
            }

            RingIndex = RingIndex + 1;
            return Temp;
        }
    }
}