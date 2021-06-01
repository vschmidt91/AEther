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
        public double TimingAmount { get; set; } = .95;

        [Category("Pipeline")]
        public int TimingSpins { get; set; } = 1 << 8;

        [Category("Domain")]
        public double MinFrequency { get; set; } = 27.5;

        [Category("Domain")]
        public double MaxFrequency { get; set; } = 12000;

        [Category("Domain")]
        public int FrequencyResolution { get; set; } = 12;

        [Category("DFT")]
        public int TimeResolution { get; set; } = 1 << 8;

        [Category("DFT")]
        public int MaxParallelization { get; set; } = 1;

        [Category("DFT")]
        public bool UseSIMD { get; set; } = true;

        [Category("Splitter")]
        public double FrequencyWindow { get; set; } = 1;

        [Category("Splitter")]
        public double TimeWindow { get; set; } = .03;

        [Category("DMX")]
        public int DMXPort { get; set; } = 0;

        [Category("DMX")]
        public double SinuoidThreshold { get; set; } = 0.1;

        [Category("DMX")]
        public double TransientThreshold { get; set; } = 0.05;

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
