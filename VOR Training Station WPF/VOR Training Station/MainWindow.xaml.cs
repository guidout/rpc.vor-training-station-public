using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.Expression.Encoder;
using Microsoft.Expression.Encoder.Devices;
using System.Diagnostics;
using WPFMediaKit.DirectShow.Controls;
using DirectShowLib;
//using Newtonsoft.Json.Linq;

namespace VOR_Training_Station
{
    public partial class MainWindow : Window
    {
        #region Public properties
        public int KinectCount = 0;
        //public DsDevice VideoDevice;
        public String[] VideoDevices;

        public bool camThreadsStarted = false;
        public volatile bool stopThreads = false;
        public volatile bool previewCams = false;
        public Thread cam0thread;
        public Thread cam1thread;
        public Thread cam2thread;
        public volatile bool KinectScanStart = false;
        public volatile bool runKinectScan = false;
        public Thread KinectScanThread;

        public ReadAppConfig appConfig = new ReadAppConfig();
        public UPCProductReference UPCRefereceListSelected;
        public ObservableCollection<UPCProductReference> UPCRefereceList = new ObservableCollection<UPCProductReference>();
        public bool newStartPage = false;
        #endregion
        //public bool isDebugMode = false;
        #if DEBUG
        public bool isDebugMode = true;
        #else
        public bool isDebugMode = false;
        #endif
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            initializeThreads();
            FindKinects();
        }
        public void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ContentFrame.Navigate(new StartPage());
        }
        public void initializeThreads()
        {
            KinectScanThread = new Thread(KinectScanProcess);
            KinectScanThread.Start();
        }
        private void FindKinects()
        {
            int vidDevCount = 0;
            VideoDevices = MultimediaUtil.VideoInputNames;
            foreach (String camera in VideoDevices)
            {
                if (camera.Contains("Kinect"))
                {
                    DsDevice VideoDevice = MultimediaUtil.VideoInputDevices[vidDevCount];
                    switch (KinectCount)
                    {
                        case 0:
                            TopCam.VideoCaptureDevice = VideoDevice;
                            break;
                        case 1:
                            Side1Cam.VideoCaptureDevice = VideoDevice;
                            break;
                        case 2:
                            Side2Cam.VideoCaptureDevice = VideoDevice;
                            break;
                    }
                    KinectCount++;
                }
                vidDevCount++;
            }
            previewCams = true;
            // Hide video stream
            TopCam.Visibility = Visibility.Visible;
            Side1Cam.Visibility = Visibility.Visible;
            Side2Cam.Visibility = Visibility.Visible;
            // Show cropped picture
            cam0Img.Visibility = Visibility.Collapsed;
            cam1Img.Visibility = Visibility.Collapsed;
            cam2Img.Visibility = Visibility.Collapsed;
        }
        public void StopKinects()
        {
            previewCams = false;
            TopCam.Stop();
            Side1Cam.Stop();
            Side2Cam.Stop();
            // Hide video stream
            TopCam.Visibility = Visibility.Collapsed;
            Side1Cam.Visibility = Visibility.Collapsed;
            Side2Cam.Visibility = Visibility.Collapsed;
            // Show cropped picture
            cam0Img.Visibility = Visibility.Visible;
            cam1Img.Visibility = Visibility.Visible;
            cam2Img.Visibility = Visibility.Visible;
        }
        public void PlayKinects()
        {
            TopCam.Play();
            Side1Cam.Play();
            Side2Cam.Play();
            // Show video stream
            TopCam.Visibility = Visibility.Visible;
            Side1Cam.Visibility = Visibility.Visible;
            Side2Cam.Visibility = Visibility.Visible;
            // Hide cropped picture
            cam0Img.Visibility = Visibility.Collapsed;
            cam1Img.Visibility = Visibility.Collapsed;
            cam2Img.Visibility = Visibility.Collapsed;
        }
        public void KinectScanProcess()
        {
            var processInfo = new ProcessStartInfo(fileName: "KinectScan.exe");
            if (isDebugMode)
            {
                processInfo.CreateNoWindow = false;
            }
            else
            {
                processInfo.CreateNoWindow = true;
            }
            processInfo.UseShellExecute = false;
            var process = new Process();
            process.StartInfo = processInfo;

            bool processRunning = false;
            while (!stopThreads)
            {
                if (runKinectScan & !processRunning)
                {
                    process.Start();
                    processRunning = true;
                }
                else if (!runKinectScan & processRunning)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch { }
                    processRunning = false;
                }
            }
        }
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //previewCams = false;
            stopThreads = true;
            if (File.Exists("KinectScanConfig.json"))
            {
                KinectScanConfig kinectScanConfig = new KinectScanConfig("KinectScanConfig.json");
                kinectScanConfig.setRunStatus(false, false);
                kinectScanConfig.makeConfigFile();
            }
        }
        public void resetMainWindow()
        {
            if (File.Exists("KinectScanConfig.json"))
            {
                KinectScanConfig kinectScanConfig = new KinectScanConfig("KinectScanConfig.json");
                kinectScanConfig.setRunStatus(false, false);
                kinectScanConfig.makeConfigFile();
            }
            Thread.Sleep(1000);
            PlayKinects();
        }

        private void DialogOK_Click(object sender, RoutedEventArgs e)
        {
            winDialog.IsOpen = false;
        }

        private void mwAllPicsSubmitted_click(object sender, RoutedEventArgs e)
        {
            winDialog.IsOpen = false;
            if (newStartPage)
            {
                ContentFrame.Navigate(new StartPage());
                newStartPage = false;
            }
            
        }
    }
}
