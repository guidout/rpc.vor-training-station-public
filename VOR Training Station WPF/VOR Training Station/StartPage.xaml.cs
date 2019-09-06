using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;

namespace VOR_Training_Station
{
    public partial class StartPage : Page
    {
        #region Public properties
        bool allCamerasAvailable = true;
        //public ObservableCollection<UPCProductReference> UPCRefereceList = new ObservableCollection<UPCProductReference>();
        //public UPCProductReference UPCRefereceListSelected;
        public MainWindow mw = Application.Current.MainWindow as MainWindow;
        public TensorIoTAPI tensorIoTAPI;
        #endregion

        public StartPage()
        {
            InitializeComponent();
            this.DataContext = this;
            UPCcode_TextBox.Focus();
            UPCReferenceList_ComboBox.Visibility = Visibility.Collapsed;
        }

        private void UPCcode_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (allCamerasAvailable && UPCcode_TextBox.Text.Length >= 13)
            {
                // Look for UPCcode
                // UPCRefereceList = new ObservableCollection<UPCProductReference>();
                // Make API call
                var client = new RestClient("https://pygv9kdmg6.execute-api.us-west-2.amazonaws.com/dev/Pepsi/" + UPCcode_TextBox.Text);
                //var client = new RestClient("https://pygv9kdmg6.execute-api.us-west-2.amazonaws.com/dev/Pepsi/0000000000124");
                client.Authenticator = new HttpBasicAuthenticator("", "");
                //var request = new RestRequest("statuses/home_timeline.json", DataFormat.Json);
                var response = client.Get(new RestRequest());
                JObject responseParsed = JObject.Parse(response.Content);
                // TEST
                //JObject responseParsed = JObject.Parse("{'upcs':[{'UPC':'0000000000123','RefNo':'12345678','SKU':'000000000000','brandName':'GATZROLL','desc':'28OZPLGATZROLL1/15','height':'','length':'','packageGroup':'28OZPL1/15','packageType':'1234','weight':'','width':''},{'UPC':'0000000000123','RefNo':'911234567','SKU':'000000000000','brandName':'ddddd','desc':'gggggg','height':'','length':'','packageGroup':'28OZPL1/15','packageType':'1234','weight':'','width':''}]}");
                mw.UPCRefereceList = new ObservableCollection<UPCProductReference>();
                foreach (var upcs in responseParsed["products"])
                {
                    mw.UPCRefereceList.Add(new UPCProductReference()
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
                bool UPCfound = mw.UPCRefereceList.Count != 0;
                if (UPCfound)
                {
                    UPCReferenceList_ComboBox.ItemsSource = mw.UPCRefereceList;
                    UPCReferenceList_ComboBox.DisplayMemberPath = "comboboxDisp";
                    UPCnotfound_TextBlock.Visibility = Visibility.Collapsed;
                    UPCcode_TextBox.Visibility = Visibility.Collapsed;
                    UPCReferenceList_ComboBox.Visibility = Visibility.Visible;
                    RescanUPC_button.Visibility = Visibility.Visible;
                }
                else {
                    mw.mwBusy.Visibility = Visibility.Collapsed;
                    mw.mwUPCnotFoundDialog.Visibility = Visibility.Visible;
                    mw.winDialog.IsOpen = true;
                    //UPCnotfound_TextBlock.Visibility = Visibility.Visible;
                    UPCcode_TextBox.Text = "";
                }
            }
        }
        private void UPCcode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UPCcode_TextBox.IsEnabled)
            {
                UPCcode_TextBox.Focus();
            }
            else
            {
                
            }

        }
        private void RescanUPC_click(object sender, RoutedEventArgs e)
        {
            UPCReferenceList_ComboBox.Visibility = Visibility.Collapsed;
            RescanUPC_button.Visibility = Visibility.Collapsed;
            UPCcode_TextBox.Text = "";
            UPCcode_TextBox.Visibility = Visibility.Visible;
            UPCcode_TextBox.Focus();
        }

        private void UPCReferenceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mw.UPCRefereceListSelected = mw.UPCRefereceList[0];
            NavigationService.Navigate(new Scan1(mw.UPCRefereceListSelected));
        }

        //private void DialogOK_Click(object sender, RoutedEventArgs e)
        //{
        //    mw.winDialog.IsOpen = false;
        //    //StartPageDialog.IsOpen = false;
        //    UPCcode_TextBox.Focus();
        //}
    }
}
