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
using OxyPlot.Axes;



// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace PublicStatusIndicator.GUI_Elements
{
    public sealed partial class Settings : UserControl
    {
        MainPage ParentPage;
        private readonly StatusIndicator _virtualIndicator;

        /// <summary>
        /// Plot model to display rotated effects
        /// </summary>
        public PlotModel RotatePlot { get; set;}

        /// <summary>
        /// Plot model to display pulsed effects
        /// </summary>
        public PlotModel PulsePlot { get; set; }


        public PlotModel MovePlot { get; set; }


        public Settings(MainPage parent, StatusIndicator.IndicatorConfig config)
        {
            ParentPage = parent;
            this.InitializeComponent();

            _virtualIndicator = new StatusIndicator(config);

            // Convert templates to grayscale waveforms
            int[] process_GryTemp = ConverteColor2NormalizedGrayvalues(_virtualIndicator.ProcessTemplate);
            int[] sauron_GryTemp = ConverteColor2NormalizedGrayvalues(_virtualIndicator.SauronTemplate);
            
            // Display all rotated effects in according plot-model
            RotatePlot = CreatePlotModel(process_GryTemp, "Rotated Effect", "In Process", OxyColors.Yellow);
            Add2PlotModel(RotatePlot, sauron_GryTemp, "Saurons Eye", OxyColors.Red);


            // Convert templates to grayscale waveforms
            int[] bad_GryTemp = ConverteColor2NormalizedGrayvalues(_virtualIndicator.BadTemplate);
            int[] unstable_GryTemp = ConverteColor2NormalizedGrayvalues(_virtualIndicator.UnstableTemplate);
            int[] stable_GryTemp = ConverteColor2NormalizedGrayvalues(_virtualIndicator.StableTemplate);

            // Display all pulsed effects in according plot-model
            PulsePlot = CreatePlotModel(bad_GryTemp, "Pulsed Effect", "Is Bad", OxyColors.Red);
            Add2PlotModel(PulsePlot, unstable_GryTemp, "Is Unstable", OxyColors.Orange);
            Add2PlotModel(PulsePlot, stable_GryTemp, "Is Stable", OxyColors.Green);

            SauronHabits moves = new SauronHabits(
                new SauronHabits.NervousEye.Config() { Interval = 5, Section = 1},
                new SauronHabits.CuriousEye.Config() { Interval = 20, Section = 30, Duration = 40}
                );

            int[] nMove = new int[100];
            int i = 0;
            for (i = 0; i < nMove.Length; i++)
            {
                nMove[i] = moves.DitherEyeRandomly() * 5;
            }
            int[] fMove = new int[100];

            i = 0;
            fMove[i] = moves.ChangeFixPoint(30);
            for (i++; i < fMove.Length/2; i++)
                fMove[i] = moves.ChangeFixPoint();
            fMove[i] = moves.ChangeFixPoint(-60);
            for (i++; i < fMove.Length; i++)
                fMove[i] = moves.ChangeFixPoint();

            MovePlot = CreatePlotModel(nMove, "Move Effect", "Nervous Dither", OxyColors.Yellow);
            Add2PlotModel(MovePlot, fMove, "Random Fixpoint", OxyColors.Orange);

            DataContext = this;
        }

        /// <summary>
        /// Creats Plot model and sets up basic configuration like axis appearence and colors
        /// </summary>
        /// <param name="y_data"></param>
        /// <param name="title">Type of effect. To be displayed left of the y-axis</param>
        /// <param name="label">Name of waveform. To be displayed in the upper right corner</param>
        /// <param name="color">Color of waveform</param>
        /// <returns></returns>
        private static PlotModel CreatePlotModel(int[] y_data, string title, string label, OxyColor color)
        {
            var model = new PlotModel();

            model.Background = OxyColors.Black;
            model.TextColor = OxyColors.White;
            model.PlotAreaBorderColor = OxyColors.White;

            var seriesRaw = new LineSeries();
            seriesRaw.Color = color;
            seriesRaw.Title = label;

            model.Axes.Add(new LinearAxis { Position = AxisPosition.Left, Title = title});
            model.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, IsAxisVisible = false });

            // Add datapoints
            for (int i = 1; i < y_data.Length; i++)
            {
                seriesRaw.Points.Add(new DataPoint(i, y_data[i]));
            }
            model.Series.Add(seriesRaw);

            return model;
        }

        /// <summary>
        /// Appends additional axis to existing plot model
        /// </summary>
        /// <param name="plot"></param>
        /// <param name="y_data"></param>
        /// <param name="label">Name of waveform. To be displayed in the upper right corner</param>
        /// <param name="color">Color of waveform</param>
        private static void Add2PlotModel(PlotModel plot, int[] y_data, string label, OxyColor color)
        {
            var seriesRaw = new LineSeries();
            seriesRaw.Color = color;
            seriesRaw.Title = label;

            // Add datapoints
            for (int i = 1; i < y_data.Length; i++)
            {
                seriesRaw.Points.Add(new DataPoint(i, y_data[i]));
            }
            plot.Series.Add(seriesRaw);
        }

        const int NORM_MAX = 100;
        /// <summary>
        /// Converts colored waveform into grayscale values. Normalizes output to given maximum.
        /// </summary>
        /// <param name="waveform"></param>
        /// <returns></returns>
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
                gray[i] = gray[i] * NORM_MAX / Wmax;
            }
            return gray;
        }
    }
}
