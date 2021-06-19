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
            return new()
            {
                Translation = maxTranslation * random.NextVector3(-Vector3.One, Vector3.One),
                Rotation = maxRotation * MathUtil.Pi * random.NextVector3(-Vector3.One, Vector3.One),
                ScaleLog = maxScaleLog * random.NextFloat(-1f, +1f),
            };
        }

    }
}
