using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.UI;
using System.Windows.Threading;
using Emgu.CV.Structure;

namespace VisionProcessing2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //If application starts in a debug environment, allow all tools for the ImageBox.
            //Note: Disabled for now. I like the FPS viewer and histogram.
//            #if DEBUG
//            CapturedImageBox.FunctionalMode = ImageBox.FunctionalModeOption.Everything;
//#else
//            CapturedImageBox.FunctionalMode = ImageBox.FunctionalModeOption.PanAndZoom;
//#endif
            getAvailableCameras();
            timerSetup();
            console();
            startCapture();
        }

        private void getAvailableCameras()
        {
            //Implement later! Requires DirectShow.net library

            //DsDevice[] _SystemCamereas = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            //WebCams = new Video_Device[_SystemCamereas.Length];
            //for (int i = 0; i < _SystemCamereas.Length; i++)
            //{
            //    WebCams[i] = new Video_Device(i, _SystemCamereas[i].Name, _SystemCamereas[i].ClassID); //fill web cam array
            //    Camera_Selection.Items.Add(WebCams[i].ToString());
            //}
            //if (Camera_Selection.Items.Count > 0)
            //{
            //    Camera_Selection.SelectedIndex = 0; //Set the selected device the default
            //    captureButton.Enabled = true; //Enable the start
            //}
        }

        DispatcherTimer timer;
        DispatcherTimer timerGC;
        /// <summary>
        /// Initializes a timer that runs off the system clock, and on each interval (1/30 of a second) run ProcessSource method.
        /// </summary>
        private void timerSetup()
        {
            timerGC = new DispatcherTimer();
            timerGC.Tick += TimerGC_Tick;
            timerGC.Interval = new TimeSpan(0, 0, 1);
            timerGC.Start();
            timer = new DispatcherTimer();
            //Defines the method to run when the interval is met.
            timer.Tick += new EventHandler(ProcessSource);
            //Defines the interval as 1/30 of a second, which allows for 30 fps.
            timer.Interval = new TimeSpan(0, 0, 0, 1/30, 0);
            timer.Start();
        }

        private void TimerGC_Tick(object sender, EventArgs e)
        {
            GC.Collect();
        }
        #region Display Image
        private CameraManagement camManager = null;
        TextBoxOutputter textBoxOutput;
        /// <summary>
        /// Initializes the capture and handles involved exceptions.
        /// </summary>
        private void console()
        {
            textBoxOutput = new TextBoxOutputter(TextBoxConsole);
            Console.SetOut(textBoxOutput);
            Console.WriteLine("Initialized custom console output. Enjoy!");
        }
        private void startCapture()
        {
            //Disable OpenCL processing and catch exceptions if CvInvoke.dll isn't found.
            try { CvInvoke.UseOpenCL = false; }
            catch (TypeInitializationException ex)
            {
                MessageBox.Show("An exception has occured. Did you include the necessary 64-bit DLLs from EMGU?\n"
                + "\nBEGIN MESSAGE \n ================\n" + ex.Message);
            }

            //Create a new capture and attach an event to it.
            try { camManager = new CameraManagement(); }
            catch (NullReferenceException ex) { MessageBox.Show("Camera Manager could not be instantiated.\nBEGIN MESSAGE\n============\n" + ex.Message); }
            setDimensions();
            setOptimalProperties();
        }
        int ratio;
        int height;
        int width;
        private void setDimensions()
        {
            //Defines a ratio. > 1 reduces the total size, < 1 enlarges.
            ratio = 2;
            Mat frame = new Mat();
            camManager.Retrieve(frame, 0);
            CapturedImageBox.Width = frame.Width / ratio;
            CapturedImageBox.Height = frame.Height / ratio;
            MedianImageBox.Width = frame.Width / ratio;
            MedianImageBox.Height = frame.Height / ratio;
            height = frame.Height / ratio;
            width = frame.Width / ratio;
            Console.WriteLine("Width of frame: {0} Width of ImageBox: {1} Final width: {2}", frame.Width, CapturedImageBox.Width, frame.Width / ratio);
            Console.WriteLine("Height of frame: {0} Height of ImageBox: {1} Final height: {2}", frame.Height, CapturedImageBox.Height, frame.Height / ratio);
            SourceCanvas.Width = frame.Width / ratio;
            SourceCanvas.Height = frame.Height / ratio;
            SourceCanvas.MaxHeight = frame.Width / ratio;
            SourceCanvas.MaxWidth = frame.Width / ratio;
            MedianCanvas.Width = frame.Width / ratio;
            MedianCanvas.Height = frame.Height / ratio;
            TextBoxConsole.MaxHeight = frame.Height / ratio;
            Console.WriteLine("Image zoom changed to {0}", CapturedImageBox.ZoomScale);
            CapturedImageBox.OnZoomScaleChange += zoomScaleUpdated;
            CapturedImageBox.SetZoomScale(1/ratio, new System.Drawing.Point(0, 0));
        }
        private void setOptimalProperties()
        {
            camManager.exposure = -9;
            exposureSlider.Value = -9;
            camManager.brightness = 110;
            brightnessSlider.Value = 110;
        }
        private void zoomScaleUpdated(object sender, EventArgs arg)
        {
            Console.WriteLine("Image zoom changed to {0}", CapturedImageBox.ZoomScale);
        }
        private void ProcessSource(object sender, EventArgs arg)
        {
            //Forces the renderer to invalidate the screen, which then forces a redraw, and then in turn increases the camera FPS. Programming.
            InvalidateVisual();
            Mat frame = new Mat();
            camManager.Retrieve(frame, 0);
            sendFrame(frame);
            ProcessMedian(frame);
        }
        private int medianTolerance = 1;
        private void ProcessMedian(Mat frame)
        {
            using (Image<Rgba, Byte> frameImage = frame.ToImage<Rgba, Byte>())
            {
                Image<Hsv, Byte> rgbFrame = frameImage.Convert<Hsv, Byte>();
                CvInvoke.MedianBlur(frame, frame, medianTolerance);
                MedianImageBox.Image = frame;
            }
        }
        private void ProcessHSV()
        {
            //CvInvoke.CvtColor(frame, HSVFrame, ColorConversion.Bgr2Hsv);
            //Image<Hsv, byte> imageHSVFrame = HSVFrame.ToImage<Hsv, byte>();
            //imageHSVFrame.ThresholdToZero(lower);
            //imageHSVFrame.ThresholdToZeroInv(upper);
            //Image<Gray, byte> filteredImageHSV = imageHSVFrame.InRange(lower, upper);
            //hsvImageBox.Image = filteredImageHSV;
        }
        private void sendFrame(Mat frame)
        {
            CapturedImageBox.Image = frame;
        }
        #endregion
        #region Camera Settings Buttons
        private void exposureSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try { camManager.exposure = e.NewValue; Console.WriteLine("Exposure Slider: {0} Value set: {1}", e.NewValue, camManager.exposure); }
            catch(NullReferenceException) {  }
            try
            {
                if (camManager.exposure >= -3)
                {
                    Console.WriteLine("WARNING: FPS will significantly drop due to overexposure!");
                }
            }
            catch(NullReferenceException) { }
        }

        private void brightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try { camManager.brightness = e.NewValue; Console.WriteLine("Brightness Slider: {0} Value set: {1}", e.NewValue, camManager.brightness); }
            catch (NullReferenceException) { }
        }
        private void focusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try { camManager.focus = e.NewValue; Console.WriteLine("Focus Slider: {0} Value set: {1}", e.NewValue, camManager.focus); }
            catch (NullReferenceException) { }
        }
        private void medianSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if(medianTolerance % 2 == 1)
            {
                medianTolerance = (int)e.NewValue;
                Console.WriteLine("Median Slider: {0} Value set: {1}", e.NewValue, medianTolerance);
            }
            else
            {
                Console.WriteLine("Median Slider: {0} Value set: {1}", e.NewValue, medianTolerance);
            }
        }
        #endregion
        private void ImageTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem currentTab = ImageTabControl.SelectedItem as TabItem;
            if(currentTab == ImageTabControlSource)
            {
                CapturedImageBox.Width = width;
                SourceCanvas.MaxWidth = width;
                CapturedImageBox.Height = height;
                SourceCanvas.MaxHeight = height;
            }
            else if(currentTab == ImageTabControlMedian)
            {
                MedianImageBox.Width = width;
                MedianCanvas.MaxWidth = width;
                MedianImageBox.Height = height;
                MedianCanvas.MaxHeight = height;
            }
            else if(currentTab == ImageTabControlHSV)
            {
                HSVImageCanvas.Width = width;
                HSVImageCanvas.Height = height;
            }
        }

    }
}
