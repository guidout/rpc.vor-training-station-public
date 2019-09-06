using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using Ozeki.Media;
using Ozeki.Camera;
using Ozeki.Common;

namespace BasicCameraViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private OzekiCamera _webCamera;
        private DrawingImageProvider _imageProvider;
        private MediaConnector _mediaConnector;
        private CameraURLBuilderWPF _myCameraUrlBuilder;
        public string CameraUrl { get; set; }

        public ICommand ComposeCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }

        private void InitCommands()
        {
            ComposeCommand = new RelayCommand(Compose);
            ConnectCommand = new RelayCommand(Connect);
            DisconnectCommand = new RelayCommand(Disconnect);
        }

        private void Disconnect()
        {
            videoViewer.Stop();
            _webCamera.Stop();
            _mediaConnector.Disconnect(_webCamera.VideoChannel, _imageProvider);
            _webCamera = null;
        }

        private void Compose()
        {
            var data = new CameraURLBuilderData { DeviceTypeFilter = DiscoverDeviceType.USB };
            _myCameraUrlBuilder = new CameraURLBuilderWPF(data);
            var result = _myCameraUrlBuilder.ShowDialog();
            if (result != true)
                return;
            CameraUrl = _myCameraUrlBuilder.CameraURL;

            OnPropertyChanged("CameraUrl");

            InvokeGuiThread(() =>
            {
                ConnectButton.IsEnabled = true;
            });
        }

        private void InvokeGuiThread(Action action)
        {
            Dispatcher.BeginInvoke(action);
        }

        public MainWindow()
        {
            InitCommands();
            InitializeComponent();

            _imageProvider = new DrawingImageProvider();
            _mediaConnector = new MediaConnector();
            videoViewer.SetImageProvider(_imageProvider);
        }

        private void Connect()
        {
            if (_webCamera != null)
            {
                videoViewer.Stop();
                _webCamera.Stop();
                _mediaConnector.Disconnect(_webCamera.VideoChannel, _imageProvider);
            }
            ConnectButton.IsEnabled = false;
            _webCamera = new OzekiCamera(CameraUrl);
            _webCamera.CameraStateChanged += _webCamera_CameraStateChanged;
            _mediaConnector.Connect(_webCamera.VideoChannel, _imageProvider);
            _webCamera.Start();
            videoViewer.Start();
        }

        void _webCamera_CameraStateChanged(object sender, CameraStateEventArgs e)
        {
            InvokeGuiThread(() =>
            {
                switch (e.State)
                {
                    case CameraState.Streaming:
                        DisconnectButton.IsEnabled = true;
                        break;
                    case CameraState.Disconnected:
                        DisconnectButton.IsEnabled = false;
                        ConnectButton.IsEnabled = true;
                        break;
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    internal class RelayCommand : ICommand
    {
        Action<object> _action;

        public RelayCommand(Action action)
        {
            _action = i => action();
        }

        public RelayCommand(Action<object> action)
        {
            _action = action;
        }

        public void Execute(object parameter)
        {
            _action(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;
    }
}