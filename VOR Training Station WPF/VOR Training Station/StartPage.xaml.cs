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
        public MainWindow mw = Application.Current.MainWindow as MainWindow;
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
            if (allCamerasAvailable && UPCcode_TextBox.Text.Length >= 12)
            {
                mw.UPCRefereceList = new ObservableCollection<UPCProductReference>();
                TensorIoTAPI TensorAPI = new TensorIoTAPI();
                Tuple< ObservableCollection<UPCProductReference>, string> APIresult = TensorAPI.GetReferenceCodes(UPCcode_TextBox.Text);
                mw.UPCRefereceList = APIresult.Item1;
                string errMsg = APIresult.Item2;
                if (!string.IsNullOrEmpty(errMsg))
                {
                    mw.ErrorPopup(errMsg, backToMainPage:true, fromAPI:true);
                    return;
                }
                bool UPCfound = mw.UPCRefereceList.Count != 0;
                if (UPCfound)
                {
                    UPCReferenceList_ComboBox.ItemsSource = mw.UPCRefereceList;
                    UPCReferenceList_ComboBox.DisplayMemberPath = "comboboxDisp";
                    UPCnotfound_TextBlock.Visibility = Visibility.Collapsed;
                    UPCReferenceList_ComboBox.Visibility = Visibility.Visible;
                    RescanUPC_button.Visibility = Visibility.Visible;

                    StartInfoMsg.Visibility = Visibility.Collapsed;
                    UPCcode_TextBox.IsEnabled = false;
                    StartInfoMsgLabel.Visibility = Visibility.Visible;
                    UPCcode_TextBox.Width = 100;
                }
                else {
                    mw.mwAllPicsSubmitted.Visibility = Visibility.Collapsed;
                    mw.mwErrorDialog.Visibility = Visibility.Collapsed;
                    mw.mwUPCnotFoundDialog.Visibility = Visibility.Visible;
                    mw.winDialog.IsOpen = true;
                    //UPCnotfound_TextBlock.Visibility = Visibility.Visible;
                    UPCcode_TextBox.Text = "";
                }
            }
        }
        private void UPCcode_LostFocus(object sender, RoutedEventArgs e)
        {
            if (UPCcode_TextBox.Width == 0)
            {
                UPCcode_TextBox.Focus();
            }
        }
        private void RescanUPC_click(object sender, RoutedEventArgs e)
        {
            UPCReferenceList_ComboBox.Visibility = Visibility.Collapsed;
            RescanUPC_button.Visibility = Visibility.Collapsed;
            UPCcode_TextBox.Text = "";
            UPCcode_TextBox.Visibility = Visibility.Visible;
            UPCcode_TextBox.IsEnabled = true;
            UPCcode_TextBox.Focus();
            StartInfoMsg.Visibility = Visibility.Visible;
            StartInfoMsgLabel.Visibility = Visibility.Collapsed;
            UPCcode_TextBox.Width = 0;
        }

        private void UPCReferenceList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mw.UPCRefereceListSelected = mw.UPCRefereceList[0];
            NavigationService.Navigate(new Scan1(mw.UPCRefereceListSelected));
        }
    }
}
