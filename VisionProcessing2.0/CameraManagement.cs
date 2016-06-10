using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisionProcessing2._0
{
    class CameraManagement : Capture
    {
        public CameraManagement()
        {
            setInitialSettings();
        }
        #region Camera Properties
        private double cBrightness;
        private double cExposure;
        private double cFocus;
        private double cFPS;
        private void setInitialSettings()
        {
            cBrightness = brightness;
            cExposure = exposure;
            cFocus = focus;
            cFPS = fps;
        }
        public double brightness
        {
            get { return GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness); }
            set { SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Brightness, value); }
        }
        public double exposure
        {
            get { return GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure); }
            set { SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Exposure, value); }
        }
        public double focus
        {
            get { return GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus); }
            set { SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Focus, value); }
        }
        public double fps
        {
            get { return GetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps); }
            set { SetCaptureProperty(Emgu.CV.CvEnum.CapProp.Fps, value); }
        }
        #endregion
    }
}
