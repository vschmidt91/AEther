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
using System.Drawing;

namespace AEther.WindowsForms
{
    public class ShaderManager : GraphicsComponent, IDisposable
    {

        public event EventHandler<FileSystemEventArgs>? FileChanged;

        readonly IncludeHandler Includes;
        readonly FileSystemWatcher? Watcher;
        readonly string BasePath;

        protected bool IsDisposed;

        public ShaderManager(Graphics graphics, string basePath, bool watch = false)
            : base(graphics)
        {

            BasePath = basePath;

            //var includes = Directory.EnumerateFiles(basePath, "*.fxi", SearchOption.AllDirectories);
                //.ToDictionary(path => new FileInfo(path).Name, File.ReadAllText);
            Includes = new IncludeHandler(Path.Join(basePath, "include"));

            if (watch)
            {
                Watcher = new(basePath)
                {
                    NotifyFilter = NotifyFilters.LastWrite,
                    EnableRaisingEvents = true,
                };
                Watcher.Changed += (obj, evt) => FileChanged?.Invoke(this, evt);
            }

        }

        public ShaderBytecode Compile(string key, ShaderMacro[]? macros = null)
        {

            var path = Path.Join(BasePath, key);
            var file = new FileInfo(path);

            ShaderFlags shaderFlags = ShaderFlags.None;
            EffectFlags effectFlags = EffectFlags.None;

#if DEBUG
            shaderFlags |= ShaderFlags.EnableStrictness;
            shaderFlags |= ShaderFlags.Debug;
#else
            shaderFlags |= ShaderFlags.OptimizationLevel3;
#endif

            var profile = "fx_5_0";
            var sourceCode = File.ReadAllText(file.FullName);

            using var compiled = ShaderBytecode.Compile(sourceCode, profile, shaderFlags, effectFlags, macros ?? Array.Empty<ShaderMacro>(), Includes, file.Name);

            if (compiled.Bytecode is null)
            {
                throw new CompilationException(compiled.Message);
            }

            //#if DEBUG
            //            Debug.WriteLine(file.Name);
            //            Debug.Write(compiled.Bytecode.Disassemble(DisassemblyFlags.EnableInstructionNumbering));
            //#endif

            return compiled.Bytecode;

        }

        public void Dispose()
        {
            if(!IsDisposed)
            {
                Includes.Dispose();
                Watcher?.Dispose();
                GC.SuppressFinalize(this);
                IsDisposed = true;
            }
        }
    }
}
