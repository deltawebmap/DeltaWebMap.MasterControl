using DeltaWebMap.MasterControl.WebInterface.Extras;
using LibDeltaSystem;
using LibDeltaSystem.CoreNet.IO;
using LibDeltaSystem.CoreNet.NetMessages.Master;
using LibDeltaSystem.WebFramework;
using LibDeltaSystem.WebFramework.Extras.HTMLForm;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DeltaWebMap.MasterControl.WebInterface.Services.Machines.Manage
{
    public class MachineAddVersionService : BaseMachineFormLogService<ManagerAddVersion>
    {
        public MachineAddVersionService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        private static HTMLFormProcessor<ManagerAddVersion> form = new HTMLFormProcessor<ManagerAddVersion>("Create Version");

        public override HTMLFormProcessor<ManagerAddVersion> GetForm()
        {
            return form;
        }

        public override ChannelReader<RouterMessage> SendCommand(ManagerAddVersion cmd)
        {
            return manager.transport.AddVersion(cmd);
        }
    }

    public class MachineAddVersionDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/machines/id/{MANAGER_ID}/add_version";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MachineAddVersionService(conn, e);
        }
    }
}
