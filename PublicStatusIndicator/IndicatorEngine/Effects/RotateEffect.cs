using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    /// <summary>
    /// Effekt uses display-data with intermediate data-points to gain a smoothly appearing rotation effekt.
    /// See Effect_Memory for more detailed description
    /// </summary>
    internal class RotateEffect : Effect_Memory
    {
        /// <summary>
        ///  Constructor with smoothed dsiplay source and current target image
        ///  See base class for more detailed desctiption
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public RotateEffect(Color[] source, Color[] target)
        {
            RingIndex = 0;

            TargetLenght = target.Length;
            MaxIndex = source.Length;
            DeltaStep = MaxIndex / TargetLenght;

            Source = source;
            Temp = new Color[target.Length];
        }

        /// <summary>
        /// Generates on temporary image to display subset of smoothed source
        /// </summary>
        /// <returns></returns>
        public Color[] RotateStep()
        {
            var relIdx = RingIndex;

            for (int i = 0; i < TargetLenght; i++)
            {
                Temp[i] = Source[relIdx];
                relIdx += DeltaStep;
                relIdx = CorrectIndexToRing(relIdx);
            }

            RingIndex = RingIndex + 1;
            return Temp;
        }
    }
}