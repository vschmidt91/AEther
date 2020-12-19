using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace AEther.WPF
{
    public class TestEffect : ShaderEffect
    {

        public static readonly DependencyProperty LeftProperty = RegisterPixelShaderSamplerProperty("Left", typeof(TestEffect), 0);
        public static readonly DependencyProperty RightProperty = RegisterPixelShaderSamplerProperty("Right", typeof(TestEffect), 1);

        public Brush Left
        {

            get => ((Brush)(GetValue(LeftProperty)));
            set => SetValue(LeftProperty, value);
        }
        public Brush Right
        {

            get => ((Brush)(GetValue(RightProperty)));
            set => SetValue(RightProperty, value);
        }

        readonly FileSystemWatcher Watcher;

        public TestEffect()
        {

            var baseDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);

#if DEBUG
            baseDir = baseDir.Parent.Parent.Parent;
#endif


            var hlslFile = new FileInfo(Path.Join(baseDir.FullName, "data", "fx", "test.hlsl"));
            CompileShader(hlslFile);

            Watcher = new FileSystemWatcher(hlslFile.Directory.FullName, hlslFile.Name);
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

        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            CompileShader(new FileInfo(e.FullPath));
        }

        private void CompileShader(FileInfo hlslFile)
        {
            var psPath = hlslFile.FullName.Substring(0, hlslFile.FullName.Length - hlslFile.Extension.Length) + ".ps";
            try
            {
                ShaderCompiler.Compile(hlslFile.FullName, psPath);
            }
            catch(Exception exc)
            {
#if DEBUG
                Debug.WriteLine(exc.Message);
#else
                MessageBox.Show(exc.Message);
#endif
            }
            Dispatcher.Invoke(() =>
            {
                PixelShader = new PixelShader
                {
                    UriSource = new Uri(psPath, UriKind.Absolute),
                };
                UpdateShaderValue(LeftProperty);
                UpdateShaderValue(RightProperty);
            });
        }

    }
}
