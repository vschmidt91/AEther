
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace AEther.WindowsForms
{
    public struct AffineMomentum
    {

        public Vector3 Translation;
        public Vector3 Rotation;
        public float ScaleLog;

        public float Scale => (float)Math.Exp(ScaleLog);

        public static AffineMomentum operator +(AffineMomentum a, AffineMomentum b)
        {
            return new()
            {
                Translation = a.Translation + b.Translation,
                Rotation = a.Rotation + b.Rotation,
                ScaleLog = a.ScaleLog + b.ScaleLog,
            };
        }

        public static AffineMomentum operator *(float dt, AffineMomentum a)
        {
            return new()
            {
                Translation = dt * a.Translation,
                Rotation = dt * a.Rotation,
                ScaleLog = dt * a.ScaleLog,
            };
        }

        public AffineTransform ToTransform()
        {
            var rotation = Quaternion.RotationYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z);
            return new AffineTransform(Translation, rotation, Scale);
        }

    }
}
