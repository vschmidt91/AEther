using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace AEther.WindowsForms
{
    public readonly struct AffineTransform
    {

        public static readonly AffineTransform Identity = new AffineTransform(Vector3.Zero, Quaternion.Identity, 1f);

        public readonly Vector3 Translation;
        public readonly float Scale;
        public readonly Quaternion Rotation;

        public AffineTransform(Vector3? translation = default, Quaternion? rotation = default, float? scale = default)
        {
            Translation = translation ?? Vector3.Zero;
            Rotation = rotation ?? Quaternion.Identity;
            Scale = scale ?? 1f;
        }

        public Matrix4x4 ToMatrix()
        {
            return Matrix4x4.CreateScale(Scale) * Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Translation);
        }

        public static AffineTransform operator *(AffineTransform a, AffineTransform b)
        {
            var translation = a.Translation + a.Scale * Vector3.Transform(b.Translation, a.Rotation);
            var rotation = a.Rotation * b.Rotation;
            var scale = a.Scale * b.Scale;
            return new AffineTransform(translation, rotation, scale);
        }

    }
}
