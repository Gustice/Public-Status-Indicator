using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace PublicStatusIndicator.IndicatorEngine
{
    public class ObersableBrushes : INotifyPropertyChanged
    {
        private SolidColorBrush _color1;

        private SolidColorBrush _color10;

        private SolidColorBrush _color11;

        private SolidColorBrush _color12;

        private SolidColorBrush _color2;

        private SolidColorBrush _color3;

        private SolidColorBrush _color4;

        private SolidColorBrush _color5;

        private SolidColorBrush _color6;

        private SolidColorBrush _color7;

        private SolidColorBrush _color8;

        private SolidColorBrush _color9;

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
            set
            {
                _color1 = value;
                NotifyPropertyChanged();
            }
        }

        public SolidColorBrush Color2
        {
            get { return _color2; }
            set
            {
                _color2 = value;
                NotifyPropertyChanged();
            }
        }

        public SolidColorBrush Color3
        {
            get { return _color3; }
            set
            {
                _color3 = value;
                NotifyPropertyChanged();
            }
        }

        public SolidColorBrush Color4
        {
            get { return _color4; }
            set
            {
                _color4 = value;
                NotifyPropertyChanged();
            }
        }

        public SolidColorBrush Color5
        {
            get { return _color5; }
            set
            {
                _color5 = value;
                NotifyPropertyChanged();
            }
        }

        public SolidColorBrush Color6
        {
            get { return _color6; }
            set
            {
                _color6 = value;
                NotifyPropertyChanged();
            }
        }

        public SolidColorBrush Color7
        {
            get { return _color7; }
            set
            {
                _color7 = value;
                NotifyPropertyChanged();
            }
        }

        public SolidColorBrush Color8
        {
            get { return _color8; }
            set
            {
                _color8 = value;
                NotifyPropertyChanged();
            }
        }

        public SolidColorBrush Color9
        {
            get { return _color9; }
            set
            {
                _color9 = value;
                NotifyPropertyChanged();
            }
        }

        public SolidColorBrush Color10
        {
            get { return _color10; }
            set
            {
                _color10 = value;
                NotifyPropertyChanged();
            }
        }

        public SolidColorBrush Color11
        {
            get { return _color11; }
            set
            {
                _color11 = value;
                NotifyPropertyChanged();
            }
        }

        public SolidColorBrush Color12
        {
            get { return _color12; }
            set
            {
                _color12 = value;
                NotifyPropertyChanged();
            }
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