using LibDeltaSystem.CoreNet.NetMessages.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace DeltaWebMap.MasterControl.Framework.Entities
{
    public struct InstancePingResult
    {
        public InstanceStatusResult status;
        public byte reserved;
        public byte lib_version_major;
        public byte lib_version_minor;
        public byte app_version_major;
        public byte app_version_minor;
        public ushort initial_instance_ping_ms;
    }
}
