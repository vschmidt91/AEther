﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using Vector4 = System.Numerics.Vector4;

namespace AEther.WindowsForms
{
    public class Graphics
    {
        public class FrameHandle : IDisposable
        {

            public EventHandler? OnDisposed;

            public void Dispose()
            {
                OnDisposed?.Invoke(this, EventArgs.Empty);
            }

        }

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
                    Chain.GetFullscreenState(out var state, out var _);
                    return state;
                }
            }
            set
            {
                Chain?.SetFullscreenState(value, null);
            }

        }

        public ModeDescription NativeMode { get; protected set; }
        public Texture2D BackBuffer { get; protected set; }
        public ShaderManager Shaders { get; protected set; }
        public DeviceContext Context => Device.ImmediateContext;

        public readonly Device Device;
        public readonly ConstantBuffer<FrameConstants> FrameConstants;

        readonly DeviceDebug? Debug;
        readonly SwapChain Chain;
        readonly Model Quad;

        public Graphics(IntPtr handle)
        {

            (Device, Chain) = CreateDevice(handle);

            using (var factory = Chain.GetParent<Factory>())
            {
                factory.MakeWindowAssociation(handle, WindowAssociationFlags.IgnoreAll);
            }

            if (Device.CreationFlags.HasFlag(DeviceCreationFlags.Debug))
            {
                Debug = new DeviceDebug(Device);
            }


            BackBuffer = new Texture2D(Chain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0));
            FrameConstants = new ConstantBuffer<FrameConstants>(Device);
            Shaders = CreateShader();
            Quad = new Model(Device, new Grid(2, 2));

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

                Debug?.ReportLiveDeviceObjects(ReportingLevel.Detail);

                Context.ClearState();
                Context.Flush();
                Chain.ResizeBuffers(Chain.Description.BufferCount, mode.Width, mode.Height, mode.Format, SwapChainFlags.AllowModeSwitch);
                Chain.ResizeTarget(ref mode);

                BackBuffer = new Texture2D(Chain.GetBackBuffer<SharpDX.Direct3D11.Texture2D>(0));

            }


        }

        public void Dispose()
        {

            Context.ClearState();
            Context.Flush();

            Shaders.Dispose();
            FrameConstants.Dispose();
            Quad.Dispose();
            BackBuffer.Dispose();

            Chain.Dispose();
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
             => new(new SharpDX.Direct3D11.Texture2D(Device, new Texture2DDescription
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
             }));

        (Device, SwapChain) CreateDevice(IntPtr handle, Format? format = default)
        {

            var modeFormat = format ?? Format.R8G8B8A8_UNorm;
            var modeFlags = DisplayModeEnumerationFlags.Scaling;
            using var dxgiFactory = new Factory1();
            using var dxgiAdapater = dxgiFactory.GetAdapter(0);
            using var output = dxgiAdapater.GetOutput(0);
            var modes = output.GetDisplayModeList(modeFormat, modeFlags);
            var nativeWidth = modes.Max(m => m.Width);
            var nativeHeight = modes.Max(m => m.Height);
            var nativeRate = modes.Max(m => m.RefreshRate.Numerator / m.RefreshRate.Denominator);
            var targetMode = new ModeDescription(nativeWidth, nativeHeight, new Rational(nativeRate, 1), modeFormat);
            output.GetClosestMatchingMode(null, targetMode, out var nativeMode);
            NativeMode = nativeMode;

            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = NativeMode,
                IsWindowed = true,
                OutputHandle = handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
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
                out var device,
                out var chain);

            return (device, chain);

        }

        ShaderManager CreateShader()
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
            ShaderManager shader = null;
            try
            {
                shader = new ShaderManager(this, shaderPath, true);
            }
            catch (CompilationException exc)
            {
#if DEBUG
                MessageBox.Show(null, exc.Message, string.Empty);
#else
                System.Diagnostics.Debug.WriteLine(exc.Message);
#endif
            }
            return shader;
        }

        public void SetModel(Model? model = default)
        {
            model ??= Quad;
            if (Context != default && model != default)
            {
                Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
                Context.InputAssembler.SetVertexBuffers(0, model.VertexBufferBinding);
                Context.InputAssembler.SetIndexBuffer(model.IndexBuffer, Format.R32_UInt, 0);
            }
        }

        public FrameHandle RenderFrame()
        {

            var dt = .01f;
            var t = (float)DateTime.Now.TimeOfDay.TotalSeconds;

            FrameConstants.Value.AspectRatio = BackBuffer.Width / (float)BackBuffer.Height;
            FrameConstants.Value.Time.X = t;
            FrameConstants.Value.Time.Y = dt;
            FrameConstants.Update(Context);

            var frame = new FrameHandle();
            frame.OnDisposed += (sender, evt) =>
            {
                Chain.TryPresent(1, PresentFlags.None);
            };
            return frame;

        }

        public void SetFullscreenTarget(Texture2D target, DepthStencilView? dsv = null)
        {
            SetModel(null);
            Context.Rasterizer.SetViewport(target.ViewPort);
            Context.OutputMerger.SetRenderTargets(dsv, target.GetRenderTargetView());
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

        public void Draw(Shader shader, int? numInstances = default, int? techniqueIndex = default, int? indexOffset = default, int? vertexOffset = default, int? instanceOffset = default)
        {

            Context.InputAssembler.GetIndexBuffer(out var indexBuffer, out var _, out var indexBufferOffset);
            var indexOffsetValue = indexOffset ?? indexBufferOffset;
            var indexCount = indexBuffer.Description.SizeInBytes / sizeof(uint) - indexOffsetValue;

            var technique = shader[techniqueIndex ?? 0];
            for(int passIndex = 0; passIndex < technique.PassCount; ++passIndex)
            {

                var pass = technique[passIndex];
                var inputLayout = pass.GetInputLayout(Device);
                Context.InputAssembler.InputLayout = inputLayout;
                pass.Apply(Context);

                if (numInstances.HasValue)
                {
                    Context.DrawIndexedInstanced(indexCount, numInstances.Value, indexOffsetValue, vertexOffset ?? 0, instanceOffset ?? 0);
                }
                else
                {
                    Context.DrawIndexed(indexCount, indexOffsetValue, vertexOffset ?? 0);
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