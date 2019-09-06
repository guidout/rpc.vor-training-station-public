using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Authenticators;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;



namespace VOR_Training_Station
{
    public class TensorIoTAPI
    {
        public string UPCcode;
        public Collection<string> RefNo = new Collection<string>();
        public Collection<string> SKU = new Collection<string>();
        public Collection<string> brandName = new Collection<string>();
        public Collection<string> desc = new Collection<string>();
        public Collection<string> height = new Collection<string>();
        public Collection<string> length = new Collection<string>();
        public Collection<string> packageGroup = new Collection<string>();
        public Collection<string> packageType = new Collection<string>();
        public Collection<string> weight = new Collection<string>();
        public Collection<string> width = new Collection<string>();

        public TensorIoTAPI()
        {
            
        }

        public void getReferences(string UPCcode)
        {
            this.UPCcode = UPCcode;
            var client = new RestClient("https://pygv9kdmg6.execute-api.us-west-2.amazonaws.com/dev/Pepsi/" + this.UPCcode);
            //var client = new RestClient("https://pygv9kdmg6.execute-api.us-west-2.amazonaws.com/dev/Pepsi/0000000000124");
            client.Authenticator = new HttpBasicAuthenticator("", "");
            //var request = new RestRequest("statuses/home_timeline.json", DataFormat.Json);
            var response = client.Get(new RestRequest());

            //JObject responseParsed = JObject.Parse(response.Content);
            // TEST
            JObject responseParsed = JObject.Parse("{'upcs':[{'UPC':'0000000000123','RefNo':'12345678','SKU':'000000000000','brandName':'GATZROLL','desc':'28OZPLGATZROLL1/15','height':'','length':'','packageGroup':'28OZPL1/15','packageType':'1234','weight':'','width':''},{'UPC':'0000000000123','RefNo':'911234567','SKU':'000000000000','brandName':'ddddd','desc':'gggggg','height':'','length':'','packageGroup':'28OZPL1/15','packageType':'1234','weight':'','width':''}]}");
            foreach (var upcs in responseParsed["upcs"])
            {
                this.RefNo.Add((string)upcs["RefNo"]);
                this.SKU.Add((string)upcs["SKU"]);
                this.brandName.Add((string)upcs["brandName"]);
                this.desc.Add((string)upcs["desc"]);
                this.height.Add((string)upcs["height"]);
                this.length.Add((string)upcs["length"]);
                this.packageGroup.Add((string)upcs["packageGroup"]);
                this.packageType.Add((string)upcs["packageType"]);
                this.weight.Add((string)upcs["weight"]);
                this.width.Add((string)upcs["width"]);
            }
        }
    }
}
