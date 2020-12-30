using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace AEther.WindowsForms
{
    public class SceneState : GraphicsState
    {

        readonly ComputeBuffer InstanceBuffer;
        readonly RenderBuffer RenderBuffer;
        readonly Tree<SceneNode> Scene;
        readonly CameraPerspective Camera;

        public SceneState(Graphics graphics)
            : base(graphics)
        {

            Scene = CreateScene();
            Camera = CreateCamera();

            var instanceSize = Marshal.SizeOf<Instance>();
            InstanceBuffer = new ComputeBuffer(Graphics.Device, instanceSize, 1 << 12, true);
            RenderBuffer = new RenderBuffer(InstanceBuffer.Size);
        }

        Tree<SceneNode> CreateScene()
        {

            var random = new Random();

            var model = new Model(Graphics.Device, new Sphere(7, 7));
            var scene = 0.UnfoldToTree(n =>
            {
                AffineMomentum transform, momentum;
                SceneNode node;
                IEnumerable<int> children;
                if (n < 2)
                {
                    var translationScale = 10 * (float)Math.Pow(.5, n);
                    transform = random.NextMomentum(translationScale, 1, 0);
                    momentum = random.NextMomentum(0, 1, 0);
                    //acceleration = .1f * Random.NextMomentum(0f);
                    node = new SceneNode(transform, momentum, default);
                    children = Enumerable.Repeat(n + 1, 5);
                }
                else
                {
                    var color = random.NextVector3(Vector3.Zero, .1f * Vector3.One);
                    transform = random.NextMomentum(5, 0, 0);
                    transform = new AffineMomentum(transform.Translation, transform.Rotation, 0);
                    //momentum = 5 * Random.NextMomentum(0f);
                    //var acceleration = .1f * Random.NextMomentum(0f);
                    node = new Geometry(model, color, transform, default, default);
                    children = Enumerable.Empty<int>();
                }
                return (node, children);
            });

            scene.Item.Transform = AffineMomentum.Identity;

            return scene;
            //return scene.Fold<Scene>((node, children) => new Scene(node, children.ToArray()));

        }

        static CameraPerspective CreateCamera()
             => new()
             {
                NearPlane = 0.1f,
                FarPlane = 100f,
                FieldOfView = 90f,
                Position = 10 * Vector3.One,
                Target = Vector3.Zero,
            };

        public override void Render()
        {

            Camera.AspectRatio = Graphics.FrameConstants.Value.AspectRatio;
            Graphics.FrameConstants.Value.View = Camera.View;
            Graphics.FrameConstants.Value.Projection = Camera.Projection;

            Graphics.Shader["geometry.fx"].ShaderResources["Instances"].SetResource(InstanceBuffer.GetShaderResourceView());

            Graphics.Context.ClearRenderTargetView(Graphics.BackBuffer.GetRenderTargetView(), Color4.Black);
            Graphics.Context.Rasterizer.SetViewport(Graphics.BackBuffer.ViewPort);
            Graphics.Context.OutputMerger.SetRenderTargets(null, Graphics.BackBuffer.GetRenderTargetView());

            RenderBuffer.Clear();
            void finishBuffer(object? sender, RenderBuffer.BufferFinishedEventArgs evt)
            {
                Graphics.SetModel(evt.Model);
                InstanceBuffer.Update(Graphics.Context, evt.Instances);
                Graphics.Draw(Graphics.Shader["geometry.fx"], evt.Instances.Count);
            }
            RenderBuffer.BufferFinished += finishBuffer;

            //Scene.Update(DT, AffineTransform.Identity, RenderBuffer);

            var scene = Scene.WithWorldTransform().FlattenPreOrder();
            foreach (var (node, transform) in scene)
            {
                node.Update(.01f);
                switch (node)
                {
                    case Geometry geometry:
                        var model = geometry.Model;
                        if (model != default)
                        {
                            var instance = new Instance
                            {
                                Color = geometry.Color,
                                Transform = transform.ToMatrix(),
                            };
                            RenderBuffer.Add(model, instance);
                        }
                        break;
                }
            }

            RenderBuffer.Finish();

            RenderBuffer.BufferFinished -= finishBuffer;

        }

    }
}
