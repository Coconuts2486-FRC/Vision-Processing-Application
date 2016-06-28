using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionProcessing2._0
{
    class HSVFilter
    {
        public enum Context
        {
            lowerHue,
            upperHue,
            lowerSaturation,
            upperSaturation,
            lowerBrightness,
            upperBrightness
        }
        public event EventHandler Changed;
        public HSVFilter()
        {
            
        }
        public Hsv lowerFilter;
        public Hsv upperFilter;

    }
}
