using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Diagnostics;
using SharpDX.Direct3D;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;
using System.Windows.Forms;

namespace AEther.WindowsForms
{
    public class ShaderManager : IDisposable
    {

        public Shader this[string key] => Shaders[key];
        public IEnumerable<string> Keys => Shaders.Keys;

        readonly Dictionary<string, Shader> Shaders = new Dictionary<string, Shader>();

        IncludeHandler Includes;
        FileSystemWatcher? Watcher;

        public ShaderManager(Device device, string basePath, bool watch)
        {

            if (!Directory.Exists(basePath)) throw new DirectoryNotFoundException();

            var includes = Directory.EnumerateFiles(basePath, "*.fxi", SearchOption.AllDirectories)
                .ToDictionary(path => new FileInfo(path).Name, File.ReadAllText);
            Includes = new IncludeHandler(includes);

            var effects = Directory.EnumerateFiles(basePath, "*.fx", SearchOption.AllDirectories);

            foreach (var effect in effects)
            {
                LoadShader(device, effect);
            }

            if (watch)
            {
                Watcher = new FileSystemWatcher(basePath, "*.fx");
                Watcher.NotifyFilter = NotifyFilters.Attributes
                    | NotifyFilters.CreationTime
                    | NotifyFilters.DirectoryName
                    | NotifyFilters.FileName
                    | NotifyFilters.LastAccess
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Security
                    | NotifyFilters.Size;
                Watcher.Changed += (o, e) => LoadShader(device, e.FullPath);
                Watcher.EnableRaisingEvents = true;
            }

        }

        private void LoadShader(Device device, string path)
        {

            FileInfo file = new FileInfo(path);

            ShaderFlags shaderFlags = ShaderFlags.None;
            EffectFlags effectFlags = EffectFlags.None;

#if DEBUG
            shaderFlags |= ShaderFlags.Debug;
#else
            shaderFlags |= ShaderFlags.OptimizationLevel3;
#endif

            string sourceCode;
            while (true)
            {
                try
                {
                    sourceCode = File.ReadAllText(file.FullName);
                    break;
                }
                catch(FileNotFoundException)
                {
                    return;
                }
                catch(IOException)
                {
                    Thread.Sleep(10);
                }
            }

            try
            {
                using (var compiled = ShaderBytecode.Compile(sourceCode, "fx_5_0", shaderFlags, effectFlags, new ShaderMacro[] { }, Includes, file.Name))
                {

                    if (compiled.Bytecode is null)
                    {
                        throw new CompilationException(compiled.Message);
                    }

#if DEBUG
                    //File.WriteAllText(file.FullName + ".txt", compiled.Bytecode.Disassemble(DisassemblyFlags.EnableInstructionNumbering));
#endif

                    Shaders[file.Name] = new Shader(device, compiled.Bytecode);

                }

            }
            catch(CompilationException exc)
            {
#if DEBUG
                MessageBox.Show(null, exc.Message, file.Name);
#else
                Debug.WriteLine("ERROR compiling " + file.Name);
                Debug.WriteLine(exc.Message);
#endif
            }
        }

        public void Dispose()
        {

            foreach(var shader in Shaders.Values)
            {
                shader.Dispose();
            }

            Utilities.Dispose(ref Includes);
            Watcher?.Dispose();

        }
    }
}
