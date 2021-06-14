using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace AEther.WindowsForms
{
    public struct DirectionalLight
    {

        public Vector3 Intensity;
        public Vector3 Direction;

        public bool IsVolumetric;
        public bool CastsShadows;

        public Matrix GetTransform()
        {
            var axis = Vector3.Right;
            var pole = Vector3.Normalize(Vector3.Cross(Direction, axis));
            var pole2 = Vector3.Normalize(Vector3.Cross(Direction, pole));
            var lightTransform = Matrix.Zero;
            lightTransform.Column1 = new Vector4(pole2, 0);
            lightTransform.Column2 = new Vector4(pole, 0);
            lightTransform.Column3 = new Vector4(Direction, 0);
            lightTransform.Column4 = Vector4.UnitW;
            return lightTransform;
        }

    }
}
