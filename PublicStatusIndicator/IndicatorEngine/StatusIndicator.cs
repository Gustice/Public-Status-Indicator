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
        private Color[] _badTemplate;
        private Color[] _unstableTemplate;
        private Color[] _stableTemplate;
        private Color[] _processTemplate;

        // Generatet single image out of prepared waveform considering demanded displaystate
        private readonly Color[] _physicallRing;

        // Objects to generate following image/frame to be displayed
        private PulseEffect _VirBadEffect;
        private PulseEffect _VirStabelEffect;
        private PulseEffect _VirUnstabelEffect;
        private RotateEffect _VirPrcsEffect;

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
        public byte MaxBrightnes
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
            _processTemplate = InitRotatingPulse(_virtualRotateLenght);
            _stableTemplate = InitStablePulse(_virtualPulselength);
            _unstableTemplate = InitUnstablePulse(_virtualPulselength);
            _badTemplate = InitBadPause(_virtualPulselength);

            _VirPrcsEffect = new RotateEffect(_processTemplate, _physicallRing);
            _VirStabelEffect = new PulseEffect(_stableTemplate, _physicallRing);
            _VirUnstabelEffect = new PulseEffect(_unstableTemplate, _physicallRing);
            _VirBadEffect = new PulseEffect(_badTemplate, _physicallRing);
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
                pulseTemp[i] = Color.FromArgb(MaxBrightnes, 
                    (byte) p[i],            // Full Red
                    (byte) (p[i]*12/16),    // Add some Green
                    0x00);

            return pulseTemp;
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
                pulseTemp[i] = Color.FromArgb(MaxBrightnes, 
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
                pulseTemp[i] = Color.FromArgb(MaxBrightnes, 
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
                pulseTemp[i] = Color.FromArgb(MaxBrightnes, 
                    (byte) p2[i],           // Add grayscale waveform fully to red
                    (byte) (p1[i]*12/16),   // Add grayscale waveform partly to green
                    0x00);

            return pulseTemp;
        }

        /// <summary>
        ///  Returns plotable grayscale waveforms of colored pulse waveforms
        /// </summary>
        /// <param name="processStateProfile"></param>
        /// <param name="goodStateProfile"></param>
        /// <param name="badStateProfile"></param>
        public void GetCurvesAsGrayValues(out byte[] processStateProfile, out byte[] goodStateProfile,
            out byte[] badStateProfile)
        {
            processStateProfile = MakeGrayValuesToDataProfile(_processTemplate);
            goodStateProfile = MakeGrayValuesToDataProfile(_stableTemplate);
            badStateProfile = MakeGrayValuesToDataProfile(_badTemplate);
        }

        /// <summary>
        /// Transforms colored waveform to plotable (and normalized) graysscale waveform
        /// </summary>
        /// <param name="colorArray"></param>
        /// <returns></returns>
        private static byte[] MakeGrayValuesToDataProfile(Color[] colorArray)
        {
            byte[] outputArray = new byte[colorArray.Length];

            int[] grayArray = new int[colorArray.Length];
            int maxValue = 0;
            for (int i = 0; i < colorArray.Length; i++)
            {
                grayArray[i] = colorArray[i].R;
                grayArray[i] += colorArray[i].G;
                grayArray[i] += colorArray[i].B;
                grayArray[i] = grayArray[i] * colorArray[i].A / byte.MaxValue;
                maxValue = Math.Max(maxValue, grayArray[i]);
            }
            
            // Normalize values to byte
            for (int i = 0; i < grayArray.Length; i++)
                outputArray[i] = (byte) (grayArray[i] * byte.MaxValue / maxValue);

            return outputArray;
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


        private int _fadingCnt;
        private EngineState _fadingState;
        private EngineState _lastState = EngineState.Blank;

        /// <summary>
        /// Generats following image to an appropriate state.
        /// Also handles fading on change condition between different states.
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        public Color[] EffectAccordingToState(EngineState currentState)
        {
            if (currentState != _lastState)
            {
                _fadingState = _lastState;
                _lastState = currentState;
                _fadingCnt = FadingCycles;
                ResetEffect(currentState);
            }

            Color[] appearing = GenerateNewImage(currentState);

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
                        colorSeries[i] = Color.FromArgb(MaxBrightnes, 0x00, 0x00, 0x00);
                    break;
                case EngineState.Idle:
                    colorSeries = new Color[_physicallRing.Length];
                    for (int i = 0; i < colorSeries.Length; i++)
                        colorSeries[i] = Color.FromArgb(MaxBrightnes, 0x02, 0x02, 0x02);
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
        /// Return colored templates for all animated states
        /// </summary>
        /// <param name="process"></param>
        /// <param name="bad"></param>
        /// <param name="unstable"></param>
        /// <param name="stable"></param>
        public void GetAllTemplates(out Color[] process, out Color[] bad, out Color[] unstable, out Color[] stable )
        {
            process = _processTemplate;
            bad = _badTemplate;
            unstable = _unstableTemplate;
            stable = _stableTemplate;
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