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
            #if DEBUG
            CapturedImageBox.FunctionalMode = ImageBox.FunctionalModeOption.Everything;
            #else
            CapturedImageBox.FunctionalMode = ImageBox.FunctionalModeOption.PanAndZoom;
            #endif
            startCapture();
            timerSetup();
        }
        DispatcherTimer timer;
        private void timerSetup()
        {
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(ProcessFrame);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 33);
            timer.Start();
        }
        #region Display Image
        private CameraManagement camManager = null;
        TextBoxOutputter textBoxOutput;
        /// <summary>
        /// Initializes the capture and handles involved exceptions.
        /// </summary>
        private void startCapture()
        {
            textBoxOutput = new TextBoxOutputter(TextBoxConsole);
            Console.SetOut(textBoxOutput);
            Console.WriteLine("Initialized custom console output. Enjoy!");
            //Disable OpenCL rendering and catch exceptions if CvInvoke.dll isn't found.
            try { CvInvoke.UseOpenCL = false; }
            catch (TypeInitializationException ex) { MessageBox.Show(
                "An exception has occured. Did you include the necessary 64-bit DLLs from EMGU?\n"
                + "\nBEGIN MESSAGE \n ================\n" + ex.Message); }

            //Create a new capture and attach an event to it.
            try { camManager = new CameraManagement(); }
            catch (NullReferenceException ex) { MessageBox.Show("Camera Manager could not be instantiated.\nBEGIN MESSAGE\n============\n" + ex.Message); }
            setDimensions();
            setOptimalProperties();
            timerSetup();
        }
        private void setDimensions()
        {
            int ratio = 2;
            Mat frame = new Mat();
            camManager.Retrieve(frame, 0);
            CapturedImageBox.Width = frame.Width / ratio;
            CapturedImageBox.Height = frame.Height / ratio;
            Console.WriteLine("Width of frame: {0} Width of ImageBox: {1} Final width: {2}", frame.Width, CapturedImageBox.Width, frame.Width / ratio);
            Console.WriteLine("Height of frame: {0} Height of ImageBox: {1} Final height: {2}", frame.Height, CapturedImageBox.Height, frame.Height / ratio);
            SourceCanvas.Width = frame.Width / ratio;
            SourceCanvas.Height = frame.Height / ratio;
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
        private void ProcessFrame(object sender, EventArgs arg)
        {
            Mat frame = new Mat();
            camManager.Retrieve(frame, 0);
            sendFrame(frame);
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
            catch(NullReferenceException ex) { }
            try
            {
                if (camManager.exposure >= -3)
                {
                    Console.WriteLine("WARNING: FPS will significantly drop due to overexposure!");
                }
            }
            catch(NullReferenceException ex) { }
        }

        private void brightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try { camManager.brightness = e.NewValue; Console.WriteLine("Brightness Slider: {0} Value set: {1}", e.NewValue, camManager.brightness); }
            catch (NullReferenceException ex) { }
        }
        private void focusSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try { camManager.focus = e.NewValue; Console.WriteLine("Focus Slider: {0} Value set: {1}", e.NewValue, camManager.focus); }
            catch (NullReferenceException ex) { }
        }
        #endregion
    }
}
