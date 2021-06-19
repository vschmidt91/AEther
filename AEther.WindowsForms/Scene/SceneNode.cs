using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace AEther.WindowsForms
{

    public class SceneNode
    {

        public AffineMomentum Transform = AffineMomentum.Identity;
        public AffineMomentum Momentum = AffineMomentum.Identity;
        public AffineMomentum Acceleration = AffineMomentum.Identity;

        public virtual void Update(float dt)
        {
            Momentum += dt * Acceleration;
            Transform += dt * Momentum;
        }

    }
}
