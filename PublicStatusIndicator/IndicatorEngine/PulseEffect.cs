using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    internal class PulseEffect : Effect
    {
        public PulseEffect(Color[] source, Color[] target)
        {
            CurrentIdx = 0;
            TargetLenght = target.Length;
            MaxIndex = source.Length;
            DeltaStep = MaxIndex / TargetLenght;
            Source = source;
            Temp = new Color[target.Length];
        }

        public Color[] PulseStep()
        {
            for (var i = 0; i < TargetLenght; i++)
            {
                Temp[i] = Source[CurrentIdx];
            }

            CurrentIdx += DeltaStep;
            CurrentIdx = SaturateIndex(CurrentIdx);
            return Temp;
        }
    }
}