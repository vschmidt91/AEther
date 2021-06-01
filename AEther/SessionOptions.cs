using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace AEther
{
    [Serializable]
    public class SessionOptions
    {

        public Domain Domain => Domain.FromRange(MinFrequency, MaxFrequency, FrequencyResolution);

        [Category("Pipeline")]
        public int BufferCapacity { get; set; } = -1;

        [Category("Pipeline")]
        public float TimingAmount { get; set; } = .9f;

        [Category("Pipeline")]
        public int TimingSpinwaits { get; set; } = 100;

        [Category("Domain")]
        public float MinFrequency { get; set; } = 27.5f;

        [Category("Domain")]
        public float MaxFrequency { get; set; } = 12000f;

        [Category("Domain")]
        public int FrequencyResolution { get; set; } = 24;

        [Category("DFT")]
        public int TimeResolution { get; set; } = 1 << 10;

        [Category("DFT")]
        public int MaxParallelization { get; set; } = -1;

        [Category("DFT")]
        public bool UseSIMD { get; set; } = true;

        [Category("Splitter")]
        public float FrequencyWindow { get; set; } = 1f;

        [Category("Splitter")]
        public float TimeWindow { get; set; } = .03f;

        [Category("DMX")]
        public int DMXPort { get; set; } = 0;

        [Category("DMX")]
        public float SinuoidThreshold { get; set; } = 0.1f;

        [Category("DMX")]
        public float TransientThreshold { get; set; } = 0.01f;

        public static SessionOptions ReadFromFile(string path)
        {

            var file = new FileInfo(path);
            var serializer = new XmlSerializer(typeof(SessionOptions));
            using var stream = file.Open(FileMode.Open, FileAccess.Read);
            var result = serializer.Deserialize(stream);
            if (result is not SessionOptions options)
                throw new InvalidCastException();
            return options;

        }

        public void WriteToFile(string path)
        {

            var file = new FileInfo(path);
            var serializer = new XmlSerializer(typeof(SessionOptions));
            using var stream = file.Open(FileMode.OpenOrCreate, FileAccess.Write);
            serializer.Serialize(stream, this);

        }
        
    }
}
