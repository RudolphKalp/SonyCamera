using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonyCameraCommunication.AVContent
{
    public class TransferringImages
    {
        public string method { get; set; }
        public List<Params> @params { get; set; }
        public int id { get; set; }
        public string version { get; set; }
    }
}
