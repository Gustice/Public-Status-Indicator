using System;
using System.Collections.Generic;
using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    public class StatusIndicator
    {
        private const int FadingStart = 10;

        private const byte StartSeq = 0xE0;
        private const byte Brightness = 10;

        private const byte CommonAlpha = StartSeq & Brightness;

        private readonly Color[] _badTemplate;
        private readonly Color[] _unstableTemplate;
        private readonly Color[] _stableTemplate;
        private readonly Color[] _processTemplate;

        private readonly Color[] _virtualRing;


        private readonly PulseEffect _virBadEffect;
        private readonly PulseEffect _virStabelEffect;
        private readonly PulseEffect _virUnstabelEffect;
        private readonly RotateEffect _virPrcsEffect;

        int _virtualLenght;
        int _virtualPulselength;

        public byte MaxBrighness { get; set; } = 0xFF;

        public StatusIndicator(int targetCnt, int smoothFactor, int pulseDuration)
        {
            _virtualLenght = targetCnt * smoothFactor;
            _virtualPulselength = pulseDuration;

            _virtualRing = new Color[targetCnt];
            _processTemplate = new Color[_virtualLenght];
            _stableTemplate = new Color[_virtualLenght];
            _unstableTemplate = new Color[_virtualLenght];
            _badTemplate = new Color[_virtualLenght];

            InitPulse(_virtualLenght, ref _processTemplate);
            InitPulsePause(_virtualPulselength, ref _stableTemplate);
            InitColoredPulsesPause(_virtualPulselength, ref _unstableTemplate);
            InitPulsesPause(_virtualPulselength, ref _badTemplate);

            _virPrcsEffect = new RotateEffect(_processTemplate, _virtualRing);
            _virStabelEffect = new PulseEffect(_stableTemplate, _virtualRing);
            _virUnstabelEffect = new PulseEffect(_unstableTemplate, _virtualRing);
            _virBadEffect = new PulseEffect(_badTemplate, _virtualRing);
        }

        private void InitPulse(int length, ref Color[] pulseTemp)
        {
            int[] p = new int[length];
            int offsetBrightnes = byte.MaxValue / 4;

            // Erzeuge einen zentralen Puls für den Process
            p = GenerateGausianPulse(p.Length, 10, offsetBrightnes);
            for (int i = 0; i < length; i++)
                pulseTemp[i] = Color.FromArgb(MaxBrighness, (byte) p[i], (byte) (p[i]*12/16), 0x00);
        }

        private void InitPulsePause(int length, ref Color[] pulseTemp)
        {
            int[] p = new int[length];
            int offsetBrightnes = byte.MaxValue / 4;

            // Zeuge einen erkennbaren Puls für den Good-Status 
            int[] tempG = GenerateGausianPulse(p.Length / 2, 3, offsetBrightnes);
            for (int i = 0; i < tempG.Length; i++)
                p[i] = tempG[i];
            for (int i = tempG.Length; i < p.Length; i++)
                p[i] = offsetBrightnes;
            for (int i = 0; i < length; i++)
                pulseTemp[i] = Color.FromArgb(MaxBrighness, 0x00, (byte)p[i], 0x00);
        }

        private void InitPulsesPause(int length, ref Color[] pulseTemp)
        {
            int[] p = new int[length];
            int offsetBrightnes = byte.MaxValue / 4;

            // Erzuge drei erkennbaren Pulse für den Bad-Status 
            int[] tempB = GenerateGausianPulse(p.Length / 6, 3, offsetBrightnes);
            for (int i = 0; i < tempB.Length; i++)
            {
                p[i] = tempB[i];
                p[i + tempB.Length] = tempB[i];
                p[i + 2 * tempB.Length] = tempB[i];
            }
            for (int i = tempB.Length * 3; i < p.Length; i++)
                p[i] = offsetBrightnes;
            for (int i = 0; i < length; i++)
                pulseTemp[i] = Color.FromArgb(MaxBrighness, (byte)p[i], 0x00, 0x00);
        }

        private void InitColoredPulsesPause(int length, ref Color[] pulseTemp)
        {
            int[] p1 = new int[length];
            int[] p2 = new int[length];

            int offsetBrightnes = byte.MaxValue / 4;

            // Erzuge drei erkennbaren Pulse für den fast Good-Status 
            int[] tempP1 = GenerateGausianPulse(p1.Length / 4, 3, offsetBrightnes);
            for (int i = 0; i < tempP1.Length; i++)
            {
                p1[i] = tempP1[i];
                p1[i + tempP1.Length] = tempP1[i];
            }
            for (int i = tempP1.Length * 2; i < p1.Length; i++)
                p1[i] = offsetBrightnes;

            for (int i = 0; i < length; i++)
                p2[i] = 0;

            int[] tempP2 = GenerateGausianPulse(p1.Length / 4, 4, 0);
            for (int i = 0; i < tempP2.Length; i++)
            {
                p2[i + tempP2.Length] = tempP2[i];
            }

            for (int i = 0; i < length; i++)
                pulseTemp[i] = Color.FromArgb(MaxBrighness, (byte)p2[i], (byte) (p1[i]*12/16), 0x00);
        }

        public void GetCurvesAsGrayValues(out byte[] processStateProfile, out byte[] goodStateProfile,
            out byte[] badStateProfile)
        {
            processStateProfile = MakeGrayValuesToDataProfile(_processTemplate);
            goodStateProfile = MakeGrayValuesToDataProfile(_stableTemplate);
            badStateProfile = MakeGrayValuesToDataProfile(_badTemplate);
        }

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
            // Gleichverteilung auf Byte normieren
            for (int i = 0; i < grayArray.Length; i++)
                outputArray[i] = (byte) (grayArray[i] * byte.MaxValue / maxValue);

            return outputArray;
        }

        private static int[] GenerateGausianPulse(int length, int peakForm, int offset)
        {
            int[] tempplate = new int[length];
            int temporaryMax = 0;
            int idx = 0;
            int midLen = tempplate.Length / 2;
            for (int i = -midLen; i < midLen; i++)
            {
                tempplate[idx] = (int) (Math.Exp(-Math.Pow(peakForm * i / midLen, 2)) * byte.MaxValue) + offset;
                temporaryMax = Math.Max(temporaryMax, tempplate[idx]);
                idx++;
            }
            // Gleichverteilung auf Byte normieren
            for (int i = 0; i < tempplate.Length; i++)
                tempplate[i] = tempplate[i] * byte.MaxValue / temporaryMax;

            return tempplate;
        }


        private int _fadingCnt;
        private EngineState _fadingState;
        private EngineState _lastState = EngineState.Blank;

        public Color[] EffectAccordingToState(EngineState currentState)
        {
            if (currentState != _lastState)
            {
                _fadingState = _lastState;
                _lastState = currentState;
                _fadingCnt = FadingStart;
                ResetEffekt(currentState);
            }

            Color[] ringColors1 = GenerateNewImage(currentState);

            if (_fadingCnt > 0)
            {
                _fadingCnt--;
                Color[] ringColors2 = GenerateNewImage(_fadingState);

                int onfadingCnt = FadingStart - _fadingCnt;
                for (int i = 0; i < _virtualRing.Length; i++)
                {
                    _virtualRing[i].R = (byte) ((ringColors1[i].R * onfadingCnt + ringColors2[i].R * _fadingCnt) / FadingStart);
                    _virtualRing[i].G = (byte) ((ringColors1[i].G * onfadingCnt + ringColors2[i].G * _fadingCnt) / FadingStart);
                    _virtualRing[i].B = (byte)((ringColors1[i].B * onfadingCnt + ringColors2[i].B * _fadingCnt) / FadingStart);
                }
            }
            else
            {
                for (int i = 0; i < _virtualRing.Length; i++)
                    _virtualRing[i] = ringColors1[i];
            }
            return _virtualRing;
        }

        private Color[] GenerateNewImage(EngineState state)
        {
            Color[] colorSeries = new Color[0];
            switch (state)
            {
                case EngineState.Blank:
                    colorSeries = new Color[_virtualRing.Length];
                    for (int i = 0; i < colorSeries.Length; i++)
                        colorSeries[i] = Color.FromArgb(MaxBrighness, 0x00, 0x00, 0x00);
                    break;
                case EngineState.Idle:
                    colorSeries = new Color[_virtualRing.Length];
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

        private void ResetEffekt(EngineState state)
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
    }

    public class IndicatorConfig
    {
        /// @todo 
    }
}