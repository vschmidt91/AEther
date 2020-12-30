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
using System.Collections.Concurrent;
using System.Linq;

using AEther.CSCore;

namespace AEther.WindowsForms
{
    public partial class MainForm : Form
    {

        const string WASAPIEntry = "WASAPI";

        SampleSource? SoundInput;
        Graphics? Graphics;
        Task? Task;
        CancellationTokenSource? Cancel;

        bool Fullscreen;

        public MainForm()
        {
            InitializeComponent();
        }

        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);
            Graphics?.Resize(ClientSize.Width, ClientSize.Height);
        }

        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            await StopAsync();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            var audioDevices = Recorder.GetAvailableDevices();
            lbInput.Items.Clear();
            lbInput.Items.AddRange(audioDevices.ToArray());
            lbInput.Items.Add(WASAPIEntry);
            lbInput.SelectedItem = WASAPIEntry;
            pgConfiguration.SelectedObject = new Configuration();
            Task = RunAsync();
        }

        protected async override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Space:
                    TogglePanel();
                    break;
                case Keys.F11:
                    ToggleFullscreen();
                    break;
                case Keys.Escape:
                    await StopAsync();
                    Dispose();
                    Close();
                    break;
                default:
                    base.OnKeyDown(e);
                    break;
            }
        }

        void TogglePanel()
        {
            tlpPanel.Visible ^= true;
        }

        async Task RunAsync()
        {

            var configuration = (Configuration)pgConfiguration.SelectedObject;

            try
            {
                Invoke(new MethodInvoker(() =>
                {
                    Graphics = new Graphics(configuration.Domain, Handle, configuration.TimeResolution, configuration.UseFloatTextures, configuration.UseMapping);
                    Graphics.Resize(ClientSize.Width, ClientSize.Height);
                    var stateIndex = lbState.SelectedIndex;
                    lbState.Items.Clear();
                    lbState.Items.AddRange(new object[]
                    {
                        new ShaderState(Graphics, Graphics.Shader["histogram.fx"]),
                        new ShaderState(Graphics, Graphics.Shader["spectrum.fx"]),
                        new FluidState(Graphics),
                        new IFSState(Graphics),
                        //new SceneState(Graphics),
                    });
                    if(stateIndex == -1)
                    {
                        lbState.SelectedIndex = 0;
                    }
                    else
                    {
                        lbState.SelectedIndex = stateIndex;
                    }
                    Graphics.OnRender += Graphics_OnRender;
                }));
            }
            catch (InvalidOperationException) { }

            var deviceName = lbInput.SelectedItem as string;

            Input input;
            if(deviceName == WASAPIEntry)
            {
                input = new WASAPI();
            }
            else
            {
                input = new Recorder(lbInput.SelectedIndex);
                input.Playback();
            }
            SoundInput = input;


            var capacity = configuration.ChannelCapacity;
            var session = new Session(configuration, SoundInput.Format);
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

            Cancel = new CancellationTokenSource();
            var inputs = SoundInput.ReadAllAsync(Cancel.Token);
            await foreach (var output in chain(inputs))
            {
                var latency = (DateTime.Now - output.Time).TotalMilliseconds;
                latencyMean += latencyMix * (latency - latencyMean);
                latencyDev += latencyMix * (Math.Abs(latency - latencyMean) - latencyDev);
                if (++latencyCounter == latencyWindow)
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

        private void Graphics_OnRender(object? sender, EventArgs e)
        {
            var state = lbState.SelectedItem as GraphicsState;
            state?.Render();
        }

        async Task StopAsync()
        {
            Cancel?.Cancel();
            await (Task ?? Task.CompletedTask);
            SoundInput?.Dispose();
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


        public void Render()
        {
            Graphics?.Render();
        }

        private async void pgConfiguration_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            await StopAsync();
            Task = RunAsync();
        }

        private async void lbInput_SelectedValueChanged(object sender, EventArgs e)
        {
            if (Task == null)
                return;
            await StopAsync();
            Task = RunAsync();
        }
    }
}
