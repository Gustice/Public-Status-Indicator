using System;
using System.Collections.Generic;
using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    public class StatusIndicator
    {
        private const int FadingStart = 10;

        private const byte StartSeq = 0xE0;
        private const byte OverallBrightness = 10;
        private const byte OffsetBrightness = byte.MaxValue / 6;

        private const byte CommonAlpha = StartSeq & OverallBrightness;

        private readonly Color[] _badTemplate;
        private readonly Color[] _unstableTemplate;
        private readonly Color[] _stableTemplate;
        private readonly Color[] _processTemplate;

        private readonly Color[] _physicallRing;


        private readonly PulseEffect _virBadEffect;
        private readonly PulseEffect _virStabelEffect;
        private readonly PulseEffect _virUnstabelEffect;
        private readonly RotateEffect _virPrcsEffect;

        int _virtualLenght;
        int _virtualPulselength;

        public byte MaxBrighness { get; set; } = 0xFF;

        public StatusIndicator(IndicatorConfig configuration)
        //public StatusIndicator(int targetCnt, int smoothFactor, int pulseDuration)
        {
            int targetCnt = configuration.TargetPixels;
            _virtualLenght = targetCnt * configuration.RotateSmoothFactor;
            _virtualPulselength = configuration.PulseImages;

            _physicallRing = new Color[targetCnt];
            _processTemplate = new Color[_virtualLenght];
            _stableTemplate = new Color[_virtualPulselength];
            _unstableTemplate = new Color[_virtualPulselength];
            _badTemplate = new Color[_virtualPulselength];

            InitRotatingPulse(_virtualLenght, ref _processTemplate);
            InitStablePulse(_virtualPulselength, ref _stableTemplate);
            InitUnstablePulse(_virtualPulselength, ref _unstableTemplate);
            InitBadPause(_virtualPulselength, ref _badTemplate);

            _virPrcsEffect = new RotateEffect(_processTemplate, _physicallRing);
            _virStabelEffect = new PulseEffect(_stableTemplate, _physicallRing);
            _virUnstabelEffect = new PulseEffect(_unstableTemplate, _physicallRing);
            _virBadEffect = new PulseEffect(_badTemplate, _physicallRing);
        }

        private void InitRotatingPulse(int length, ref Color[] pulseTemp)
        {
            int[] p = new int[length];

            // Erzeuge einen zentralen Puls für den Process
            p = GenerateGausianPulse(p.Length, 10, OffsetBrightness);
            for (int i = 0; i < length; i++)
                pulseTemp[i] = Color.FromArgb(MaxBrighness, (byte) p[i], (byte) (p[i]*12/16), 0x00);
        }

        /// <summary>
        /// Generates pulse to display stabel-state.
        /// The effect should appear as "brathing" green signal
        /// </summary>
        /// <param name="length"></param>
        /// <param name="pulseTemp"></param>
        private void InitStablePulse(int length, ref Color[] pulseTemp)
        {
            int i = 0;
            int[] p = new int[length];

            int[] tempG = GenerateGausianPulse(p.Length / 2, 3, OffsetBrightness);
            // Generate "brathing pulse"
            for (; i < tempG.Length; i++)
                p[i] = tempG[i];

            // Fill remaining with offset brightness
            for (; i < p.Length; i++)
                p[i] = OffsetBrightness;

            // Transform to green pulse
            for (i = 0; i < length; i++)
                pulseTemp[i] = Color.FromArgb(MaxBrighness, 0x00, (byte)p[i], 0x00);
        }

        /// <summary>
        /// Generates pulse to display bad-state.
        /// The effect should appear as three "nervouse" pulses followed by a longer pause
        /// </summary>
        /// <param name="length"></param>
        /// <param name="pulseTemp"></param>
        private void InitBadPause(int length, ref Color[] pulseTemp)
        {
            int i = 0;
            int[] p = new int[length];

            int[] tempB = GenerateGausianPulse(p.Length / 6, 3, OffsetBrightness);

            // Generate n pulses
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

            // Transform to red pulse
            for (i = 0; i < length; i++)
                pulseTemp[i] = Color.FromArgb(MaxBrighness, (byte)p[i], 0x00, 0x00);
        }

        private void InitUnstablePulse(int length, ref Color[] pulseTemp)
        {
            int i = 0;
            int[] p1 = new int[length];
            int[] p2 = new int[length];

            int[] tempP1 = GenerateGausianPulse(p1.Length / 4, 3, OffsetBrightness);


            // Generate n pulses
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


            for (i = 0; i < p2.Length; i++)
                p2[i] = 0;

            int[] tempP2 = GenerateGausianPulse(p1.Length / 4, 4, 0);

            int k = 0;
            for (i= tempP1.Length; i < tempP1.Length+ tempP2.Length; i++)
            {
                p2[i] = tempP2[k];
                k++;
            }

            // Transform to two-colored Pulse
            for (i = 0; i < length; i++)
                pulseTemp[i] = Color.FromArgb(MaxBrighness, (byte)p2[i], (byte) (p1[i]*12/16), 0x00);
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
            
            // Normalize values
            for (int i = 0; i < grayArray.Length; i++)
                outputArray[i] = (byte) (grayArray[i] * byte.MaxValue / maxValue);

            return outputArray;
        }

        /// <summary>
        /// Generates parametrized gausian pulse
        /// Pulse is normalized to 100 %
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
            for (int i = -midLen; i < midLen; i++)
            {
                tempplate[idx] = (int) (Math.Exp(-Math.Pow((float)peakForm * i / midLen, 2)) * byte.MaxValue);
                temporaryMax = Math.Max(temporaryMax, tempplate[idx]);
                idx++;
            }
            // Gleichverteilung auf Byte normieren
            for (int i = 0; i < tempplate.Length; i++)
                tempplate[i] = tempplate[i] * (byte.MaxValue - offset) / temporaryMax + offset;

            return tempplate;
        }


        private int _fadingCnt;
        private EngineState _fadingState;
        private EngineState _lastState = EngineState.Blank;

        /// <summary>
        /// Generats following image to an appropriate state.
        /// Handles also fading between on change condition.
        /// </summary>
        /// <param name="currentState"></param>
        /// <returns></returns>
        public Color[] EffectAccordingToState(EngineState currentState)
        {
            if (currentState != _lastState)
            {
                _fadingState = _lastState;
                _lastState = currentState;
                _fadingCnt = FadingStart;
                ResetEffect(currentState);
            }

            Color[] ringColors1 = GenerateNewImage(currentState);

            if (_fadingCnt > 0)
            {
                _fadingCnt--;
                Color[] ringColors2 = GenerateNewImage(_fadingState);

                int onfadingCnt = FadingStart - _fadingCnt;
                for (int i = 0; i < _physicallRing.Length; i++)
                {
                    _physicallRing[i].R = (byte) ((ringColors1[i].R * onfadingCnt + ringColors2[i].R * _fadingCnt) / FadingStart);
                    _physicallRing[i].G = (byte) ((ringColors1[i].G * onfadingCnt + ringColors2[i].G * _fadingCnt) / FadingStart);
                    _physicallRing[i].B = (byte)((ringColors1[i].B * onfadingCnt + ringColors2[i].B * _fadingCnt) / FadingStart);
                }
            }
            else
            {
                for (int i = 0; i < _physicallRing.Length; i++)
                    _physicallRing[i] = ringColors1[i];
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
                        colorSeries[i] = Color.FromArgb(MaxBrighness, 0x00, 0x00, 0x00);
                    break;
                case EngineState.Idle:
                    colorSeries = new Color[_physicallRing.Length];
                    for (int i = 0; i < colorSeries.Length; i++)
                        colorSeries[i] = Color.FromArgb(MaxBrighness, 0x02, 0x02, 0x02);
                    break;
                case EngineState.Progress:
                    colorSeries = _virPrcsEffect.RotateStep();
                    break;
                case EngineState.Bad:
                    colorSeries = _virBadEffect.PulseStep();
                    break;
                case EngineState.Unstable:
                    colorSeries = _virUnstabelEffect.PulseStep();
                    break;
                case EngineState.Stable:
                    colorSeries = _virStabelEffect.PulseStep();
                    break;
            }

            return colorSeries;
        }

        /// <summary>
        /// Resets fading states after completion of changecondition. 
        /// The reset makes shure that pulsed states are displayed from the beginning again wenn state is set again to appropriate state.
        /// </summary>
        /// <param name="state"></param>
        private void ResetEffect(EngineState state)
        {
            switch (state)
            {
                case EngineState.Progress:
                    _virPrcsEffect.ResetIndex();
                    break;
                case EngineState.Stable:
                    _virStabelEffect.ResetIndex();
                    break;
                case EngineState.Bad:
                    _virBadEffect.ResetIndex();
                    break;
            }
        }

        /// <summary>
        /// return als colored templates for differend states
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