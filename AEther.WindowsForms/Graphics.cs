using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Diagnostics;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;

namespace AEther.WindowsForms
{
    public class Graphics
    {

        public event EventHandler<ModeDescription>? OnModeChange;

        public bool IsFullscreen
        {
            get
            {
                if(Chain == null)
                {
                    return false;
                }
                else
                {
                    Chain.GetFullscreenState(out var state, out _);
                    return state;
                }
            }
            set
            {
                Chain?.SetFullscreenState(value, null);
            }

        }

        public readonly ModeDescription NativeMode;

        SharpDX.Direct3D11.Texture2D BackBufferResource;
        public Texture2D BackBuffer { get; protected set; }

        public DeviceContext Context => Device.ImmediateContext;

        public readonly Device Device;
        public readonly ConstantBuffer<FrameConstants> FrameConstants;
        public readonly ShaderManager Shaders;
        readonly DeviceDebug? Debug;
        readonly SwapChain Chain;
        readonly Model Quad;

        int IndexCount;

        public Graphics(IntPtr handle)
        {

            var format = Format.R8G8B8A8_UNorm;
            var modeFlags = DisplayModeEnumerationFlags.Scaling;
            using var dxgiFactory = new Factory1();
            using var dxgiAdapater = dxgiFactory.GetAdapter(0);
            using var output = dxgiAdapater.GetOutput(0);
            NativeMode = output.GetDisplayModeList(format, modeFlags)
                .OrderByDescending(m => m.Width)
                .ThenByDescending(m => m.Height)
                .ThenByDescending(m => m.RefreshRate.Numerator / m.RefreshRate.Denominator)
                .First();

            var desc = new SwapChainDescription()
            {
                BufferCount = 2,
                ModeDescription = NativeMode,
                IsWindowed = true,
                OutputHandle = handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.FlipDiscard,
                Usage = Usage.RenderTargetOutput,
                Flags = SwapChainFlags.AllowModeSwitch,
            };

            DeviceCreationFlags deviceFlags = DeviceCreationFlags.None;
#if DEBUG
            deviceFlags |= DeviceCreationFlags.Debug;
#endif

            Device.CreateWithSwapChain(
                DriverType.Hardware,
                deviceFlags,
                new[] { FeatureLevel.Level_10_0 },
                desc,
                out Device,
                out Chain);

            using (var factory = Chain.GetParent<Factory>())
            {
                factory.MakeWindowAssociation(handle, WindowAssociationFlags.IgnoreAll);
            }

            if (Device.CreationFlags.HasFlag(DeviceCreationFlags.Debug))
            {
                Debug = new DeviceDebug(Device);
            }

            BackBufferResource = Chain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0);
            BackBuffer = new Texture2D(BackBufferResource);
            FrameConstants = new ConstantBuffer<FrameConstants>(Device);
            Shaders = CreateShaderManager();
            Quad = new Model(Device, Mesh.CreateGrid(2, 2));

            var c = new GraphicsCapabilities(Device);

        }

        public void Resize(int width, int height, Rational? refreshRate = default, DisplayModeScaling? scaling = default, DisplayModeScanlineOrder? scanlineOrdering = default)
        {
            var oldMode = Chain.Description.ModeDescription;
            var mode = new ModeDescription
            {
                Width = width,
                Height = height,
                Format = Chain.Description.ModeDescription.Format,
                RefreshRate = refreshRate ?? oldMode.RefreshRate,
                Scaling = scaling ?? oldMode.Scaling,
                ScanlineOrdering = scanlineOrdering ?? oldMode.ScanlineOrdering,
            };
            SetMode(mode);
        }

        public void Resize(ModeDescription targetMode)
        {

            using var dxgiFactory = new Factory1();
            using var dxgiAdapater = dxgiFactory.GetAdapter(0);
            using var output = dxgiAdapater.GetOutput(0);
            output.GetClosestMatchingMode(null, targetMode, out var newMode);

            SetMode(newMode);

        }

        public void SetMode(ModeDescription mode)
        {

            if (!Chain.Description.ModeDescription.Equals(mode))
            {

                BackBuffer.Dispose();
                BackBufferResource.Dispose();

                //Debug?.ReportLiveDeviceObjects(ReportingLevel.Detail);

                Context.ClearState();
                Context.Flush();

                Chain.ResizeBuffers(Chain.Description.BufferCount, mode.Width, mode.Height, mode.Format, SwapChainFlags.AllowModeSwitch);
                Chain.ResizeTarget(ref mode);

                BackBufferResource = Chain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0);
                BackBuffer = new Texture2D(BackBufferResource);

                OnModeChange?.Invoke(this, mode);

            }


        }

        public void Dispose()
        {

            Shaders.Dispose();
            FrameConstants.Dispose();
            Quad.Dispose();
            //BackBuffer.Dispose();

            Chain.Dispose();


            Context.ClearState();
            Context.Flush();
            Context.Dispose();
            Device.Dispose();

            Debug?.ReportLiveDeviceObjects(ReportingLevel.Detail);
            Debug?.Dispose();

        }

        public Texture2D CreateTexture(int width, int height, Format format,
            BindFlags bindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
            CpuAccessFlags accessFlags = CpuAccessFlags.None,
            ResourceOptionFlags optionFlags = ResourceOptionFlags.None,
            ResourceUsage usage = ResourceUsage.Default)
             => CreateTexture(new Texture2DDescription
             {
                 ArraySize = 1,
                 BindFlags = bindFlags,
                 CpuAccessFlags = accessFlags,
                 Format = format,
                 Width = width,
                 Height = height,
                 MipLevels = 1,
                 OptionFlags = optionFlags,
                 SampleDescription = new SampleDescription(1, 0),
                 Usage = usage,
             });

        public Texture2D CreateTexture(Texture2DDescription description)
            => new(new SharpDX.Direct3D11.Texture2D(Device, description));

        ShaderManager CreateShaderManager()
        {

            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var shaderDir = Path.GetDirectoryName(assemblyPath);

#if DEBUG
            shaderDir = Path.Join(shaderDir, "..", "..", "..");
            //shaderDir = Directory.GetParent(shaderDir)?.FullName ?? string.Empty;
            //shaderDir = Directory.GetParent(shaderDir)?.FullName ?? string.Empty;
            //shaderDir = Directory.GetParent(shaderDir)?.FullName ?? string.Empty;
#endif
            var shaderPath = Path.Join(shaderDir, "data", "fx");
            return new ShaderManager(this, shaderPath, true);
        }

        public void SetModel(Model? model = default)
        {
            model ??= Quad;
            if (Context != default && model != default)
            {
                Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                Context.InputAssembler.SetVertexBuffers(0, model.VertexBufferBinding);
                Context.InputAssembler.SetIndexBuffer(model.IndexBuffer, Format.R32_UInt, 0);
                IndexCount = model.IndexBuffer.Description.SizeInBytes / sizeof(uint);
            }
        }

        public void Present()
        {
            Chain.TryPresent(1, PresentFlags.DoNotWait);
        }

        public Shader CreateShader(string key, ShaderMacro[]? macros = null)
        {
            var bytecode = Shaders.Compile(key, macros);
            var shader = new Shader(Device, bytecode);
            shader.ConstantBuffers[0].SetConstantBuffer(FrameConstants.Buffer);
            return shader;
        }

        public void RenderFrame()
        {

            var dt = .01f;
            var t = (float)DateTime.Now.TimeOfDay.TotalSeconds;

            FrameConstants.Value.AspectRatio = BackBuffer.Width / (float)BackBuffer.Height;
            FrameConstants.Value.Time.X = t;
            FrameConstants.Value.Time.Y = dt;
            FrameConstants.Update(Context);

        }

        public void SetFullscreenTarget(Texture2D target, DepthStencilView? dsv = null)
        {
            SetModel(null);
            Context.Rasterizer.SetViewport(target.ViewPort);
            Context.OutputMerger.SetRenderTargets(dsv, target.RTView);
        }

        public void Compute(Shader shader, (int, int, int)? threadCount = default, int? techniqueIndex = default)
        {

            if (Context == default)
            {
                throw new Exception();
            }

            var (tx, ty, tz) = threadCount ?? (1, 1, 1);
            var technique = shader[techniqueIndex ?? 0];
            for (int passIndex = 0; passIndex < technique.PassCount; ++passIndex)
            {
                var pass = technique[passIndex];
                pass.Apply(Context);
                Context.Dispatch(tx, ty, tz);
            }

        }

        public void Draw(Shader shader, int? instanceCount = default, int? techniqueIndex = default, int? indexOffset = default, int? vertexOffset = default, int? instanceOffset = default)
        {

            var technique = shader[techniqueIndex ?? 0];
            for(int passIndex = 0; passIndex < technique.PassCount; ++passIndex)
            {

                var pass = technique[passIndex];
                pass.Apply(Context);

                if (instanceCount is int n)
                {
                    Context.DrawIndexedInstanced(IndexCount, n, indexOffset ?? 0, vertexOffset ?? 0, instanceOffset ?? 0);
                }
                else
                {
                    Context.DrawIndexed(IndexCount, indexOffset ?? 0, vertexOffset ?? 0);
                }

                for(int i = 0; i < shader.ShaderResources.Count; ++i)
                {
                    Context.PixelShader.SetShaderResource(i, null);
                    Context.VertexShader.SetShaderResource(i, null);
                }

            }
        }

    }
}