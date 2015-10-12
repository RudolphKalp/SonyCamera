using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SonyCameraCommunication
{
    public class ImageControl
    {
        private string imageURL;

        public void ImageTransfer(string imageResp)
        {
            imageURL = string.Format("{0}/{1}", GetCameraURL(imageResp), GetActionType(imageResp));
        }

        public void ReceiveImage(string imageURL, string directory, int imageCount)
        {
            HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create(imageURL);
            imageRequest.Method = "GET";
            HttpWebResponse imageResponse = (HttpWebResponse)imageRequest.GetResponse();
            using (Stream inputStream = imageResponse.GetResponseStream())
            {
                using (Stream outputStream = File.OpenWrite(string.Format("{0}\\{1}.jpg", directory, imageCount)))
                {
                    byte[] buffer = new byte[4096];
                    int bytesRead;
                    do
                    {
                        bytesRead = inputStream.Read(buffer, 0, buffer.Length);
                        outputStream.Write(buffer, 0, bytesRead);
                    } while (bytesRead != 0);
                }
            }
        }

        private string ImageRequest(string cameraUrl, string imageRequest)
        {
            Uri urlURI = new Uri(imageURL);

            HttpWebRequest imageReq = (HttpWebRequest)WebRequest.Create(imageURL);
            imageReq.Method = "POST";
            imageReq.AllowWriteStreamBuffering = false;
            imageReq.ContentType = "application/json; charset=utf-8";
            imageReq.Accept = "Accept-application/json";
            imageReq.ContentLength = imageRequest.Length;
            using (var imageWrite = new StreamWriter(imageReq.GetRequestStream()))
            {
                imageWrite.Write(imageRequest);
            }
            var imageResp = (HttpWebResponse)imageReq.GetResponse();
            Stream imageStream = imageResp.GetResponseStream();
            StreamReader imageRead = new StreamReader(imageStream);
            string readImage = imageRead.ReadToEnd();
            return readImage;
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
                    if (actionType == "avContent")
                    {
                        break;
                    }
                }
            }
            return actionType;
        }
    }
}
