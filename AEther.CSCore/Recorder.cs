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

        public Recorder(int device)
            : base(CreateDevice(device))
        { }

        public static string[] GetAvailableDevices()
        {
            using var enumerator = new MMDeviceEnumerator();
            using var endpoints = enumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active);
            return endpoints
                .Select(device => device.FriendlyName)
                .ToArray();
        }

        static WasapiCapture CreateDevice(int device)
        {

            using var enumerator = new MMDeviceEnumerator();
            using var endpoints = enumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active);
            var endpoint = endpoints[device];

            var eventSync = false;
            var mode = AudioClientShareMode.Shared;
            var format = new WaveFormat();
            var latency = 0;
            var priority = ThreadPriority.Normal;
            return new WasapiCapture(eventSync, mode, latency, format, priority)
            {
                Device = endpoint
            };

        }

    }
}
