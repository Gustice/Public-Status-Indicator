using System.Collections.Generic;
using Windows.UI;

namespace PublicStatusIndicator.IndicatorEngine
{
    /// <summary>
    /// Applicable engine states that can be displayed.
    /// </summary>
    public enum EngineState
    {
        /// <summary>
        /// Playing dead:    
        /// No Color, just blank out all LEDs.
        /// </summary>
        Blank,

        /// <summary>
        /// I'm alive:       
        /// Dim white color to show that something migh got switched on and the indicatior might be fully operational
        /// </summary>
        Idle,

        /// <summary>
        /// I'm on it:
        /// Yellow rotating pulse to show that something might be in progress
        /// </summary>
        Progress,

        /// <summary>
        /// Fix it please:          
        /// Red insistent pulses that show that problems occured during
        /// </summary>
        Bad,

        /// <summary>
        /// Allmost done:
        /// It seems to be good. But there appeared some issues.
        /// </summary>
        Unstable,

        /// <summary>
        /// All great:
        /// Operation successfull.
        /// </summary>
        Stable,

        /// <summary>
        /// Blame the unworthy dwarf who dared to mess this up
        /// </summary>
        Sauron,
    }





    /// <summary>
    /// Dictionary defines for strings and colors
    /// </summary>
    public class EngineDefines
    {
        public static readonly Dictionary<EngineState, string> StateOutputs = new Dictionary<EngineState, string>
        {
            {EngineState.Blank, "Blank"},
            {EngineState.Idle, "Idle"},
            {EngineState.Progress, "Progress"},
            {EngineState.Bad, "BAD"},
            {EngineState.Unstable, "Unstable"},
            {EngineState.Stable, "Stable"},
            {EngineState.Sauron, "O.o"},
        };

        public static readonly Dictionary<EngineState, Color> StateColors = new Dictionary<EngineState, Color>
        {
            {EngineState.Blank,     Color.FromArgb(0xFF, 0x00, 0x00, 0x00)},
            {EngineState.Idle,      Color.FromArgb(0xFF, 0x08, 0x08, 0x08)},
            {EngineState.Progress,  Color.FromArgb(0xFF, 0x40, 0x40, 0x00)},
            {EngineState.Bad,       Color.FromArgb(0xFF, 0x80, 0x00, 0x00)},
            {EngineState.Unstable,  Color.FromArgb(0xFF, 0x40, 0x60, 0x00)},
            {EngineState.Stable,    Color.FromArgb(0xFF, 0x00, 0x80, 0x00)},
            {EngineState.Sauron,    Color.FromArgb(0xFF, 0x80, 0x04, 0x00)},
        };


        public static readonly List<ProfileElement> SummonSauron = new List<ProfileElement>
        {
            {new ProfileElement(EngineState.Blank,20) },
            {new SauronProfileElement(SauronEffect.States.Appear,20) },
            {new SauronProfileElement(SauronEffect.States.Idle,int.MaxValue) },
        };
        public static readonly List<ProfileElement> DismissSauron = new List<ProfileElement>
        {
            {new SauronProfileElement(SauronEffect.States.Disappear,20) },
            {new ProfileElement(EngineState.Blank,int.MaxValue) },
        };

        public static readonly List<ProfileElement> MoveHimRight = new List<ProfileElement>
        {
            {new SauronProfileElement(SauronEffect.States.Move, 5, 1) },
            {new SauronProfileElement(SauronEffect.States.Idle,int.MaxValue) },
        };
        public static readonly List<ProfileElement> MoveHimLeft = new List<ProfileElement>
        {
            {new SauronProfileElement(SauronEffect.States.Move, 5, -1) },
            {new SauronProfileElement(SauronEffect.States.Idle,int.MaxValue) },
        };

        public static readonly List<ProfileElement> SauronAppearAndDisappear = new List<ProfileElement>
        {
            {new ProfileElement(EngineState.Blank,20) },
            {new SauronProfileElement(SauronEffect.States.Appear,20) },
            {new SauronProfileElement(SauronEffect.States.Idle,20) },
            {new SauronProfileElement(SauronEffect.States.Disappear,20) },
        };

        public static readonly List<ProfileElement> SauronBlame = new List<ProfileElement>
        {
            {new ProfileElement(EngineState.Blank,20) },
            {new SauronProfileElement(SauronEffect.States.Appear,20) },
            {new SauronProfileElement(SauronEffect.States.Idle,20) },
            {new SauronProfileElement(SauronEffect.States.Move,20,20) },
            {new SauronProfileElement(SauronEffect.States.Mad,100) },
            {new SauronProfileElement(SauronEffect.States.Disappear,20) },
        };
    }


    public class SauronProfileElement : ProfileElement
    {
        public SauronEffect.States SauronState;
        public int NewPosition;

        public SauronProfileElement(SauronEffect.States subState, int duration, int newPos = 0) : base (EngineState.Sauron, duration)
        {
            SauronState = subState;
            NewPosition = 0;
        }
    }

    public class ProfileElement
    {
        public EngineState State;
        public int Duration;

        public ProfileElement(EngineState state, int duration)
        {
            State = state;
            Duration = duration;
            
        }
    }
}