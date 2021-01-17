using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace AEther.WindowsForms
{
    public class Grid : Mesh
    {

        public Grid(int w, int h)
            : base(CreateVertices(w, h), CreateIndices(w, h))
        { }

        static Vertex[] CreateVertices(int w, int h)
            => Enumerable.Range(0, w * h)
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

        static uint[] CreateIndices(int w, int h)
            => Enumerable.Range(0, 6 * (w - 1) * (h - 1))
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

    }
}
