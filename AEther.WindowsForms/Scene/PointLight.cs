using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace AEther.WindowsForms
{
    public struct PointLight
    {

        public Vector3 Intensity;
        public Vector3 Position;

        public bool IsVolumetric;
        public bool CastsShadows;

        public Matrix GetTransform(Vector3 eyePos)
        {
            var d = Vector3.Normalize(eyePos - Position);
            var axis = Vector3.Right;
            var pole = Vector3.Normalize(Vector3.Cross(d, axis));
            var pole2 = Vector3.Normalize(Vector3.Cross(d, pole));
            var lightTransform = Matrix.Zero;
            lightTransform.Column1 = new Vector4(pole2, 0);
            lightTransform.Column2 = new Vector4(pole, 0);
            lightTransform.Column3 = new Vector4(d, 0);
            lightTransform.Column4 = Vector4.UnitW;
            return lightTransform;
        }

    }
}
