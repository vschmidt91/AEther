using System.Reflection;

namespace AEther
{
    public static class ResourceReader
    {

        public static Stream Read(string name, Assembly? assemblyArg = null)
        {

            var assembly = assemblyArg ?? Assembly.GetCallingAssembly();

            var extendedName = assembly
                .ManifestModule
                .ScopeName
                .Replace(".dll", "." + name)
                .Replace(".exe", "." + name)
                .Replace("\\", ".")
                .Replace("/", ".");

            if (assembly.GetManifestResourceStream(extendedName) is not Stream stream)
                throw new FileNotFoundException(extendedName);

            return stream;

        }

        public static string[] GetResources(Assembly? assembly = null)
            => (assembly ?? Assembly.GetCallingAssembly()).GetManifestResourceNames();

    }
}
