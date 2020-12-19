using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace AEther
{
    public static class DllExporter
    {
        
        public static void Export(string resourceName, string fileName)
        {

            var callingAssembly = Assembly.GetCallingAssembly();
            var executingAssembly = Assembly.GetExecutingAssembly();

            var path = Path.Combine(executingAssembly.Location, fileName);

            var extendedName = callingAssembly
                .ManifestModule
                .Name
                .Replace(".dll", "." + resourceName);

            if (File.Exists(path))
                return;

            using (Stream s = callingAssembly.GetManifestResourceStream(extendedName))
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                byte[] b = new byte[s.Length];
                s.Read(b, 0, b.Length);
                fs.Write(b, 0, b.Length);
            }

        }

    }
}
