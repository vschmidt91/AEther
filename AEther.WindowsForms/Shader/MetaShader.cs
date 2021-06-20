﻿using System;
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
        readonly HashSet<string> Defines;

        public MetaShader(string[] defines, params (HashSet<string>, Shader)[] values)
        {
            Defines = defines.ToHashSet();
            Values = values;
        }

        public Shader this[params string[] switches] => this[(IEnumerable<string>)switches];

        public Shader this[IEnumerable<string> switches]
        {
            get
            {
                var switchesFiltered = switches.Where(Defines.Contains);
                foreach(var (switches2, shader) in Values)
                {
                    if(switches2.SetEquals(switchesFiltered))
                    {
                        return shader;
                    }
                }
                throw new KeyNotFoundException();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            foreach (var shader in Shaders)
            {
                shader.Dispose();
            }
        }

    }
}