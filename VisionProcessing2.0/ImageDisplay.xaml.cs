using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Emgu.CV;
using System.Windows.Threading;
using Emgu.CV.Structure;
using Microsoft.Win32;
using Emgu.CV.Util;
using System.Threading;
using System.Collections.Generic;

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
            timerSetup();
            console();
            startCapture();
            LoadOnStartup();
            bindControlFunctions();
            contourDataAnalyzer = new ContourDataAnalyzer();
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
            Console.WriteLine("{0} - Application started. \nHave a fantastic match!", start);
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
            if(frame.Width != 0)
            {
                int ratioWidth = frame.Width / ratio;
                int ratioHeight = frame.Height / ratio;

                SourceImageBox.Width = ratioWidth;
                SourceCol.Width = new GridLength(ratioWidth);
                SourceImageBox.Height = ratioHeight;
                SourceRow.Height = new GridLength(ratioHeight);

                MedianImageBox.Width = ratioWidth;
                MedianCol.Width = new GridLength(ratioWidth);
                MedianImageBox.Height = ratioHeight;
                MedianRow.Height = new GridLength(ratioHeight);

                HSVImageBox.Width = ratioWidth;
                HSVImageBox.Height = ratioHeight;
                height = frame.Height / ratio;
                width = frame.Width / ratio;

                TextBoxConsole.MaxHeight = ratioHeight;

                SourceImageBox.SetZoomScale(0.5, new System.Drawing.Point(0, 0));
                MedianImageBox.SetZoomScale(0.5, new System.Drawing.Point(0, 0));
                HSVImageBox.SetZoomScale(0.5, new System.Drawing.Point(0, 0));
            }
            else
            {
                if (!System.IO.Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CocoNuts Vision Processing"))
                    System.IO.Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CocoNuts Vision Processing");
                string[] lines = { DateTime.Now.ToString(), "Oh no! No camera was detected! :(", "", "====================" };
                System.IO.File.AppendAllLines(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CocoNuts Vision Processing\\Log.txt", lines);
                Environment.Exit(0);
            }
            
        }
        private void setOptimalProperties()
        {
            camManager.exposure = -9;
            exposureSlider.Value = -9;
            camManager.brightness = 100;
            brightnessSlider.Value = 100;
        }
        private void zoomScaleUpdated(object sender, EventArgs arg)
        {
            //Console.WriteLine("Image zoom changed to {0}", CapturedImageBox.ZoomScale);
        }
        Mat frame;
        private void SetContours()
        {
            Console.WriteLine("Before width: " + ContoursImageBox.Width);
            ContoursImageBox.Width = 320;
            Console.WriteLine("After width: " + ContoursImageBox.Width);
            ContoursImageBox.Height = 240;
        }
        private void ProcessSource(object sender, EventArgs arg)
        {
            //Forces the renderer to invalidate the screen, which then forces a redraw of the image, and then in turn increases the visual FPS. Programming.
            InvalidateVisual();
            frame = new Mat();
            camManager.Retrieve(frame, 0);
            SourceImageBox.Image = frame;
            if (setContours) SetContours();
            setContours = false;
            ProcessMedian(frame);
        }
        //int medianTolerance = 1;
        bool setContours = true;
        private void ProcessMedian(Mat frame)
        {
            Mat filtered = new Mat();
            CvInvoke.PyrDown(frame, filtered);
            CvInvoke.PyrUp(filtered, frame);
            CvInvoke.MedianBlur(frame, filtered, camManager.dataHolder.medianTolerance);
            MedianImageBox.Image = filtered;
            ProcessHSV(filtered);
        }
        HSVFilter hsvFilter = new HSVFilter();
        private void ProcessHSV(Mat frame)
        {
            using (Image<Hsv, byte> hsv = frame.ToImage<Hsv, byte>())
            {
                Image<Gray, byte> dest = hsv.InRange(hsvFilter.lowerFilter, hsvFilter.upperFilter);
                HSVImageBox.Image = dest;
                ProcessContours(dest);
            }
        }
        VectorOfVectorOfPoint contours;
        private void ProcessContours(Image<Gray, byte> source)
        {
            double cannyThreshold = 180.0;
            double cannyThresholdLinking = 120.0;
            Image<Gray, byte> dest = new Image<Gray, byte>(source.Width, source.Height);
            CvInvoke.Canny(source, dest, cannyThreshold, cannyThresholdLinking);
            ContoursImageBox.Image = dest;
            using (contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(dest, contours, dest, Emgu.CV.CvEnum.RetrType.Tree, Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxNone, new System.Drawing.Point(0, 0));
                contoursNumber.Text = (contours.Size / 2).ToString();
                ShowBoundings();
            }
        }
        ContourDataAnalyzer contourDataAnalyzer;
        private void ShowBoundings()
        {
            contourDataAnalyzer.UpdateInfo(contours);
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
            SourceImageBox.SetZoomScale(0.5, new System.Drawing.Point(0, 0));
            MedianImageBox.SetZoomScale(0.5, new System.Drawing.Point(0, 0));
            HSVImageBox.SetZoomScale(0.5, new System.Drawing.Point(0, 0));
            ContoursImageBox.SetZoomScale(0.5, new System.Drawing.Point(0, 0));
            TabItem currentTab = ImageTabControl.SelectedItem as TabItem;
            if(currentTab == ImageTabControlSource)
            {
                Console.WriteLine("Source tab selected.");
                //CapturedImageBox.Width = width;
                //SourceCanvas.MaxWidth = width;
                //CapturedImageBox.Height = height;
                //SourceCanvas.MaxHeight = height;
            }
            else if(currentTab == ImageTabControlMedian)
            {
                Console.WriteLine("Median tab selected.");
                MedianImageBox.Width = width;
                //MedianCanvas.MaxWidth = width;
                MedianImageBox.Height = height;
                //MedianCanvas.MaxHeight = height;
            }
            else if(currentTab == ImageTabControlHSV)
            {
                selectionCountHSV++;
                if (selectionCountHSV >= 2)
                {
                    //Canvas.SetTop(HSVHost, -120);
                    //Canvas.SetLeft(HSVHost, -160);
                }
                Console.WriteLine("HSV tab selected. Count: {0}", selectionCountHSV);
                HSVImageBox.Width = width;
                //HSVImageCanvas.Width = width;
                HSVImageBox.Height = height;
                //HSVImageCanvas.Height = height;
            }
            else if(currentTab == ContoursTab)
            {
                //ContoursImageBox.Width = 320;
                //ContoursImageBox.Height = 240;
                Console.WriteLine("Contours Image Box");
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

            camManager.dataHolder.NetworkTablesAddress = StaticResources.NetworkTablesAddress;
            camManager.dataHolder.ratio = StaticResources.ContourRatio;
            camManager.dataHolder.filter = StaticResources.SnapshotEnabled;

            camManager.dataHolder.minBrightness = StaticResources.minBrightness;
            camManager.dataHolder.maxBrightness = StaticResources.maxBrightness;
            camManager.dataHolder.minExposure = StaticResources.minExposure;
            camManager.dataHolder.maxExposure = StaticResources.maxExposure;
            camManager.dataHolder.minFocus = StaticResources.minFocus;
            camManager.dataHolder.maxFocus = StaticResources.maxFocus;
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

            StaticResources.NetworkTablesAddress = camManager.dataHolder.NetworkTablesAddress;
            AddressTextBox.Text = camManager.dataHolder.NetworkTablesAddress;

            StaticResources.ContourRatio = camManager.dataHolder.ratio;
            StaticResources.SnapshotEnabled = camManager.dataHolder.filter;

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

            StaticResources.minFocus = camManager.dataHolder.minFocus;
            focusSlider.Minimum = camManager.dataHolder.minFocus;
            StaticResources.maxFocus = camManager.dataHolder.maxFocus;
            focusSlider.Maximum = camManager.dataHolder.maxFocus;
            StaticResources.minBrightness = camManager.dataHolder.minBrightness;
            brightnessSlider.Minimum = camManager.dataHolder.minBrightness;
            StaticResources.maxBrightness = camManager.dataHolder.maxBrightness;
            brightnessSlider.Maximum = camManager.dataHolder.maxBrightness;
            StaticResources.minExposure = camManager.dataHolder.minExposure;
            exposureSlider.Minimum = camManager.dataHolder.minExposure;
            StaticResources.maxExposure = camManager.dataHolder.maxExposure;
            exposureSlider.Maximum = camManager.dataHolder.maxExposure;

        }
        private void SystemEvents_SessionEnded(object sender, ExitEventArgs e)
        {
            updateHSV();
            try
            {
                //Write filename to console
                Console.WriteLine("{0} - Saving file to My Documents folder because application is closing. \nHope you had a great match!", DateTime.Now);
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
        private void LoadOnStartup()
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).ToString() + "\\CocoNuts Vision Processing\\LatestBackup.xml";
                Console.WriteLine("Loading file from " + path);
                System.Xml.Serialization.XmlSerializer reader = new System.Xml.Serialization.XmlSerializer(typeof(DataHolder));
                System.IO.StreamReader file = new System.IO.StreamReader(@"" + path);
                camManager.dataHolder = reader.Deserialize(file) as DataHolder;
                file.Close();
                updateDataHolder();
                if(StaticResources.SnapshotEnabled)
                {
                    TakeSnapButton.Content = "Add Snapshot";
                }
            }
            catch (InvalidOperationException ex) { MessageBox.Show("File not found. \n" + ex.Message); }
            catch (AccessViolationException) { }
            catch (Exception)
            {
                camManager.dataHolder = new DataHolder { brightness = 100, exposure = -6, focus = 20, fps = 0, lhue = 0, uhue = 180, lsat = 0, usat = 255, lval = 0, uval = 255, medianTolerance = 1, NetworkTablesAddress="127.0.0.1", maxBrightness = 200, minBrightness = 0, maxExposure = -2, minExposure = -9, maxFocus = 200, minFocus = 0 };
                updateDataHolder();
            }
        }
        private void GetReportButton_Click(object sender, RoutedEventArgs e)
        {
            NetworkTables.NetworkTable table = NetworkTables.NetworkTable.GetTable("CamData");
            double[] heights = table.GetNumberArray("Height", new double[] { 0 });
            foreach(double h in heights)
            {
                Console.WriteLine(h);
            }
        }

        private void ChangeParameters(object sender, RoutedEventArgs e)
        {
            ChangeParameters paramWindow = new ChangeParameters();
            paramWindow.Show();
        }

        private void AddressUpdated(object sender, RoutedEventArgs e)
        {
            StaticResources.NetworkTablesAddress = AddressTextBox.Text;
        }

        private void TakeSnapshot(object sender, RoutedEventArgs e)
        {
            //If a snapshot is already present
            if (StaticResources.SnapshotEnabled)
            {
                Console.WriteLine("Snapshot already enabled. Adding another!");
                contourDataAnalyzer.AddSnapshot();
            }
            //First-time
            else
            {
                NetworkTables.NetworkTable table = NetworkTables.NetworkTable.GetTable("CamData");
                double[] heights = table.GetNumberArray("Height", new double[] { 1 });
                double[] widths = table.GetNumberArray("Width", new double[] { 1 });

                try
                {
                    StaticResources.ContourRatio.Add(widths[0] / heights[0]);
                    StaticResources.SnapshotEnabled = true;
                    TakeSnapButton.Content = "Add Snapshot";
                }
                catch (IndexOutOfRangeException) { MessageBox.Show("Index out of range. Please clear snapshot and try again."); }
            }
        }

        private void ClearSnapshot(object sender, RoutedEventArgs e)
        {
            StaticResources.SnapshotEnabled = false;
            StaticResources.ContourRatio = new List<double>();
            TakeSnapButton.Content = "Take Snapshot";
        }
    }
}