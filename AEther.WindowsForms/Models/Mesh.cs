
using SharpDX;

namespace AEther.WindowsForms
{
    public record Mesh
    (
        Vertex[] Vertices,
        uint[] Indices
    )
    {

        public static readonly Mesh Empty = new(Array.Empty<Vertex>(), Array.Empty<uint>());

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

        public static Mesh CreateSphere(int w, int h, bool invert = false)
        {
            var vertices = Enumerable.Range(0, w * h)
                .Select(i =>
                {
                    int x = i % w;
                    int y = i / h;
                    Vector2 uv = new()
                    {
                        X = -1 + 2 * y / (float)(h - 1),
                        Y = -1 + 2 * x / (float)(w - 1),
                    };
                    Vector3 position = -Vector3.Normalize(MapOctahedron(uv));
                    return new Vertex()
                    {
                        Position = position,
                        Normal = invert ? -position : position,
                        UV = .5f + .5f * uv,
                    };
                }).ToArray();
            var indices = Enumerable.Range(0, 6 * (w - 1) * (h - 1))
                .Select(i =>
                {

                    if (invert)
                    {
                        switch (i % 6)
                        {
                            case 1: ++i; break;
                            case 2: --i; break;
                            case 4: ++i; break;
                            case 5: --i; break;
                        }
                    }

                    int x = (i / 6) % (w - 1);
                    int y = (i / 6) / (w - 1);
                    if ((x >= w / 2) == (y >= h / 2))
                    {
                        switch (i % 6)
                        {
                            case 0: break;
                            case 1: ++y; break;
                            case 2: ++x; break;
                            case 3: ++x; break;
                            case 4: ++y; break;
                            case 5: ++x; ++y; break;
                        }
                    }
                    else
                    {
                        switch (i % 6)
                        {
                            case 0: break;
                            case 1: ++y; break;
                            case 2: ++x; ++y; break;
                            case 3: ++x; break;
                            case 4: break;
                            case 5: ++x; ++y; break;
                        }
                    }

                    //if (x == 0 && y >= h / 2) y = (h - 1) - y;
                    //if (y == 0 && x >= w / 2) x = (w - 1) - x;
                    //if (x == w - 1 && y >= h / 2) y = (h - 1) - y;
                    //if (y == h - 1 && x >= w / 2) x = (w - 1) - x;
                    return (uint)(y * w + x);
                }).ToArray();
            return new Mesh(vertices, indices);
        }

        public static Mesh CreateGrid(int w, int h)
        {
            var vertices = Enumerable.Range(0, w * h)
                .Select(i =>
                {
                    int x = i % w;
                    int y = i / w;
                    Vector2 uv = new()
                    {
                        X = x / (float)(w - 1),
                        Y = y / (float)(h - 1),
                    };
                    return new Vertex()
                    {
                        Position = new Vector3()
                        {
                            X = 2 * uv.X - 1,
                            Y = 2 * uv.Y - 1,
                            Z = 0,
                        },
                        Normal = -Vector3.UnitZ,
                        UV = uv,
                    };
                }).ToArray();
            var indices = Enumerable.Range(0, 6 * (w - 1) * (h - 1))
                .Select(i =>
                {
                    int x = (i / 6) % (w - 1);
                    int y = (i / 6) / (w - 1);
                    if ((x >= w / 2) == (y >= h / 2))
                    {
                        switch (i % 6)
                        {
                            case 0: break;
                            case 1: ++y; break;
                            case 2: ++x; break;
                            case 3: ++x; break;
                            case 4: ++y; break;
                            case 5: ++x; ++y; break;
                        }
                    }
                    else
                    {
                        switch (i % 6)
                        {
                            case 0: break;
                            case 1: ++y; break;
                            case 2: ++x; ++y; break;
                            case 3: ++x; break;
                            case 4: break;
                            case 5: ++x; ++y; break;
                        }
                    }
                    return (uint)(y * w + x);
                }).ToArray();
            return new Mesh(vertices, indices);
        }

        static Vector3 MapOctahedron(Vector2 uv)
        {
            float d = Math.Abs(uv.X) + Math.Abs(uv.Y);
            return new Vector3()
            {
                X = d < 1 ? uv.X : Math.Sign(uv.X) * (1 - Math.Abs(uv.Y)),
                Y = d < 1 ? uv.Y : Math.Sign(uv.Y) * (1 - Math.Abs(uv.X)),
                Z = 1 - d,
            };
        }

    }
}
