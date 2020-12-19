using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Vector4 = System.Numerics.Vector4;

namespace AEther.WindowsForms
{
    public class IFSState : GraphicsState
    {

        public Texture2D Source;
        public Texture2D Target;
        public readonly IFSElement[] Elements;

        public IFSState(Graphics graphics)
            : base(graphics)
        {

            var ifsSize = Math.Max(Graphics.BackBuffer.Width, Graphics.BackBuffer.Height);

            var ifsDescription = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R16G16B16A16_Float,
                Width = ifsSize,
                Height = ifsSize,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
            };
            Source = new Texture2D(new SharpDX.Direct3D11.Texture2D(Graphics.Device, ifsDescription));
            Target = new Texture2D(new SharpDX.Direct3D11.Texture2D(Graphics.Device, ifsDescription));

            Elements = new IFSElement[]
            {
                new IFSElement(Graphics.Shader["ifs-input.fx"]),
                new IFSAffine(Graphics.Shader["ifs-fisheye.fx"]) {  Speed = -.687f },
                //new IFSAffine(Graphics.Shader["ifs-rcp.fx"]) {  Speed = +.0535f },
                //new IFSAffine(Graphics.Shader["ifs-hyperbolic.fx"]) { Speed = +.335f },
                new IFSAffine(Graphics.Shader["ifs-sphere-inversion.fx"]) {  Speed = +.58684f },
                new IFSAffine(Graphics.Shader["ifs-sqrt.fx"]) {  Speed = -.435f },
                //new IFSAffine(Graphics.Shader["ifs-polar.fx"]) {  Speed = -.4935f },
                new IFSAffine(Graphics.Shader) {  Speed = +.535f },
                //new IFSAffine(Graphics.Shader) {  Speed = -.0256f },
                //new IFSAffine(Graphics.Shader) {  Speed = .2125f },
                //new IFSAffine(Graphics.Shader) { Speed = -.04f },
            };

            var random = new Random();
            foreach (var element in Elements)
            {
                element.Weight = random.NextVector4(Vector4.Zero, Vector4.One);
                element.Weight = Enumerable.Range(0, 4)
                    .Select(_ => random.NextDouble())
                    .Select(x => (float)Math.Pow(x, 2))
                    .ToArray()
                    .ToVector4();
            }

            NormalizeIFS();

        }


        public override void Render()
        {

            Graphics.Shader["ifs-output.fx"].ShaderResources["Spectrum0"].SetResource(Graphics.Spectrum[0].Texture.GetShaderResourceView());
            Graphics.Shader["ifs-output.fx"].ShaderResources["Spectrum1"].SetResource(Graphics.Spectrum[1].Texture.GetShaderResourceView());

            foreach (var element in Elements)
            {
                element.Shader.ShaderResources["Spectrum0"].SetResource(Graphics.Spectrum[0].Texture.GetShaderResourceView());
                element.Shader.ShaderResources["Spectrum1"].SetResource(Graphics.Spectrum[1].Texture.GetShaderResourceView());
            }

            Graphics.SetModel(null);

            var t = (float)DateTime.Now.TimeOfDay.TotalSeconds;
            UpdateIFS(t);
            NormalizeIFS();

            Graphics.Context.ClearRenderTargetView(Source.GetRenderTargetView(), new Color4(SharpDX.Vector4.Zero));
            for (int n = 0; n < 8; ++n)
            {

                Graphics.Context.ClearRenderTargetView(Target.GetRenderTargetView(), new Color4(SharpDX.Vector4.Zero));
                Graphics.Context.Rasterizer.SetViewport(Target.ViewPort);
                for (var j = 0; j < Elements.Length; ++j)
                {
                    var element = Elements[j];
                    element.Update(t + j);
                    var shader = element.Shader;
                    Graphics.Context.OutputMerger.SetRenderTargets(null, Target.GetRenderTargetView());
                    shader.ShaderResources["Source"].SetResource(Source.GetShaderResourceView());
                    shader.Variables["Weight"].AsVector().Set(element.Weight);
                    Graphics.Draw(shader);
                }

                (Source, Target) = (Target, Source);

            }

            Graphics.Context.Rasterizer.SetViewport(Graphics.BackBuffer.ViewPort);
            Graphics.Context.OutputMerger.SetRenderTargets(null, Graphics.BackBuffer.GetRenderTargetView());
            Graphics.Shader["ifs-output.fx"]?.ShaderResources["Source"].SetResource(Source.GetShaderResourceView());
            Graphics.Draw(Graphics.Shader["ifs-output.fx"]);

        }

        void UpdateIFS(float t)
        {
            if (Elements == default)
            {
                throw new Exception();
            }
            for (int i = 0; i < Elements.Length; ++i)
            {
                var element = Elements[i];
                element.Weight = Enumerable.Range(0, 4)
                    .Select(k => Math.Sin((1 + .173475645 * k) * (t + i)))
                    .Select(x => (float)Math.Pow(x, 2))
                    .ToArray()
                    .ToVector4();
            }
        }

        void NormalizeIFS()
        {
            if (Elements != default)
            {
                var sum = Vector4.Zero;
                foreach (var element in Elements)
                {
                    sum += element.Weight;
                }
                foreach (var element in Elements)
                {
                    element.Weight *= Vector4.One / sum;
                }
            }
        }

    }
}
