using System;

namespace PublicStatusIndicator.IndicatorEngine
{
    /// <summary>
    /// Boundend Value class. Creates Value which maintaines its own constraines during addition of values.
    /// </summary>
    public class BoundedInt
    {
        int _min;
        int _max;

        private int _val;
        /// <summary>
        /// Bounded Value
        /// </summary>
        public int Value
        {
            get { return _val; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        public BoundedInt(int min, int max)
        {
            _min = min;
            _max = max;
        }

        /// <summary>
        /// Gives relative position to current value.
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        public int RelativeTo(int delta)
        {
            // In case this Value is much greater then allowed range
            delta = delta % (_max - _min);
            int temp = _val;

            if (delta > 0) // if a is positive
            {
                temp += delta;
                if (temp > _max)
                {
                    temp -= (_max - _min);
                }
            }
            else // if a is negative
            {
                temp += delta;
                if (temp < _min)
                {
                    temp += (_max - _min);
                }
            }
            return temp;
        }

        /// <summary>
        /// Adds Delta to current value.
        /// </summary>
        /// <param name="delta"></param>
        /// <returns></returns>
        public int Add(int delta)
        {
            _val = RelativeTo(delta);
            return _val;
        }

        /// <summary>
        /// Determin shorts distance to demanded postion from current position
        /// </summary>
        /// <param name="abs"></param>
        /// <returns></returns>
        public int ShortesTo(int abs)
        {
            // simple distance
            int diff1 = abs - _val;
            // distance to rolled-over position
            int diff2 = abs - (_max - _min) - _val;

            if (Math.Abs(diff1) < Math.Abs(diff2))
            {
                return diff1;
            }
            else
            {
                return diff2;
            }
        }
    }
}