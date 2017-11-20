using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    internal class RotateEffect : Effect
    {
        public RotateEffect(Color[] source, Color[] target)
        {
            CurrentIdx = 0;
            TargetLenght = target.Length;
            MaxIndex = source.Length;
            DeltaStep = MaxIndex / TargetLenght;
            Source = source;
            Temp = new Color[target.Length];
        }

        public Color[] RotateStep()
        {
            var relIdx = CurrentIdx;

            for (int i = 0; i < TargetLenght; i++)
            {
                Temp[i] = Source[relIdx];
                relIdx += DeltaStep;
                relIdx = SaturateIndex(relIdx);
            }

            CurrentIdx += DeltaStep;
            CurrentIdx = SaturateIndex(CurrentIdx);
            return Temp;
        }
    }
}