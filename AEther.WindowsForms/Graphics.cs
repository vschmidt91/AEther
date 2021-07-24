using System;
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
            Shaders = CreateShaderManager();
            Shaders.FileChanged += Shaders_FileChanged;
            Quad = new Model(Device, Mesh.CreateGrid(2, 2));

            var c = new GraphicsCapabilities(Device);

        }

        private void Shaders_FileChanged(object? sender, FileSystemEventArgs e)
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

        public MetaShader CreateMetaShader(string key, params string[] defines)
        {
            var shader = new MetaShader(this, key, defines);
            return shader;
        }

        public Shader CreateShader(string key, params ShaderMacro[] macros)
        {
            var bytecode = Shaders.Compile(key, macros);
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

        public void RenderFrame()
        {

        }

        public void SetFullscreenTarget(Texture2D target, DepthStencilView? dsv = null)
        {
            SetModel(null);
            Context.Rasterizer.SetViewport(target.ViewPort);
            Context.OutputMerger.SetRenderTargets(dsv, target.RTView);
        }

        public void SetViewport(Viewport viewPort)
        {
            Context.Rasterizer.SetViewport(viewPort);
        }

        public void SetRenderTargets(DepthStencilView? depthBuffer, params Texture2D[] renderTargets)
        {
            if(depthBuffer == null)
            {
                SetViewport(renderTargets.FirstOrDefault()?.ViewPort ?? default);
            }
            else
            {
                var resource = depthBuffer.ResourceAs<SharpDX.Direct3D11.Texture2D>();
                SetViewport(new Viewport(0, 0, resource.Description.Width, resource.Description.Height));
            }
            Context.OutputMerger.SetRenderTargets(depthBuffer, renderTargets.Select(t => t.RTView).ToArray());
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