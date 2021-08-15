using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
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
        public event EventHandler<string>? OnShaderChange;

        public Texture2D BackBuffer { get; protected set; }
        public readonly ModeDescription NativeMode;
        public readonly Dictionary<string, SharpDX.Direct3D11.Buffer> ShaderConstants = new();

        int IndexCount;
        SharpDX.Direct3D11.Texture2D BackBufferResource;
        readonly ShaderManager Shaders;
        readonly FileSystemWatcher ShaderWatcher;
        readonly Device Device;
        readonly DeviceDebug? Debug;
        readonly SwapChain Chain;
        readonly Model Quad;
        DeviceContext Context => Device.ImmediateContext;

        public Graphics(IntPtr handle)
        {

            var format = Format.R8G8B8A8_UNorm;
            var modeFlags = DisplayModeEnumerationFlags.Scaling;
            using var dxgiFactory = new Factory1();
            using var dxgiAdapater = dxgiFactory.GetAdapter(0);
            using var output = dxgiAdapater.GetOutput(0);
            NativeMode = output.GetDisplayModeList(format, modeFlags)
                .OrderByDescending(m => m.Width * m.Height)
                .ThenByDescending(m => m.RefreshRate.Numerator / (float)m.RefreshRate.Denominator)
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
                new[] { FeatureLevel.Level_11_0 },
                desc,
                out Device,
                out Chain);

            using var factory = Chain.GetParent<Factory>();
            factory.MakeWindowAssociation(handle, WindowAssociationFlags.None);

            if (Device.CreationFlags.HasFlag(DeviceCreationFlags.Debug))
            {
                Debug = new DeviceDebug(Device);
            }

            BackBufferResource = Chain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0);
            BackBuffer = new Texture2D(BackBufferResource);
            (Shaders, ShaderWatcher) = CreateShaderManager();
            ShaderWatcher.Changed += ShaderWatcher_Changed;
            Quad = new Model(Device, Mesh.CreateGrid(2, 2));

            var c = new GraphicsCapabilities(Device);

        }

        private void ShaderWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            OnShaderChange?.Invoke(sender, e.FullPath);
        }

        public bool IsFullscreen
        {
            get
            {
                Chain.GetFullscreenState(out var state, out _);
                return state;
            }
            set
            {
                Chain.SetFullscreenState(value, null);
            }

        }

        public Shader LoadShader(string key, params ShaderMacro[] macros)
        {
            var bytecode = Shaders.Load(key, macros);
            var shader = new Shader(Device, bytecode);
            foreach (var kvp in ShaderConstants)
            {
                shader.ConstantBuffers[kvp.Key].SetConstantBuffer(kvp.Value);
            }
            return shader;
        }

        public Model CreateModel(Mesh mesh)
            => new(Device, mesh);

        public void SetMode(ModeDescription targetMode)
        {

            using var dxgiFactory = new Factory1();
            using var dxgiAdapater = dxgiFactory.GetAdapter(0);
            using var output = dxgiAdapater.GetOutput(0);
            output.GetClosestMatchingMode(null, targetMode, out var mode);

            if (!Chain.Description.ModeDescription.Equals(mode))
            {
                BackBufferResource.Dispose();
                BackBuffer.Dispose();
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
            Quad.Dispose();
            Chain.Dispose();
            Context.ClearState();
            Context.Flush();
            Context.Dispose();
            Device.Dispose();

            Debug?.ReportLiveDeviceObjects(ReportingLevel.Detail);
            Debug?.Dispose();

        }

        public ConstantBuffer<T> CreateConstantBuffer<T>()
            where T : struct
            => new(Device);

        public ComputeBuffer CreateComputeBuffer(int stride, int size, bool cpuWrite)
            => new(Device, stride, size, cpuWrite);

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

        public DepthBuffer CreateDepthBuffer(int width, int height, Format format)
        {
            var texture = new SharpDX.Direct3D11.Texture2D(Device, new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.DepthStencil | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = format,
                Width = width,
                Height = height,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
            });
            return new DepthBuffer(texture);
        }

        public TextureCube CreateTextureCube(int width, int height, Format format)
        {
            var texture = new SharpDX.Direct3D11.Texture2D(Device, new()
            {
                ArraySize = 6,
                BindFlags = BindFlags.ShaderResource | BindFlags.DepthStencil,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = format,
                Width = width,
                Height = height,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.TextureCube,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
            });
            return new TextureCube(texture);
        }

        (ShaderManager, FileSystemWatcher) CreateShaderManager()
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
            var shader = new ShaderManager(this, shaderPath);
            var watcher = new FileSystemWatcher(shaderPath)
            {
                NotifyFilter = NotifyFilters.LastWrite,
                EnableRaisingEvents = true,
            };
            return (shader, watcher);
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

        RenderTargetView[] RTViews = Array.Empty<RenderTargetView>();

        public void SetRenderTarget(Texture2D depthBuffer)
        {
            Context.Rasterizer.SetViewport(0, 0, depthBuffer.Width, depthBuffer.Height);
            Context.OutputMerger.SetRenderTargets(depthBuffer.DSView);
        }

        public void SetRenderTarget(Texture2D? depthBuffer, Texture2D renderTarget)
        {
            Context.Rasterizer.SetViewport(0, 0, renderTarget.Width, renderTarget.Height);
            Context.OutputMerger.SetRenderTargets(depthBuffer?.DSView, renderTarget.RTView);
        }

        public void SetRenderTargets(Texture2D? depthBuffer, Texture2D[] renderTargets)
        {
            if(RTViews.Length < renderTargets.Length)
            {
                RTViews = new RenderTargetView[renderTargets.Length];
            }
            Array.Clear(RTViews, 0, RTViews.Length);
            for(var i = 0; i < renderTargets.Length; ++i)
            {
                RTViews[i] = renderTargets[i].RTView;
            }
            if (depthBuffer is Texture2D db)
            {
                Context.Rasterizer.SetViewport(0, 0, db.Width, db.Height);
            }
            else if (renderTargets.FirstOrDefault() is Texture2D rt)
            {
                Context.Rasterizer.SetViewport(0, 0, rt.Width, rt.Height);
            }
            Context.OutputMerger.SetRenderTargets(depthBuffer?.DSView, RTViews);
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

            UnsetResources(shader);

        }

        public void Draw(Shader shader, int instanceCount = 0, int techniqueIndex = 0, int indexOffset = 0, int vertexOffset = 0, int instanceOffset = 0)
        {

            var technique = shader[techniqueIndex];
            for(int passIndex = 0; passIndex < technique.PassCount; ++passIndex)
            {

                var pass = technique[passIndex];
                pass.Apply(Context);

                if (0 < instanceCount)
                {
                    Context.DrawIndexedInstanced(IndexCount, instanceCount, indexOffset, vertexOffset, instanceOffset);
                }
                else
                {
                    Context.DrawIndexed(IndexCount, indexOffset, vertexOffset);
                }

                UnsetResources(shader);

            }
        }

        void UnsetResources(Shader shader)
        {

            for (int i = 0; i < shader.ShaderResources.Count; ++i)
            {
                Context.ComputeShader.SetShaderResource(i, null);
                Context.PixelShader.SetShaderResource(i, null);
                Context.VertexShader.SetShaderResource(i, null);
                Context.GeometryShader.SetShaderResource(i, null);
            }

            for (int i = 0; i < shader.UnorderedAccesses.Count; ++i)
            {
                Context.ComputeShader.SetUnorderedAccessView(i, null);
            }
        }


    }
}