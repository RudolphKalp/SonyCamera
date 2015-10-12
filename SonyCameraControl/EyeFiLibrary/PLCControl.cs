using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace SonyCameraControl
{
    public class PLCControl : IPLCControl
    {
        private const string plcIP = "192.168.1.5";
        private const string plcConnection = "http://192.168.1.5:9080/HOSTLINK";
        private const string ready = "/IR*";
        private const string begin = "/WI0101*";
        private const string end = "/WI0102";
        private const string trigger = "/WI0104";
        private const string laserTrigger = "/RI00*";
        private const string cameraTriggered = "/RO00*";

        public bool PingPLC()
        {
            Ping pingSender = new Ping();
            PingOptions options = new PingOptions();

            options.DontFragment = true;

            string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 1000;
            PingReply reply = pingSender.Send(plcIP, timeout, buffer, options);
            if (reply.Status == IPStatus.Success)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool RequestID()
        {
            try
            {
                HttpWebRequest idRequest = (HttpWebRequest)WebRequest.Create(plcConnection + ready);
                idRequest.Method = "GET";
                WebResponse idResp = idRequest.GetResponse();
                Stream idStream = idResp.GetResponseStream();
                StreamReader idRead = new StreamReader(idStream);
                string respID = idRead.ReadToEnd();
                if (respID == "IR01*")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (WebException exc)
            {
                return false;
            }
        }

        public void Begin()
        {
            HttpWebRequest beginRequest = (HttpWebRequest)WebRequest.Create(plcConnection + begin);
            beginRequest.Method = "GET";
            WebResponse beginResp = beginRequest.GetResponse();
            Stream beginStream = beginResp.GetResponseStream();
            StreamReader beginRead = new StreamReader(beginStream);
            string respBegin = beginRead.ReadToEnd();
        }

        public void Reset()
        {
            HttpWebRequest resetRequest = (HttpWebRequest)WebRequest.Create(plcConnection + end);
            resetRequest.Method = "GET";
            WebResponse resetResp = resetRequest.GetResponse();
            Stream resetStream = resetResp.GetResponseStream();
            StreamReader resetRead = new StreamReader(resetStream);
            string respReset = resetRead.ReadToEnd();
        }

        public void TriggerCamera()
        {
            HttpWebRequest triggerRequest = (HttpWebRequest)WebRequest.Create(plcConnection + trigger);
            triggerRequest.Method = "GET";
            WebResponse triggerResp = triggerRequest.GetResponse();
            Stream triggerStream = triggerResp.GetResponseStream();
            StreamReader triggerRead = new StreamReader(triggerStream);
            string respTrigger = triggerRead.ReadToEnd();
        }

        public bool LaserTrigger()
        {
            HttpWebRequest laserRequest = (HttpWebRequest)WebRequest.Create(plcConnection + laserTrigger);
            laserRequest.Method = "GET";
            WebResponse laserResp = laserRequest.GetResponse();
            Stream laserStream = laserResp.GetResponseStream();
            StreamReader laserRead = new StreamReader(laserStream);
            string respLaser = laserRead.ReadToEnd();
            if (respLaser == "RI00*")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool CameraBusy()
        {
            HttpWebRequest cameraRequest = (HttpWebRequest)WebRequest.Create(plcConnection + cameraTriggered);
            cameraRequest.Method = "GET";
            WebResponse cameraResp = cameraRequest.GetResponse();
            Stream cameraStream = cameraResp.GetResponseStream();
            StreamReader cameraRead = new StreamReader(cameraStream);
            string respCamera = cameraRead.ReadToEnd();
            if (respCamera == "RO04*")
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static IPLCControl CreatePLCControl()
        {
            return (IPLCControl)new PLCControl();
        }
    }
}
