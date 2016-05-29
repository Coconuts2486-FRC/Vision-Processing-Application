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
        /// <summary>
        /// Initializes the capture and handles involved exceptions.
        /// </summary>
        private void startCapture()
        {
            //Disable OpenCL rendering and catch exceptions if CvInvoke.dll isn't found.
            try { CvInvoke.UseOpenCL = false; }
            catch (System.TypeInitializationException ex) { MessageBox.Show(
                "An exception has occured. Did you include the necessary 64-bit DLLs from EMGU?\n"
                + "\nBEGIN MESSAGE \n ================\n" + ex.Message); }

            //Create a new capture and attach an event to it.
            try { capture = new Capture(); capture.ImageGrabbed += ProcessFrame;}
            catch (NullReferenceException ex) { MessageBox.Show(ex.Message); }
        }

        private void ProcessFrame(object sender, EventArgs arg)
        {
            Mat frame = new Mat();
            capture.Retrieve(frame, 0);
            sendFrame(frame);
            textBlock.Text = "Processing...";
        }
        private void sendFrame(Mat frame)
        {
            CapturedImageBox.Image = frame;
            textBlock.Text = "Sending...";
        }
        private void ReleaseData()
        {
            if (capture != null)
            {
                capture.Dispose();
            }
        }
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (capture != null)
            {
                if (captureInProgress == true)
                {
                    startButton.Content = "Start Capture";
                    capture.Pause();
                }
                else
                {
                    startButton.Content = "Stop";
                    capture.Start();
                }
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
