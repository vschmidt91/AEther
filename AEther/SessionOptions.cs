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
        public int BufferCapacity { get; set; } =
#if DEBUG
            -1
#else
            16
#endif
            ;

        [Category("Pipeline")]
        public bool MicroTimingEnabled { get; set; } = true;

        [Category("Domain")]
        public double MinFrequency { get; set; } = 27.5;

        [Category("Domain")]
        public double MaxFrequency { get; set; } = 12000;

        [Category("Domain")]
        public int FrequencyResolution { get; set; } = 24;

        [Category("DFT")]
        public int TimeResolution { get; set; } = 1 << 10;

        [Category("DFT")]
        public int MaxParallelization { get; set; } = 0;

        [Category("DFT")]
        public bool SIMDEnabled { get; set; } = true;

        [Category("Splitter")]
        public double SinuoidWidth { get; set; } = .5;

        [Category("Splitter")]
        public double SinuoidLength { get; set; } = 0.2;

        [Category("Splitter")]
        public double TransientWidth { get; set; } = 2.0;

        [Category("Splitter")]
        public double TransientLength { get; set; } = .05;

        [Category("DMX")]
        public int DMXPort { get; set; } = 0;

        [Category("DMX")]
        public double SinuoidThreshold { get; set; } = 0.1;

        [Category("DMX")]
        public double TransientThreshold { get; set; } = 0.1;

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
