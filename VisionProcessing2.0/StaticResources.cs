using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionProcessing2._0
{
    static class StaticResources
    {
        public static string NetworkTablesAddress { get; set; }

        public static bool SnapshotEnabled = false;

        public static double minBrightness;
        public static double maxBrightness;
        public static double minExposure;
        public static double maxExposure;
        public static double minFocus;
        public static double maxFocus;

        public static List<double> ContourRatio = new List<double>();
    }
}
