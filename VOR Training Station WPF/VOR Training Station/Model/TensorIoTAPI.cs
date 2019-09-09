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
using System.Net;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace VOR_Training_Station
{
    public class TensorIoTAPI
    {
        public TensorIoTAPI()
        {
            
        }

        public ObservableCollection<UPCProductReference> GetReferenceCodes(string UPCcode)
        {
            // Look for UPCcode
            // UPCRefereceList = new ObservableCollection<UPCProductReference>();
            // Make API call
            var client = new RestClient("https://pygv9kdmg6.execute-api.us-west-2.amazonaws.com/dev/Pepsi/" + UPCcode);
            client.Authenticator = new HttpBasicAuthenticator("", "");
            var response = client.Get(new RestRequest());
            JObject responseParsed = JObject.Parse(response.Content);
            ObservableCollection<UPCProductReference> UPCRefereceList = new ObservableCollection<UPCProductReference>();
            foreach (var upcs in responseParsed["products"])
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
            return UPCRefereceList;
        }
        public void SendPicturesToAWS(string UPCcode, string RefNo)
        {
            // Create json string
            JObject postBody =
                new JObject(
                    new JProperty("listOfSides",
                        new JArray(new List<string>() {
                            UPCcode + "-" + RefNo + "-Top",
                            UPCcode + "-" + RefNo + "-Large",
                            UPCcode + "-" + RefNo + "-Small",})
                        ),
                    new JProperty("UPC", UPCcode),
                    new JProperty("refNo", RefNo)
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
                if (uncroppedUrls.Count == 3 && croppedUrls.Count == 3 && irUrls.Count == 3) // all 9 urls have been returned correctly
                {
                    ////////////////////////////////
                    // SEND UNCROPPED PICTURES
                    foreach (var uncroppedUrl in uncroppedUrls)
                    {
                        string ImgData;
                        string urlKey = (string)uncroppedUrl["fields"]["key"];
                        string ImgFileName;
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
                    ////////////////////////////////
                    // SEND CROPPED PICTURES
                    foreach (var croppedUrl in croppedUrls)
                    {
                        string ImgData;
                        string urlKey = (string)croppedUrl["fields"]["key"];
                        string ImgFileName;
                        if (urlKey.Contains("Top")) { ImgFileName = "ColorCropImg-Top.jpg"; }
                        else if (urlKey.Contains("Small")) { ImgFileName = "ColorCropImg-Small.jpg"; }
                        else { ImgFileName = "ColorCropImg-Large.jpg"; }
                        using (FileStream stream = File.Open(ImgFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                ImgData = reader.ReadToEnd();
                            }
                        }

                        MultipartFormDataContent formData = new MultipartFormDataContent
                        {
                            { new StringContent((string)croppedUrl["fields"]["key"]), "key" },
                            { new StringContent((string)croppedUrl["fields"]["AWSAccessKeyId"]), "AWSAccessKeyId" },
                            { new StringContent((string)croppedUrl["fields"]["x-amz-security-token"]),"x-amz-security-token" },
                            { new StringContent((string)croppedUrl["fields"]["policy"]), "policy" },
                            { new StringContent((string)croppedUrl["fields"]["signature"]), "signature" },
                            { new StringContent(ImgData), "file" }
                        };
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                            httpClient.BaseAddress = new Uri((string)croppedUrl["url"]);
                            var httpResponse = httpClient.PostAsync(new Uri((string)croppedUrl["url"]), formData).Result;
                            if (httpResponse.StatusCode != HttpStatusCode.NoContent)
                            {
                                // TO DO: Handle ERROR
                            }
                            var responseString = httpResponse.Content.ReadAsStringAsync();
                        }
                    }
                    ////////////////////////////////
                    // SEND DEPTH PICTURES
                    foreach (var irUrl in irUrls)
                    {
                        string ImgData;
                        string urlKey = (string)irUrl["fields"]["key"];
                        string ImgFileName;
                        if (urlKey.Contains("Top")) { ImgFileName = "DepthImg-Top.png"; }
                        else if (urlKey.Contains("Small")) { ImgFileName = "DepthImg-Small.png"; }
                        else { ImgFileName = "DepthImg-Large.png"; }
                        using (FileStream stream = File.Open(ImgFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                ImgData = reader.ReadToEnd();
                            }
                        }
                        MultipartFormDataContent formData = new MultipartFormDataContent
                        {
                            { new StringContent((string)irUrl["fields"]["key"]), "key" },
                            { new StringContent((string)irUrl["fields"]["AWSAccessKeyId"]), "AWSAccessKeyId" },
                            { new StringContent((string)irUrl["fields"]["x-amz-security-token"]),"x-amz-security-token" },
                            { new StringContent((string)irUrl["fields"]["policy"]), "policy" },
                            { new StringContent((string)irUrl["fields"]["signature"]), "signature" },
                            { new StringContent(ImgData), "file" }
                        };
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                            httpClient.BaseAddress = new Uri((string)irUrl["url"]);
                            var httpResponse = httpClient.PostAsync(new Uri((string)irUrl["url"]), formData).Result;
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
    }
}
