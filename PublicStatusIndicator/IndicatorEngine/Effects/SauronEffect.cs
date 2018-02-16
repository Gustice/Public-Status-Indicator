using System;
using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    internal class SauronEffect
    {
        #region HardCoded
        const int DITHER_INTEVAL = 25 / 2;    // All x frames the eye could slightly move
        const int DITHER_SWING = 1;           // eyemovement for either direction

        const int EYE_MOVE_INTERVAL = 25 * 4;     // All x frames the eye can move to a completely different position
        const int EYE_MOVE_DURATION = 25 / 2;

        const int BLINK_INTERVAL = 25 * 10;   // All x frames the eye can blink
        const int BLAZE_MARGIN = byte.MaxValue * 50/ byte.MaxValue;          // The maximum brightness will be reduced to add noise which appears as blazing flames
        #endregion

        Random _blazeingP = new Random();

        int _targetLen;
        int _maxIndex;
        int _reduceFactor;

        Color[] Source;
        Color[] DimmedSource;
        Color[] TempOut;

        BoundedInt _R_Idx;

        SauronHabits Habits;

        public SauronEffect(Color[] source, Color[] target)
        {
            _targetLen = target.Length;
            Source = source;

            _maxIndex = source.Length;
            _reduceFactor = _maxIndex / _targetLen;

            TempOut = new Color[target.Length];
            DimmedSource = DimmFullScaleSource(Source);

            Habits = new SauronHabits(
                new SauronHabits.NervousEye.Config { Interval = DITHER_INTEVAL, Section = DITHER_SWING },
                new SauronHabits.CuriousEye.Config { Interval = EYE_MOVE_INTERVAL, Section = _maxIndex, Duration = EYE_MOVE_DURATION }
                );


            _R_Idx = new BoundedInt(0, _maxIndex-1);
        }

        Color[] DimmFullScaleSource(Color[] source)
        {
            Color[] dimmed = new Color[source.Length];
            int maxR = 0;
            int maxG = 0;
            int maxB = 0; 

            //for (int i = 0; i < source.Length; i++)
            //{
            //    if (source[i].R > maxR)
            //        maxR = source[i].R;
            //    if (source[i].G > maxG)
            //        maxG = source[i].G;
            //    if (source[i].B > maxB)
            //        maxB = source[i].B;
            //}

            int ilumFactor = byte.MaxValue - BLAZE_MARGIN;

            maxR = byte.MaxValue * ilumFactor / byte.MaxValue;
            maxG = byte.MaxValue * ilumFactor / byte.MaxValue;
            maxB = byte.MaxValue * ilumFactor / byte.MaxValue;

            // Normalize Pulse to maximum value by considering demanded offset value
            for (int i = 0; i < source.Length; i++)
            {
                dimmed[i].R = (byte)((int)source[i].R * maxR / byte.MaxValue);
                dimmed[i].G = (byte)((int)source[i].G * maxG / byte.MaxValue);
                dimmed[i].B = (byte)((int)source[i].B * maxB / byte.MaxValue);
            }

            return dimmed;
        }

        States step = States.Appear;

        int _Intensity = 0;
        const int DELTA_INTESITY = 5;
        const int MAX_INTESITY = 100;

        int stateCnt = 0;
        public Color[] SauronStep()
        {

            switch (step)
            {
                case States.Appear:
                    _Intensity += DELTA_INTESITY;
                    if (_Intensity >= MAX_INTESITY)
                    {
                        _Intensity = MAX_INTESITY;
                        step = States.Idle;
                        stateCnt = 0;
                    }
                    DisplayBlazingSpot(_Intensity);
                    break;

                case States.Idle:
                    // Dither direction ocasionally
                    relIdx = _R_Idx.RelativeTo(Habits.DitherEyeRandomly());
                    stateCnt++;
                    if (stateCnt >= 30)
                    {
                        step = States.Move;
                        stateCnt = 0;
                        relIdx = Habits.ChangeFixPoint(20);
                    }
                    DisplayBlazingSpot(_Intensity);
                    break;

                case States.Move:
                    relIdx = Habits.ChangeFixPoint();
                    stateCnt++;
                    if (stateCnt >= 30)
                    {
                        step = States.Mad;
                        stateCnt = 0;
                    }
                    DisplayBlazingSpot(_Intensity);
                    break;

                case States.Mad:
                    DisplayBlazingSpot(_Intensity);
                    step = States.Disappear;
                    break;

                case States.Disappear:
                    _Intensity -= DELTA_INTESITY;
                    if (_Intensity <= 0)
                    {
                        _Intensity = 0;
                        step = States.Idle;
                        stateCnt = 0;
                    }
                    DisplayBlazingSpot(_Intensity);
                    break;
                case States.Random:
                    break;
            }

            return TempOut;
        }

        int relIdx = 0;


        private void DisplayBlazingSpot(int intens)
        {
            // Display Blazing Spot
            int add = 0;
            for (int i = 0; i < _targetLen; i++)
            {
                TempOut[i] = DimmedSource[relIdx];

                add = (_blazeingP.Next(0, (int)Source[relIdx].R * BLAZE_MARGIN / byte.MaxValue));
                TempOut[i].R = (byte)(TempOut[i].R + (byte)add);
                TempOut[i].R = (byte)((int)TempOut[i].R * intens / MAX_INTESITY);

                add = (_blazeingP.Next(0, (int)Source[relIdx].G * BLAZE_MARGIN / byte.MaxValue));
                TempOut[i].G = (byte)(TempOut[i].G + (byte)add);
                TempOut[i].G = (byte)((int)TempOut[i].G * intens / MAX_INTESITY);

                add = (_blazeingP.Next(0, (int)Source[relIdx].B * BLAZE_MARGIN / byte.MaxValue));
                TempOut[i].B = (byte)(TempOut[i].B + (byte)add);
                TempOut[i].B = (byte)((int)TempOut[i].B * intens / MAX_INTESITY);

                relIdx += _reduceFactor;
                relIdx = CorrectIndexToRing(relIdx);
            }
        }



        //States _nextState = States.Appear;
        public void SetNextMode(States next)
        {
            step = next;
        }

        /// <summary>
        /// Return valid revolved index 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        protected int CorrectIndexToRing(int index)
        {
            if (index >= _maxIndex)
            {
                index -= _maxIndex;
            }
            return index;
        }

        int _nextFixtPoint = -1;
        public void MoveEyeTo(int direction)
        {
            _nextFixtPoint = direction;
        }

        /// <summary>
        /// Sauron States
        /// </summary>
        public enum States
        {
            /// <summary>
            /// Saurons eye is supposed to appear
            /// </summary>
            Appear,

            /// <summary>
            /// Saurons eye is supposed to disappear
            /// </summary>
            Disappear,

            /// <summary>
            /// Move Saurons eye to new fixpoint
            /// </summary>
            Move,

            /// <summary>
            /// Make Sauron mad
            /// </summary>
            Mad,

            /// <summary>
            /// Execute randome Moves
            /// </summary>
            Random,

            /// <summary>
            /// Make nothing special
            /// </summary>
            Idle,
        }
    }


    class SauronHabits
    {

        NervousEye Dither;
        CuriousEye Curious;

        public SauronHabits(NervousEye.Config cfgNervous, CuriousEye.Config cfgCurious)
        {
            Dither = new NervousEye(cfgNervous);
            Curious = new CuriousEye(cfgCurious);
        }


        public int DitherEyeRandomly()
        {
            return Dither.DitherEyeRandomly();
        }

        public int ChangeFixPointRandomly()
        {
            return Curious.ChangeFixPointRandomly();
        }

        public int ChangeFixPoint(int deltaPhi = 0)
        {
            if (deltaPhi != 0)
            {
                Curious.InitNewMove(deltaPhi);
            }
            return Curious.MovingStep();
        }

        int _rdmBlinkCnt = 0;
        void BlinkEyeRandomly()
        {

        }



        public class CuriousEye
        {
            Random _movementP = new Random();

            int _interval;
            int _maxSection;
            int _duration;
            int _halfDuration;


            public CuriousEye(CuriousEye.Config config)
            {
                _interval = config.Interval;
                _maxSection = config.Section;
                _duration = config.Duration;
                _halfDuration = _duration / 2;

                _rdmCnt = _interval;

            }

            int _deltaDirection = 0;
            float _alpha = 0;

            int _rdmCnt;
            public int ChangeFixPointRandomly()
            {
                // Get next fixpoint
                if (_rdmCnt <= 0)
                {
                    _rdmCnt = _interval;
                    _deltaDirection = _movementP.Next(_maxSection) - _maxSection / 2;
                    InitNewMove(_deltaDirection);
                }
                _rdmCnt--;

                return MovingStep();
            }

            int _AccStep = 0xFFFF;
            int _tempT = 0;

            float _phi = 0;
            float _phi_T_2 = 0;
            float _omega_T_2 = 0;

            int _phi_0 = 0;
            int _phi_T = 0;

            
            public void InitNewMove(int delta)
            {
                _alpha = (float) delta / 2 * 8 / (_duration * _duration);
                _AccStep = 0;
                _tempT = 0;
                _phi_0 = _phi_T;
            }

            public int MovingStep()
            {
                _tempT++;
                switch (_AccStep)
                {
                    case 0:
                        _phi = _alpha * (_tempT * _tempT) / 2;
                        _AccStep++;
                        break;

                    case 1:
                        _phi = _alpha * (_tempT * _tempT) / 2;
                        if (_tempT >= _halfDuration)
                        {
                            _omega_T_2 = _alpha * _tempT;
                            _phi_T_2 = _phi;
                            _tempT = 0;
                            _AccStep++;
                        }
                        break;

                    case 2:
                        _phi = _phi_T_2 + _omega_T_2 * _tempT - _alpha * (_tempT * _tempT) / 2;
                        if (_tempT >= _halfDuration)
                        {
                            _omega_T_2 = _alpha * _tempT;
                            _phi_T = (int)_phi;
                            _AccStep++;
                        }
                        break;

                    default:
                        // Make just nothing
                        break;
                }

                return _phi_0 + (int)_phi;
            }


            public struct Config
            {
                public int Interval;
                public int Section;
                public int Duration;
            }
        }


        public class NervousEye
        {
            Random _ditheringP = new Random();

            int _interval;
            int _section;

            public NervousEye(NervousEye.Config config)
            {
                _interval = config.Interval;
                _section = config.Section;
            }


            int _deltaDirection = 0;
            int _rdmCnt = 0;
            public int DitherEyeRandomly()
            {
                if (_rdmCnt <= 0)
                {
                    _rdmCnt = _interval;
                    _deltaDirection = _ditheringP.Next(_section * 2 +1) - _section;
                }
                _rdmCnt--;
                return _deltaDirection;
            }

            public struct Config
            {
                public int Interval;
                public int Section;
            }
        }
    }



    // @todo: This class has to be tested properly
    class BoundedInt
    {
        int _min;
        int _max;

        private int _val;
        public int Value
        {
            get { return _val; }
        }

        public BoundedInt(int min, int max)
        {
            _min = min;
            _max = max;
        }

        public int RelativeTo(int a)
        {
            // In case this Value is much greater then allowed range
            a = a % (_max - _min);
            int temp = _val;

            if (a > 0)
            {
                temp += a;

                if (temp > _max)
                {
                    temp -= (_max-_min);
                }
            }
            else
            {
                if (-a > temp)
                {
                    temp += a;

                    if(temp < _min)
                    {
                        temp += (_max - _min);
                    }
                }
            }
            return temp;
        }

        public int Add(int a)
        {
            _val = RelativeTo(a);
            return _val;
        }

    }
}