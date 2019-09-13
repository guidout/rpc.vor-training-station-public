using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VOR_Training_Station
{
    public class KinectScanConfig
    {
        public string TopCameraSerialNum;
        public string ColorCropImg0_name;
        public string ColorCropImg1_name;
        public string ColorCropImg2_name;
        public string ColorImg0_name;
        public string ColorImg1_name;
        public string ColorImg2_name;
        public string DepthImg0_name;
        public string DepthImg1_name;
        public string DepthImg2_name;
        public double FrameMarginLRSides;
        public double FrameMarginTBSides;
        
        public string configFileName;
        public double ImgCrop_edgeTreshold_HorizBottom;
        public double ImgCrop_edgeTreshold_HorizTop;
        public double ImgCrop_edgeTreshold_Vert;
        public double DepthDelta_mm;
        public string chromaKeySetting;

        public bool takePictures = true;
        public bool keepAlive = true;
        public string exitError;

        public KinectScanConfig(string configFileName)
        {
            this.exitError = "";
            this.configFileName = configFileName;
            this.takePictures = true;
            this.keepAlive = true;
        }
        public void setRunStatus(bool takePictures, bool keepAlive)
        {
            this.takePictures = takePictures;
            this.keepAlive = keepAlive;
        }
        public void setCropSettings(double FrameMarginLRSides, double FrameMarginTBSides,
            double ImgCrop_edgeTreshold_HorizBottom,
            double ImgCrop_edgeTreshold_HorizTop,
            double ImgCrop_edgeTreshold_Vert,
            double DepthDelta_mm)
        {
            this.FrameMarginLRSides = FrameMarginLRSides;
            this.FrameMarginTBSides = FrameMarginTBSides;
            this.ImgCrop_edgeTreshold_HorizBottom = ImgCrop_edgeTreshold_HorizBottom;
            this.ImgCrop_edgeTreshold_HorizTop = ImgCrop_edgeTreshold_HorizTop;
            this.ImgCrop_edgeTreshold_Vert = ImgCrop_edgeTreshold_Vert;
            this.DepthDelta_mm = DepthDelta_mm;
        }
        public void setColorImgNames(string ColorImgName_prefix)
        {
            this.ColorImg0_name = ColorImgName_prefix + "0.jpg";
            this.ColorImg1_name = ColorImgName_prefix + "1.jpg";
            this.ColorImg2_name = ColorImgName_prefix + "2.jpg";
        }
        public void setDepthImgNames(string DepthImgName_prefix)
        {
            this.DepthImg0_name = DepthImgName_prefix + "0.png";
            this.DepthImg1_name = DepthImgName_prefix + "1.png";
            this.DepthImg2_name = DepthImgName_prefix + "2.png";
        }
        public void setColorCropImgName(string ColorCropImgName_prefix)
        {
            this.ColorCropImg0_name = ColorCropImgName_prefix + "0.jpg";
            this.ColorCropImg1_name = ColorCropImgName_prefix + "1.jpg";
            this.ColorCropImg2_name = ColorCropImgName_prefix + "2.jpg";
        }
        public void SetChromaKey(string chromaKeySetting)
        {
            this.chromaKeySetting = chromaKeySetting;
        }
        public void makeConfigFile()
        {
            if (System.IO.File.Exists(this.configFileName)) // Probably NOT needed since WriteAllText creates the file if doesn't exist
            {
                //using (StreamReader reader = File.OpenText(this.configFileName))
                //{
                //    JObject jsonObject = (JObject)JToken.ReadFrom(new JsonTextReader(reader));
                //}

                JObject jsonContent = JObject.FromObject(this);
                File.WriteAllText(this.configFileName, jsonContent.ToString());
            }
            else
            {
                var f = File.Create(this.configFileName);
                f.Close();
                //this.jsonContent = File.ReadAllText(this.configFileName);

                JObject jsonContent = JObject.FromObject(this);
                File.WriteAllText(this.configFileName, jsonContent.ToString());
            }
        }

        public void readConfigFile()
        {
            if (System.IO.File.Exists(this.configFileName))
            {
                //this.jsonContent = System.IO.File.ReadAllText(this.configFileName);
            }
            else
            {
                //File.WriteAllText(this.configFileName, "{}");
                //this.jsonContent = "{}";
            }
        }
    }
}
