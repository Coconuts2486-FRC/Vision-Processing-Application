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
using Emgu.CV.CvEnum;
using Microsoft.Win32;

namespace VisionProcessing2._0
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        DateTime start = new DateTime();
        public MainWindow()
        {
            start = DateTime.Now;
            //Create a new capture and attach an event to it.
            try { camManager = new CameraManagement(); }
            catch (NullReferenceException ex) { MessageBox.Show("Camera Manager could not be instantiated.\nBEGIN MESSAGE\n============\n" + ex.Message); }
            Application.Current.Exit += new ExitEventHandler(SystemEvents_SessionEnded);
            cooldownSetup();
            InitializeComponent();
            getAvailableCameras();
            timerSetup();
            console();
            startCapture();
            bindControlFunctions();
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
        DispatcherTimer coolDown;
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
        private void cooldownSetup()
        {
            coolDown = new DispatcherTimer();
            coolDown.Tick += CoolDown_Tick;
            coolDown.Interval = new TimeSpan(0, 0, 0, 0, 10);
        }
        private void CoolDown_Tick(object sender, EventArgs e)
        {
            coolDown.Stop();
            Console.WriteLine("Cooldown met.");
        }

        private void TimerGC_Tick(object sender, EventArgs e)
        {
            GC.Collect();
        }
        #region Display Image
        public CameraManagement camManager;
        TextBoxOutputter textBoxOutput;
        /// <summary>
        /// Initializes the capture and handles involved exceptions.
        /// </summary>
        private void console()
        {
            textBoxOutput = new TextBoxOutputter(TextBoxConsole);
            Console.SetOut(textBoxOutput);
            Console.WriteLine("{0} - Application started. Have a fantastic match!", start);
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
            int ratioWidth = frame.Width / ratio;
            int ratioHeight = frame.Height / ratio;
            CapturedImageBox.Width = ratioWidth;
            CapturedImageBox.Height = ratioHeight;
            MedianImageBox.Width = ratioWidth;
            MedianImageBox.Height = ratioHeight;
            HSVImageBox.Width = ratioWidth;
            HSVImageBox.Height = ratioHeight;
            height = frame.Height / ratio;
            width = frame.Width / ratio;
            Console.WriteLine("Width of frame: {0} Width of ImageBox: {1} Final width: {2}", frame.Width, CapturedImageBox.Width, frame.Width / ratio);
            Console.WriteLine("Height of frame: {0} Height of ImageBox: {1} Final height: {2}", frame.Height, CapturedImageBox.Height, frame.Height / ratio);
            SourceCanvas.Width = width;
            SourceCanvas.Height = height;
            SourceCanvas.MaxHeight = height;
            SourceCanvas.MaxWidth = width;
            MedianCanvas.Width = width;
            MedianCanvas.Height = height;
            MedianCanvas.MaxHeight = height;
            MedianCanvas.MaxWidth = width;
            HSVImageCanvas.Width = width;
            HSVImageCanvas.Height = height;
            HSVImageCanvas.MaxHeight = height;
            HSVImageCanvas.MaxWidth = width;
            TextBoxConsole.MaxHeight = height;
            Console.WriteLine("Image zoom changed to {0}", CapturedImageBox.ZoomScale);
            CapturedImageBox.OnZoomScaleChange += zoomScaleUpdated;
            CapturedImageBox.SetZoomScale(0.5, new System.Drawing.Point(0, 0));
            MedianImageBox.SetZoomScale(0.5, new System.Drawing.Point(0, 0));
            HSVImageBox.SetZoomScale(0.5, new System.Drawing.Point(0, 0));
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
        Mat frame;
        private void ProcessSource(object sender, EventArgs arg)
        {
            //Forces the renderer to invalidate the screen, which then forces a redraw of the image, and then in turn increases the visual FPS. Programming.
            InvalidateVisual();
            frame = new Mat();
            camManager.Retrieve(frame, 0);
            CapturedImageBox.Image = frame;
            ProcessMedian(frame);
        }
        //int medianTolerance = 1;
        private void ProcessMedian(Mat frame)
        {
            Mat filtered = new Mat();
            CvInvoke.MedianBlur(frame, filtered, camManager.dataHolder.medianTolerance);
            MedianImageBox.Image = filtered;
            ProcessHSV(filtered);
        }
        HSVFilter hsvFilter = new HSVFilter();
        private void ProcessHSV(Mat frame)
        {
            using (Image<Hsv, byte> hsv = frame.ToImage<Hsv, byte>())
            {
                
                HSVImageBox.Image = hsv.InRange(hsvFilter.lowerFilter, hsvFilter.upperFilter);
            }
                
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
            try
            {
                if (camManager.dataHolder.medianTolerance % 2 == 1)
                {
                    camManager.dataHolder.medianTolerance = (int)e.NewValue;
                }
                Console.WriteLine("Median Slider: {0} Value set: {1}", e.NewValue, camManager.dataHolder.medianTolerance);
            }
            catch (NullReferenceException) { }
        }
        #endregion
        private int selectionCountHSV;
        private void ImageTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CapturedImageBox.SetZoomScale(0.5, new System.Drawing.Point(0, 0));
            MedianImageBox.SetZoomScale(0.5, new System.Drawing.Point(0, 0));
            HSVImageBox.SetZoomScale(0.5, new System.Drawing.Point(0, 0));
            TabItem currentTab = ImageTabControl.SelectedItem as TabItem;
            if(currentTab == ImageTabControlSource)
            {
                Console.WriteLine("Source tab selected.");
                CapturedImageBox.Width = width;
                SourceCanvas.MaxWidth = width;
                CapturedImageBox.Height = height;
                SourceCanvas.MaxHeight = height;
            }
            else if(currentTab == ImageTabControlMedian)
            {
                Console.WriteLine("Median tab selected.");
                MedianImageBox.Width = width;
                MedianCanvas.MaxWidth = width;
                MedianImageBox.Height = height;
                MedianCanvas.MaxHeight = height;
            }
            else if(currentTab == ImageTabControlHSV)
            {
                selectionCountHSV++;
                if (selectionCountHSV >= 2)
                {
                    Canvas.SetTop(HSVHost, -120);
                    Canvas.SetLeft(HSVHost, -160);
                }
                Console.WriteLine("HSV tab selected. Count: {0}", selectionCountHSV);
                HSVImageBox.Width = width;
                HSVImageCanvas.Width = width;
                HSVImageBox.Height = height;
                HSVImageCanvas.Height = height;
            }
        }
        #region HSV Sliders
        private void updateRangeSliders()
        {
            if (!coolDown.IsEnabled)
            {
                try
                {
                    hsvFilter.setValue((int)hueSlider.LowerValue, HSVFilter.Context.lowerHue);
                    hsvFilter.setValue((int)hueSlider.UpperValue, HSVFilter.Context.upperHue);
                    hsvFilter.setValue((int)saturationSlider.LowerValue, HSVFilter.Context.lowerSaturation);
                    hsvFilter.setValue((int)saturationSlider.UpperValue, HSVFilter.Context.upperSaturation);
                    hsvFilter.setValue((int)valueSlider.LowerValue, HSVFilter.Context.lowerValue);
                    hsvFilter.setValue((int)valueSlider.UpperValue, HSVFilter.Context.upperValue);
                    Console.WriteLine(hsvFilter.ToString());
                    //coolDown.Start();
                }
                catch (NullReferenceException) { }
            }
#if DEBUG
            else
            {
                Console.WriteLine("Blocked.");
            }
#endif
        }
        private void hueSlider_LowerValueChanged(object sender, MahApps.Metro.Controls.RangeParameterChangedEventArgs e)
        {
            updateRangeSliders();   
        }
        private void hueSlider_UpperValueChanged(object sender, MahApps.Metro.Controls.RangeParameterChangedEventArgs e)
        {
            updateRangeSliders();
        }

        private void saturationSlider_LowerValueChanged(object sender, MahApps.Metro.Controls.RangeParameterChangedEventArgs e)
        {
            updateRangeSliders();
        }

        private void saturationSlider_UpperValueChanged(object sender, MahApps.Metro.Controls.RangeParameterChangedEventArgs e)
        {
            updateRangeSliders();
        }

        private void valueSlider_LowerValueChanged(object sender, MahApps.Metro.Controls.RangeParameterChangedEventArgs e)
        {
            updateRangeSliders();
        }

        private void valueSlider_UpperValueChanged(object sender, MahApps.Metro.Controls.RangeParameterChangedEventArgs e)
        {
            updateRangeSliders();
        }
        #endregion

        public static RoutedCommand SaveKey = new RoutedCommand();
        public static RoutedCommand SaveAsKey = new RoutedCommand();
        public static RoutedCommand LoadKey = new RoutedCommand();
        private void bindControlFunctions()
        {
            SaveKey.InputGestures.Add(new KeyGesture(Key.S, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(SaveKey, SaveButton_Click));
            SaveAsKey.InputGestures.Add(new KeyGesture(Key.A, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(SaveAsKey, SaveAsButton_Click));
            LoadKey.InputGestures.Add(new KeyGesture(Key.L, ModifierKeys.Control));
            CommandBindings.Add(new CommandBinding(LoadKey, LoadButton_Click));
            
        }
        string path;
        /// <summary>
        /// Save data and create a new file.
        /// </summary>
        private void SaveData()
        {
            updateHSV();
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.FileName = ""; //Default file name, left blank to force user to put in name
            dlg.DefaultExt = ".xml"; //Sets the default extension
            dlg.Filter = "XML documents (.xml)|*.xml"; //Forces user to use XML

            //Shows dialog box and sets variable for whether or not user cancelled
            bool? result = dlg.ShowDialog();

            //Process save file dialog box results
            if (result == true)
            {
                try
                {
                    //Get path
                    path = dlg.FileName;
                    //Write filename to console
                    Console.WriteLine("Saving file to " + dlg.FileName);
                    System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(DataHolder));
                    System.IO.FileStream file = System.IO.File.Create(path);
                    writer.Serialize(file, camManager.dataHolder);
                    file.Close();
                }
                catch (InvalidOperationException) { MessageBox.Show("Storage is either full or file exists and is read only."); }
                catch (System.IO.PathTooLongException) { MessageBox.Show("Path too long."); }
            }
        }
        private void updateHSV()
        {
            camManager.dataHolder.lhue = hsvFilter.lowerHue;
            camManager.dataHolder.uhue = hsvFilter.upperHue;
            camManager.dataHolder.lsat = hsvFilter.lowerSaturation;
            camManager.dataHolder.usat = hsvFilter.upperSaturation;
            camManager.dataHolder.lval = hsvFilter.lowerValue;
            camManager.dataHolder.uval = hsvFilter.upperValue;
        }
        private void updateDataHolder()
        {
            camManager.brightness = camManager.dataHolder.brightness;
            brightnessSlider.Value = camManager.brightness;
            camManager.exposure = camManager.dataHolder.exposure;
            exposureSlider.Value = camManager.exposure;
            camManager.focus = camManager.dataHolder.focus;
            focusSlider.Value = camManager.focus;
            camManager.fps = camManager.dataHolder.fps;
            medianSlider.Value = camManager.dataHolder.medianTolerance;
            hsvFilter.setValue((int)camManager.dataHolder.lhue, HSVFilter.Context.lowerHue);
            hueSlider.LowerValue = camManager.dataHolder.lhue;
            hsvFilter.setValue((int)camManager.dataHolder.uhue, HSVFilter.Context.upperHue);
            hueSlider.UpperValue = camManager.dataHolder.uhue;
            hsvFilter.setValue((int)camManager.dataHolder.lsat, HSVFilter.Context.lowerSaturation);
            saturationSlider.LowerValue = camManager.dataHolder.lsat;
            hsvFilter.setValue((int)camManager.dataHolder.usat, HSVFilter.Context.upperSaturation);
            saturationSlider.UpperValue = camManager.dataHolder.usat;
            hsvFilter.setValue((int)camManager.dataHolder.lval, HSVFilter.Context.lowerValue);
            valueSlider.LowerValue = camManager.dataHolder.lval;
            hsvFilter.setValue((int)camManager.dataHolder.uval, HSVFilter.Context.upperValue);
            valueSlider.UpperValue = camManager.dataHolder.uval;
        }
        private void SystemEvents_SessionEnded(object sender, ExitEventArgs e)
        {
            updateHSV();
            try
            {
                //Write filename to console
                Console.WriteLine("{0} - Saving file to My Documents folder because application is closing. Hope you had a great match!", DateTime.Now);
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(DataHolder));
                if(!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CocoNuts Vision Processing"))
                {
                    System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CocoNuts Vision Processing");
                }
                System.IO.FileStream file = System.IO.File.Create(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CocoNuts Vision Processing\\LatestBackup.xml");
                writer.Serialize(file, camManager.dataHolder);
                file.Close();
            }
            catch (InvalidOperationException) { MessageBox.Show("Storage is either full or file exists and is read only."); }
        }
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Save button clicked!");
            updateHSV();
            if (!string.IsNullOrEmpty(path))
            {
                Console.WriteLine("Saving file to " + path);
                System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(typeof(DataHolder));
                System.IO.FileStream file = System.IO.File.Create(path);
                writer.Serialize(file, camManager.dataHolder);
                file.Close();
            }
            else
            {
                SaveData();
            }
        }
        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Save as button clicked!");
            SaveData();
        }
        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Load button clicked!");
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = ".xml";
            dlg.Filter = "XML documents (.xml)|*.xml";

            bool? result = dlg.ShowDialog();

            if(result == true)
            {
                try
                {
                    Console.WriteLine("Loading file from " + dlg.FileName);
                    path = dlg.FileName;
                    System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(DataHolder));
                    System.IO.StreamReader file = new System.IO.StreamReader(@"" + dlg.FileName);
                    camManager.dataHolder = reader.Deserialize(file) as DataHolder;
                    file.Close();
                    updateDataHolder();             
                }
                catch (InvalidOperationException ex) { MessageBox.Show("File not found. \n" + ex.Message); }
                catch (AccessViolationException) { }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }
    }
}