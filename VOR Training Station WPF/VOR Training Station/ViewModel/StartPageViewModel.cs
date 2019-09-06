using Newtonsoft.Json.Linq;
using PropertyChanged;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VOR_Training_Station
{
    [AddINotifyPropertyChangedInterface]
    public class StartPageViewModel
    {
        public ObservableCollection<UPCProductReference> UPCRefereceList { get; set; }
        public UPCProductReference UPCRefereceListSelected;

        public StartPageViewModel()
        {
            UPCRefereceList = new ObservableCollection<UPCProductReference>();
            var client = new RestClient("https://pygv9kdmg6.execute-api.us-west-2.amazonaws.com/dev/Pepsi/0000000000123");
            //var client = new RestClient("https://pygv9kdmg6.execute-api.us-west-2.amazonaws.com/dev/Pepsi/0000000000124");
            client.Authenticator = new HttpBasicAuthenticator("", "");
            //var request = new RestRequest("statuses/home_timeline.json", DataFormat.Json);
            var response = client.Get(new RestRequest());
            //JObject responseParsed = JObject.Parse(response.Content);
            // TEST
            JObject responseParsed = JObject.Parse("{'upcs':[{'UPC':'0000000000123','RefNo':'12345678','SKU':'000000000000','brandName':'GATZROLL','desc':'28OZPLGATZROLL1/15','height':'','length':'','packageGroup':'28OZPL1/15','packageType':'1234','weight':'','width':''},{'UPC':'0000000000123','RefNo':'911234567','SKU':'000000000000','brandName':'ddddd','desc':'gggggg','height':'','length':'','packageGroup':'28OZPL1/15','packageType':'1234','weight':'','width':''}]}");
            foreach (var upcs in responseParsed["upcs"])
            {
                UPCRefereceList.Add(new UPCProductReference()
                {
                    UPCcode = (string)upcs["UPC"],
                    RefNo = (string)upcs["RefNo"],
                    SKU = (string)upcs["SKU"],
                    brandName = (string)upcs["brandName"],
                    desc = (string)upcs["desc"],
                    height = (string)upcs["height"],
                    length = (string)upcs["length"],
                    packageGroup = (string)upcs["packageGroup"],
                    packageType = (string)upcs["RepackageTypefNo"],
                    weight = (string)upcs["weight"],
                    width = (string)upcs["width"]
                });
            }
        }
    }
}
