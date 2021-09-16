using NAudio.Wave;
using System;
using System.IO;

namespace AEther.NAudio
{
    public static class Player
    {

        public static WaveStream CreateInput(string path)
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
