using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;

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

    }
}
