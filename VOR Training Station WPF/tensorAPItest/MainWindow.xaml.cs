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

using Amazon.S3;
using Amazon.S3.Transfer;
using System.IO;
using Amazon;

namespace tensorAPItest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string bucketName = "https://rehrig-images.s3.amazonaws.com";
        private const string keyName = "000000000000/123456/56ceb778-a995-492d-a33f-9ada923acf98/uncropped/Side1-uncropped.jpg";
        private const string filePath = "ColorCropImg0.jpg";
        // Specify your bucket region (an example region is shown).
        private static readonly RegionEndpoint bucketRegion = RegionEndpoint.USWest2;
        private static IAmazonS3 s3Client;
        public MainWindow()
        {
            InitializeComponent();
            
            s3Client = new AmazonS3Client(("ASIAUHWVAKRP43HO256C", "ASIAUHWVAKRP43HO256C");
            UploadFileAsync().Wait();
        }
    private static async Task UploadFileAsync()
    {
        try
        {
            var fileTransferUtility =
                new TransferUtility(s3Client);

                //// Option 1. Upload a file. The file name is used as the object key name.
                //await fileTransferUtility.UploadAsync(filePath, bucketName);
                //Console.WriteLine("Upload 1 completed");

                // Option 2. Specify object key name explicitly.
                await fileTransferUtility.UploadAsync(filePath, bucketName, keyName);
                Console.WriteLine("Upload 2 completed");

                //// Option 3. Upload data from a type of System.IO.Stream.
                //using (var fileToUpload =
                //    new FileStream(filePath, FileMode.Open, FileAccess.Read))
                //{
                //    await fileTransferUtility.UploadAsync(fileToUpload,
                //                               bucketName, keyName);
                //}
                //Console.WriteLine("Upload 3 completed");

                // Option 4. Specify advanced settings.
                var fileTransferUtilityRequest = new TransferUtilityUploadRequest
            {
                BucketName = bucketName,
                FilePath = filePath,
                StorageClass = S3StorageClass.StandardInfrequentAccess,
                PartSize = 6291456, // 6 MB.
                Key = keyName,
                CannedACL = S3CannedACL.PublicRead
            };
            fileTransferUtilityRequest.Metadata.Add("param1", "Value1");
            fileTransferUtilityRequest.Metadata.Add("param2", "Value2");

            await fileTransferUtility.UploadAsync(fileTransferUtilityRequest);
            Console.WriteLine("Upload 4 completed");
        }
        catch (AmazonS3Exception e)
        {
            Console.WriteLine("Error encountered on server. Message:'{0}' when writing an object", e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
        }
    }
}
}
