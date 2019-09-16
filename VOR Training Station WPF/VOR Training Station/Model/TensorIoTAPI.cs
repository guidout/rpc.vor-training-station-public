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
using System.Drawing;

namespace VOR_Training_Station
{
    public class TensorIoTAPI
    {
        public TensorIoTAPI()
        {

        }

        public Tuple<ObservableCollection<UPCProductReference>, string> GetReferenceCodes(string UPCcode)
        {
            ObservableCollection<UPCProductReference> UPCRefereceList = new ObservableCollection<UPCProductReference>();
            string errMsg = "";
            try
            {
                // Look for UPCcode
                // UPCRefereceList = new ObservableCollection<UPCProductReference>();
                // Make API call
                var client = new RestClient("https://pygv9kdmg6.execute-api.us-west-2.amazonaws.com/dev/Pepsi/" + UPCcode);
                client.Authenticator = new HttpBasicAuthenticator("", "");
                var response = client.Get(new RestRequest());
                JObject responseParsed = JObject.Parse(response.Content);
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
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
            }
            return Tuple.Create(UPCRefereceList, errMsg);
        }
        public string SendPicturesToAWS(string UPCcode, string RefNo)
        {
            try
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
                            string urlKey = (string)uncroppedUrl["fields"]["key"];
                            string ImgFileName;
                            if (urlKey.Contains("Top")) { ImgFileName = "ColorImg-Top.jpg"; }
                            else if (urlKey.Contains("Small")) { ImgFileName = "ColorImg-Small.jpg"; }
                            else { ImgFileName = "ColorImg-Large.jpg"; }

                            MultipartFormDataContent formData = new MultipartFormDataContent
                        {
                            { new StringContent((string)uncroppedUrl["fields"]["key"]), "key" },
                            { new StringContent((string)uncroppedUrl["fields"]["AWSAccessKeyId"]), "AWSAccessKeyId" },
                            { new StringContent((string)uncroppedUrl["fields"]["x-amz-security-token"]),"x-amz-security-token" },
                            { new StringContent((string)uncroppedUrl["fields"]["policy"]), "policy" },
                            { new StringContent((string)uncroppedUrl["fields"]["signature"]), "signature" },
                            { new ByteArrayContent(File.ReadAllBytes(ImgFileName)), "file" }
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
                            string urlKey = (string)croppedUrl["fields"]["key"];
                            string ImgFileName;
                            if (urlKey.Contains("Top")) { ImgFileName = "ColorCropImg-Top.jpg"; }
                            else if (urlKey.Contains("Small")) { ImgFileName = "ColorCropImg-Small.jpg"; }
                            else { ImgFileName = "ColorCropImg-Large.jpg"; }


                            MultipartFormDataContent formData = new MultipartFormDataContent
                        {
                            { new StringContent((string)croppedUrl["fields"]["key"]), "key" },
                            { new StringContent((string)croppedUrl["fields"]["AWSAccessKeyId"]), "AWSAccessKeyId" },
                            { new StringContent((string)croppedUrl["fields"]["x-amz-security-token"]),"x-amz-security-token" },
                            { new StringContent((string)croppedUrl["fields"]["policy"]), "policy" },
                            { new StringContent((string)croppedUrl["fields"]["signature"]), "signature" },
                            { new ByteArrayContent(File.ReadAllBytes(ImgFileName)), "file" }
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
                            string urlKey = (string)irUrl["fields"]["key"];
                            string ImgFileName;
                            if (urlKey.Contains("Top")) { ImgFileName = "DepthImg-Top.png"; }
                            else if (urlKey.Contains("Small")) { ImgFileName = "DepthImg-Small.png"; }
                            else { ImgFileName = "DepthImg-Large.png"; }

                            MultipartFormDataContent formData = new MultipartFormDataContent
                        {
                            { new StringContent((string)irUrl["fields"]["key"]), "key" },
                            { new StringContent((string)irUrl["fields"]["AWSAccessKeyId"]), "AWSAccessKeyId" },
                            { new StringContent((string)irUrl["fields"]["x-amz-security-token"]),"x-amz-security-token" },
                            { new StringContent((string)irUrl["fields"]["policy"]), "policy" },
                            { new StringContent((string)irUrl["fields"]["signature"]), "signature" },
                            { new ByteArrayContent(File.ReadAllBytes(ImgFileName)), "file" }
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
                return "";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
        public void SendSinglePictureToAWS_test(string UPCcode, string RefNo, string PicFileName)
        {
            // Create json string
            JObject postBody =
                new JObject(
                    new JProperty("listOfSides",
                        new JArray(new List<string>() {
                            UPCcode + "-" + RefNo + "-Top" })
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
                if (uncroppedUrls.Count == 1)
                {
                    ////////////////////////////////
                    // SEND UNCROPPED PICTURES
                    foreach (var uncroppedUrl in uncroppedUrls)
                    {
                        string urlKey = (string)uncroppedUrl["fields"]["key"];
                        //string ImgFileName;
                        //ImgFileName = "ColorImg-Top.jpg";


                        //ImgData = "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";
                        //byte[] data = Convert.FromBase64String(ImgData);
                        //string decodedString = Encoding.UTF8.GetString(data);
                        //ImgData = decodedString;

                        MultipartFormDataContent formData = new MultipartFormDataContent
                        {
                            { new StringContent((string)uncroppedUrl["fields"]["key"]), "key" },
                            { new StringContent((string)uncroppedUrl["fields"]["AWSAccessKeyId"]), "AWSAccessKeyId" },
                            { new StringContent((string)uncroppedUrl["fields"]["x-amz-security-token"]),"x-amz-security-token" },
                            { new StringContent((string)uncroppedUrl["fields"]["policy"]), "policy" },
                            { new StringContent((string)uncroppedUrl["fields"]["signature"]), "signature" },
                            { new ByteArrayContent(File.ReadAllBytes(PicFileName)), "file" }
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


                    //// METHOD https://stackoverflow.com/questions/32294541/upload-image-to-web-api-using-c-sharp
                    //foreach (var uncroppedUrl in uncroppedUrls)
                    //{
                    //    string test = (string)uncroppedUrl["fields"]["key"];
                    //    Bitmap myImage = new Bitmap(PicFileName);
                    //    byte[] myFileData = (byte[])(new ImageConverter()).ConvertTo(myImage, typeof(byte[]));

                    //    var client2 = new RestClient("https://rehrig-images.s3.amazonaws.com/");
                    //    var request2 = new RestRequest(Method.POST);
                    //    //request2.AddHeader("Postman-Token", "f25ac3cd-8b97-4908-a39a-6d91955bd4d9");
                    //    //request2.AddHeader("cache-control", "no-cache");
                    //    request2.AddHeader("content-type", "multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW");
                    //    string POSTparamString = "------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"key\"\r\n\r\n" + (string)uncroppedUrl["fields"]["key"] + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"AWSAccessKeyId\"\r\n\r\n" + (string)uncroppedUrl["fields"]["AWSAccessKeyId"] + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"x-amz-security-token\"\r\n\r\n" + (string)uncroppedUrl["fields"]["x-amz-security-token"] + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"policy\"\r\n\r\n" + (string)uncroppedUrl["fields"]["policy"] + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"signature\"\r\n\r\n" + (string)uncroppedUrl["fields"]["signature"] + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; name=\"file\"; filename=\"" + PicFileName + "\"\r\nContent-Type: image/jpeg\r\n\r\n" + Encoding.UTF8.GetString(myFileData) + "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW--";
                    //    request2.AddParameter("multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW",
                    //        POSTparamString,
                    //        ParameterType.RequestBody);
                    //    IRestResponse response2 = client.Execute(request2);


                    //    //// METHOD
                    //    //RestClient client2 = new RestClient("https://rehrig-images.s3.amazonaws.com/");
                    //    //RestRequest request2 = new RestRequest(Method.POST);
                    //    //request2.AddHeader("content-type", "multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW");
                    //    //request2.AddParameter("multipart/form-data; boundary=----WebKitFormBoundary7MA4YWxkTrZu0gW", "------WebKitFormBoundary7MA4YWxkTrZu0gW" + 
                    //    //    "\r\nContent-Disposition: form-data; " +
                    //    //    "name=\"key\"\r\n\r\n" + (string)uncroppedUrl["fields"]["key"] + 
                    //    //    "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; " + 
                    //    //    "name=\"AWSAccessKeyId\"\r\n\r\n" + (string)uncroppedUrl["fields"]["AWSAccessKeyId"] + 
                    //    //    "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; " + 
                    //    //    "name=\"x-amz-security-token\"\r\n\r\n" + (string)uncroppedUrl["fields"]["x-amz-security-token"] + 
                    //    //    "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; " + 
                    //    //    "name=\"policy\"\r\n\r\n" + (string)uncroppedUrl["fields"]["policy"] + 
                    //    //    "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; " + 
                    //    //    "name=\"signature\"\r\n\r\n" + (string)uncroppedUrl["fields"]["signature"] + 
                    //    //    "\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW\r\nContent-Disposition: form-data; " + 
                    //    //    "name=\"file\"; " + 
                    //    //    "filename=\"" +  PicFileName + "\"\r\nContent-Type: image/jpeg\r\n\r\n\r\n------WebKitFormBoundary7MA4YWxkTrZu0gW--",
                    //    //    ParameterType.RequestBody);
                    //    //IRestResponse response2 = client.Execute(request2);
                    //}
                }
            }
        }
    }
}
