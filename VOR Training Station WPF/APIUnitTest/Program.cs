using System;
using System.Collections.Generic;
using System.IO;
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

            TensorIoTAPI TensorAPI = new TensorIoTAPI();
            TensorAPI.SendSinglePictureToAWS_test(UPCRefereceListSelected.UPCcode, UPCRefereceListSelected.RefNo, "Snipaste_2019-09-12_12-38-58.jpg");

            string test;
        }
    }
}
