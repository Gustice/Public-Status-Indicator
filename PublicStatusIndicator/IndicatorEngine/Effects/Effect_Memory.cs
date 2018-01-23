using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    /// <summary>
    /// This Effekt memory class allows to display intermediate states. 
    /// So the movement of a pulse across a LED strip for example appears to be smoother.
    /// 
    /// The idea is to generate patterns with much more data-points beween the initial display:
    /// Example:
    /// A LED stripe with 5 elements has to display a rotating pulse:
    /// 
    /// Pulse to be rotated along the strip
    /// 1. |         
    /// 2. ||        
    /// 3. ||||||||| 
    /// 4. ||        
    /// 5. |          
    /// 
    ///                                                 Display State
    /// initial display     with intermediate states    init    two     three   four ....
    /// 1. |                |                            1                       5      
    ///                     |                                    1                      5
    ///                     |                                            1              
    /// 2. ||               ||                           2                       1      
    ///                     |||                                  2                      ...
    ///                     ||||                                         2       
    /// 3. |||||||||        |||||||||                    3                       2
    ///                     ||||                                 3               
    ///                     |||                                          3       
    /// 4. ||               ||                           4                       3
    ///                     |                                    4               
    ///                     |                                            4       
    /// 5. |                |                            5                       4
    ///                     |                                    5               
    ///                     |                                            5       
    ///                     
    ///  Appearence
    /// 1. |           >   |       >   |       >   ||        
    /// 2. ||          >   |||     >   ||||    >   ||||||||| 
    /// 3. |||||||||   >   ||||    >   |||     >   ||        
    /// 4. ||          >   |       >   |       >   |         
    /// 5. |           >   |       >   |       >   |             
    /// 
    /// 
    /// Although the strip has no more elements the iteratively transposed displaying appears hopefully smoother.
    /// </summary>

    internal class Effect_Memory
    {
        /// <summary>
        /// Count of elements of display medium (like LED strip or LCD-Display)
        /// In the above example it is 5
        /// </summary>
        protected int TargetLenght;

        /// <summary>
        /// Selfflipping Index value to current state of progress
        /// In the above example it is the pointer to 1
        /// It flips automatically back to the beginning if it exceeds the limits of defined waveform
        /// </summary>
        private int cRingIdx;
        public int RingIndex
        {
            get { return cRingIdx; }
            set
            {
                int Temp = value;
                if (Temp >= MaxIndex)
                {
                    Temp -= MaxIndex;
                }
                cRingIdx = Temp; }
        }

        /// <summary>
        /// Count of all datapoints to be smoothlee displayed
        /// In the above example it is 15
        /// </summary>
        protected int MaxIndex;

        /// <summary>
        /// Is the difference between displayed point in current state (this only applyes to rotated effects)
        /// In the above example it is 3
        /// </summary>
        protected int DeltaStep;

        /// <summary>
        /// Waveform to be gradually displayed 
        /// </summary>
        protected Color[] Source;

        /// <summary>
        /// current Image
        /// </summary>
        protected Color[] Temp;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected int CorrectIndexToRing(int index)
        {
            if (index >= MaxIndex)
            {
                index -= MaxIndex;
            }
            return index;
        }

        /// <summary>
        /// Resets Ring Index
        /// </summary>
        public void ResetIndex()
        {
            RingIndex = 0;
        }
    }
}