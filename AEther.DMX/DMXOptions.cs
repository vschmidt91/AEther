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

namespace AEther.DMX
{
    public class DMXOptions
    {

        public bool Enabled { get; set; } = false;
        public int COMPort { get; set; } = 0;
        public double SinuoidThreshold { get; set; } = 0.1;
        public double TransientThreshold { get; set; } = 0.1;
        
    }
}
