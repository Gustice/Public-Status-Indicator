using System;
using System.Collections.Generic;
using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    public class StatusIndicator
    {
        // Defines how many cycles are used to fade one displayed stete into another
        private const int FadingCycles = 10;

        // Defines offset brightness for background illumination
        private const byte OffsetBrightness = byte.MaxValue / 6;

        // Prepared colored waveforms to display states
        public Color[] BadTemplate;
        public Color[] UnstableTemplate;
        public Color[] StableTemplate;
        public Color[] ProcessTemplate;

        SauronEffect.SauronsEye SauronsTemplates;
        public Color[] SauronsIris;
        public Color[] SauronsAurora;
        public Color[] SauronsFire;

        // Generatet single image out of prepared waveform considering demanded displaystate
        private readonly Color[] _physicallRing;

        // Objects to generate following image/frame to be displayed
        private PulseEffect _VirBadEffect;
        private PulseEffect _VirStabelEffect;
        private PulseEffect _VirUnstabelEffect;
        private RotateEffect _VirPrcsEffect;
        private SauronEffect _Sauron;

        /// <summary>
        /// Length of predefined smoothned color values to be displayed on an particular LED-strip as rotated image with intermediate states
        /// </summary>
        int _virtualRotateLenght;

        /// <summary>
        /// Length of predefined color values to be displayed on an particular LED-strip as pulsed waveform
        /// </summary>
        int _virtualPulselength;

        private byte _maxBrightnes = 0xFF;

        /// <summary>
        /// Maximal gray-value to which all waveforms are normalized
        /// </summary>
        public byte MaxBrightness
        {
            get { return _maxBrightnes; }
            set {
                if (_maxBrightnes != value)
                {
                    _maxBrightnes = value;
                    InitAllWaveforms();
                }
            }
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="configuration"></param>
        public StatusIndicator(IndicatorConfig configuration)
        {
            int targetCnt = configuration.TargetPixels;
            _virtualRotateLenght = targetCnt * configuration.RotateSmoothFactor;
            _virtualPulselength = configuration.PulseImages;

            _physicallRing = new Color[targetCnt];

            InitAllWaveforms();
        }

        /// <summary>
        /// Initiates all colored waveforms and generation objects
        /// </summary>
        private void InitAllWaveforms()
        {
            ProcessTemplate = InitRotatingPulse(_virtualRotateLenght);
            StableTemplate = InitStablePulse(_virtualPulselength);
            UnstableTemplate = InitUnstablePulse(_virtualPulselength);
            BadTemplate = InitBadPause(_virtualPulselength);

            _VirPrcsEffect = new RotateEffect(ProcessTemplate, _physicallRing);
            _VirStabelEffect = new PulseEffect(StableTemplate, _physicallRing);
            _VirUnstabelEffect = new PulseEffect(UnstableTemplate, _physicallRing);
            _VirBadEffect = new PulseEffect(BadTemplate, _physicallRing);

            SauronsTemplates = InitSauronsEye(_physicallRing.Length);
            SauronsIris = SauronsTemplates.Iris;
            SauronsAurora = SauronsTemplates.Aurora;
            SauronsFire   = SauronsTemplates.Fire;

            _Sauron = new SauronEffect(SauronsTemplates, _physicallRing);
        }

        /// <summary>
        /// Generates "In Process" waveform -> Rotated Image
        /// Generates a yellow colored waveform with a single pulse. The waveform is supposed to be displayed as rotated image.
        /// The to pulse is set up to appear as somewhat sharp pulse with moderate background illumination.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="pulseTemp"></param>
        private Color[] InitRotatingPulse(int length)
        {
            Color[] pulseTemp = new Color[length];

            int[] p = new int[length];
            p = GenerateGausianPulse(length, 10, OffsetBrightness);

            // Generate yellow waveform from grayvalues. 
            for (int i = 0; i < length; i++)
                pulseTemp[i] = Color.FromArgb(MaxBrightness, 
                    (byte) p[i],            // Full Red
                    (byte) (p[i]*12/16),    // Add some Green
                    0x00);

            return pulseTemp;
        }

        const int SAURON_SMOOTHNESS_FAKTOR = 3;
        const int SAURONS_EYEPROP = 80;
        const int SAURONS_BLAZINGPROP = 100 - SAURONS_EYEPROP;
        const int SAURONS_FIRE = 50;

        /// <summary>
        /// Generates a waveform-set for Saurons eye with following approach:
        /// The Iris Pulse is a sharply defined pulse to display the eye.
        /// The Aurora is a slightly more deviated pulse which represents the envelope curve for superposed flackering fire of the Iris:
        /// Iris and Aurora appears as blazing eye.
        /// The Fire is a wide deviated pulse which represents the envelope curve for fire-like flickering in case mad state.
        /// </summary>
        /// <param name="phyLenth"></param>
        /// <returns></returns>
        private SauronEffect.SauronsEye InitSauronsEye(int phyLenth)
        {
            SauronEffect.SauronsEye temp = new SauronEffect.SauronsEye();

            phyLenth = phyLenth * SAURON_SMOOTHNESS_FAKTOR;

            // Generate Iris
            temp.Iris = GetColoredPulse(phyLenth, 12, SAURONS_EYEPROP);

            // Generate envelope for blazing flames araound iris
            temp.Aurora = GetColoredPulse(phyLenth, 6, SAURONS_BLAZINGPROP);

            // Generate envelope for fire in case of madness
            temp.Fire = GetColoredPulse(phyLenth, 2, SAURONS_FIRE);

            return temp;
        }

        /// <summary>
        /// Init normalized pulse for Saurons eye
        /// </summary>
        /// <param name="phyLenth"></param>
        /// <param name="peak"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        private Color[] GetColoredPulse(int phyLenth, int peak, int prop)
        {
            int[] p = new int[phyLenth];
            p = GenerateGausianPulse(phyLenth, peak, 0);
            // Scale Pulse to maximum 
            for (int i = 0; i < p.Length; i++)
                p[i] = p[i] * prop / 100;

            Color[] temp = new Color[phyLenth];
            // Generate waveform from grayvalues. 
            for (int i = 0; i < phyLenth; i++)
                temp[i] = Color.FromArgb(MaxBrightness,
                    (byte)p[i],             // Full Red
                    (byte)(p[i] * 4 / 16),  // Add some Green
                    0x00);
            return temp;
        }

        public void DeviateSauronsFixPoint(int delta)
        {
            _Sauron.R_Idx.Add(delta);
        }

        /// <summary>
        /// Generates "Stable" waveform -> Pulsed Image
        /// Generates a green colored waveform with a single pulse followed by a pause.
        /// The effect should appear as "brathing" green signal with moderate background illumination.
        /// </summary>
        /// <param name="length"></param>
        /// <param name="pulseTemp"></param>
        private Color[] InitStablePulse(int length)
        {
            Color[] pulseTemp = new Color[length];

            int i = 0;
            int[] p = new int[length];

            int[] tempG = GenerateGausianPulse(length / 2, 3, OffsetBrightness);
            
            // Add "brathing pulse" to waveform
            for (; i < tempG.Length; i++)
                p[i] = tempG[i];

            // Fill remaining with offset brightness
            for (; i < p.Length; i++)
                p[i] = OffsetBrightness;

            // Generate green waveform from grayvalues. 
            for (i = 0; i < length; i++)
                pulseTemp[i] = Color.FromArgb(MaxBrightness, 
                    0x00, 
                    (byte)p[i],     // Full Green
                    0x00);

            return pulseTemp;
        }

        /// <summary>
        /// Generates "Bad" waveform -> Pulsed Image
        /// Generates a red colored waveform with three pulses.
        /// The effect should appear as three "nervouse" pulses followed by a longer pause
        /// </summary>
        /// <param name="length"></param>
        /// <param name="pulseTemp"></param>
        private Color[] InitBadPause(int length)
        {
            Color[] pulseTemp = new Color[length];

            int i = 0;
            int[] p = new int[length];

            int[] tempB = GenerateGausianPulse(p.Length / 6, 3, OffsetBrightness);

            // Add n pulses to waveform
            for (int np = 0; np < 3; np++)
            {
                int j = 0;
                int next_i = i + tempB.Length;
                for (; i < next_i; i++)
                {
                    p[i] = tempB[j];
                    j++;
                }
            }

            // Fill remaining with offset brightness
            for (; i < p.Length; i++)
                p[i] = OffsetBrightness;

            // Generate red waveform from grayvalues. 
            for (i = 0; i < length; i++)
                pulseTemp[i] = Color.FromArgb(MaxBrightness, 
                    (byte)p[i],     // Full Red
                    0x00, 
                    0x00);

            return pulseTemp;
        }


        /// <summary>
        /// Generates "Unstable" waveform -> Pulsed Image
        /// Generates a green / yellow colored waveform. The first pulse is green, the scond is yellow.
        /// The effect should appear as two colored faster "breathing" pulses followed by a longer pause
        /// </summary>
        /// <param name="length"></param>
        /// <param name="pulseTemp"></param>
        private Color[] InitUnstablePulse(int length)
        {
            Color[] pulseTemp = new Color[length];

            int i = 0;
            int[] p1 = new int[length];
            int[] p2 = new int[length];

            int[] tempP1 = GenerateGausianPulse(p1.Length / 4, 3, OffsetBrightness);

            // Add n pulses to waveform
            for (int np = 0; np < 2; np++)
            {
                int next_i = i + tempP1.Length;
                int j = 0;
                for (; i < next_i; i++)
                {
                    p1[i] = tempP1[j];
                    j++;
                }
            }

            // Fill remaining with offset brightness
            for (; i < p1.Length; i++)
                p1[i] = OffsetBrightness;


            // Generate grayvalue-channel and initiate with dark values
            for (i = 0; i < p2.Length; i++)
                p2[i] = 0;

            int[] tempP2 = GenerateGausianPulse(p1.Length / 4, 4, 0);

            // Add pulses to waveform
            int k = 0;
            for (i= tempP1.Length; i < tempP1.Length+ tempP2.Length; i++)
            {
                p2[i] = tempP2[k];
                k++;
            }

            // Generate colored waveform from grayvalue-channels. 
            for (i = 0; i < length; i++)
                pulseTemp[i] = Color.FromArgb(MaxBrightness, 
                    (byte) p2[i],           // Add grayscale waveform fully to red
                    (byte) (p1[i]*12/16),   // Add grayscale waveform partly to green
                    0x00);

            return pulseTemp;
        }

        /// <summary>
        /// Generates parametrized gausian pulse.
        /// Pulse is normalized to 100 %. Concentration can be defined by peakForm value.
        /// Peak is ajusted allway to middle positoin of given lenth
        /// </summary>
        /// <param name="length">Data points for gausian pulse. Pulse is allways located in the centre</param>
        /// <param name="peakForm">Concentration of pulse in the middle. Set to 2 for a flat curve and to 4 or higher for a more sharpe peak in the middle</param>
        /// <param name="offset">Offset value for background-brightness</param>
        /// <returns></returns>
        private static int[] GenerateGausianPulse(int length, int peakForm, int offset)
        {
            int[] tempplate = new int[length];
            int temporaryMax = 0;
            int idx = 0;
            int midLen = tempplate.Length / 2;

            // Generates pulse around middle position
            for (int i = -midLen; i < midLen; i++)
            {
                tempplate[idx] = (int) (Math.Exp(-Math.Pow((float)peakForm * i / midLen, 2)) * byte.MaxValue);
                temporaryMax = Math.Max(temporaryMax, tempplate[idx]);
                idx++;
            }

            // Normalize Pulse to maximum value by considering demanded offset value
            for (int i = 0; i < tempplate.Length; i++)
                tempplate[i] = tempplate[i] * (byte.MaxValue - offset) / temporaryMax + offset;

            return tempplate;
        }



        private EngineState _state = EngineState.Blank;
        public EngineState State
        {
            get { return _state; }
            set {
                _state = value;

                _fadingState = _lastState;
                _lastState = _state;
                _fadingCnt = FadingCycles;
                ResetEffect(_state);
            }
        }

        private List<ProfileElement> _profile = null;
        public List<ProfileElement> Profile
        {
            get { return _profile; }
            set {
                _profile = value;
                profileCnt = 0;
                ProfileIdx = 0;
            }
        }

        private int _profileIdx = 0;

        public int ProfileIdx
        {
            get { return _profileIdx; }
            set {
                _profileIdx = value;

                try
                {
                    var type = _profile[ProfileIdx].GetType();
                    if (type == typeof(SauronProfileElement))
                    {
                        _sauronState = ((SauronProfileElement)_profile[ProfileIdx]).SauronState;
                        if (_sauronState == SauronEffect.States.Move)
                        {
                            SauronProfileElement temp = (SauronProfileElement)_profile[ProfileIdx];
                            _Sauron.ChangeFixPointTo(temp.NewPosition, temp.Duration);
                        }
                    }
                }
                catch
                {
                    // nothing if not possible
                }
            }
        }


        private int _fadingCnt;
        private EngineState _fadingState;
        private EngineState _lastState = EngineState.Blank;

        int profileCnt = 0;
        public Color[] EffectAccordingToProfile()
        {
            Color[] tempReturn;

            if (_profile != null)
            {
                var type = _profile[ProfileIdx].GetType();
                if (type == typeof(SauronProfileElement) )  
                {
                    _sauronState = ((SauronProfileElement)_profile[ProfileIdx]).SauronState;
                }
                State = _profile[ProfileIdx].State;

                profileCnt++;
                if (profileCnt >= _profile[ProfileIdx].Duration)
                {
                    profileCnt = 0;
                    ProfileIdx++;
                    if (ProfileIdx >= _profile.Count)
                    {
                        ProfileIdx = 0;
                        _profile = null;
                    }
                }
                tempReturn = EffectAccordingToState();
            }
            else
            {
                tempReturn = EffectAccordingToState();
            }
            return tempReturn;
        }

        /// <summary>
        /// Generats following image to an appropriate state.
        /// Also handles fading on change condition between different states.
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        public Color[] EffectAccordingToState()
        {
            Color[] appearing = GenerateNewImage(_state);

            // If in fading condition
            if (_fadingCnt > 0)
            {
                _fadingCnt--;
                Color[] fading = GenerateNewImage(_fadingState);

                int onFadingCnt = FadingCycles - _fadingCnt;

                // fade last state and enhance new state
                for (int i = 0; i < _physicallRing.Length; i++)
                {
                    _physicallRing[i].R = (byte) ((appearing[i].R * onFadingCnt + fading[i].R * _fadingCnt) / FadingCycles);
                    _physicallRing[i].G = (byte) ((appearing[i].G * onFadingCnt + fading[i].G * _fadingCnt) / FadingCycles);
                    _physicallRing[i].B = (byte)((appearing[i].B * onFadingCnt + fading[i].B * _fadingCnt) / FadingCycles);
                }
            }
            else
            {
                for (int i = 0; i < _physicallRing.Length; i++)
                    _physicallRing[i] = appearing[i];
            }
            return _physicallRing;
        }

        SauronEffect.States _sauronState = SauronEffect.States.Idle;
        /// <summary>
        /// Generats following image to an appropriate state.
        /// </summary>
        /// <param name="state"></param>
        /// <returns></returns>
        private Color[] GenerateNewImage(EngineState state)
        {
            Color[] colorSeries = new Color[0];
            switch (state)
            {
                case EngineState.Blank:
                    colorSeries = new Color[_physicallRing.Length];
                    for (int i = 0; i < colorSeries.Length; i++)
                        colorSeries[i] = Color.FromArgb(MaxBrightness, 0x00, 0x00, 0x00);
                    break;
                case EngineState.Idle:
                    colorSeries = new Color[_physicallRing.Length];
                    for (int i = 0; i < colorSeries.Length; i++)
                        colorSeries[i] = Color.FromArgb(MaxBrightness, 0x02, 0x02, 0x02);
                    break;
                case EngineState.Progress:
                    colorSeries = _VirPrcsEffect.RotateStep();
                    break;
                case EngineState.Bad:
                    colorSeries = _VirBadEffect.PulseStep();
                    break;
                case EngineState.Unstable:
                    colorSeries = _VirUnstabelEffect.PulseStep();
                    break;
                case EngineState.Stable:
                    colorSeries = _VirStabelEffect.PulseStep();
                    break;

                case EngineState.Sauron:
                    colorSeries = _Sauron.SauronStep(_sauronState);
                    break;
            }

            return colorSeries;
        }

        /// <summary>
        /// Resets fading states after completion of changecondition. 
        /// The reset makes shure pulsed states are displayed from the beginning again when state is set again to faded state.
        /// </summary>
        /// <param name="state"></param>
        private void ResetEffect(EngineState state)
        {
            switch (state)
            {
                case EngineState.Progress:
                    _VirPrcsEffect.ResetIndex();
                    break;
                case EngineState.Stable:
                    _VirStabelEffect.ResetIndex();
                    break;
                case EngineState.Bad:
                    _VirBadEffect.ResetIndex();
                    break;
            }
        }

        /// <summary>
        /// Class for configuraion object to configurate the indicator engine. 
        /// </summary>
        public class IndicatorConfig
        {
            /// <summary>
            /// Number of Pixels / LEDs / Lamps to display rotated ore pulsed effekts
            /// </summary>
            public int TargetPixels { get; set; }

            /// <summary>
            /// Faktor for intermediate states for rotated effekts
            /// </summary>
            public int RotateSmoothFactor { get; set; }

            /// <summary>
            /// Number of images to be displayed as pulsed effekt
            /// </summary>
            public int PulseImages { get; set; }

            public IndicatorConfig (int pixels, int smoothfactor, int pulseLength)
            {
                TargetPixels = pixels;
                RotateSmoothFactor = smoothfactor;
                PulseImages = pulseLength;
            }
        }

    }
}