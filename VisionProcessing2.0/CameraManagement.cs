using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionProcessing2._0
{
    public class CameraManagement : Capture
    {
        public CameraManagement()
        {
            
        }
        #region Getters and setters
        public DataHolder dataHolder = new DataHolder();
        public double brightness
        {
            get { return GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness); }
            set { SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, value); dataHolder.brightness = value; }
        }
        public double exposure
        {
            get { return GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure); }
            set { SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, value); dataHolder.exposure = value; }
        }
        public double focus
        {
            get { return GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus); }
            set { SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, value); dataHolder.focus = value; }
        }
        public double fps
        {
            get { return GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps); }
            set { SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps, value); dataHolder.fps = value; }
        }
        #endregion
    }
}
