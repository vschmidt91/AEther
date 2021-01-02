﻿using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NAudio.Dmo;
using NAudio.Wave;

namespace AEther.NAudio
{
    public static class Player
    {

        static WaveStream CreateInput(string path)
        {
            var file = new FileInfo(path);
            return file.Extension switch
            {
                ".wav" => new WaveFileReader(path),
                ".mp3" => new Mp3FileReader(path),
                _ => throw new Exception(),
            };
        }

    }
}
