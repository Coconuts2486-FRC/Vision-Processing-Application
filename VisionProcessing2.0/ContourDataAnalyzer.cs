using Emgu.CV.Util;
using Emgu;
using System;
using System.Collections.Generic;
using NetworkTables;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;

namespace VisionProcessing2._0
{
    public class ContourDataAnalyzer
    {
        System.Drawing.Point[][] Points;
        VectorOfVectorOfPoint Contours;
        public NetworkTable table;

        public ContourDataAnalyzer()
        {
            //StaticResources.NetworkTablesAddress = "127.0.0.1";
            NetworkTable.SetClientMode();
            NetworkTable.SetUpdateRate(0.1);
            NetworkTable.SetNetworkIdentity("CocoNutsImageProcessing");
            NetworkTable.SetIPAddress(StaticResources.NetworkTablesAddress);
            table = NetworkTable.GetTable("CamData");
        }

        int threshold = 20;
        public void UpdateInfo(VectorOfVectorOfPoint VVP)
        {
            Contours = VVP;
            rect = new System.Drawing.Rectangle[Contours.Size];
            if(Contours.Size <= threshold)
            {
                masterHeights = new List<double>();
                masterWidths = new List<double>();
                masterArea = new List<double>();
                masterCenterX = new List<double>();
                masterCenterY = new List<double>();

                Points = VVP.ToArrayOfArray();
                getRectangles();
                publish();
            }
            else Console.WriteLine("Threshold of {0} contours has been met. \nStopping processing of contours.", threshold);
            //iterate();
        }

        System.Drawing.Rectangle[] rect;
        int count;
        private void getRectangles()
        {
            count = 0;
            foreach (System.Drawing.Point[] point in Points)
            {
                if(count % 2 == 0)
                {
                    System.Drawing.PointF[] pointF = Array.ConvertAll(point, new Converter<System.Drawing.Point, System.Drawing.PointF>(PointToPointF));
                    rect[count] = PointCollection.BoundingRectangle(pointF);
                    getInfo(count);
                }
                count++;
            }
        }

        List<double> masterHeights;
        List<double> masterWidths;
        List<double> masterArea;
        List<double> masterCenterX;
        List<double> masterCenterY;
        private void getInfo(int i)
        {
            if(StaticResources.SnapshotEnabled == true)
            {
                //Console.WriteLine(StaticResources.ContourRatio);
                double height = rect[i].Height;
                double width = rect[i].Width;
                double ratio = width / height;
                foreach (double rat in StaticResources.ContourRatio)
                {
                    if ((width / height) / rat <= 1.05 && (width / height) / rat >= 0.95)
                    {
                        masterWidths.Add(rect[i].Width);
                        masterHeights.Add(rect[i].Height);
                        masterArea.Add(rect[i].Width * rect[i].Height);
                        masterCenterX.Add(rect[i].X + (rect[i].Width / 2));
                        masterCenterY.Add(rect[i].Y + (rect[i].Height / 2));
                        break; 
                    }
                }
            }
            else
            {
                masterWidths.Add(rect[i].Width);
                masterHeights.Add(rect[i].Height);
                masterArea.Add(rect[i].Width * rect[i].Height);
                masterCenterX.Add(rect[i].X + (rect[i].Width / 2));
                masterCenterY.Add(rect[i].Y + (rect[i].Height / 2));
            }
        }

        public void AddSnapshot()
        {
            double height = rect[0].Height;
            double width = rect[0].Width;
            double ratio = width / height;
            StaticResources.ContourRatio.Add(ratio);
        }

        private void publish()
        {
            table.PutNumberArray("Height", masterHeights.ToArray());
            table.PutNumber("Height Count", masterHeights.Count);
            table.PutNumberArray("Width", masterWidths.ToArray());
            table.PutNumber("Width Count", masterWidths.Count);
            table.PutNumberArray("Area", masterArea.ToArray());
            table.PutNumberArray("CenterX", masterCenterX.ToArray());
            table.PutNumberArray("CenterY", masterCenterY.ToArray());
        }

        private static System.Drawing.PointF PointToPointF(System.Drawing.Point point)
        {
            return point;
        }
    }
}
