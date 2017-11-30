using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using PublicStatusIndicator.IndicatorEngine;
using System.Runtime.CompilerServices;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PublicStatusIndicator.GUI_Elements
{
    public sealed partial class Preview : UserControl, INotifyPropertyChanged
    {
        #region HardCodedSettings
        private const int VIRTUAL_LEN = 12;
        #endregion

        public ObersableBrushes _displayRing { get; }

        private string _statusOutput = "Status";
        public string StatusOutput
        {
            get { return _statusOutput; }
            set { _statusOutput = value; NotifyPropertyChanged(); }
        }

        private SolidColorBrush _centerColor = new SolidColorBrush(Colors.DarkGray);
        public SolidColorBrush CenterColor
        {
            get { return _centerColor; }
            set { _centerColor = value; NotifyPropertyChanged(); }
        }

        private Color[] _virtualRing;
        private readonly StatusIndicator _virtualIndicator;

        MainPage ParentPage;

        private EngineState _state = EngineState.Idle;


        public Preview()
        {
        }
        public Preview(MainPage parent)
        {
            _virtualRing = new Color[VIRTUAL_LEN];
            _virtualIndicator = new StatusIndicator(VIRTUAL_LEN, VIRTUAL_LEN * 8);

            ParentPage = parent;
            _displayRing = new ObersableBrushes();

            this.InitializeComponent();
            DataContext = this;
        }

        public void RefreshPage()
        {
            _virtualRing = _virtualIndicator.EffectAccordingToState(_state);
            _displayRing.SetAllVaules(_virtualRing);
        }

        public void ChangeState(EngineState newState)
        {
            _state = newState;
            StatusOutput = EngineDefines.StateOutputs[_state];
            CenterColor = new SolidColorBrush(EngineDefines.StateColors[_state]);
        }

        #region PropChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class ObersableBrushes : INotifyPropertyChanged
    {
        private SolidColorBrush _color1;
        private SolidColorBrush _color2;
        private SolidColorBrush _color3;
        private SolidColorBrush _color4;
        private SolidColorBrush _color5;
        private SolidColorBrush _color6;
        private SolidColorBrush _color7;
        private SolidColorBrush _color8;
        private SolidColorBrush _color9;
        private SolidColorBrush _color10;
        private SolidColorBrush _color11;
        private SolidColorBrush _color12;


        public ObersableBrushes()
        {
            Color1 = new SolidColorBrush(Colors.DarkGray);
            Color2 = new SolidColorBrush(Colors.DarkGray);
            Color3 = new SolidColorBrush(Colors.DarkGray);
            Color4 = new SolidColorBrush(Colors.DarkGray);
            Color5 = new SolidColorBrush(Colors.DarkGray);
            Color6 = new SolidColorBrush(Colors.DarkGray);
            Color7 = new SolidColorBrush(Colors.DarkGray);
            Color8 = new SolidColorBrush(Colors.DarkGray);
            Color9 = new SolidColorBrush(Colors.DarkGray);
            Color10 = new SolidColorBrush(Colors.DarkGray);
            Color11 = new SolidColorBrush(Colors.DarkGray);
            Color12 = new SolidColorBrush(Colors.DarkGray);
        }

        public SolidColorBrush Color1
        {
            get { return _color1; }
            set { _color1 = value; NotifyPropertyChanged(); }
        }

        public SolidColorBrush Color2
        {
            get { return _color2; }
            set { _color2 = value; NotifyPropertyChanged(); }
        }

        public SolidColorBrush Color3
        {
            get { return _color3; }
            set { _color3 = value; NotifyPropertyChanged(); }
        }

        public SolidColorBrush Color4
        {
            get { return _color4; }
            set { _color4 = value; NotifyPropertyChanged(); }
        }

        public SolidColorBrush Color5
        {
            get { return _color5; }
            set { _color5 = value; NotifyPropertyChanged(); }
        }

        public SolidColorBrush Color6
        {
            get { return _color6; }
            set { _color6 = value; NotifyPropertyChanged(); }
        }

        public SolidColorBrush Color7
        {
            get { return _color7; }
            set { _color7 = value; NotifyPropertyChanged(); }
        }

        public SolidColorBrush Color8
        {
            get { return _color8; }
            set { _color8 = value; NotifyPropertyChanged(); }
        }

        public SolidColorBrush Color9
        {
            get { return _color9; }
            set { _color9 = value; NotifyPropertyChanged(); }
        }

        public SolidColorBrush Color10
        {
            get { return _color10; }
            set { _color10 = value; NotifyPropertyChanged(); }
        }

        public SolidColorBrush Color11
        {
            get { return _color11; }
            set { _color11 = value; NotifyPropertyChanged(); }
        }

        public SolidColorBrush Color12
        {
            get { return _color12; }
            set { _color12 = value; NotifyPropertyChanged(); }
        }

        public void SetAllVaules(Color[] newColors)
        {
            Color1 = new SolidColorBrush(newColors[0]);
            Color2 = new SolidColorBrush(newColors[1]);
            Color3 = new SolidColorBrush(newColors[2]);
            Color4 = new SolidColorBrush(newColors[3]);
            Color5 = new SolidColorBrush(newColors[4]);
            Color6 = new SolidColorBrush(newColors[5]);
            Color7 = new SolidColorBrush(newColors[6]);
            Color8 = new SolidColorBrush(newColors[7]);
            Color9 = new SolidColorBrush(newColors[8]);
            Color10 = new SolidColorBrush(newColors[9]);
            Color11 = new SolidColorBrush(newColors[10]);
            Color12 = new SolidColorBrush(newColors[11]);
        }

        #region PropChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }


}
