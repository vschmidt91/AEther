using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace AEther.WindowsForms
{
    public class Sphere : Mesh
    {

        public Sphere(int w, int h, bool invert = false)
            : base(CreateVertices(w, h, invert), CreateIndices(w, h, invert))
        { }

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

        static Vertex[] CreateVertices(int w, int h, bool invert)
            => Enumerable.Range(0, w * h)
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

        static uint[] CreateIndices(int w, int h, bool invert)
            => Enumerable.Range(0, 6 * (w - 1) * (h - 1))
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

    }
}
