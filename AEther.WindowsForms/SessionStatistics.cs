using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace AEther.WindowsForms
{
    public class SessionStatistics
    {

        [Category("Audio")]
        public int AudioEvents { get; set; }

        [Category("Audio")]
        public string? Key { get; set; }

        [Category("Graphics")]
        public double GraphicsTime { get; set; }

        [Category("Graphics")]
        public int GraphicsEvents { get; set; }

        [Category("Graphics")]
        public int GraphicsBandwidth { get; set; }

    }
}
