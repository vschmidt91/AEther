using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.D3DCompiler;

namespace AEther.WindowsForms
{
    public class IncludeHandler : Include
    {

        public class ShadowType : IDisposable
        {
            public void Dispose()
            { }
        }

        public IDisposable Shadow { get; set; } = new ShadowType();

        private Dictionary<string, string> Includes;

        public IncludeHandler(Dictionary<string, string> includes)
        {
            Includes = includes;
        }

        public void Close(Stream stream)
        {
            stream.Flush();
            stream.Close();
        }

        public void Dispose()
        {
            Includes.Clear();
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            if (!Includes.TryGetValue(fileName, out var include))
                throw new FileNotFoundException(fileName);
            return new MemoryStream(new UTF8Encoding().GetBytes(include));
        }

    }
}
