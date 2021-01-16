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

            EffectTechnique Technique;
            readonly ShaderPass[] Passes;

            public ShaderTechnique(EffectTechnique technique)
            {
                Technique = technique;
                Passes = Enumerable.Range(0, Technique.Description.PassCount)
                    .Select(j => Technique.GetPassByIndex(j))
                    .Select(t => new ShaderPass(t))
                    .ToArray();
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
                Utilities.Dispose(ref Technique);
                foreach(var pass in Passes)
                {
                    pass.Dispose();
                }
            }
        }

        public class ShaderPass : IDisposable
        {

            EffectPass Pass;
            InputLayout? InputLayout;

            public ShaderPass(EffectPass pass)
            {
                Pass = pass;
            }

            public void Apply(DeviceContext context, int? flags = default)
            {
                if(flags.HasValue)
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
                GC.SuppressFinalize(this);
                Utilities.Dispose(ref InputLayout);
                Utilities.Dispose(ref Pass);
            }

            public InputLayout GetInputLayout(SharpDX.Direct3D11.Device device, InputElement[]? inputElements = default)
            {
                if(InputLayout == null)
                {
                    InputLayout = new InputLayout(device, Pass.Description.Signature, inputElements ?? InputElements);
                }
                return InputLayout;
            }

        }

        public ShaderTechnique this[int i] => Techniques[i];
        public int TechniqueCount => Techniques.Length;

        public readonly Dictionary<string, EffectVariable> Variables;
        public readonly Dictionary<string, EffectShaderResourceVariable> ShaderResources;
        public readonly Dictionary<string, EffectDepthStencilViewVariable> DepthStencils;
        public readonly Dictionary<string, EffectRenderTargetViewVariable> RenderTargets;
        public readonly Dictionary<string, EffectUnorderedAccessViewVariable> UnorderedAccesses;
        public readonly EffectConstantBuffer[] ConstantBuffers;

        ShaderBytecode Bytecode;
        Effect Effect;
        readonly ShaderTechnique[] Techniques;

        public Shader(SharpDX.Direct3D11.Device device, ShaderBytecode bytecode)
        {

            Bytecode = bytecode;
            Effect = new Effect(device, bytecode);

            Variables = Enumerable.Range(0, Effect.Description.GlobalVariableCount)
                .Select(i => Effect.GetVariableByIndex(i))
                .ToDictionary(v => v.Description.Name, v => v);

            ShaderResources = Variables.ToDictionary(v => v.Key, v => v.Value.AsShaderResource());
            DepthStencils = Variables.ToDictionary(v => v.Key, v => v.Value.AsDepthStencilView());
            RenderTargets = Variables.ToDictionary(v => v.Key, v => v.Value.AsRenderTargetView());
            UnorderedAccesses = Variables.ToDictionary(v => v.Key, v => v.Value.AsUnorderedAccessView());

            ConstantBuffers = Enumerable.Range(0, Effect.Description.ConstantBufferCount)
                .Select(i => Effect.GetConstantBufferByIndex(i))
                .ToArray();

            Techniques = Enumerable.Range(0, Effect.Description.TechniqueCount)
                .Select(i => new ShaderTechnique(Effect.GetTechniqueByIndex(i)))
                .ToArray();

        }

        public void Dispose()
        {

            GC.SuppressFinalize(this);

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
                obj.Dispose();

            ShaderResources.Clear();
            DepthStencils.Clear();
            RenderTargets.Clear();
            UnorderedAccesses.Clear();
            Variables.Clear();

            foreach (var constantBuffer in ConstantBuffers)
            {
                constantBuffer?.Dispose();
            }
            Array.Clear(ConstantBuffers, 0, ConstantBuffers.Length);

            Utilities.Dispose(ref Effect);
            Utilities.Dispose(ref Bytecode);

        }

    }
}
