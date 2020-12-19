using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace AEther.WindowsForms
{
    public abstract class Light : SceneNode
    {
        
        public Vector3 Color { get; set; }

        public bool CastsShadows = true;
        public bool IsVolumetric = true;

        public Light(Vector3? color = null)
        {
            Color = color ?? Vector3.One;
        }

    }
}
