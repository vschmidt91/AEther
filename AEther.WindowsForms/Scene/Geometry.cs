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
        public Vector3 Color;

        public Geometry(Model model, Vector3? color = default, AffineMomentum? transform = default, AffineMomentum? momentum = default, AffineMomentum? acceleration = default)
            : base(transform, momentum, acceleration)
        {
            Model = model;
            Color = color ?? Vector3.One;
        }

    }
}
