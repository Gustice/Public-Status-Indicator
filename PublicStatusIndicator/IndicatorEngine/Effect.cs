using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    internal class Effect
    {
        protected int TargetLenght;
        protected int CurrentIdx;
        protected int MaxIndex;
        protected int DeltaStep;
        protected Color[] Source;
        protected Color[] Temp;

        protected int SaturateIndex(int index)
        {
            if (index >= MaxIndex)
            {
                index -= MaxIndex;
            }
            return index;
        }
        public void ResetIndex()
        {
            CurrentIdx = 0;
        }
    }
}