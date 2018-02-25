using System;


namespace PublicStatusIndicator.IndicatorEngine
{
    /// <summary>
    /// Concentrates different haits of sauron and kan invoke the either randomly or at trigger event
    /// </summary>
    class SauronHabits
    {

        NervousEye Dither;
        CuriousEye Curious;
        BlinkyEye Blinky;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cfgNervous"></param>
        /// <param name="cfgCurious"></param>
        /// <param name="cfgBlinky"></param>
        public SauronHabits(NervousEye.Config cfgNervous, CuriousEye.Config cfgCurious, BlinkyEye.Config cfgBlinky)
        {
            Dither = new NervousEye(cfgNervous);
            Curious = new CuriousEye(cfgCurious);
            Blinky = new BlinkyEye(cfgBlinky);
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
        /// Make Sauron to blink randomly.
        /// </summary>
        /// <returns></returns>
        public int BlinkRandomly()
        {
            return Blinky.BlinkEyeRandomly();
        }

        /// <summary>
        /// Blinky eye habit. 
        /// Methods to blink ey ocasianlly by decreasing and increasing intensity.
        /// Can be used randomly or on purpose.
        /// </summary>
        public class BlinkyEye
        {
            Random _blinkP = new Random();
            int _interval;
            int _duration;

            public static int MAX_INTESITY = 100;

            public BlinkyEye(Config config)
            {
                _interval = config.Interval;
                _duration = config.Duration;

                _deltaIntens = MAX_INTESITY / 2 / _duration;
            }

            int _rdmCnt = 0;
            public int BlinkEyeRandomly()
            {
                if (_rdmCnt <= 0)
                {
                    _rdmCnt = _interval;
                    double p = _blinkP.Next(0,2)-1;
                    if (p >= 0.0)
                    {
                        _step = 0;
                    }
                }
                _rdmCnt--;

                return BlinkStates();
            }

            int _step = 2;
            int _Intensity = MAX_INTESITY;
            int _deltaIntens;

            /// <summary>
            /// Forms Blink ramp by decreasing and increasing Intensitiy
            /// </summary>
            /// <returns></returns>
            int BlinkStates()
            {
                switch (_step)
                {
                    case 0:
                        _Intensity -= _deltaIntens;
                        if (_Intensity <= 0)
                        {
                            _Intensity = 0;
                            _step++;
                        }
                        break;

                    case 1:
                        _Intensity += _deltaIntens;
                        if (_Intensity >= MAX_INTESITY)
                        {
                            _Intensity = MAX_INTESITY;
                            _step++;
                        }
                        break;

                    default:
                        break;
                }
                return _Intensity;
            }


            /// <summary>
            /// Configuration Object 
            /// </summary>
            public struct Config
            {
                /// <summary>
                /// Interval to decide randomly wheter a new blink shall be executed.
                /// </summary>
                public int Interval;

                /// <summary>
                /// Frames in which the blink has to be finished.
                /// </summary>
                public int Duration;
            }
        }

        /// <summary>
        /// Curios eye habit. 
        /// Methods to move eyes from fixepoint to fixepoint in a parabolic (not diabolic) matter.
        /// Can be used randomly or on purpose.
        /// </summary>
        public class CuriousEye : EyeMovement
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

            int _demandedDelta = 0;

            /// <summary>
            /// Initiate to approach new fixpoint
            /// </summary>
            /// <param name="delta">if not 0, a new fixpoint will be approached relatively to current position</param>
            /// <param name="duration">if not 0, the default movement time is overwritten with given value, works only together wit new fixpoint position</param>
            public override void InitNewMove(int delta, int duration = 0)
            {
                if (duration != 0)
                {
                    base.InitNewMove(delta, duration);
                }
                else
                {
                    base.InitNewMove(delta, _duration);
                }
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
                    _deltaDirection = _ditheringP.Next(_section * 2 + 1) - _section;
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
}