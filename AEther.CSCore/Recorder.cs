using CSCore;
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace AEther.CSCore
{
    public class Recorder : Input
    {

        readonly string Name;

        public Recorder(string deviceName)
            : base(CreateDevice(deviceName))
        {
            Name = deviceName;
        }

        public override string ToString()
            => Name;

        static IEnumerable<MMDevice> GetAvailableDevices()
        {
            using var enumerator = new MMDeviceEnumerator();
            using var endpoints = enumerator.EnumAudioEndpoints(DataFlow.Capture, DeviceState.Active);
            return endpoints.ToArray();
        }

        public static IEnumerable<string> GetAvailableDeviceNames()
            => GetAvailableDevices().Select(d => d.FriendlyName);

        static WasapiCapture CreateDevice(string deviceName)
        {
            var devices = GetAvailableDevices();
            var device = devices.First(d => d.FriendlyName.Equals(deviceName));
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
