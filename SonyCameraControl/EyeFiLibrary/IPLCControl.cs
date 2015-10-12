using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SonyCameraControl
{
    public interface IPLCControl
    {
        bool PingPLC();
        bool RequestID();
        void Begin();
        void Reset();
        void TriggerCamera();
        bool LaserTrigger();
        bool CameraBusy();
    }
}
