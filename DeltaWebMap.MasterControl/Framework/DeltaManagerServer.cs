using DeltaWebMap.MasterControl.Framework.Config;
using DeltaWebMap.MasterControl.Framework.IOServer;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace DeltaWebMap.MasterControl.Framework
{
    /// <summary>
    /// Represnts a manager server we know about, even if it isn't conneted
    /// </summary>
    public class DeltaManagerServer
    {
        public readonly string label;
        public readonly IPAddress address;
        public readonly short id;
        public readonly byte[] key;
        public readonly DeltaConfigManagerServer configEntry;

        public MasterControlClient transport;
        
        public DeltaManagerServer(DeltaConfigManagerServer cfg)
        {
            this.label = cfg.label;
            this.address = IPAddress.Parse(cfg.ip_addr);
            this.id = cfg.id;
            this.key = Convert.FromBase64String(cfg.key);
            this.configEntry = cfg;
        }

        public void SetTransport(MasterControlClient transport)
        {
            //Unset active transport, if any
            DisconnectTransport();

            //Set
            this.transport = transport;
            this.transport.authenticated = true;
        }

        public void DisconnectTransport()
        {
            if (transport != null)
            {
                transport.server = null;
                transport.authenticated = false;
            }
        }
    }
}
