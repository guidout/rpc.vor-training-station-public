using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VOR_Training_Station
{
    class BitmapHelper
    {
        public string imgLocation;
        public BitmapImage image;

        public BitmapHelper(string imgLocation)
        {
            this.imgLocation = imgLocation;
            this.image = new BitmapImage();
            this.image.BeginInit();
            this.image.CacheOption = BitmapCacheOption.OnLoad;
            this.image.UriSource = new Uri(this.imgLocation, UriKind.Relative);
            //image.UriSource = new Uri(AppDomain.CurrentDomain.BaseDirectory + kinectScanConfig.ColorCropImg0_name, UriKind.Absolute);
            this.image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            this.image.EndInit();
        }
    }
}
