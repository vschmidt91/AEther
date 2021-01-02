﻿using System;
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

        [Category("Domain")]
        public float MinFrequency { get; set; } = 30f;

        [Category("Domain")]
        public float MaxFrequency { get; set; } = 12000f;

        [Category("Domain")]
        public int FrequencyResolution { get; set; } = 12;


        [Category("DFT")]
        public int TimeResolution { get; set; } = 1 << 8;

        [Category("DFT")]
        public int MaxParallelization { get; set; } = -1;

        [Category("DFT")]
        public bool UseSIMD { get; set; } = false;

        [Category("Splitter")]
        public float FrequencyWindow { get; set; } = 1f;

        [Category("Splitter")]
        public float TimeWindow { get; set; } = .03f;

        [Category("Normalizer")]
        public float NormalizerFloorRoom { get; set; } = 0f;

        [Category("Normalizer")]
        public float NormalizerHeadRoom { get; set; } = .05f;

        public static SessionOptions? ReadFromFile(string path)
        {

            if(!File.Exists(path))
            {
                return default;
            }

            SessionOptions? result = default;

            var file = new FileInfo(path);
            var serializer = new XmlSerializer(typeof(SessionOptions));
            using (var stream = file.Open(FileMode.Open, FileAccess.Read))
            {
                try
                {
                    result = serializer.Deserialize(stream) as SessionOptions;
                }
                catch(InvalidOperationException exc)
                {
                    Debug.WriteLine("ERROR reading configuration: " + exc.Message);
                }
            }

            return result;

        }

        public void WriteToFile(string path)
        {

            var file = new FileInfo(path);
            var serializer = new XmlSerializer(typeof(SessionOptions));
            using var stream = file.Open(FileMode.OpenOrCreate, FileAccess.Write);
            try
            {
                serializer.Serialize(stream, this);
            }
            catch (XmlException exc)
            {
                Debug.WriteLine("ERROR writing configuration: " + exc.Message);
            }

        }
        
    }
}