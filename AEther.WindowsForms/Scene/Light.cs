﻿
using SharpDX;

namespace AEther.WindowsForms
{
    public class Light : SceneNode
    {

        public Vector3 Intensity;
        public bool CastsShadows;
        public bool IsVolumetric;

        //public Matrix GetTransform(Vector3 eyePos)
        //{
        //    if(IsDirectional)
        //    {
        //        var axis = Vector3.Right;
        //        var pole = Vector3.Normalize(Vector3.Cross(Position, axis));
        //        var pole2 = Vector3.Normalize(Vector3.Cross(Position, pole));
        //        var lightTransform = Matrix.Zero;
        //        lightTransform.Column1 = new Vector4(pole2, 0);
        //        lightTransform.Column2 = new Vector4(pole, 0);
        //        lightTransform.Column3 = new Vector4(Position, 0);
        //        lightTransform.Column4 = Vector4.UnitW;
        //        return lightTransform;
        //    }
        //    else
        //    {
        //        var d = Vector3.Normalize(eyePos - Position);
        //        var axis = Vector3.Right;
        //        var pole = Vector3.Normalize(Vector3.Cross(d, axis));
        //        var pole2 = Vector3.Normalize(Vector3.Cross(d, pole));
        //        var lightTransform = Matrix.Zero;
        //        lightTransform.Column1 = new Vector4(pole2, 0);
        //        lightTransform.Column2 = new Vector4(pole, 0);
        //        lightTransform.Column3 = new Vector4(d, 0);
        //        lightTransform.Column4 = Vector4.UnitW;
        //        return lightTransform;
        //    }
        //}

    }
}
