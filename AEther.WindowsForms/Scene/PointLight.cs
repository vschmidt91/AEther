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

        public Vector3 Position;
        public Vector3 Color;
        public bool IsVolumetric;
        public bool CastsShadows;

        public Matrix GetTransform(Vector3 eyePos)
        {
            Vector3 d = Vector3.Normalize(eyePos - Position);
            Vector3 axis = Vector3.UnitX;
            Vector3 pole = Vector3.Normalize(Vector3.Cross(d, axis));
            Vector3 pole2 = Vector3.Normalize(Vector3.Cross(d, pole));
            Matrix lightTransform = Matrix.Identity;
            lightTransform.Column1 = new Vector4(pole2, 0);
            lightTransform.Column2 = new Vector4(pole, 0);
            lightTransform.Column3 = new Vector4(d, 0);
            return lightTransform;
        }

    }
}
