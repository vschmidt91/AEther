using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace AEther.WindowsForms
{
    public class Geometry : SceneNode
    {
        
        public Model Model;
        public Vector4 Color;

        public Geometry(Model model, Vector4? color = default, AffineMomentum? transform = default, AffineMomentum? momentum = default, AffineMomentum? acceleration = default)
            : base(transform, momentum, acceleration)
        {
            Model = model;
            Color = color ?? Vector4.One;
        }

        public Instance ToInstance()
        => new()
        {
            Transform = Transform.ToTransform().ToMatrix(),
            Color = Color,
        };

}
}
