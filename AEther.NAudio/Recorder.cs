using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NAudio.Wave;

namespace AEther.NAudio
{
    public class Recorder : Input
    {

        readonly string Name;

        public Recorder(WaveInCapabilities device)
            : base(CreateDevice(device))
        {
            Name = device.ProductName;
        }

        public static WaveInCapabilities[] GetAvailableDevices()
        {
            return Enumerable.Range(0, WaveIn.DeviceCount)
                .Select(i => WaveIn.GetCapabilities(i))
                .ToArray();
        }

        static WaveIn CreateDevice(WaveInCapabilities device)
        {
            var devices = GetAvailableDevices();
            var deviceIndex = Array.FindIndex(devices, d => d.NameGuid == device.NameGuid);
            return new WaveIn()
            {
                DeviceNumber = deviceIndex
            };
        }

        public override string ToString()
            => Name;

    }
}
