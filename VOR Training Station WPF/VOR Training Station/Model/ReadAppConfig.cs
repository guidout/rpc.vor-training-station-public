//extern alias destination;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VOR_Training_Station
{
    public class ReadAppConfig
    {
        // VOR APP SETTINGS
        public String TopCameraUrl;
        public String Side1CameraUrl;
        public String Side2CameraUrl;

        // KINECTSCAN SETTINGS
        public double FrameMarginLRSides;
        public double FrameMarginTBSides;
        public double DepthDelta_mm;
        public double ImgCrop_edgeTreshold_HorizBottom;
        public double ImgCrop_edgeTreshold_HorizTop;
        public double ImgCrop_edgeTreshold_Vert;
        public string chromaKeySetting;

        public ReadAppConfig()
        {
            if ( !Directory.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VOR Training Station") )
            {
                Directory.CreateDirectory( Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VOR Training Station");
            }
            using (StreamReader reader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VOR Training Station\\VORconfig.json"))
            {
                JObject jsonAppSettings = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                this.TopCameraUrl = (String)jsonAppSettings["TopCameraUrl"];
                this.Side1CameraUrl = (String)jsonAppSettings["Side1CameraUrl"];
                this.Side2CameraUrl = (String)jsonAppSettings["Side2CameraUrl"];
                this.FrameMarginLRSides = (double)jsonAppSettings["FrameMarginLRSides"];
                this.FrameMarginTBSides = (double)jsonAppSettings["FrameMarginTBSides"];
                this.DepthDelta_mm = (double)jsonAppSettings["DepthDelta_mm"];
                this.ImgCrop_edgeTreshold_HorizBottom = (double)jsonAppSettings["ImgCrop_edgeTreshold_HorizBottom"];
                this.ImgCrop_edgeTreshold_HorizTop = (double)jsonAppSettings["ImgCrop_edgeTreshold_HorizTop"];
                this.ImgCrop_edgeTreshold_Vert = (double)jsonAppSettings["ImgCrop_edgeTreshold_Vert"];
                this.chromaKeySetting = (string)jsonAppSettings["chromaKeySetting"];
            }
        }
    }
}
