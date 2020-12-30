using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NAudio.Wave;

namespace AEther.NAudio
{
    public class Recorder : Input
    {

        public Recorder(int deviceNumber)
            : base(new WaveIn() { DeviceNumber = deviceNumber })
        { }

        public static string[] GetAvailableDevices()
        {
            return Enumerable.Range(0, WaveIn.DeviceCount)
                .Select(i => WaveIn.GetCapabilities(i))
                .Select(device => device.ProductName)
                .ToArray();
        }

    }
}
