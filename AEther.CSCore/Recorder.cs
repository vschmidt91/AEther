using CSCore.SoundIn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using CSCore;
using CSCore.CoreAudioAPI;

namespace AEther.CSCore
{
    public class Recorder : Input
    {

        readonly string Name;

        public Recorder(MMDevice device)
            : base(CreateDevice(device))
        {
            Name = device.FriendlyName;
        }

        public override string ToString()
            => Name;

        public static MMDevice[] GetAvailableDevices()
        {
            using var enumerator = new MMDeviceEnumerator();
            using var endpoints = enumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active);
            return endpoints.ToArray();
        }

        static WasapiCapture CreateDevice(MMDevice device)
        {
            var eventSync = false;
            var mode = AudioClientShareMode.Shared;
            var format = new WaveFormat();
            var latency = 0;
            var priority = ThreadPriority.Normal;
            return new WasapiCapture(eventSync, mode, latency, format, priority)
            {
                Device = device
            };

        }

    }
}
