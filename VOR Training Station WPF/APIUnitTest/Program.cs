using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using VOR_Training_Station;

namespace APIUnitTest
{
    class Program
    {
        static void Main(string[] args)
        {
            UPCProductReference UPCRefereceListSelected = new UPCProductReference();
            UPCRefereceListSelected.UPCcode = "000123";
            UPCRefereceListSelected.RefNo = "000123";

            JObject postBody =
                new JObject(
                    new JProperty("listOfSides",
                        new JArray(new List<string>() {
                            UPCRefereceListSelected.UPCcode + "-" + UPCRefereceListSelected.RefNo + "-Top",
                            UPCRefereceListSelected.UPCcode + "-" + UPCRefereceListSelected.RefNo + "-Large",
                            UPCRefereceListSelected.UPCcode + "-" + UPCRefereceListSelected.RefNo + "-Small",})
                        ),
                    new JProperty("UPC", UPCRefereceListSelected.UPCcode),
                    new JProperty("refNo", UPCRefereceListSelected.RefNo)
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
                if (uncroppedUrls.Count == 3 && croppedUrls.Count == 3 && irUrls.Count == 3)
                {
                    foreach (var uncroppedUrl in uncroppedUrls)
                    {
                        string data = System.IO.File.ReadAllText("C:\\Users\\gritelli\\Desktop\\ColorCropImg0.jpg");

                        //string test2 = (string)uncroppedUrl["fields"]["key"];
                        //client = new RestClient((string)uncroppedUrl["url"]);
                        //request = new RestRequest();
                        //request.Method = Method.POST;

                        //request.AddParameter("key", (string)uncroppedUrl["fields"]["key"], ParameterType.RequestBody);
                        //request.AddParameter("AWSAccessKeyId", (string)uncroppedUrl["fields"]["AWSAccessKeyId"], ParameterType.RequestBody);
                        //request.AddParameter("x-amz-security-token", (string)uncroppedUrl["fields"]["x-amz-security-token"], ParameterType.RequestBody);
                        //request.AddParameter("policy", (string)uncroppedUrl["fields"]["policy"], ParameterType.RequestBody);
                        //request.AddParameter("signature", (string)uncroppedUrl["fields"]["signature"], ParameterType.RequestBody);
                        //request.AddParameter("file", data, ParameterType.RequestBody);
                        ////request.AddFile("file", "C:\\Users\\gritelli\\Desktop\\ColorCropImg0.jpg", "image/jpeg");

                        //response = client.Execute(request);

                        MultipartFormDataContent formData = new MultipartFormDataContent
                        {
                            { new StringContent((string)uncroppedUrl["fields"]["key"]), "key" },
                            { new StringContent((string)uncroppedUrl["fields"]["AWSAccessKeyId"]), "AWSAccessKeyId" },
                            { new StringContent((string)uncroppedUrl["fields"]["x-amz-security-token"]),"x-amz-security-token" },
                            { new StringContent((string)uncroppedUrl["fields"]["policy"]), "policy" },
                            { new StringContent((string)uncroppedUrl["fields"]["signature"]), "signature" },
                            { new StringContent(data), "file" }
                        };
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
                            httpClient.BaseAddress = new Uri((string)uncroppedUrl["url"]);
                            var httpResponse = httpClient.PostAsync(new Uri((string)uncroppedUrl["url"]), formData).Result;
                            var responseString = httpResponse.Content.ReadAsStringAsync();
                        }


                    }
                }
            }
        }
    }
}
