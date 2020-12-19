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

        public AffineMomentum Transform;
        public AffineMomentum Momentum;
        public AffineMomentum Acceleration;

        public SceneNode(AffineMomentum? transform = default, AffineMomentum? momentum = default, AffineMomentum? acceleration = default)
        {
            Transform = transform ?? AffineMomentum.Identity;
            Momentum = momentum ?? AffineMomentum.Identity;
            Acceleration = acceleration ?? AffineMomentum.Identity;
        }

        public virtual void Update(float dt)
        {
            Momentum += dt * Acceleration;
            Transform += dt * Momentum;
        }

    }
}
