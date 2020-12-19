﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Assimp;
using SharpDX;
using System.Drawing.Imaging;

namespace AEther.WindowsForms
{
    public class AssimpAssetImporter : IAssetImporter
    {

        static readonly AssimpContext Context = new AssimpContext();

        readonly Dictionary<Type, Func<string, object>> Handler = new Dictionary<Type, Func<string, object>>
        {
            { typeof(Mesh[]), ImportMesh },
            { typeof(BitmapData), ImportTexture },
        };

        public AssimpAssetImporter()
        {
        }

        public T? Import<T>(string path)
            where T : class
        {
            if (Handler.TryGetValue(typeof(T), out var handler))
                return handler(path) as T;
            return default;
        }

        public static Mesh[] ImportMesh(string path)
        {

            var flags = PostProcessSteps.None;

            flags |= PostProcessSteps.OptimizeGraph;
            flags |= PostProcessSteps.MakeLeftHanded;
            flags |= PostProcessSteps.OptimizeMeshes;
            flags |= PostProcessSteps.FixInFacingNormals;
            flags |= PostProcessSteps.GenerateNormals;
            //flags |= PostProcessSteps.GenerateSmoothNormals;
            flags |= PostProcessSteps.PreTransformVertices;
            flags |= PostProcessSteps.Triangulate;
            flags |= PostProcessSteps.GenerateUVCoords;

            var scene = Context.ImportFile(path, flags);

            Vector3 toSharpDX(Vector3D v)
                => new Vector3(v.X, v.Y, v.Z);

            Vector2 toSharpDX2(Vector3D v)
                => new Vector2(v.X, v.Y);

            return scene.Meshes
                .Select(m => new Mesh(
                    Enumerable.Range(0, m.VertexCount).Select(i => new Vertex
                    {
                        Position = toSharpDX(m.Vertices[i]),
                        Normal = m.HasNormals ? Vector3.Normalize(toSharpDX(m.Normals[i])) : Vector3.Zero,
                        UV = m.HasTextureCoords(0) ? toSharpDX2(m.TextureCoordinateChannels[0][i]) : Vector2.Zero,
                    }).ToArray(),
                    m.GetUnsignedIndices()))
                .ToArray();

        }

        public static BitmapData ImportTexture(string path)
        {
            var image = Image.FromFile(path);
            var bitmap = image as Bitmap ?? new Bitmap(image);
            var data = bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size), ImageLockMode.ReadOnly, bitmap.PixelFormat);
            return data;
        }

    }
}
