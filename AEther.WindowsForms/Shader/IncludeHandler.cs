using SharpDX.D3DCompiler;
using System.Text;

namespace AEther.WindowsForms
{
    public class IncludeHandler : Include
    {

        public class ShadowType : IDisposable
        {
            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }
        }

        public IDisposable Shadow { get; set; } = new ShadowType();

        readonly string BasePath;

        protected bool IsDisposed;

        public IncludeHandler(string basePath)
        {
            BasePath = basePath;
        }

        public void Close(Stream stream)
        {
            stream.Flush();
            stream.Close();
        }

        public void Dispose()
        {
            if (!IsDisposed)
            {
                Shadow.Dispose();
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }

        public Stream Open(IncludeType type, string fileName, Stream parentStream)
        {
            var path = Path.Join(BasePath, fileName);
            var str = File.ReadAllText(path);
            var utf8 = new MemoryStream(Encoding.UTF8.GetBytes(str));
            return utf8;
            //if (!Includes.TryGetValue(fileName, out var include))
            //    throw new FileNotFoundException(fileName);
            //return new MemoryStream(new UTF8Encoding().GetBytes(include));
        }

    }
}
