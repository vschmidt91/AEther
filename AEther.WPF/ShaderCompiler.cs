using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace AEther.WPF
{
    public static class ShaderCompiler
    {

        public static void Compile(string sourcePath, string destinationPath)
        {

            var fxcPath = Path.Join(Directory.GetCurrentDirectory(), "data", "fx", "fxc.exe");
            var fxcProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fxcPath,
                    Arguments = "/T ps_3_0 /Fo \"" + destinationPath + "\" \"" + sourcePath + "\"",
                    RedirectStandardError = true,
                }
            };
            fxcProcess.Start();
            fxcProcess.WaitForExit();
            if(fxcProcess.ExitCode != 0)
            {
                var error = fxcProcess.StandardError.ReadToEnd();
                throw new Exception(error);
            }

        }

    }
}
