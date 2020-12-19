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


        public Vector3 Color;
        public Vector3 Direction;

        public bool CastsShadows;
        public bool IsVolumetric;

        public Matrix GetTransform()
        {
            Vector3 axis = Vector3.UnitX;
            Vector3 pole = Vector3.Normalize(Vector3.Cross(Direction, axis));
            Vector3 pole2 = Vector3.Normalize(Vector3.Cross(Direction, pole));
            Matrix lightTransform = Matrix.Identity;
            lightTransform.Column1 = new Vector4(pole2, 0);
            lightTransform.Column2 = new Vector4(pole, 0);
            lightTransform.Column3 = new Vector4(Direction, 0);
            return lightTransform;
        }

    }
}
