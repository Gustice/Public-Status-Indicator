using System;
using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    public class StatusIndicator
    {
        private const int FadingStart = 10;

        private readonly Color[] _badTemplate;
        private readonly Color[] _goodTemplate;
        private readonly Color[] _processTemplate;
        private readonly Color[] _virtualRing;

        private int _fadingCnt;
        private EngineState _fadingState;
        private EngineState _lastState = EngineState.Idle;
        private readonly PulseEffect _virBadEffect;

        private readonly PulseEffect _virGoodEffect;
        private readonly RotateEffect _virPrcsEffect;


        public StatusIndicator(int targetCnt, int smoothFactor)
        {
            var virtualTemplateLength = targetCnt * smoothFactor;
            _virtualRing = new Color[targetCnt];
            _processTemplate = new Color[virtualTemplateLength];
            _goodTemplate = new Color[virtualTemplateLength];
            _badTemplate = new Color[virtualTemplateLength];


            InitCurves(virtualTemplateLength, ref _processTemplate, ref _goodTemplate, ref _badTemplate);

            _virPrcsEffect = new RotateEffect(_processTemplate, _virtualRing);
            _virGoodEffect = new PulseEffect(_goodTemplate, _virtualRing);
            _virBadEffect = new PulseEffect(_badTemplate, _virtualRing);
        }

        private void InitCurves(int length, ref Color[] processTemp, ref Color[] goodTemp, ref Color[] badTemp)
        {
            var prcs = new int[length];
            var good = new int[length];
            var bad = new int[length];
            var offsetBrightnes = byte.MaxValue / 4;


            // Erzeuge einen zentralen Puls für den Process
            prcs = GenerateGausianPulse(prcs.Length, 10, offsetBrightnes);
            for (var i = 0; i < length; i++)
                processTemp[i] = Color.FromArgb(0xFF, (byte) prcs[i], (byte) prcs[i], 0x00);

            // Zeuge einen erkennbaren Puls für den Good-Status 
            var tempG = GenerateGausianPulse(prcs.Length / 2, 3, offsetBrightnes);
            for (var i = 0; i < tempG.Length; i++)
                good[i] = tempG[i];
            for (var i = tempG.Length; i < good.Length; i++)
                good[i] = offsetBrightnes;
            for (var i = 0; i < length; i++)
                goodTemp[i] = Color.FromArgb(0xFF, 0x00, (byte) good[i], 0x00);


            // Erzuge drei erkennbaren Pulse für den Bad-Status 
            var tempB = GenerateGausianPulse(prcs.Length / 4, 3, offsetBrightnes);
            for (var i = 0; i < tempB.Length; i++)
            {
                bad[i] = tempB[i];
                bad[i + tempB.Length] = tempB[i];
                bad[i + 2 * tempB.Length] = tempB[i];
            }
            for (var i = tempB.Length * 3; i < bad.Length; i++)
                bad[i] = offsetBrightnes;
            for (var i = 0; i < length; i++)
                badTemp[i] = Color.FromArgb(0xFF, (byte) bad[i], 0x00, 0x00);
        }

        public void GetCurvesAsGrayValues(out byte[] processStateProfile, out byte[] goodStateProfile,
            out byte[] badStateProfile)
        {
            processStateProfile = MakeGrayValuesToDataProfile(_processTemplate);
            goodStateProfile = MakeGrayValuesToDataProfile(_goodTemplate);
            badStateProfile = MakeGrayValuesToDataProfile(_badTemplate);
        }

        private static byte[] MakeGrayValuesToDataProfile(Color[] colorArray)
        {
            var outputArray = new byte[colorArray.Length];

            var grayArray = new int[colorArray.Length];
            var maxValue = 0;
            for (var i = 0; i < colorArray.Length; i++)
            {
                grayArray[i] = colorArray[i].R;
                grayArray[i] += colorArray[i].G;
                grayArray[i] += colorArray[i].B;
                grayArray[i] = grayArray[i] * colorArray[i].A / byte.MaxValue;
                maxValue = Math.Max(maxValue, grayArray[i]);
            }
            // Gleichverteilung auf Byte normieren
            for (var i = 0; i < grayArray.Length; i++)
                outputArray[i] = (byte) (grayArray[i] * byte.MaxValue / maxValue);

            return outputArray;
        }

        private static int[] GenerateGausianPulse(int length, int peakForm, int offset)
        {
            var tempplate = new int[length];
            var temporaryMax = 0;
            var idx = 0;
            var midLen = tempplate.Length / 2;
            for (var i = -midLen; i < midLen; i++)
            {
                tempplate[idx] = (int) (Math.Exp(-Math.Pow(peakForm * i / midLen, 2)) * byte.MaxValue) + offset;
                temporaryMax = Math.Max(temporaryMax, tempplate[idx]);
                idx++;
            }
            // Gleichverteilung auf Byte normieren
            for (var i = 0; i < tempplate.Length; i++)
                tempplate[i] = tempplate[i] * byte.MaxValue / temporaryMax;

            return tempplate;
        }

        public Color[] EffectAccordingToState(EngineState currentState)
        {
            if (currentState != _lastState)
            {
                _fadingState = _lastState;
                _lastState = currentState;
                _fadingCnt = FadingStart;
                ResetEffekt(currentState);
            }
            


            var ringColors1 = GenerateNewImage(currentState);

            if (_fadingCnt > 0)
            {
                _fadingCnt--;
                var ringColors2 = GenerateNewImage(_fadingState);

                var onfadingCnt = FadingStart - _fadingCnt;
                for (var i = 0; i < _virtualRing.Length; i++)
                {
                    _virtualRing[i].R = (byte) ((ringColors1[i].R * onfadingCnt + ringColors2[i].R * _fadingCnt) / 10);
                    _virtualRing[i].G = (byte) ((ringColors1[i].G * onfadingCnt + ringColors2[i].G * _fadingCnt) / 10);
                    _virtualRing[i].B = 0;
                }
            }
            else
            {
                for (var i = 0; i < _virtualRing.Length; i++)
                    _virtualRing[i] = ringColors1[i];
            }
            return _virtualRing;
        }

        private Color[] GenerateNewImage(EngineState state)
        {
            var colorSeries = new Color[0];
            switch (state)
            {
                case EngineState.Idle:
                    colorSeries = new Color[_virtualRing.Length];
                    for (var i = 0; i < colorSeries.Length; i++)
                        colorSeries[i] = Color.FromArgb(0xFF, 0x20, 0x20, 0x20);
                    break;
                case EngineState.Progress:
                    colorSeries = _virPrcsEffect.RotateStep();
                    break;
                case EngineState.Good:
                    colorSeries = _virGoodEffect.PulseStep();
                    break;
                case EngineState.Bad:
                    colorSeries = _virBadEffect.PulseStep();
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
                case EngineState.Good:
                    _virGoodEffect.ResetIndex();
                    break;
                case EngineState.Bad:
                    _virBadEffect.ResetIndex();
                    break;
            }
        }
    }

    public enum EngineState
    {
        Idle,
        Progress,
        Good,
        Bad,
        Blank
    }
}