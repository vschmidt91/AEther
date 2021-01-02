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
    public class ShaderManager : GraphicsComponent, IDisposable
    {

        public Shader this[string key] => GetShader(key);

        public IEnumerable<string> Keys => Shaders.Keys;

        readonly Dictionary<string, Shader> Shaders = new Dictionary<string, Shader>();
        readonly IncludeHandler Includes;
        readonly FileSystemWatcher? Watcher;
        readonly string BasePath;

        public ShaderManager(Graphics graphics, string basePath, bool watch = false, bool preload = true)
            : base(graphics)
        {

            if (!Directory.Exists(basePath))
            {
                throw new DirectoryNotFoundException();
            }

            BasePath = basePath;

            var includes = Directory.EnumerateFiles(basePath, "*.fxi", SearchOption.AllDirectories)
                .ToDictionary(path => new FileInfo(path).Name, File.ReadAllText);
            Includes = new IncludeHandler(includes);

            if(preload)
            {
                var effects = Directory.EnumerateFiles(basePath, "*.fx", SearchOption.AllDirectories);
                foreach (var effect in effects)
                {
                    var path = new FileInfo(effect);
                    Shaders[path.Name] = LoadShader(path.FullName);
                }
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
                Watcher.Changed += Watcher_Changed;
                Watcher.EnableRaisingEvents = true;
            }

        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Name))
                return;
            for(int i = 0; i < 10; ++i)
            {
                try
                {
                    Shaders[e.Name] = LoadShader(e.FullPath);
                }
                catch(IOException)
                {
                    Thread.Sleep(100);
                    continue;
                }
                catch(CompilationException exc)
                {
#if DEBUG
                    MessageBox.Show(null, exc.Message, string.Empty);
#else
                    Debug.WriteLine(exc.Message);
#endif
                }
                return;
            }
        }

        private Shader GetShader(string key)
        {
            if(Shaders.TryGetValue(key, out var shader))
            {
                return shader;
            }
            else
            {
                var path = Path.Join(BasePath, key);
                return Shaders[key] = LoadShader(path);
            }
        }

        private Shader LoadShader(string path)
        {

            FileInfo file = new FileInfo(path);

            ShaderFlags shaderFlags = ShaderFlags.None;
            EffectFlags effectFlags = EffectFlags.None;

#if DEBUG
            shaderFlags |= ShaderFlags.Debug;
#else
            shaderFlags |= ShaderFlags.OptimizationLevel3;
#endif

            var profile = "fx_5_0";
            var sourceCode = File.ReadAllText(file.FullName);
            var macros = Array.Empty<ShaderMacro>();

            using var compiled = ShaderBytecode.Compile(sourceCode, profile, shaderFlags, effectFlags, macros, Includes, file.Name);

            if (compiled.Bytecode is null)
            {
                throw new CompilationException(compiled.Message);
            }

#if DEBUG
            //File.WriteAllText(file.FullName + ".txt", compiled.Bytecode.Disassemble(DisassemblyFlags.EnableInstructionNumbering));
#endif

            var shader = new Shader(Graphics.Device, compiled.Bytecode)
            {
                Name = file.Name,
            };

            shader.ConstantBuffers[0].SetConstantBuffer(Graphics.FrameConstants.Buffer);

            return shader;

        }

        public void Dispose()
        {

            foreach(var shader in Shaders.Values)
            {
                shader.Dispose();
            }

            Includes.Dispose();
            Watcher.Dispose();

        }
    }
}
