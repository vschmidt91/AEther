using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AEther.WindowsForms
{
    public readonly struct AffineMomentum
    {

        public static readonly AffineMomentum Identity = new AffineMomentum(Vector3.Zero, Vector3.Zero, 0f);

        public readonly Vector3 Translation;
        public readonly Vector3 Rotation;
        public readonly float ScaleLog;

        public float Scale => (float)Math.Exp(ScaleLog);

        public AffineMomentum(Vector3? translation = default, Vector3? rotation = default, float? scaleLog = default)
        {
            Translation = translation ?? Vector3.Zero;
            Rotation = rotation ?? Vector3.Zero;
            ScaleLog = scaleLog ?? 0f;
        }

        public static AffineMomentum operator +(AffineMomentum a, AffineMomentum b)
        {
            var translation = a.Translation + b.Translation;
            var rotation = a.Rotation + b.Rotation;
            var scaleLog = a.ScaleLog + b.ScaleLog;
            return new AffineMomentum(translation, rotation, scaleLog);
        }

        public static AffineMomentum operator *(float dt, AffineMomentum a)
        {
            var translation = dt * a.Translation;
            var rotation = dt * a.Rotation;
            var scaleLog = dt * a.ScaleLog;
            return new AffineMomentum(translation, rotation, scaleLog);
        }

        public AffineTransform ToTransform()
        {
            var rotation = Quaternion.CreateFromYawPitchRoll(Rotation.X, Rotation.Y, Rotation.Z);
            return new AffineTransform(Translation, rotation, Scale);
        }

    }
}
