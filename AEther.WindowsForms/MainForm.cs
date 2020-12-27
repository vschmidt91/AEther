using System;
using System.Collections.Generic;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave.SampleProviders;
using SharpDX.Windows;
using System.Collections.Concurrent;

namespace AEther.WindowsForms
{
    public partial class MainForm : RenderForm
    {

        Graphics? Graphics;

        Task Task = Task.CompletedTask;
        readonly CancellationTokenSource Cancel = new();
        bool Fullscreen;

        private readonly PropertyGrid ConfigurationPanel;

        public MainForm()
            : base("")
        {

            ConfigurationPanel = new PropertyGrid
            {
                Dock = DockStyle.Left,
                Visible = false,
                SelectedObject = new Configuration(),
            };
            Controls.Add(ConfigurationPanel);
            ConfigurationPanel.PropertyValueChanged += ConfigurationPanel_PropertyValueChanged;
            KeyPreview = true;

        }

        private async void ConfigurationPanel_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            await StopAsync();
            Task = RunAsync();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Graphics?.Resize(ClientSize.Width, ClientSize.Height);
        }
        protected override void WndProc(ref Message message)
        {
            base.WndProc(ref message);
            switch (message.Msg)
            {
                case 0x0112:
                    switch (message.WParam.ToInt32())
                    {
                        case 0xF030:
                        case 0xF120:
                            Graphics?.Resize(ClientSize.Width, ClientSize.Height);
                            break;
                    }
                    break;
            }
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            await StopAsync();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Task = RunAsync();
        }

        protected async override void OnKeyDown(KeyEventArgs e)
        {
            switch(e.KeyCode)
            {
                case Keys.Space:
                    ToggleConfiguration();
                    break;
                case Keys.F11:
                    ToggleFullscreen();
                    break;
                case Keys.F10:
                    ToggleBorderlessFullscreen();
                    break;
                case Keys.Escape:
                    await StopAsync();
                    Dispose();
                    Close();
                    break;
            }
            base.OnKeyDown(e);
        }

        void ToggleConfiguration()
        {
            ConfigurationPanel.Visible ^= true;
        }

        async Task RunAsync()
        {

            var configuration = (Configuration)ConfigurationPanel.SelectedObject;

            try
            {
                Invoke(new MethodInvoker(() =>
                {
                    Graphics = new Graphics(configuration.Domain, Handle, configuration.TimeResolution, configuration.UseFloatTextures, configuration.UseMapping);
                    Graphics.Resize(ClientSize.Width, ClientSize.Height);
                    //var state = new FluidState(Graphics);
                    var state = new ShaderState(Graphics, Graphics.Shader["histogram.fx"]);
                    //var state = new ShaderState(Graphics, Graphics.Shader["spectrum.fx"]);
                    //var state = new IFSState(Graphics);
                    Graphics.OnRender += (obj, evt) => state.Render();
                    //Graphics.State = new SceneState(Graphics);
                }));
            }
            catch (InvalidOperationException) { }

            var sampleSource = new CSCore.Listener();

            var capacity = configuration.ChannelCapacity;
            var session = new Session(configuration, sampleSource.Format);
            var chain = session.CreateBatcher()
                .Buffer(capacity)
                .Chain(session.CreateDFT())
                .Buffer(capacity)
                .Chain(session.CreateSplitter())
                .Buffer(capacity);

            var latencyWindow = configuration.TimeResolution;
            var latencyMix = 2.0 / (1 + latencyWindow);
            var latencyMean = 0.0;
            var latencyDev = 1.0;

            var latencyCounter = 0;
            var inputs = sampleSource.ReadAllAsync(Cancel.Token);
            await foreach (var output in chain(inputs))
            {
                var latency = (DateTime.Now - output.Time).TotalMilliseconds;
                latencyMean += latencyMix * (latency - latencyMean);
                latencyDev += latencyMix * (Math.Abs(latency - latencyMean) - latencyDev);
                if(++latencyCounter == latencyWindow)
                {
                    try
                    {
                        Invoke(new MethodInvoker(() =>
                        {
                            Text = $"Latency: {Math.Round(latencyMean, 1)} \u00B1 {Math.Round(latencyDev, 1)} ms";
                        }));
                    }
                    catch (InvalidOperationException) { }
                    latencyCounter = 0;
                }
                Graphics?.ProcessInput(output);
            }

        }

        async Task StopAsync()
        {
            Cancel.Cancel();
            await (Task ?? Task.CompletedTask);
            Graphics?.Dispose();
        }

        void ToggleFullscreen()
        {
            if(Fullscreen)
            {
                Hide();
                Show();
            }
            else 
            {
                if (Graphics != null)
                {
                    Graphics.Resize(Graphics.NativeMode);
                    Graphics.IsFullscreen = true;
                }
            }
            Fullscreen ^= true;
        }

        void ToggleBorderlessFullscreen()
        {
            if (IsFullscreen)
            {
                WindowState = FormWindowState.Maximized;
                FormBorderStyle = FormBorderStyle.Sizable;
            }
            else
            {
                WindowState = FormWindowState.Normal;
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            IsFullscreen ^= true;
            Activate();
        }


        public void Render()
        {
            Graphics?.Render();
        }

    }
}
