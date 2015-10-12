using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Net.Json;
using System.Net.NetworkInformation;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.Runtime.Serialization.Json;

namespace SonyCameraCommunication
{
    public class CameraControl
    {
        private string cameraURL;
        private bool recModeActive;

        public void ControlCamera(string cameraResp)
        {
            cameraURL = string.Format("{0}/{1}", GetCameraURL(cameraResp), GetActionType(cameraResp));
        }

        #region Camera Details
        private string CameraRequest(string _cameraUrl, string _cameraRequest)
        {
            Uri urlURI = new Uri(cameraURL);

            HttpWebRequest cameraReq = (HttpWebRequest)WebRequest.Create(cameraURL);
            cameraReq.Method = "POST";
            cameraReq.AllowWriteStreamBuffering = false;
            cameraReq.ContentType = "application/json; charset=utf-8";
            cameraReq.Accept = "Accept-application/json";
            cameraReq.ContentLength = _cameraRequest.Length;
            using (var cameraWrite = new StreamWriter(cameraReq.GetRequestStream()))
            {
                cameraWrite.Write(_cameraRequest);
            }
            var cameraResp = (HttpWebResponse)cameraReq.GetResponse();
            Stream cameraStream = cameraResp.GetResponseStream();
            StreamReader cameraRead = new StreamReader(cameraStream);
            string readCamera = cameraRead.ReadToEnd();
            
            return readCamera;
        }

        public string GetCameraURL(string cameraResp)
        {
            string[] cameraXML = cameraResp.Split('\n');
            string cameraURL = "";
            foreach (string cameraString in cameraXML)
            {
                string getCameraURL = "";
                if (cameraString.Contains("<av:X_ScalarWebAPI_ActionList_URL>"))
                {
                    getCameraURL = cameraString.Substring(cameraString.IndexOf('>') + 1);
                    cameraURL = getCameraURL.Substring(0, getCameraURL.IndexOf('<'));
                }
            }
            return cameraURL;
        }

        public string GetActionType(string cameraResp)
        {
            string[] cameraXML = cameraResp.Split('\n');
            string actionType = "";
            foreach (string cameraString in cameraXML)
            {
                string getType = "";
                if (cameraString.Contains("<av:X_ScalarWebAPI_ServiceType>"))
                {
                    getType = cameraString.Substring(cameraString.IndexOf('>') + 1);
                    actionType = getType.Substring(0, getType.IndexOf('<'));
                    if (actionType == "camera")
                    {
                        break;
                    }
                }
            }
            return actionType;
        }
        #endregion

        #region Record Mode
        public string StartRecMode()
        {
            string startRecMode = JsonConvert.SerializeObject(new Camera.CameraSetup
            {
                method = "startRecMode",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            recModeActive = true;

            return CameraRequest(cameraURL, startRecMode);
        }

        public bool IsRecModeActive()
        {
            return recModeActive;
        }

        public string StopRecMode()
        {
            string stopRecMode = JsonConvert.SerializeObject(new Camera.CameraSetup
            {
                method = "stopRecMode",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, stopRecMode);
        }
        #endregion

        #region Trigger Capture
        public string TriggerCamera()
        {
            string _triggerCamera = JsonConvert.SerializeObject(new Camera.StillCapture
            {
                method = "actTakePicture",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            return CameraRequest(cameraURL, _triggerCamera);
        }
        #endregion

        #region Shoot Mode
        public string SetShootMode()
        {
            string shootModeReq = JsonConvert.SerializeObject(new Camera.ShootMode
            {
                method = "setShootMode",
                @params = new List<string> { "still"},
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, shootModeReq);
        }

        public string GetShootMode()
        {
            string shootModeReq = JsonConvert.SerializeObject(new Camera.ShootMode
            {
                method = "getShootMode",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            string _shootMode = "Unknown";
            if (CameraRequest(cameraURL, shootModeReq).Contains("still"))
            {
                _shootMode = "Still";
            }
            return _shootMode;
        }
        #endregion

        #region Exposure Mode
        public string GetExposureMode()
        {
            string exposureReq = JsonConvert.SerializeObject(new Camera.ExposureMode
            {
                method = "getExposureMode",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            string _exposureMode = "Unknown";
            if (CameraRequest(cameraURL, exposureReq).Contains("Manual"))
            {
                _exposureMode = "Manual";
            }
            return _exposureMode;
        }

        public string SetExposureMode()
        {
            string exposureReq = JsonConvert.SerializeObject(new Camera.ExposureMode
            {
                method = "setExposureMode",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, exposureReq);
        }
        #endregion

        #region F Number
        public string SetFNumber()
        {
            string fNumberReq = JsonConvert.SerializeObject(new Camera.FNumber
            {
                method = "setFNumber",
                @params = new List<string> { "9.5" },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, fNumberReq);
        }

        public string GetFNumber()
        {
            string fNumberReq = JsonConvert.SerializeObject(new Camera.FNumber
            {
                method = "getFNumber",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            string _fNumber = "Unknown";
            if (CameraRequest(cameraURL, fNumberReq).Contains("Manual"))
            {
                _fNumber = "Manual";
            }
            return _fNumber;
        }

        public string GetAvailableFNumber()
        {
            string fNumberReq = JsonConvert.SerializeObject(new Camera.CameraSetup
            {
                method = "getSupportedFNumber",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, fNumberReq);
        }
        #endregion

        #region Shutter Speed
        public string SetShutterSpeed()
        {
            string shutterReq = JsonConvert.SerializeObject(new Camera.ShutterSpeed
            {
                method = "setShutterSpeed",
                @params = new List<string> { "1/90" },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, shutterReq);
        }

        public string GetShutterSpeed()
        {
            string shutterReq = JsonConvert.SerializeObject(new Camera.ShutterSpeed
            {
                method = "getShutterSpeed",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, shutterReq);
        }

        public string GetAvailableShutterSpeed()
        {
            string shutterReq = JsonConvert.SerializeObject(new Camera.CameraSetup
            {
                method = "getSupportedShutterSpeed",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, shutterReq);
        }
        #endregion

        #region ISO Speed Rate
        public string SetISO()
        {
            string fNumberReq = JsonConvert.SerializeObject(new Camera.CameraSetup
            {
                method = "setIsoSpeedRate",
                @params = new List<string> { "100" },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, fNumberReq);
        }

        public string GetISO()
        {
            string fNumberReq = JsonConvert.SerializeObject(new Camera.CameraSetup
            {
                method = "getIsoSpeedRate",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, fNumberReq);
        }
        #endregion

        #region White Balance
        public string SetWhiteBalance()
        {
            string whiteBalanceReq = JsonConvert.SerializeObject(new Camera.WhiteBalance
            {
                method = "setWhiteBalance",
                @params = new List<string> { "Daylight" },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, whiteBalanceReq);
        }

        public string GetWhiteBalance()
        {
            string whiteBalanceReq = JsonConvert.SerializeObject(new Camera.WhiteBalance
            {
                method = "getWhiteBalance",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, whiteBalanceReq);
        }
        #endregion

        #region Focus Mode
        public string SetFocusMode()
        {
            string focusReq = JsonConvert.SerializeObject(new Camera.FocusMode
            {
                method = "setFocusMode",
                @params = new List<string> { "MF" },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, focusReq);
        }

        public string GetFocusMode()
        {
            string focusReq = JsonConvert.SerializeObject(new Camera.FocusMode
            {
                method = "getFocusMode",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            recModeActive = false;

            return CameraRequest(cameraURL, focusReq);
        }
        #endregion

        #region Get Event
        public string GetEvent()
        {
            string _getEvent = JsonConvert.SerializeObject(new Camera.EventNotification 
            { 
                method = "getEvent",
                @params = new List<bool> { true},
                id = 1,
                version = "1.0"
            });

            recModeActive = true;

            return CameraRequest(cameraURL, _getEvent);
        }
        #endregion

        #region Await Take Picture
        public string AwaitTakePicture()
        {
            string awaitPictureReq = JsonConvert.SerializeObject(new Camera.AwaitPicture 
            { 
                method = "awaitTakePicture",
                @params = new List<string> { },
                id = 1,
                version = "1.0"
            });

            return CameraRequest(cameraURL, awaitPictureReq);
        }
        #endregion
    }
}
