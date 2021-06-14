using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace AEther.WindowsForms
{
    public class MetaShader : IDisposable
    {

        public IEnumerable<Shader> Shaders => Values.Select(d => d.Item2);

        readonly (HashSet<string>, Shader)[] Values;

        public MetaShader(params (HashSet<string>, Shader)[] values)
        {
            Values = values;
        }

        public Shader this[params string[] switches]
        {
            get
            {
                var v = Values.FirstOrDefault(v => v.Item1.SetEquals(switches));
                return v == default ? throw new KeyNotFoundException() : v.Item2;
            }
        }

        public void Dispose()
        {
            foreach(var shader in Shaders)
            {
                shader.Dispose();
            }
        }

    }
}
