using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using OxyPlot;
using OxyPlot.Series;
using PublicStatusIndicator.IndicatorEngine;
using Windows.UI;



// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PublicStatusIndicator.GUI_Elements
{
    public sealed partial class Settings : UserControl
    {
        MainPage ParentPage;
        private readonly StatusIndicator _virtualIndicator;

        public PlotModel RotatePlot { get; set;}
        public PlotModel PulsePlot { get; set; }

        public Settings(MainPage parent, StatusIndicator.IndicatorConfig config)
        {
            ParentPage = parent;
            this.InitializeComponent();

            _virtualIndicator = new StatusIndicator(config);

            Color[] _badTemp;
            Color[] _unstableTemp;
            Color[] _stableTemp;
            Color[] _processTemp;
            _virtualIndicator.GetAllTemplates(out _processTemp, out _badTemp, out _unstableTemp, out _stableTemp);

            int[] bad_GryTemp = ConverteColor2NormalizedGrayvalues(_badTemp);
            int[] unstable_GryTemp = ConverteColor2NormalizedGrayvalues(_unstableTemp);
            int[] stable_GryTemp = ConverteColor2NormalizedGrayvalues(_stableTemp);
            int[] process_GryTemp = ConverteColor2NormalizedGrayvalues(_processTemp);

            RotatePlot = CreatePlotModel(process_GryTemp, "Rotated Effect", "In Process", OxyColors.Yellow);

            PulsePlot = CreatePlotModel(bad_GryTemp, "Pulsed Effect", "Is Bad", OxyColors.Red);
            Add2PlotModel(PulsePlot, unstable_GryTemp, "Is Unstable", OxyColors.Orange);
            Add2PlotModel(PulsePlot, stable_GryTemp, "Is Stable", OxyColors.Green);

            DataContext = this;
        }

        private static PlotModel CreatePlotModel(int[] y_data, string title, string label, OxyColor color)
        {
            var model = new PlotModel { Title = title };

            model.Background = OxyColors.Black;
            model.TextColor = OxyColors.White;
            model.PlotAreaBorderColor = OxyColors.White;

            var seriesRaw = new LineSeries()
            {
                MarkerStroke = color,
            };

            for (int i = 1; i < y_data.Length; i++)
            {
                seriesRaw.Points.Add(new DataPoint(i, y_data[i]));
            }
            model.Series.Add(seriesRaw);
            return model;
        }

        private static void Add2PlotModel(PlotModel plot, int[] y_data, string label, OxyColor color)
        {
            var seriesRaw = new LineSeries()
            {
                MarkerStroke = color,
            };

            for (int i = 1; i < y_data.Length; i++)
            {
                seriesRaw.Points.Add(new DataPoint(i, y_data[i]));
            }
            plot.Series.Add(seriesRaw);
        }

        int NORM_MAX = 1000;
        int[] ConverteColor2NormalizedGrayvalues(Color[] waveform)
        {
            int[] gray = new int[waveform.Length];
            int Wmax = 0;

            for (int i = 0; i < gray.Length; i++)
            {
                gray[i] = (waveform[i].R + waveform[i].G + waveform[i].B) * waveform[i].A;
                if (gray[i] > Wmax)
                {
                    Wmax = gray[i];
                }
            }
            for (int i = 0; i < gray.Length; i++)
            {
                gray[i] = gray[i] * 100 / Wmax;
            }
            return gray;
        }
    }
}
