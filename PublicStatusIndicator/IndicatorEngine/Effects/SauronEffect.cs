using System;
using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    public class SauronEffect
    {
        #region HardCoded
        const int DITHER_INTEVAL = 25 / 2;    // All x frames the eye could slightly move
        const int DITHER_SWING = 1;           // eyemovement for either direction

        const int EYE_MOVE_INTERVAL = 25 * 4;     // All x frames the eye can move to a completely different position
        const int EYE_MOVE_DURATION = 25 / 2;
        const int EYE_MOVE_FAST = 25 / 4;

        const int EYE_BLINK_INTERVAL = 25 * 2;     // All x frames the eye can move to a completely different position
        const int EYE_BLINK_DURATION = 5;

        const int BLINK_INTERVAL = 25 * 10;   // All x frames the eye can blink
        #endregion

        int _targetLen;
        int _maxIndex;
        int _reductionFactor;

        Color[] EyeSrc;
        Color[] BlazeEnvelope;
        Color[] FireEnvelope;
        Color[] TempOut;

        /// <summary>
        /// Variable fo current fixpoint which is also reference point for eye movement
        /// </summary>
        public BoundedInt FixPnt { get; set; }

        EyeMovement Mover;
        SauronHabits Habits;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="template"></param>
        /// <param name="target"></param>
        public SauronEffect(SauronEffect.SauronsEye template, Color[] target)
        {
            _targetLen = target.Length;
            EyeSrc = template.Iris;
            BlazeEnvelope = template.Aurora;
            FireEnvelope = template.Fire;


            _maxIndex = template.Iris.Length;
            _reductionFactor = _maxIndex / _targetLen;

            TempOut = new Color[target.Length];

            Habits = new SauronHabits(
                new SauronHabits.NervousEye.Config { Interval = DITHER_INTEVAL, Section = DITHER_SWING },
                new SauronHabits.CuriousEye.Config { Interval = EYE_MOVE_INTERVAL, Section = _maxIndex, Duration = EYE_MOVE_DURATION },
                new SauronHabits.BlinkyEye.Config { Duration = EYE_BLINK_DURATION, Interval = EYE_BLINK_INTERVAL }
                );
            Mover = new EyeMovement();

            FixPnt = new BoundedInt(0, _maxIndex-1);
        }


        int _Intensity = 0;
        const int DELTA_INTESITY = 5;
        const int MAX_INTESITY = 100;

        /// <summary>
        /// Genretes a frame to display Sauron in demanded state
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        public Color[] SauronStep(States state)
        {

            switch (state)
            {
                case States.Appear:
                    {
                        _Intensity += DELTA_INTESITY;
                        if (_Intensity >= MAX_INTESITY)
                        {
                            _Intensity = MAX_INTESITY;
                        }

                        BlazingSpot(_Intensity, FixPnt.Value);
                    }
                    break;

                case States.Idle:
                    BlazingSpot(_Intensity, FixPnt.Value);
                    break;

                case States.Nervous:
                    {
                        // Dither direction ocasionally
                        int relIdx = FixPnt.RelativeTo(Habits.DitherEyeRandomly());
                        int TempIntens = Habits.BlinkRandomly();
                        int blink = _Intensity * TempIntens / SauronHabits.BlinkyEye.MAX_INTESITY;

                        BlazingSpot(blink, relIdx);
                    }
                    break;

                case States.Move:
                    {
                        int relIdx = Mover.MovingStep();
                        relIdx = FixPnt.RelativeTo(relIdx);
                        BlazingSpot(_Intensity, relIdx);
                        if (Mover.MovementFinished == true)
                        {
                            Mover.MovementFinished = false;
                            FixPnt.Add(Mover.DemandedDelta);
                        }
                    }
                    break;

                case States.Mad:
                    DisplayMadSauron(FixPnt.Value);
                    break;

                case States.Disappear:
                    {
                        _Intensity -= DELTA_INTESITY;
                        if (_Intensity <= 0)
                        {
                            _Intensity = 0;
                        }
                        BlazingSpot(_Intensity, FixPnt.Value);
                    }
                    break;

                case States.Random:
                    {


                    }
                    break;
            }

            return TempOut;
        }

        /// <summary>
        /// Initiates movement to demanded position
        /// </summary>
        /// <param name="absPos"></param>
        /// <param name="duration"></param>
        public void InitMoveToFixPoint(int absPos, int duration)
        {
            if (absPos < 0)
            {
                absPos = absPos + EyeSrc.Length;
            }

            int diff = FixPnt.ShortesTo(absPos);
            Mover.InitNewMove(diff, duration);
        }

        /// <summary>
        /// Initiates relative movement to current position
        /// </summary>
        /// <param name="delta"></param>
        /// <param name="duration"></param>
        public void InitMoveFixpointBy(int delta, int duration)
        {
            Mover.InitNewMove(delta, duration);
        }

        /// <summary>
        /// Displays Iris
        /// </summary>
        /// <param name="intens"></param>
        private void EyeSpot(int intens)
        {
            int sIdx = FixPnt.Value;
            // Display Blazing Spot
            for (int i = 0; i < _targetLen; i++)
            {
                TempOut[i] = EyeSrc[sIdx];

                TempOut[i].R = (byte)((int)TempOut[i].R * intens / MAX_INTESITY);
                TempOut[i].G = (byte)((int)TempOut[i].G * intens / MAX_INTESITY);
                TempOut[i].B = (byte)((int)TempOut[i].B * intens / MAX_INTESITY);

                sIdx += _reductionFactor;
                sIdx = CorrectIndexToRing(sIdx);
            }
        }

        Random _blazeingP = new Random();
        Random _fireI = new Random();
        Random _flickrP = new Random();

        /// <summary>
        /// Displays Iris with small flickering band.
        /// Iris appears as slightly blured glazing dot
        /// </summary>
        /// <param name="intens"></param>
        /// <param name="relIdx"></param>
        private void BlazingSpot(int intens, int relIdx)
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

                relIdx += _reductionFactor;
                relIdx = CorrectIndexToRing(relIdx);
            }
        }

        int _madState = 0;
        int _madWhaitCnt = 0;
        float _FireIntensity = 0;


        internal void ReinitMadMode()
        {
            _madState = 0;
        }

        const int MAD_WAIT_STATES = 50;
        const int MAX_FIRE_INTENS = byte.MaxValue/4;
        const int FIRE_INTENS_SLOPE = 1;
        /// <summary>
        /// Mad Sauron.
        /// Saron keeps looking at preset fixpoint and ramps up background fire
        /// With backbround fire the iris changes color to white
        /// After short waiting the background fire decays again.
        /// </summary>
        /// <param name="relIdx"></param>
        private void DisplayMadSauron(int relIdx)
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
                TempOut[i].G = (byte)(burn * hBurn / 8 + eye * _FireIntensity / MAX_FIRE_INTENS + blaze/4);
                TempOut[i].B = (byte)(eye * _FireIntensity / MAX_FIRE_INTENS / 3);

                relIdx += _reductionFactor;
                relIdx = CorrectIndexToRing(relIdx);
            }
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

            /// <summary>
            /// Dither around fixpoint
            /// </summary>
            Nervous,
        }
    }
}