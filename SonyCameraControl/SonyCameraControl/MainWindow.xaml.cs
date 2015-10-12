using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Web;
using System.Net;
using System.Net.NetworkInformation;
using System.IO;
using System.Windows.Threading;
using System.Net.Sockets;
using Newtonsoft.Json;
using SonyCameraCommunication;
using SonyCameraCommunication.AVContent;
using SonyCameraCommunication.Camera;
using EyeFiLibrary;
using System.Threading;

namespace SonyCameraControl
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IPLCControl plcControl = PLCControl.CreatePLCControl();
        private static string cameraResp;
        CameraControl _cameraCotrol = new CameraControl();
        ImageControl _imageControl = new ImageControl();
        Transaction _transaction = new Transaction();

        public string captureDirectory = @"C:\Captures";
        public string captureID;

        DispatcherTimer _timer;

        public const string macAddress = "00185664d41d";
        public const string cnonce = "";
        public const string uploadkey = "00000000000000000000000000000000";
        public const string credential = macAddress + cnonce + uploadkey;
        
        public MainWindow()
        {
            InitializeComponent();

            triggerButton.IsEnabled = true;

            DetectCamera();

            //if (plcControl.PingPLC())
            //{
            //    if (plcControl.RequestID())
            //    {
            //        triggerButton.IsEnabled = true;
            //        connectionButton.IsEnabled = true;
            //        initialisedLabel.Visibility = Visibility.Visible;
            //        stateLabel.Visibility = Visibility.Visible;
            //        vehicleLabel.Visibility = Visibility.Visible;
            //        presentLabel.Visibility = Visibility.Visible;
            //        connectionButton.Content = "Connect";
            //        stateLabel.Content = "PLC Initialised";

            //        _timer = new DispatcherTimer();
            //        _timer.Interval = TimeSpan.FromMilliseconds(100);
            //        _timer.Tick += _timer_Tick;
            //    }
            //    else
            //    {
            //        triggerButton.IsEnabled = true;
            //        MessageBox.Show("PLC is not present! Please ensure it is connected to the network port and switched on.", "PLC ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
            //    }
            //}
        }

        private bool CheckCamera()
        {
            HttpListener eyeFiListen = new HttpListener();
            eyeFiListen.Prefixes.Add("http://:8080/");
            return false;
        }

        public void _timer_Tick(object sender, EventArgs e)
        {
            shootModeLabel.Content = _cameraCotrol.GetShootMode();
            exposureModeLabel.Content = _cameraCotrol.GetExposureMode();
            fNumberLabel.Content = _cameraCotrol.GetFNumber();
            isoSpeedLabel.Content = _cameraCotrol.GetISO();
            whiteBalanceModeLabel.Content = _cameraCotrol.GetWhiteBalance();
            focusModeLabel.Content = _cameraCotrol.GetFocusMode();
            shutterSpeedLabel.Content = _cameraCotrol.GetShutterSpeed();
            //if (_transaction.plcControl.LaserTrigger())
            //{
            //    presentLabel.Content = "Vehicle Present";
            //}
            //else
            //{
            //    presentLabel.Content = "No Vehicle Present";
            //}

            //if (_transaction.plcControl.CameraBusy())
            //{
            //    cameraTrigLabel.Content = "Camera Capturing";
            //}
            //else
            //{
            //    cameraTrigLabel.Content = "Camera not busy.";
            //}
        }

        private void DetectCamera()
        {
            CameraDiscovery cameraDiscover = new CameraDiscovery();

            if (cameraDiscover.UDPSocketSetup())
            {
                stateLabel.Content = "UDP Socket Ready";
                if (cameraDiscover.MSearch())
                {
                    stateLabel.Content = "Camera found.";

                    cameraResp = cameraDiscover.DeviceDescription();
                    _cameraCotrol.ControlCamera(cameraResp);
                    _imageControl.ImageTransfer(cameraResp);
                    if (_cameraCotrol.StartRecMode() == "{\"id\":1,\"result\":[0]}")
                    {
                        stateLabel.Content = "Camera Ready.";

                        _timer = new DispatcherTimer();
                        _timer.Interval = TimeSpan.FromMilliseconds(100);
                        _timer.Tick += _timer_Tick;

                        _timer.Start();
                    }
                }
            }
        }

        private void connectionButton_Click(object sender, RoutedEventArgs e)
        {
            plcControl.Begin();
            _timer.Start();
            //_transaction.Connect();
        }
        
        private void triggerButton_Click(object sender, RoutedEventArgs e)
        {
            //plcControl.TriggerCamera();
            CaptureSingleImage();
        }

        private void CaptureSingleImage()
        {
            string imageResp = _cameraCotrol.TriggerCamera();
            string getURL = "";
            string imageURL = "";
            imageURL = "This PC\\ILCE-6000\\Storage Media\\2015-04-09";

            if (imageResp.Contains("["))
            {
                getURL = imageResp.Substring(imageResp.IndexOf("[[\"") + 3);
                imageURL = getURL.Substring(0, getURL.IndexOf("\"]]"));
                getURL = imageURL.Substring(imageURL.IndexOf(':') + 1);
            }

            imagesStateLabel.Content = "Image Captured";

            _imageControl.ReceiveImage(imageURL, Directory.GetCurrentDirectory(), 0);

            imagesStateLabel.Content = "Image Saved";
            Thread.Sleep(2000);
        }

        private void CaptureTimeImages(int photoCount, int photoDelay)
        {
            #region Sony Camera Control

            if (!Directory.Exists(captureDirectory))
            {
                Directory.CreateDirectory(captureDirectory);
            }

            captureID = Guid.NewGuid().ToString();
            string newCaptureDirectory = string.Format(@"{0}\\{1}", captureDirectory, captureID);
            Directory.CreateDirectory(newCaptureDirectory);
            for (int i = 0; i < photoCount; i++ )
            {
                string imageResp = _cameraCotrol.TriggerCamera();
                string getURL = "";
                string imageURL = "";
                imageURL = "This PC\\ILCE-6000\\Storage Media\\2015-04-09";

                if (imageResp.Contains("["))
                {
                    getURL = imageResp.Substring(imageResp.IndexOf("[[\"") + 3);
                    imageURL = getURL.Substring(0, getURL.IndexOf("\"]]"));
                    getURL = imageURL.Substring(imageURL.IndexOf(':') + 1);
                }

                imagesStateLabel.Content = "Image Captured";

                _imageControl.ReceiveImage(imageURL, newCaptureDirectory, i + 1);

                imagesStateLabel.Content = "Image Saved";
                Thread.Sleep(photoDelay);
            }
            
            #endregion

            //_transaction.Connect();
        }

        private void exitButton_Click(object sender, RoutedEventArgs e)
        {
            //if (plcControl.RequestID())
            //{
            //    plcControl.Reset();   
            //}
            try
            {
                _timer.Stop();
                _cameraCotrol.StopRecMode();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString(), "Exit Error!");
            }
            Application.Current.Shutdown();
        }

        private void timeTriggerButton_Click(object sender, RoutedEventArgs e)
        {
            if ((photoCountTextBox.Text != null) && (photoCountTextBox.Text != "") && (photoDelayTextBox.Text != null) && (photoDelayTextBox.Text != ""))
            {
                int photoCount = Convert.ToInt32(photoCountTextBox.Text);
                int photoDelay = Convert.ToInt32(photoDelayTextBox.Text);
                CaptureTimeImages(photoCount, photoDelay);
            }
            else
            {
                MessageBox.Show("Please enter a value for the count and for the delay!!", "Invalid Values");
            }
        }

        private void readyButton_Click(object sender, RoutedEventArgs e)
        {
            ReadyForImages();
        }

        private void ReadyForImages()
        {
            string imageResp = _cameraCotrol.AwaitTakePicture();
            string getURL = "";
            string imageURL = "";
            imageURL = "This PC\\ILCE-6000\\Storage Media\\2015-04-09";

            if (imageResp.Contains("["))
            {
                getURL = imageResp.Substring(imageResp.IndexOf("[[\"") + 3);
                imageURL = getURL.Substring(0, getURL.IndexOf("\"]]"));
                getURL = imageURL.Substring(imageURL.IndexOf(':') + 1);
            }

            imagesStateLabel.Content = "Image Captured";

            _imageControl.ReceiveImage(imageURL, Directory.GetCurrentDirectory(), 0);

            imagesStateLabel.Content = "Image Saved";

        }
    }
}
