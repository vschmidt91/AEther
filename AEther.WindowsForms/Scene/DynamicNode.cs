//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace AEther.WindowsForms
//{
//    public class DynamicNode : SceneNode
//    {

//        public AffineMomentum Momentum;
//        public AffineMomentum Acceleration;

//        public virtual void Update(AffineTransform parentTransform, float dt)
//        {

//            Momentum.ApplyMomentum(ref Acceleration, dt);
//            Transform = Transform * Momentum.ToTransform();

//        }

//    }
//}
