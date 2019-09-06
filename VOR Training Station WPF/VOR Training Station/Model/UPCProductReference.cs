using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOR_Training_Station
{
    public class UPCProductReference
    {
        private string _UPCcode;
        public string UPCcode
        {
            get { return _UPCcode; }
            set { _UPCcode = value; }
        }
        private string _RefNo;
        public string RefNo
        {
            get { return _RefNo; }
            set { _RefNo = value; }
        }
        private string _SKU;
        public string SKU
        {
            get { return _SKU; }
            set { _SKU = value; }
        }
        private string _brandName;
        public string brandName
        {
            get { return _brandName; }
            set { _brandName = value; }
        }
        private string _desc;
        public string desc
        {
            get { return _desc; }
            set { _desc = value; }
        }
        private string _height;
        public string height
        {
            get { return _height; }
            set { _height = value; }
        }
        private string _length;
        public string length
        {
            get { return _length; }
            set { _length = value; }
        }
        private string _packageGroup;
        public string packageGroup
        {
            get { return _packageGroup; }
            set { _packageGroup = value; }
        }
        private string _packageType;
        public string packageType
        {
            get { return _packageType; }
            set { _packageType = value; }
        }
        private string _weight;
        public string weight
        {
            get { return _weight; }
            set { _weight = value; }
        }
        private string _width;
        public string width
        {
            get { return _width; }
            set { _width = value; }
        }
        public string comboboxDisp
        {
            get
            {
                return this.packageGroup + ", " + this.brandName + ", " + this.desc + ", " + this.RefNo;
            }
        }
    }
}
