using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using SharpDX;

namespace AEther.WindowsForms
{
    public class Mesh
    {

        public static readonly Mesh Empty = new Mesh(new Vertex[0], new uint[0]);

        public readonly Vertex[] Vertices;
        public readonly uint[] Indices;

        public Mesh(Vertex[] vertices, uint[] indices)
        {
            Vertices = vertices;
            Indices = indices;
        }

        public static Mesh Join(params Mesh[] meshes) => Join((IEnumerable<Mesh>)meshes);

        public static Mesh Join(IEnumerable<Mesh> meshes)
        {
            var vertices = meshes.SelectMany(m => m.Vertices).ToArray();
            var offsets = meshes.Select((m, i) => meshes.Take(i).Sum(m2 => m2.Vertices.Length)).ToArray();
            var indices = meshes.SelectMany((m, i) => m.Indices.Select(n => n + (uint)offsets[i])).ToArray();
            return new Mesh(vertices, indices);
        }

        public Mesh SplitVertices(bool fixNormals)
        {

            var vertices = new List<Vertex>();

            for (int i = 0; i < Indices.Length - 2; i += 3)
            {
                var face = new[]
                {
                    Vertices[Indices[i + 0]],
                    Vertices[Indices[i + 1]],
                    Vertices[Indices[i + 2]],
                };
                if (fixNormals)
                {
                    //Vector3 normal = face.Aggregate(Vector3.Zero, (sum, v) => sum + v.Normal);
                    Vector3 normal = Vector3.Cross(face[2].Position - face[0].Position, face[1].Position - face[0].Position);
                    normal.Normalize();
                    face[0].Normal = normal;
                    face[1].Normal = normal;
                    face[2].Normal = normal;
                }
                vertices.AddRange(face);
            }

            return new Mesh(vertices.ToArray(), Enumerable.Range(0, vertices.Count).Select(i => (uint)i).ToArray());

        }

    }
}
