using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AEther
{
    public static class ResourceReader
    {

        public static Stream Read(string name, Assembly assembly = null)
        {

            assembly = assembly ?? Assembly.GetCallingAssembly();

            var extendedName = assembly
                .ManifestModule
                .ScopeName
                .Replace(".dll", "." + name)
                .Replace(".exe", "." + name)
                .Replace("\\", ".")
                .Replace("/", ".");

            return assembly.GetManifestResourceStream(extendedName);

        }

        public static string[] GetResources(Assembly assembly = null)
            => (assembly ?? Assembly.GetCallingAssembly()).GetManifestResourceNames();

    }
}
