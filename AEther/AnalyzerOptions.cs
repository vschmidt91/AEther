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
    public class AnalyzerOptions
    {

        public Domain Domain => Domain.FromRange(MinFrequency, MaxFrequency, FrequencyResolution);

        [Category("Pipeline")]
        public int BufferCapacity { get; set; } = -1;

        [Category("Pipeline")]
        public double MicroTimingAmount { get; set; } = .95;

        [Category("Domain")]
        public double MinFrequency { get; set; } = 27.5;

        [Category("Domain")]
        public double MaxFrequency { get; set; } = 8000;

        [Category("Domain")]
        public int FrequencyResolution { get; set; } = 8;

        [Category("DFT")]
        public int TimeResolution { get; set; } = 1 << 8;

        [Category("DFT")]
        public int MaxParallelization { get; set; } = 0;

        [Category("DFT")]
        public bool SIMDEnabled { get; set; } = true;

        [Category("Splitter")]
        public double SinuoidWidth { get; set; } = 1;

        [Category("Splitter")]
        public double SinuoidLength { get; set; } = 0.2;

        [Category("Splitter")]
        public double TransientWidth { get; set; } = 2.0;

        [Category("Splitter")]
        public double TransientLength { get; set; } = .05;

        public static AnalyzerOptions ReadFromFile(string path)
        {

            var file = new FileInfo(path);
            var serializer = new XmlSerializer(typeof(AnalyzerOptions));
            using var stream = file.Open(FileMode.Open, FileAccess.Read);
            var result = serializer.Deserialize(stream);
            if (result is not AnalyzerOptions options)
                throw new InvalidCastException();
            return options;

        }

        public void WriteToFile(string path)
        {

            var file = new FileInfo(path);
            var serializer = new XmlSerializer(typeof(AnalyzerOptions));
            using var stream = file.Open(FileMode.OpenOrCreate, FileAccess.Write);
            serializer.Serialize(stream, this);

        }
        
    }
}
