using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AEther.WindowsForms
{
    public static class Extensions
    {

        public static void InvokeIfRequired(this ISynchronizeInvoke obj, Action a)
        {
            if (obj.InvokeRequired)
            {
                var args = Array.Empty<object?>();
                obj.Invoke(a, args);
            }
            else
            {
                a.Invoke();
            }
        }

        public static T InvokeIfRequired<T>(this ISynchronizeInvoke obj, Func<T> a)
        {
            object? result;
            if (obj.InvokeRequired)
            {
                var args = Array.Empty<object?>();
                result = obj.Invoke(a, args);
            }
            else
            {
                result = a.Invoke();
            }
            if(result is not T t)
            {
                throw new InvalidCastException();
            }
            return t;
        }

        public static IEnumerable<T[]> ToBatches<T>(this IEnumerable<T> values, int batchSize)
        {
            static IEnumerable<T> GetBatch(IEnumerator<T> enumerator, int batchSize)
            {
                do
                {
                    yield return enumerator.Current;
                } while (0 < --batchSize && enumerator.MoveNext());
            }

            using var enumerator = values.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return GetBatch(enumerator, batchSize).ToArray();
            }
        }

        public static Vector3 Exp(this Vector3 t)
            => new()
            {
                X = (float)Math.Exp(t.X),
                Y = (float)Math.Exp(t.Y),
                Z = (float)Math.Exp(t.Z),
            };
        
        public static Vector3 Abs(this Vector3 t)
            => new()
            {
                X = Math.Abs(t.X),
                Y = Math.Abs(t.Y),
                Z = Math.Abs(t.Z),
            };
        
        public static Vector3 Sign(this Vector3 t)
            => new()
            {
                X = Math.Sign(t.X),
                Y = Math.Sign(t.Y),
                Z = Math.Sign(t.Z),
            };
        
        public static Vector3 Pow(this Vector3 t, float pow)
            => t.Pow(new Vector3(pow));
        
        public static Vector3 Pow(this Vector3 t, Vector3 pow)
            => new()
            {
                X = (float)Math.Pow(t.X, pow.X),
                Y = (float)Math.Pow(t.Y, pow.Y),
                Z = (float)Math.Pow(t.Z, pow.Z),
            };
        
        public static float Volume(this SharpDX.BoundingBox bounds)
        {
            var size = SharpDX.Vector3.Max(SharpDX.Vector3.Zero, bounds.Maximum - bounds.Minimum);
            return size.X * size.Y * size.Z;
        }
        
        public static float NearPlane(this Matrix4x4 p)
            => -p.M43 / p.M33;
        
        public static float FarPlane(this Matrix4x4 p)
            => p.NearPlane() / (1 - 1 / p.M33);

        public static Quaternion ToQuaternion(this Vector3 v)
            => Quaternion.CreateFromYawPitchRoll(v.X, v.Y, v.Z);

        public static Vector3 ToEulerAngles(this Quaternion q)
        => new()
            {
            X = (float)Math.Atan2(2 * (q.W * q.X + q.Y * q.Z), 1 - 2 * (q.X * q.X + q.Y * q.Y)),
            Y = (float)Math.Asin(2 * (q.W * q.Y - q.X * q.Z)),
            Z = (float)Math.Atan2(2 * (q.W * q.Z + q.X * q.Y), 1 - 2 * (q.Y * q.Y + q.Z * q.Z)),
        };

        public static Vector3 GetAxisWithMaximumAngle(this Vector3 v)
        {
            Vector3 abs = v.Abs();
            float min = Math.Min(Math.Min(abs.X, abs.Y), abs.Z);
            return abs.X == min
                ? Vector3.UnitX
                : (abs.Y == min
                ? Vector3.UnitY
                : Vector3.UnitZ);
        }

        public static float ToRadians(this float d)
            => d * (float)Math.PI / 180f;

        public static float NextFloat(this Random random) => (float)random.NextDouble();

        public static float NextFloat(this Random random, float min, float max) => min + (max - min) * random.NextFloat();

        public static Vector3 NextVector3(this Random random, Vector3 min, Vector3 max)
            => min + new Vector3
            {
                X = random.NextFloat(),
                Y = random.NextFloat(),
                Z = random.NextFloat(),
            } * (max - min);

        public static IEnumerable<(T, int)> WithIndex<T>(this IEnumerable<T> items)
            => items.Select((xi, i) => (xi, i));

        public static Vector4 NextVector4(this Random random, Vector4 min, Vector4 max)
            => min + new Vector4
            {
                X = random.NextFloat(),
                Y = random.NextFloat(),
                Z = random.NextFloat(),
                W = random.NextFloat(),
            } * (max - min);

        public static Vector4 ToVector4(this SharpDX.Mathematics.Interop.RawVector4 v)
            => new(v.X, v.Y, v.Z, v.W);

        public static Vector2 XY(this Vector4 v)
            => new(v.X, v.Y);

        public static Vector3 XYZ(this Vector4 v)
            => new(v.X, v.Y, v.Z);

        public static Vector2 ToVector2(this float[] v)
            => new(v[0], v[1]);

        public static Vector3 ToVector3(this float[] v)
            => new(v[0], v[1], v[2]);

        public static Vector4 ToVector4(this float[] v)
            => new(v[0], v[1], v[2], v[3]);

    }
}
