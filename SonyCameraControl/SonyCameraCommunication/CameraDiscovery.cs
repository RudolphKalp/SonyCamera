using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace SonyCameraCommunication
{
    public class CameraDiscovery
    {

        IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 60000);
        IPEndPoint multicastEndpoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);

        Socket udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        public static string response;

        public bool UDPSocketSetup()
        {
            string udpStatus;
            try
            {
                udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpSocket.Bind(localEndPoint);
                udpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(multicastEndpoint.Address, IPAddress.Any));
                udpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
                udpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, true);

                udpStatus = "UDP-Socket setup done...";

                return true;
            }
            catch (Exception exc)
            {
                return false;
            }
        }

        public bool MSearch()
        {
            string searchString = "M-SEARCH * HTTP/1.1\r\nHOST:239.255.255.250:1900\r\nMAN:\"ssdp:discover\"\r\nMX:1\r\nST:urn:schemas-sony-com:service:ScalarWebAPI:1\r\n\r\n";

            udpSocket.SendTo(Encoding.UTF8.GetBytes(searchString), SocketFlags.None, multicastEndpoint);
            
            byte[] receiveBuffer = new byte[64000];

            int receivedBytes = 0;

            while (true)
            {
                try
                {
                    if (udpSocket.Available > 0)
                    {
                        receivedBytes = udpSocket.Receive(receiveBuffer, SocketFlags.None);

                        if (receivedBytes > 0)
                        {
                            response = Encoding.UTF8.GetString(receiveBuffer, 0, receivedBytes);
                            return true;
                        }
                    }
                }
                catch (Exception exc)
                {
                    return false;
                }
            }
        }

        public string DeviceDescription()
        {
            string[] responseStrings = response.Split('\n');
            string cameraIP = "";
            foreach (string resp in responseStrings)
            {
                if (resp.StartsWith("LOCATION: "))
                {
                    cameraIP = resp.Substring(resp.LastIndexOf(" "));
                }
            }
            HttpWebRequest descriptionReq = (HttpWebRequest)WebRequest.Create(cameraIP);
            descriptionReq.Method = "GET";
            WebResponse descriptionResp = descriptionReq.GetResponse();
            Stream descriptionStream = descriptionResp.GetResponseStream();
            StreamReader descriptionRead = new StreamReader(descriptionStream);
            string readDescription = descriptionRead.ReadToEnd();
            return readDescription;
        }
    }
}
