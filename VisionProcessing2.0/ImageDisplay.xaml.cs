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
        }

        #region Display Image
        private Capture capture = null;
        private bool captureInProgress;
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
            try { capture = new Capture(); }
            catch (NullReferenceException ex) { MessageBox.Show(ex.Message); }
            setDimensions();
        }
        private void setDimensions()
        {
            Mat frame = new Mat();
            capture.Retrieve(frame, 0);
            CapturedImageBox.Width = frame.Width;
            CapturedImageBox.Height = frame.Height;
            Console.WriteLine("Width of frame: {0} Width of ImageBox: {1}", frame.Width, CapturedImageBox.Width);
            Console.WriteLine("Height of frame: {0} Height of ImageBox: {1}", frame.Height, CapturedImageBox.Height);
            column1.Width = new GridLength(frame.Width, GridUnitType.Pixel);
        }
        private void setOptimalProperties()
        {
            
        }
        private void ProcessFrame(object sender, EventArgs arg)
        {
            Mat frame = new Mat();
            capture.Retrieve(frame, 0);
            sendFrame(frame);
        }
        private void sendFrame(Mat frame)
        {
            CapturedImageBox.Image = frame;
        }
        private void ReleaseData()
        {
            if (capture != null)
            {
                capture.Dispose();
            }
        }
        #endregion
        DispatcherTimer timer;
        private void WindowsFormsHost_Loaded(object sender, RoutedEventArgs e)
        {
            startCapture();
            timer = new DispatcherTimer();
            timer.Tick += new EventHandler(ProcessFrame);
            timer.Interval = new TimeSpan(0, 0, 0, 0, 1);
            timer.Start();
        }
    }
}
