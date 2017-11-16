using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    class StatusIndicator
    {
        int virtualTemplateLength;

        Color[] VirtualRing;

        Color[] ProcessTemplate;
        Color[] GoodTemplate;
        Color[] BadTemplate;

        public StatusIndicator(int targetCnt, int smoothFactor)
        {
            virtualTemplateLength = targetCnt * smoothFactor;
            VirtualRing = new Color[targetCnt];

            ProcessTemplate = new Color[virtualTemplateLength];
            GoodTemplate = new Color[virtualTemplateLength];
            BadTemplate = new Color[virtualTemplateLength];


            InitCurves(virtualTemplateLength, ref ProcessTemplate, ref GoodTemplate, ref BadTemplate);

            VirPrcsEffect = new RotateEffect(ProcessTemplate, VirtualRing);
            VirGoodEffect = new PulseEffect(GoodTemplate, VirtualRing);
            VirBadEffect = new PulseEffect(BadTemplate, VirtualRing);
        }

        void InitCurves(int length, ref Color[] processTemp, ref Color[] goodTemp, ref Color[] badTemp)
        {
            int[] prcs = new int[length];
            int[] good = new int[length];
            int[] bad = new int[length];
            int OffsetBrightnes = Byte.MaxValue / 4;


            // Erzeuge einen zentralen Puls für den Process
            prcs = GenerateGausianPulse(prcs.Length, 10, OffsetBrightnes);
            for (int i = 0; i < length; i++)
            {
                processTemp[i] = Color.FromArgb(0xFF, (byte)prcs[i], (byte)prcs[i], 0x00);
            }

            // Zeuge einen erkennbaren Puls für den Good-Status 
            int[] tempG = GenerateGausianPulse(prcs.Length / 2, 3, OffsetBrightnes);
            for (int i = 0; i < tempG.Length; i++)
            {
                good[i] = tempG[i];
            }
            for (int i = tempG.Length; i < good.Length; i++)
            {
                good[i] = OffsetBrightnes;
            }
            for (int i = 0; i < length; i++)
            {
                goodTemp[i] = Color.FromArgb(0xFF, 0x00, (byte)good[i], 0x00);
            }


            // Erzuge drei erkennbaren Pulse für den Bad-Status 
            int[] tempB = GenerateGausianPulse(prcs.Length / 4, 3, OffsetBrightnes);
            for (int i = 0; i < tempB.Length; i++)
            {
                bad[i] = tempB[i];
                bad[i + tempB.Length] = tempB[i];
                bad[i + (2 * tempB.Length)] = tempB[i];
            }
            for (int i = tempB.Length * 3; i < bad.Length; i++)
            {
                bad[i] = OffsetBrightnes;
            }
            for (int i = 0; i < length; i++)
            {
                badTemp[i] = Color.FromArgb(0xFF, (byte)bad[i], 0x00, 0x00);
            }
        }

        public void GetCurvesAsGrayValues(out byte[] processStateProfile, out byte[] goodStateProfile, out byte[] badStateProfile)
        {
            processStateProfile = MakeGrayValuesToDataProfile(ProcessTemplate);
            goodStateProfile = MakeGrayValuesToDataProfile(GoodTemplate);
            badStateProfile = MakeGrayValuesToDataProfile(BadTemplate);
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
                grayArray[i] = (grayArray[i] * colorArray[i].A) / Byte.MaxValue;
                maxValue = Math.Max(maxValue, grayArray[i]);
            }
            // Gleichverteilung auf Byte normieren
            for (int i = 0; i < grayArray.Length; i++)
            {
                outputArray[i] = (byte)(grayArray[i] * Byte.MaxValue / maxValue);
            }

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
                tempplate[idx] = (int)(Math.Exp(-(Math.Pow(peakForm * i / midLen, 2))) * Byte.MaxValue) + offset;
                temporaryMax = Math.Max(temporaryMax, tempplate[idx]);
                idx++;
            }
            // Gleichverteilung auf Byte normieren
            for (int i = 0; i < tempplate.Length; i++)
            {
                tempplate[i] = tempplate[i] * Byte.MaxValue / temporaryMax;
            }

            return tempplate;
        }



        E_States lastState = E_States.Idle;
        E_States FadingState;

        int fadingCnt = 0;
        const int FADING_START = 10;

        public Color[] EffectAccordingToState(E_States currentState)
        {
            if (currentState != lastState)
            {
                FadingState = lastState;
                lastState = currentState;
                fadingCnt = FADING_START;
                ResetEffekt(currentState);
            }

            Color[] RingColors1 = new Color[0];
            Color[] RingColors2 = new Color[0];

            RingColors1 = GenerateNewImage(currentState);

            if (fadingCnt > 0)
            {
                fadingCnt--;
                RingColors2 = GenerateNewImage(FadingState);

                int onfadingCnt = FADING_START - fadingCnt;
                for (int i = 0; i < VirtualRing.Length; i++)
                {
                    VirtualRing[i].R = (byte)(((int)RingColors1[i].R * onfadingCnt + (int)RingColors2[i].R * fadingCnt) / 10);
                    VirtualRing[i].G = (byte)(((int)RingColors1[i].G * onfadingCnt + (int)RingColors2[i].G * fadingCnt) / 10);
                    //VirtualRing[i].B = (byte)(((int)RingColors1[i].B * onfadingCnt + (int)RingColors2[i].B * fadingCnt) / 10);
                    VirtualRing[i].B = 0;
                }
            }
            else
            {
                for (int i = 0; i < VirtualRing.Length; i++)
                {
                    VirtualRing[i] = RingColors1[i];
                }
            }
            return VirtualRing;
        }

        private Color[] GenerateNewImage(E_States state)
        {
            Color[] colorSeries = new Color[0];
            switch (state)
            {
                case E_States.Idle:
                    colorSeries = new Color[VirtualRing.Length];
                    for (int i = 0; i < colorSeries.Length; i++)
                    {
                        colorSeries[i] = Color.FromArgb(0xFF, 0x20, 0x20, 0x20);
                    }
                    break;
                case E_States.Progress:
                    colorSeries = VirPrcsEffect.RotateStep();
                    break;
                case E_States.Good:
                    colorSeries = VirGoodEffect.PulseStep();
                    break;
                case E_States.Bad:
                    colorSeries = VirBadEffect.PulseStep();
                    break;
            }

            return colorSeries;
        }

        private void ResetEffekt(E_States state)
        {
            switch (state)
            {
                case E_States.Progress:
                    VirPrcsEffect.ResetIndex();
                    break;
                case E_States.Good:
                    VirGoodEffect.ResetIndex();
                    break;
                case E_States.Bad:
                    VirBadEffect.ResetIndex();
                    break;
            }
        }

        RotateEffect VirPrcsEffect;
        PulseEffect VirGoodEffect;
        PulseEffect VirBadEffect;


        class RotateEffect
        {
            int targetLenght;
            int currentIdx;
            int maxIndex;
            int deltaStep;

            Color[] Source;
            Color[] Temp;

            public RotateEffect(Color[] source, Color[] target)
            {
                currentIdx = 0;
                targetLenght = target.Length;
                maxIndex = source.Length;
                deltaStep = maxIndex / targetLenght;

                Source = source;
                Temp = new Color[target.Length];
            }

            public Color[] RotateStep()
            {
                int relIdx = currentIdx;

                for (int i = 0; i < targetLenght; i++)
                {
                    Temp[i] = Source[relIdx];
                    relIdx += deltaStep;
                    relIdx = SaturateIndex(relIdx);
                }

                currentIdx += deltaStep;
                currentIdx = SaturateIndex(currentIdx);
                return Temp;
            }

            private int SaturateIndex(int idx)
            {
                if (idx >= maxIndex)
                {
                    idx -= maxIndex;
                }
                return idx;
            }

            public void ResetIndex()
            {
                currentIdx = 0;
            }
        }

        class PulseEffect
        {
            int targetLenght;
            int currentIdx;
            int maxIndex;
            int deltaStep;

            Color[] Source;
            Color[] Temp;

            public PulseEffect(Color[] source, Color[] target)
            {
                currentIdx = 0;
                targetLenght = target.Length;
                maxIndex = source.Length;
                deltaStep = maxIndex / targetLenght;

                Source = source;
                Temp = new Color[target.Length];
            }

            public Color[] PulseStep()
            {
                for (int i = 0; i < targetLenght; i++)
                {
                    Temp[i] = Source[currentIdx];
                }
                currentIdx += deltaStep;
                currentIdx = SaturateIndex(currentIdx);
                return Temp;
            }

            private int SaturateIndex(int idx)
            {
                if (idx >= maxIndex)
                {
                    idx -= maxIndex;
                }
                return idx;
            }

            public void ResetIndex()
            {
                currentIdx = 0;
            }
        }
    }


    enum E_States
    {
        Idle,
        Progress,
        Good,
        Bad,
    }

}
