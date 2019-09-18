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
            if ( !File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VOR Training Station\\VORconfig.json"))
            {
                JObject newVORconfig = this.makeNewVORconfig();
                File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VOR Training Station\\VORconfig.json", newVORconfig.ToString());
            }
            using (StreamReader reader = File.OpenText(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\VOR Training Station\\VORconfig.json"))
            {
                JObject jsonAppSettings = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                this.FrameMarginLRSides = (double)jsonAppSettings["FrameMarginLRSides"];
                this.FrameMarginTBSides = (double)jsonAppSettings["FrameMarginTBSides"];
                this.DepthDelta_mm = (double)jsonAppSettings["DepthDelta_mm"];
                this.ImgCrop_edgeTreshold_HorizBottom = (double)jsonAppSettings["ImgCrop_edgeTreshold_HorizBottom"];
                this.ImgCrop_edgeTreshold_HorizTop = (double)jsonAppSettings["ImgCrop_edgeTreshold_HorizTop"];
                this.ImgCrop_edgeTreshold_Vert = (double)jsonAppSettings["ImgCrop_edgeTreshold_Vert"];
                this.chromaKeySetting = (string)jsonAppSettings["chromaKeySetting"];
            }
        }
        public JObject makeNewVORconfig()
        {
            JObject newVORconfig = new JObject();
            newVORconfig.Add("_kinect_scan_settings", (JToken)"----------------------------------------------------------");
            newVORconfig.Add("FrameMarginLRSides", (JToken)500);
            newVORconfig.Add("FrameMarginTBSides", (JToken)250);
            newVORconfig.Add("DepthDelta_mm", (JToken)50);
            newVORconfig.Add("ImgCrop_edgeTreshold_HorizBottom", (JToken)0.7);
            newVORconfig.Add("ImgCrop_edgeTreshold_HorizTop", (JToken)0.1);
            newVORconfig.Add("ImgCrop_edgeTreshold_Vert", (JToken)0.35);
            newVORconfig.Add("OPTIONS_for_chromaKeySetting", (JToken)"White | Green | Disabled");
            newVORconfig.Add("chromaKeySetting", (JToken)"Disabled");
            return newVORconfig;
        }
    }
}
