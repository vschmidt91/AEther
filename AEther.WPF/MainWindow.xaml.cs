using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;

using AEther;

namespace AEther.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        static readonly PixelFormat SpectrumFormat = PixelFormats.Rgba128Float;

        SampleSource Audio;
        Histogram[] Histograms;
        WriteableBitmap[] HistogramBitmaps;
        Session Session;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            Dispatcher.Invoke(() => Start(Configuration.ReadFromFile(e.FullPath)));
        }

        void Session_Output(object sender, SplitterEvent evt)
        {
            for (int c = 0; c < 2; ++c)
            {
                var src = evt[c].Data;
                Histograms[c].Add(src.Floats.AsSpan(0, src.FloatCount));
            }
        }

        void Start(Configuration configuration)
        {

            Audio = new CSCore.Listener(configuration.SampleChannelCapacity, configuration.SampleBufferSize, configuration.SampleBufferCount);
            Session = new Session(configuration, Audio.SampleRate);

            Histograms = Enumerable.Range(0, 2)
                .Select(c => new Histogram(Session.Domain.Count, configuration.TimeResolution))
                .ToArray();
            HistogramBitmaps = Histograms
                .Select(h => new WriteableBitmap(h.Width, h.Height, 96, 96, Histogram.Format, null))
                .ToArray();

            var testEffect = (TestEffect)Effect;
            testEffect.Left = new ImageBrush(HistogramBitmaps[0]);
            testEffect.Right = new ImageBrush(HistogramBitmaps[1]);

            Session.StartThread(Audio.SampleReader.ReadAllAsync(), Session_Output);

            Audio.Start();

        }

        void Stop()
        {
            Audio.Stop();
            Session.Join();
        }

        protected override void OnInitialized(EventArgs e)
        {

            base.OnInitialized(e);

            Configuration configuration = null;

            var baseDir = new DirectoryInfo(Directory.GetCurrentDirectory());
#if DEBUG
            baseDir = baseDir.Parent;
            baseDir = baseDir.Parent;
            baseDir = baseDir.Parent;
            baseDir = baseDir.Parent;
            baseDir = new DirectoryInfo(System.IO.Path.Join(baseDir.FullName, "AEther"));
#endif
            var configurationFile = new FileInfo(System.IO.Path.Combine(baseDir.FullName, "config.xml"));

            if (configurationFile.Exists)
            {
                configuration = Configuration.ReadFromFile(configurationFile.FullName);
            }

            var watcher = new FileSystemWatcher(configurationFile.DirectoryName, configurationFile.Name);
            watcher.NotifyFilter = NotifyFilters.Attributes
                            | NotifyFilters.CreationTime
                            | NotifyFilters.DirectoryName
                            | NotifyFilters.FileName
                            | NotifyFilters.LastAccess
                            | NotifyFilters.LastWrite
                            | NotifyFilters.Security
                            | NotifyFilters.Size;
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;

            if (configuration == null)
            {
                configuration = new Configuration();
                configuration.WriteToFile(configurationFile.FullName);
            }

            Start(configuration);

        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            Stop();
            Audio?.Dispose();
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            for (int c = 0; c < 2; ++c)
            {
                Histograms[c].Update(HistogramBitmaps[c]);
            }
            Title = "Latency: " + (int)Session.Latency + " ms, Events: " + Session.EventCount;
            InvalidateVisual();
        }

    }
}
