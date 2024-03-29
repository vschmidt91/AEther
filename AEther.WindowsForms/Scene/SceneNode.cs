﻿namespace AEther.WindowsForms
{

    public class SceneNode
    {

        public AffineMomentum Transform;
        public AffineMomentum Momentum;
        public AffineMomentum Acceleration;

        public virtual void Update(float dt)
        {
            Momentum += dt * Acceleration;
            Transform += dt * Momentum;
        }

    }
}
