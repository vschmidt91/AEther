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
        public Vector4 Color = Vector4.One;
        public float Roughness = 0f;

        public Geometry(Model model)
        {
            Model = model;
        }

        public Instance ToInstance()
        => new()
        {
            Transform = Transform.ToTransform().ToMatrix(),
            Color = Color,
            Roughness = Roughness,
        };

}
}
