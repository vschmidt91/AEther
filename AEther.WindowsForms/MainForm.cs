using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

using AEther.DMX;
using AEther.CSCore;

namespace AEther.WindowsForms
{
    public partial class MainForm : Form
    {

        public const string LoopbackEntry = "Loopback";

        readonly Graphics Graphics;
        readonly AutoResetEvent RenderEvent = new(false);
        readonly StatisticsModule Statistics;

        SampleSource? Source = null;
        Analyzer? Analyzer = null;
        DMXModule? DMX = null;
        GraphicsModule? GraphicsModule = null;

        public MainForm()
        {
            InitializeComponent();
            Graphics = new Graphics(Handle);
            Graphics.OnShaderChange += Graphics_OnShaderChange;
            Input.Items.Clear();
            Input.Items.Add(LoopbackEntry);
            Input.Items.AddRange(Recorder.GetAvailableDeviceNames().ToArray());
            Input.SelectedIndex = 0;
            AnalyzerOptions.SelectedObject = new AnalyzerOptions();
            DMXOptions.SelectedObject = new DMXOptions();
            Statistics = new StatisticsModule(this);
        }

        private async void Graphics_OnShaderChange(object? sender, string e)
        {
            await ResetAsync();
        }

        void Start()
        {
            if (AnalyzerOptions.SelectedObject is not AnalyzerOptions options)
            {
                throw new InvalidCastException();
            }
            Source = Input.SelectedItem?.ToString() switch
            {
                LoopbackEntry => new Loopback(),
                string s => new Recorder(s),
                _ => throw new Exception(),
            };
            Analyzer = new(Source.Format, options);

            if (DMXOptions.SelectedObject is DMXOptions dmxOptions)
            {
                DMX = new DMXModule(options.Domain, dmxOptions);
            }

            GraphicsModule = new GraphicsModule(Graphics, States, Source.Format.ChannelCount, options.Domain.Length, options.TimeResolution);

            Analyzer.SamplesAnalyzed += Analyzer_SamplesAnalyzed;
            Source.DataAvailable += Source_DataAvailable;
            KeyPress += MainForm_KeyPress;
            Source.Start();

        }

        private void MainForm_KeyPress(object? sender, KeyPressEventArgs evt)
        {
            GraphicsModule?.ProcessKeyPress(evt);
        }

        private void Source_DataAvailable(object? sender, ReadOnlyMemory<byte> evt)
        {
            Analyzer?.PostSamples(evt);
        }

        private void Analyzer_SamplesAnalyzed(object? sender, SampleEvent<double> evt)
        {
            Statistics?.Process(evt);
            DMX?.Process(evt);
            GraphicsModule?.Process(evt);
        }

        public async Task StopAsync()
        {
            if (Interlocked.Exchange(ref Source, null) is not SampleSource source)
            {
                return;
            }
            if (Interlocked.Exchange(ref Analyzer, null) is not Analyzer analyzer)
            {
                return;
            }
            Interlocked.Exchange(ref GraphicsModule, null)?.Dispose();
            Interlocked.Exchange(ref DMX, null)?.Dispose();
            RenderEvent.WaitOne();
            analyzer.Stop();
            try
            {
                await analyzer.Completion;
            }
            catch (OperationCanceledException)
            { }
            source.Stop();
        }

        public void Render()
        {
            RenderEvent.Reset();
            Statistics?.Update();
            GraphicsModule?.Render();
            RenderEvent.Set();
        }

        void ToggleFullscreen()
        {
            if (Graphics.IsFullscreen)
            {
                Graphics.IsFullscreen = false;
                Graphics.SetMode(Graphics.NativeMode with
                {
                    Width = ClientSize.Width,
                    Height = ClientSize.Height,
                });
            }
            else
            {
                Graphics.IsFullscreen = true;
                Graphics.SetMode(Graphics.NativeMode);
            }
        }

        async Task ResetAsync()
        {
            if (!Created)
            {
                return;
            }
            await StopAsync();
            Start();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Graphics.SetMode(Graphics.NativeMode with
            {
                Width = ClientSize.Width,
                Height = ClientSize.Height,
            });
        }

        protected async override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            await StopAsync();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            Graphics.SetMode(Graphics.NativeMode with
            {
                Width = ClientSize.Width,
                Height = ClientSize.Height,
            });
            Start();
        }

        protected async override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Space:
                    Panel.Visible ^= true;
                    break;
                case Keys.F5:
                    await StopAsync();
                    Start();
                    break;
                case Keys.F11:
                    ToggleFullscreen();
                    break;
                case Keys.Escape:
                    Close();
                    break;
            }
        }

        private async void Options_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            await ResetAsync();
        }

        private async void Input_SelectedValueChanged(object? sender, EventArgs e)
        {
            await ResetAsync();
        }

        private async void DMXOptions_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            await ResetAsync();
        }

    }
}