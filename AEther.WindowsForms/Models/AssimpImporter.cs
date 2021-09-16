using Assimp;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace AEther.WindowsForms
{
    public class AssimpAssetImporter : IAssetImporter
    {

        static readonly AssimpContext Context = new();

        static readonly Dictionary<Type, Func<string, object>> Handler = new()
        {
            { typeof(Mesh[]), ImportMesh },
            { typeof(BitmapData), ImportTexture },
        };

        readonly string? Prefix;

        public AssimpAssetImporter(string? prefix = null)
        {
            Prefix = prefix;
        }

        public T Import<T>(string path)
            where T : class
        {
            if(Prefix is not null)
            {
                path = Path.Join(Prefix, path);
            }
            if (!Handler.TryGetValue(typeof(T), out var handler))
            {
                throw new KeyNotFoundException(typeof(T).Name);
            }
            if (handler(path) is not T asset)
            {
                throw new InvalidCastException(typeof(T).Name);
            }
            return asset;
        }

        public static Mesh[] ImportMesh(string path)
        {

            static Vector3 toSharpDX(Vector3D v)
                => new(v.X, v.Y, v.Z);

            static Vector2 toSharpDX2(Vector3D v)
                => new(v.X, v.Y);

            var flags = PostProcessSteps.None;

            //flags |= PostProcessSteps.OptimizeGraph;
            //flags |= PostProcessSteps.MakeLeftHanded;
            //flags |= PostProcessSteps.OptimizeMeshes;
            //flags |= PostProcessSteps.FixInFacingNormals;
            flags |= PostProcessSteps.GenerateNormals;
            //flags |= PostProcessSteps.PreTransformVertices;
            flags |= PostProcessSteps.Triangulate;
            //flags |= PostProcessSteps.GenerateUVCoords;

            //flags |= PostProcessSteps.GenerateSmoothNormals;

            var scene = Context.ImportFile(path, flags);

            return scene.Meshes
                .Select(m => new Mesh(
                    Enumerable.Range(0, (m.VertexCount / 3) * 3).Select(i => new Vertex
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
