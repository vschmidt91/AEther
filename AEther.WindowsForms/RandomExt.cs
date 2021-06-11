using System;
using System.Collections.Generic;
using System.Text;

using SharpDX;

namespace AEther.WindowsForms
{
    public static class RandomExt
    {

        public static AffineMomentum NextMomentum(this Random random, float maxTranslation = 1f, float maxRotation = 1f, float maxScaleLog = 1f)
        {
            var translation = maxTranslation * random.NextVector3(-Vector3.One, Vector3.One);
            var rotation = maxRotation * random.NextVector3(-Vector3.One, Vector3.One);
            var scaleLog = maxScaleLog * random.NextFloat(-1f, +1f);
            return new AffineMomentum(translation, rotation, scaleLog);
        }

    }
}
