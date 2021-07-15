using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace AEther.WindowsForms
{
    public class Shader : IDisposable
    {

        public static readonly InputElement[] InputElements = new[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("NORMAL", 0, Format.R32G32B32_Float, 16, 0),
            new InputElement("TEXCOORDS", 0, Format.R32G32_Float, 32, 0),
        };

        public class ShaderTechnique : IDisposable
        {

            public ShaderPass this[int i] => Passes[i];
            public int PassCount => Passes.Length;

            readonly EffectTechnique Technique;
            readonly ShaderPass[] Passes;

            protected bool IsDisposed;

            public ShaderTechnique(SharpDX.Direct3D11.Device device, EffectTechnique technique)
            {
                Technique = technique;
                Passes = Enumerable.Range(0, Technique.Description.PassCount)
                    .Select(j => Technique.GetPassByIndex(j))
                    .Select(t => new ShaderPass(device, t))
                    .ToArray();
            }

            public void Dispose()
            {
                if(!IsDisposed)
                {
                    Technique.Dispose();
                    foreach (var pass in Passes)
                    {
                        pass.Dispose();
                    }
                    GC.SuppressFinalize(this);
                    IsDisposed = true;
                }
            }
        }

        public class ShaderPass : IDisposable
        {

            readonly EffectPass Pass;
            readonly InputLayout? InputLayout;

            protected bool IsDisposed;

            public ShaderPass(SharpDX.Direct3D11.Device device, EffectPass pass)
            {
                Pass = pass;
                try
                {
                    InputLayout = new InputLayout(device, Pass.Description.Signature, InputElements);
                }
                catch(IndexOutOfRangeException)
                { }
            }

            public void Apply(DeviceContext context, int? flags = default)
            {
                context.InputAssembler.InputLayout = InputLayout;
                if (flags.HasValue)
                {
                    Pass.Apply(context, flags.Value);
                }
                else
                {
                    Pass.Apply(context);
                }
            }

            public void Dispose()
            {
                if (!IsDisposed)
                {
                    InputLayout?.Dispose();
                    Pass.Dispose();
                    GC.SuppressFinalize(this);
                    IsDisposed = true;
                }
            }

        }

        public ShaderTechnique this[int i] => Techniques[i];
        public int TechniqueCount => Techniques.Length;

        public readonly Dictionary<string, EffectVariable> Variables;
        public readonly Dictionary<string, EffectShaderResourceVariable> ShaderResources;
        public readonly Dictionary<string, EffectDepthStencilViewVariable> DepthStencils;
        public readonly Dictionary<string, EffectRenderTargetViewVariable> RenderTargets;
        public readonly Dictionary<string, EffectUnorderedAccessViewVariable> UnorderedAccesses;
        public readonly Dictionary<string, EffectConstantBuffer> ConstantBuffers;

        readonly ShaderBytecode Bytecode;
        readonly Effect Effect;
        readonly ShaderTechnique[] Techniques;

        protected bool IsDisposed;

        public Shader(SharpDX.Direct3D11.Device device, ShaderBytecode bytecode)
        {

            Bytecode = bytecode;
            Effect = new Effect(device, bytecode);

            Variables = Enumerable.Range(0, Effect.Description.GlobalVariableCount)
                .Select(i => Effect.GetVariableByIndex(i))
                .ToDictionary(v => v.Description.Name, v => v);

            static Dictionary<string, T> filter<T>(Dictionary<string, EffectVariable> d, Func<EffectVariable, T> f)
                => d
                .ToDictionary(v => v.Key, v => f(v.Value))
                .Where(v => v.Value != null)
                .ToDictionary(v => v.Key, v => v.Value);

            ShaderResources = filter(Variables, v => v.AsShaderResource());
            DepthStencils = filter(Variables, v => v.AsDepthStencilView());
            RenderTargets = filter(Variables, v => v.AsRenderTargetView());
            UnorderedAccesses = filter(Variables, v => v.AsUnorderedAccessView());

            ConstantBuffers = Enumerable.Range(0, Effect.Description.ConstantBufferCount)
                .Select(i => Effect.GetConstantBufferByIndex(i))
                .ToDictionary(v => v.Description.Name, v => v);

            Techniques = Enumerable.Range(0, Effect.Description.TechniqueCount)
                .Select(i => new ShaderTechnique(device, Effect.GetTechniqueByIndex(i)))
                .ToArray();

        }

        public void Dispose()
        {

            if(!IsDisposed)
            {

                foreach (var technique in Techniques)
                {
                    technique.Dispose();
                }

                var comObjects = ShaderResources.Values.Cast<ComObject>()
                    .Concat(DepthStencils.Values.Cast<ComObject>())
                    .Concat(RenderTargets.Values.Cast<ComObject>())
                    .Concat(UnorderedAccesses.Values.Cast<ComObject>())
                    .Concat(Variables.Values.Cast<ComObject>());

                foreach (var obj in comObjects)
                {
                    obj?.Dispose();
                }

                ShaderResources.Clear();
                DepthStencils.Clear();
                RenderTargets.Clear();
                UnorderedAccesses.Clear();
                Variables.Clear();

                foreach (var constantBuffer in ConstantBuffers.Values)
                {
                    constantBuffer.Dispose();
                }
                ConstantBuffers.Clear();

                Effect.Dispose();
                Bytecode.Dispose();

                GC.SuppressFinalize(this);
                IsDisposed = true;

            }

        }

    }
}
