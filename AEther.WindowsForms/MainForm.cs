using System;
using System.Collections.Generic;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Linq;

using AEther.DMX;
using AEther.NAudio;

namespace AEther.WindowsForms
{
    public partial class MainForm : Form
    {

        public const string LoopbackEntry = "Loopback";

        readonly Graphics Graphics;

        Session? Session = null;
        AutoResetEvent RenderEvent = new(false);

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
            if (DMXOptions.SelectedObject is not DMXOptions dmxOptions)
            {
                throw new InvalidCastException();
            }
            var sampleSourceName = Input.SelectedItem?.ToString() ?? string.Empty;
            SampleSource sampleSource = sampleSourceName is LoopbackEntry
                ? new Loopback()
                : new Recorder(sampleSourceName);
            Session = new Session(sampleSource, options);
            Session.Modules.Add(new StatisticsModule(this));
            Session.Modules.Add(new DMXModule(options.Domain, dmxOptions));
            Session.Modules.Add(new GraphicsModule(Graphics, States, Session.Source.Format.ChannelCount, options.Domain.Length, options.TimeResolution));
            Session.Start();
        }

        public async Task StopAsync()
        {
            if (Interlocked.Exchange(ref Session, null) is Session session)
            {
                RenderEvent.WaitOne();
                await session.StopAsync();
                var modules = session.Modules.ToArray();
                session.Modules.Clear();
                foreach (var module in modules)
                {
                    module.Dispose();
                }
            }
        }

        public void Render()
        {
            RenderEvent.Reset();
            Session?.Render();
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
            base.OnKeyDown(e);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (Session is Session session)
            {
                foreach (var module in session.Modules.OfType<GraphicsModule>())
                {
                    module.ProcessKeyPress(e);
                }
            }
            base.OnKeyPress(e);
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