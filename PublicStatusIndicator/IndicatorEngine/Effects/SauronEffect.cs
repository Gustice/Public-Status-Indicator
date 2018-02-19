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
        const int EYE_MOVE_FAST = 25 / 4;


        const int BLINK_INTERVAL = 25 * 10;   // All x frames the eye can blink
        #endregion

        Random _blazeingP = new Random();
        Random _fireI = new Random();
        Random _flickrP = new Random();

        int _targetLen;
        int _maxIndex;
        int _reduceFactor;

        Color[] EyeSrc;
        Color[] BlazeEnvelope;
        Color[] FireEnvelope;
        Color[] TempOut;

        BoundedInt _R_Idx;

        SauronHabits Habits;

        public SauronEffect(SauronEffect.SauronsEye template, Color[] target)
        {
            _targetLen = target.Length;
            EyeSrc = template.Iris;
            BlazeEnvelope = template.Aurora;
            FireEnvelope = template.Fire;


            _maxIndex = template.Iris.Length;
            _reduceFactor = _maxIndex / _targetLen;

            TempOut = new Color[target.Length];

            Habits = new SauronHabits(
                new SauronHabits.NervousEye.Config { Interval = DITHER_INTEVAL, Section = DITHER_SWING },
                new SauronHabits.CuriousEye.Config { Interval = EYE_MOVE_INTERVAL, Section = _maxIndex, Duration = EYE_MOVE_DURATION }
                );


            _R_Idx = new BoundedInt(0, _maxIndex-1);
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
                        relIdx = Habits.ChangeFixPoint(20, EYE_MOVE_FAST);
                    }
                    DisplayBlazingSpot(_Intensity);
                    break;

                case States.Move:
                    relIdx = Habits.ChangeFixPoint();
                    stateCnt++;
                    if (stateCnt >= 30)
                    {
                        step = States.Mad;
                        _madState = 0;
                        stateCnt = 0;
                    }
                    DisplayBlazingSpot(_Intensity);
                    break;

                case States.Mad:
                    DisplayMadSauron();
                    stateCnt++;
                    if (stateCnt >= 5*25)
                    {
                        step = States.Disappear;
                        stateCnt = 0;
                    }
                    break;

                case States.Disappear:
                    _Intensity -= DELTA_INTESITY;
                    if (_Intensity <= 0)
                    {
                        _Intensity = 0;
                        step = States.Random;
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
            for (int i = 0; i < _targetLen; i++)
            {
                TempOut[i] = EyeSrc[relIdx];

                TempOut[i].R = (byte)(TempOut[i].R + (byte)(_blazeingP.Next(0, BlazeEnvelope[relIdx].R)));
                TempOut[i].R = (byte)((int)TempOut[i].R * intens / MAX_INTESITY);

                TempOut[i].G = (byte)(TempOut[i].G + (byte)(_blazeingP.Next(0, BlazeEnvelope[relIdx].G)));
                TempOut[i].G = (byte)((int)TempOut[i].G * intens / MAX_INTESITY);

                TempOut[i].B = (byte)(TempOut[i].B + (byte)(_blazeingP.Next(0, BlazeEnvelope[relIdx].B)));
                TempOut[i].B = (byte)((int)TempOut[i].B * intens / MAX_INTESITY);

                relIdx += _reduceFactor;
                relIdx = CorrectIndexToRing(relIdx);
            }
        }



        int _madState = 0;
        int _madWhaitCnt = 0;
        float _FireIntensity = 0;

        const int MAD_WAIT_STATES = 50;
        const int MAX_FIRE_INTENS = byte.MaxValue/4;
        const int FIRE_INTENS_SLOPE = 1;
        private void DisplayMadSauron()
        {
            switch (_madState)
            {
                case 0:

                    _FireIntensity = (float)Math.Exp((float)_madWhaitCnt/3)-1;
                    if (_FireIntensity >= MAX_FIRE_INTENS)
                    {
                        _FireIntensity = MAX_FIRE_INTENS;
                        _madState++;
                        _madWhaitCnt = 0;
                    }
                    _madWhaitCnt++;
                    break;

                case 1:
                    _madWhaitCnt++;
                    if (_madWhaitCnt >= MAD_WAIT_STATES)
                    {
                        _madState++;
                        _madWhaitCnt = 0;
                    }
                    break;

                case 2:
                    _FireIntensity = _FireIntensity * (float)Math.Exp((float)-1 / 3);
                    if (_FireIntensity <= 1)
                    {
                        _FireIntensity = 0;
                        _madState++;
                        _madWhaitCnt = 0;
                    }
                    break;

                default:
                    break;
            }

            int invFireIntens = MAX_FIRE_INTENS - (int)_FireIntensity;

            // Display Blazing Spot with fire in the background

            byte burn, hBurn, eye, fire, blaze;
            int _tempInt;

            byte cEyeIntes = (byte)_flickrP.Next(70, 100);

            for (int i = 0; i < _targetLen; i++)
            {
                eye = (byte)(EyeSrc[relIdx].R * cEyeIntes / 100);
                blaze = (byte)(_blazeingP.Next(0, BlazeEnvelope[relIdx].R));
                if (eye < _FireIntensity)
                {
                    _tempInt = (byte)((byte)_FireIntensity - eye);
                }
                else
                {
                    _tempInt = 0;
                }
                fire = (byte)(FireEnvelope[relIdx].R * _tempInt / 50);

                burn = (byte)_fireI.Next(fire * 2 / 3, fire);
                hBurn = (byte)_fireI.Next(4);

                TempOut[i].R = (byte)(burn + eye + blaze);
                TempOut[i].G = (byte)(burn * hBurn / 8 + eye * _FireIntensity / MAX_FIRE_INTENS + blaze);
                TempOut[i].B = (byte)(eye * _FireIntensity / MAX_FIRE_INTENS / 3);

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
        /// Eye Class to concentrate templates to generate different patterns
        /// </summary>
        public class SauronsEye
        {
            /// <summary>
            /// Envelope curve for iris
            /// </summary>
            public Color[] Iris;

            /// <summary>
            /// Envelope curve for blazing aroura around iris
            /// </summary>
            public Color[] Aurora;

            /// <summary>
            /// Envelope curve for fire in case of madness
            /// </summary>
            public Color[] Fire;
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


    /// <summary>
    /// Concentrates different haits of sauron and kan invoke the either randomly or at trigger event
    /// </summary>
    class SauronHabits
    {

        NervousEye Dither;
        CuriousEye Curious;
        BlinkyEye Blinky;

        // @todo finish other possible habits
        public SauronHabits(NervousEye.Config cfgNervous, CuriousEye.Config cfgCurious)
        {
            Dither = new NervousEye(cfgNervous);
            Curious = new CuriousEye(cfgCurious);

        }

        /// <summary>
        /// Make Sauron look to slightly different positions from the current fixpoint
        /// </summary>
        /// <returns></returns>
        public int DitherEyeRandomly()
        {
            return Dither.DitherEyeRandomly();
        }

        /// <summary>
        /// Make Sauron to change his fixpoint randomly. He will somewhat explore his environment.
        /// </summary>
        /// <returns></returns>
        public int ChangeFixPointRandomly()
        {
            return Curious.ChangeFixPointRandomly();
        }

        /// <summary>
        /// Make Sauron to change his fixepoint on purpose (for expample to blame somebody).
        /// </summary>
        /// <param name="deltaPhi"></param>
        /// <param name="velocity"></param>
        /// <returns></returns>
        public int ChangeFixPoint(int deltaPhi = 0, int velocity = 0)
        {
            if (deltaPhi != 0)
            {
                Curious.InitNewMove(deltaPhi, velocity);
            }
            return Curious.MovingStep();
        }


        // @todo This is not finished
        public class BlinkyEye
        {
            Random _ditheringP = new Random();
            int _interval;
            int _duration;

            int _rdmBlinkCnt = 0;
            void BlinkEyeRandomly(Config config)
            {
                _interval = config.Interval;
                _duration = config.Duration;
            }

            public int BlinkEyeRandomly()
            {

                return 0;
            }

            /// <summary>
            /// Configuration Object 
            /// </summary>
            public struct Config
            {
                public int Interval;
                public int Duration;
            }
        }

        /// <summary>
        /// Curios eye habit. 
        /// Methods to move eyes from fixepoint to fixepoint in a parabolic (not diabolic) matter.
        /// Can be used randomly or on purpose.
        /// </summary>
        public class CuriousEye
        {
            Random _movementP = new Random();

            int _interval;
            int _maxSection;
            int _duration;
            int _halfDuration;

            /// <summary>
            /// Constructor 
            /// </summary>
            /// <param name="config"></param>
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
            /// <summary>
            /// Return new random fixpoint from time to time and maintain its approach.
            /// </summary>
            /// <returns></returns>
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

            /// <summary>
            /// Initiate to approach new fixpoint
            /// </summary>
            /// <param name="delta">if not 0, a new fixpoint will be approached relatively to current position</param>
            /// <param name="duration">if not 0, the default movement time is overwritten with given value, works only together wit new fixpoint position</param>
            public void InitNewMove(int delta, int duration = 0)
            {
                if (duration != 0)
                {
                    _alpha = (float)delta / 2 * 8 / (duration * duration);
                    _halfDuration = duration / 2;
                }
                else
                {
                    _alpha = (float)delta / 2 * 8 / (_duration * _duration);
                    _halfDuration = _duration / 2;
                }
                _AccStep = 0;
                _tempT = 0;
                _phi_0 = _phi_T;
            }

            /// <summary>
            /// Execute on moveing step. Movement followes the following equasions.
            /// phi(t) = alpha / 2 * t^2
            /// Where alpha = alpha_min = Phi/2 * 8 / T^2, which is the minimum acceleration to finish movement "Phi" in demanded period "T"
            /// </summary>
            /// <returns></returns>
            public int MovingStep()
            {
                _tempT++;
                switch (_AccStep)
                {
                    // initiative acceleration
                    case 0:
                        _phi = _alpha * (_tempT * _tempT) / 2;
                        _AccStep++;
                        break;

                    // acceleration
                    case 1:
                        _phi = _alpha * (_tempT * _tempT) / 2;

                        if (_tempT >= _halfDuration) // At half way through
                        {
                            _omega_T_2 = _alpha * _tempT;   // save gained velocity
                            _phi_T_2 = _phi;                // save gained Phi
                            _tempT = 0;
                            _AccStep++;
                        }
                        break;
                    
                    // deceleration with gained velocity relative to gained Phi
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

            /// <summary>
            /// Configuration Object 
            /// </summary>
            public struct Config
            {
                /// <summary>
                /// Interval to decide randomly wheter a new fixpoint shall be approached.
                /// </summary>
                public int Interval;

                /// <summary>
                /// Maximum Degree to change the fixpoint relatevely to current position.
                /// </summary>
                public int Section;

                /// <summary>
                /// Frames in which the movement has to be finished.
                /// </summary>
                public int Duration;
            }
        }

        /// <summary>
        /// Nervous eye habit. 
        /// Methods to move eyes sightly arount current fixepoint somewat nervous (because random) matter.
        /// </summary>
        public class NervousEye
        {
            Random _ditheringP = new Random();

            int _interval;
            int _section;

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="config"></param>
            public NervousEye(NervousEye.Config config)
            {
                _interval = config.Interval;
                _section = config.Section;
            }


            int _deltaDirection = 0;
            int _rdmCnt = 0;
            /// <summary>
            /// Return random deviation from current fixpoint
            /// </summary>
            /// <returns></returns>
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

            /// <summary>
            /// Configuration Object 
            /// </summary>
            public struct Config
            {
                /// <summary>
                /// Interval to decide randomly wheter a new deviation shall be applied.
                /// </summary>
                public int Interval;

                /// <summary>
                /// Maximum deviation from fixpoint.
                /// </summary>
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