﻿using System;
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
                mw.UPCRefereceList = TensorAPI.GetReferenceCodes(UPCcode_TextBox.Text);

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
                    mw.mwAllPicsSubmitted.Visibility = Visibility.Collapsed;
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