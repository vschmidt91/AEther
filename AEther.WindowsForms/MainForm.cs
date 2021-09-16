using AEther.DMX;
using AEther.NAudio;
using System;
using System.Buffers;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AEther.WindowsForms
{
    public partial class MainForm : Form
    {

        public const string LoopbackEntry = "Loopback";

        public readonly Graphics Graphics;
        Session? Session;

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
            SampleSource source = Input.SelectedItem?.ToString() switch
            {
                LoopbackEntry => new Loopback(),
                string s => new Recorder(s),
                _ => throw new Exception(),
            };
            Session = new(this, source, States, options, dmxOptions);
            Session.Start();

        }

        public async Task StopAsync()
        {
            if (Session is Session session)
            {
                await session.StopAsync();
            }
        }

        public void Render()
        {
            if (Session is Session session)
            {
                session.Render();
            }
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