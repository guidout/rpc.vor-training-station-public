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
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using MaterialDesignThemes.Wpf;
using System.Net.Http;
using System.Net.Http.Headers;

namespace VOR_Training_Station
{
    /// <summary>
    /// Interaction logic for Scan1.xaml
    /// </summary>
    public partial class Scan1 : Page
    {
        #region Public properties
        public MainWindow mw = Application.Current.MainWindow as MainWindow;
        KinectScanConfig kinectScanConfig = new KinectScanConfig("KinectScanConfig.json");
        public TextBox UPCtextbox;
        public int PageStatus = 0; // 0 = previewing1; 1 = submitting1; 2 = previewing2; 3 = submitting2
        public UPCProductReference UPCRefereceListSelected;
        #endregion
        public Scan1(UPCProductReference UPCRefereceListSelected)
        {
            this.UPCRefereceListSelected = UPCRefereceListSelected;
            InitializeComponent();
            //NavigationService.LoadCompleted += NavigationService_LoadCompleted;
            this.DataContext = this;
            //this.UPCtextbox = UPCtextbox;

            StopAndHideKinectStream();
            PopulateProductFields(UPCRefereceListSelected);
            PicturePreviewing();


            // create timer to continously read pictures created by KinectScan
            System.Windows.Threading.DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
        private void PopulateProductFields(UPCProductReference UPCRefereceListSelected)
        {
            UPCcode_TextBox.Text = UPCRefereceListSelected.UPCcode;
            SKUcode_TextBox.Text = UPCRefereceListSelected.SKU;
            DescriptionTextBox.Text = UPCRefereceListSelected.desc;
            RefNo_TextBox.Text = UPCRefereceListSelected.RefNo;
            ManufacturerTextBox.Text = UPCRefereceListSelected.brandName;
            BrandTextBox.Text = UPCRefereceListSelected.brandName;
            PackageGroupTextBox.Text = UPCRefereceListSelected.packageGroup;
            WeightTextBox.Text = UPCRefereceListSelected.weight;
            HeightTextBox.Text = UPCRefereceListSelected.height;
            WidthTextBox.Text = UPCRefereceListSelected.width;
            LengthTextBox.Text = UPCRefereceListSelected.length;
        }

        private void StopAndHideKinectStream()
        {
            // stop and hide kinects
            mw.StopKinects();
        }

        private void Scan1_Loaded(object sender, RoutedEventArgs e)
        {
            // GENERATE FILE NAMES

            // create new kinectscan config file
            //kinectScanConfig.setColorImgNames("ColorImg");
            //kinectScanConfig.setColorCropImgName("ColorCropImg");
            //kinectScanConfig.setDepthImgNames("DepthImg");
            kinectScanConfig.setCropSettings(
                FrameMarginLRSides: mw.appConfig.FrameMarginLRSides,
                FrameMarginTBSides: mw.appConfig.FrameMarginTBSides,
                ImgCrop_edgeTreshold_HorizBottom: mw.appConfig.ImgCrop_edgeTreshold_HorizBottom,
                ImgCrop_edgeTreshold_HorizTop: mw.appConfig.ImgCrop_edgeTreshold_HorizTop,
                ImgCrop_edgeTreshold_Vert: mw.appConfig.ImgCrop_edgeTreshold_Vert,
                DepthDelta_mm: mw.appConfig.DepthDelta_mm);
            kinectScanConfig.makeConfigFile();
            Thread.Sleep(1000);
            // launch kinectscan
            mw.runKinectScan = true;
        }
        public void PicturePreviewing()
        {
            DiscardStartOver_Button.Visibility = Visibility.Visible;
            TakePicture_Button.Visibility = Visibility.Visible;
            Submit_Button.Visibility = Visibility.Collapsed;
            SubmitAndTakeMore_Button.Visibility = Visibility.Collapsed;
        }
        public void PictureSubmitting()
        {
            TakePicture_Button.Visibility = Visibility.Collapsed;
            if (PageStatus == 1)
            {
                DiscardStartOver_Button.Visibility = Visibility.Visible;
                Submit_Button.Visibility = Visibility.Visible;
                SubmitAndTakeMore_Button.Visibility = Visibility.Visible;
                SubmitAndTakeMore_Button.IsEnabled = true;
            }
            else if (PageStatus == 3)
            {
                DiscardStartOver_Button.Visibility = Visibility.Visible;
                Submit_Button.Visibility = Visibility.Visible;
                SubmitAndTakeMore_Button.Visibility = Visibility.Visible;
                SubmitAndTakeMore_Button.IsEnabled = false;
            }
        }
        private void DiscardStartOver_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (PageStatus == 0 || PageStatus == 2)
            {
                mw.runKinectScan = false;
                mw.resetMainWindow();
                //navigate back to StartPage
                NavigationService.Navigate(new StartPage());
            }
            else if  (PageStatus == 1 || PageStatus == 3)
            {
                PicturePreviewing();
                PageStatus = PageStatus == 1 ? 0 : 2;
                // Re-enable picture refreshing
                kinectScanConfig.setRunStatus(takePictures: true, keepAlive: true);
                kinectScanConfig.makeConfigFile();
            }
            deleteExistingPictures();
        }

        private void Submit_ButtonClick(object sender, RoutedEventArgs e)
        {
            //openDialog();
            // Stop KinectSan
            kinectScanConfig.setRunStatus(false, false);
            kinectScanConfig.makeConfigFile();

            // SEND PICTURES VIA API
            sendPituresToAPI(UPCRefereceListSelected, kinectScanConfig);

            // DELETE PICTURES (OPTIONAL)
            deleteExistingPictures();

            // 
            // Set Page status
            PageStatus = 2;
            // 
            DiscardStartOver_ButtonClick(null, null);


        }
        //private async void openDialog()
        //{
        //    await DialogHost.Show(mw.winDialog);
        //    //mw.winDialog.IsOpen = true;
        //}

        private void SubmitAndTakeMore_ButtonClick(object sender, RoutedEventArgs e)
        {
            // Re-enable picture refreshing
            kinectScanConfig.setRunStatus(true, true);
            kinectScanConfig.makeConfigFile();

            // Send pictures to tensor
            sendPituresToAPI(UPCRefereceListSelected, kinectScanConfig);

            // Delete picture sent 
            deleteExistingPictures();

            // Set PageStatus
            PageStatus = 2;
            PicturePreviewing();
        }
        private void TakePicture_ButtonClick(object sender, RoutedEventArgs e)
        {
            // Pause picture refreshing
            kinectScanConfig.setRunStatus(takePictures: false, keepAlive: true);
            kinectScanConfig.makeConfigFile();
            // Set PageStatus
            if (PageStatus == 0) { PageStatus = 1; }
            else if (PageStatus == 2) { PageStatus = 3; }
            // Call the submitting status
            PictureSubmitting();
        }

        public void sendPituresToAPI(UPCProductReference UPCRefereceListSelected, KinectScanConfig kinectScanConfig)
        {
            // Create json string
            JObject postBody =
                new JObject(
                    new JProperty("listOfSides",
                        new JArray(new List<string>() {
                            UPCRefereceListSelected.UPCcode + "-" + UPCRefereceListSelected.RefNo + "-Top",
                            UPCRefereceListSelected.UPCcode + "-" + UPCRefereceListSelected.RefNo + "-Large",
                            UPCRefereceListSelected.UPCcode + "-" + UPCRefereceListSelected.RefNo + "-Small",})
                        ),
                    new JProperty("UPC", UPCRefereceListSelected.UPCcode),
                    new JProperty("refNo", UPCRefereceListSelected.RefNo)
                    );
            //string test = postBody.ToString();
            var client = new RestClient("https://93o9cnkow3.execute-api.us-west-2.amazonaws.com/dev/imagedata");
            client.Authenticator = new HttpBasicAuthenticator("", "");
            var request = new RestRequest("", Method.POST);
            request.AddParameter("application/json; charset=utf-8", postBody.ToString(), ParameterType.RequestBody);
            request.RequestFormat = RestSharp.DataFormat.Json;
            var response = client.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                JObject responseParsed = JObject.Parse(response.Content);
                JArray uncroppedUrls = (JArray)responseParsed["body"]["uncroppedUrls"];
                JArray croppedUrls = (JArray)responseParsed["body"]["croppedUrls"];
                JArray irUrls = (JArray)responseParsed["body"]["irUrls"];
                if ( uncroppedUrls.Count == 3 && croppedUrls.Count == 3 && irUrls.Count == 3 )
                {
                    foreach (var uncroppedUrl in uncroppedUrls)
                    {
                        string ImgData;
                        string urlKey = (string)uncroppedUrl["fields"]["key"];
                        string ImgFileName;
                        //if ( urlKey.Contains("Top") ) { ImgData = File.ReadAllText("ColorImg-Top.jpg"); }
                        //else if (urlKey.Contains("Small")) { ImgData = File.ReadAllText("ColorImg-Small.jpg"); }
                        //else { ImgData = System.IO.File.ReadAllText("ColorImg-Large.jpg"); }
                        if (urlKey.Contains("Top")) { ImgFileName = "ColorImg-Top.jpg"; }
                        else if (urlKey.Contains("Small")) { ImgFileName = "ColorImg-Small.jpg"; }
                        else { ImgFileName = "ColorImg-Large.jpg"; }
                        using (FileStream stream = File.Open(ImgFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                ImgData = reader.ReadToEnd();
                            }
                        }

                        MultipartFormDataContent formData = new MultipartFormDataContent
                        {
                            { new StringContent((string)uncroppedUrl["fields"]["key"]), "key" },
                            { new StringContent((string)uncroppedUrl["fields"]["AWSAccessKeyId"]), "AWSAccessKeyId" },
                            { new StringContent((string)uncroppedUrl["fields"]["x-amz-security-token"]),"x-amz-security-token" },
                            { new StringContent((string)uncroppedUrl["fields"]["policy"]), "policy" },
                            { new StringContent((string)uncroppedUrl["fields"]["signature"]), "signature" },
                            { new StringContent(ImgData), "file" }
                        };
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                            httpClient.BaseAddress = new Uri((string)uncroppedUrl["url"]);
                            var httpResponse = httpClient.PostAsync(new Uri((string)uncroppedUrl["url"]), formData).Result;
                            if (httpResponse.StatusCode != HttpStatusCode.NoContent)
                            {
                                // TO DO: Handle ERROR
                            }
                            var responseString = httpResponse.Content.ReadAsStringAsync();
                        }
                    }
                }
                else
                {
                    // TO DO: Handle ERROR
                }

            }
        }
        private void deleteExistingPictures()
        {
            string[] filesInFolder = Directory
                .GetFiles(".\\", "*.*");
            foreach (string fileInFolder in filesInFolder)
            {
                if (fileInFolder.ToLower().EndsWith("jpg") || fileInFolder.ToLower().EndsWith("png"))
                {
                    try
                    {
                        File.Delete(fileInFolder);
                    }
                    catch { }
                }
            }
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (File.Exists("ColorCropImg-Top.jpg"))
                {
                    //BitmapHelper image = new BitmapHelper(kinectScanConfig.ColorCropImg0_name);
                    BitmapHelper image = new BitmapHelper("ColorCropImg-Top.jpg");
                    mw.cam0Img.Source = image.image;
                }
                if (File.Exists("ColorCropImg-Large.jpg"))
                {
                    //BitmapHelper image = new BitmapHelper(kinectScanConfig.ColorCropImg1_name);
                    BitmapHelper image = new BitmapHelper("ColorCropImg-Large.jpg");
                    mw.cam1Img.Source = image.image;
                }
                if (File.Exists("ColorCropImg-Small.jpg"))
                {
                    //BitmapHelper image = new BitmapHelper(kinectScanConfig.ColorCropImg2_name);
                    BitmapHelper image = new BitmapHelper("ColorCropImg-Small.jpg");
                    mw.cam2Img.Source = image.image;
                }
            }
            catch
            {
                
            }
        }
    }
}
