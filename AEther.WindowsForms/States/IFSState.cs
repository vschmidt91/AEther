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
            ifsSize = 1 << 11;

            var ifsDescription = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.R32G32B32A32_Float,
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
                //new IFSElement(Graphics.Shader["ifs-input.fx"]),
                //new IFSAffine(Graphics.Shader["ifs-fisheye.fx"]) {  Speed = -.0687f },
                //new IFSAffine(Graphics.Shader["ifs-rcp.fx"]) {  Speed = +.0535f },
                //new IFSAffine(Graphics.Shader["ifs-hyperbolic.fx"]) { Speed = +.335f },
                //new IFSAffine(Graphics.Shader["ifs-sphere-inversion.fx"]) {  Speed = +.58684f },
                //new IFSAffine(Graphics.Shader["ifs-sqrt.fx"]) {  Speed = -.0435f },
                //new IFSAffine(Graphics.Shader["ifs-polar.fx"]) {  Speed = -.4935f },
                //new IFSElement(Graphics.Shader["ifs-swirl.fx"]),
                new IFSElement(Graphics.Shader["ifs-hyperbolic.fx"]),
                //new IFSElement(Graphics.Shader["ifs-sqrt.fx"]),
                //new IFSAffine(Graphics.Shader) {  Scale = .7f, Speed = +.01535f, OffsetScale = 2f },
                new IFSAffine(Graphics.Shader) {  Scale = .9f, Speed = -.08535f, OffsetScale = 2f },
                //new IFSAffine(Graphics.Shader) {  Speed = -.0256f },
                //new IFSAffine(Graphics.Shader) {  Speed = .2125f },
                //new IFSAffine(Graphics.Shader) { Speed = -.04f },
            };

            Graphics.Context.ClearRenderTargetView(Source.GetRenderTargetView(), Color4.White);

        }


        public override void Render()
        {

            Graphics.SetModel(null);

            var t = (float)DateTime.Now.TimeOfDay.TotalSeconds;

            var sumWeight = Vector4.Zero;
            foreach (var element in Elements)
            {
                element.Update(t);
                sumWeight += element.Weight;
            }
            //sumWeight = .25f * Vector4.Dot(sumWeight, Vector4.One) * Vector4.One;


            //Graphics.Context.Rasterizer.SetViewport(Source.ViewPort);
            //Graphics.Context.OutputMerger.SetRenderTargets(null, Source.GetRenderTargetView());
            //Graphics.Draw(Graphics.Shader["ifs-input.fx"]);

            Graphics.Context.ClearRenderTargetView(Source.GetRenderTargetView(), Color4.White);
            for (int n = 0; n < 16; ++n)
            {

                Graphics.Context.ClearRenderTargetView(Target.GetRenderTargetView(), new Color4(SharpDX.Vector4.Zero));
                Graphics.Context.Rasterizer.SetViewport(Target.ViewPort);
                foreach(var element in Elements)
                {
                    var shader = element.Shader;
                    Graphics.Context.OutputMerger.SetRenderTargets(null, Target.GetRenderTargetView());
                    shader.ShaderResources["Source"].SetResource(Source.GetShaderResourceView());
                    shader.Variables["Weight"].AsVector().Set(element.Weight / sumWeight);
                    Graphics.Draw(shader);
                }

                (Source, Target) = (Target, Source);

            }

            Graphics.Context.Rasterizer.SetViewport(Graphics.BackBuffer.ViewPort);
            Graphics.Context.OutputMerger.SetRenderTargets(null, Graphics.BackBuffer.GetRenderTargetView());
            Graphics.Shader["ifs-output.fx"].ShaderResources["Source"].SetResource(Source.GetShaderResourceView());
            Graphics.Draw(Graphics.Shader["ifs-output.fx"]);

        }

    }
}
