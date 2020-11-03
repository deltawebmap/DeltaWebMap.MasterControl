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
    public class MachineAddInstanceService : BaseMachineFormLogService<ManagerAddInstance>
    {
        public MachineAddInstanceService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        private static HTMLFormProcessor<ManagerAddInstance> form = new HTMLFormProcessor<ManagerAddInstance>("Create Instance");

        public override HTMLFormProcessor<ManagerAddInstance> GetForm()
        {
            return form;
        }

        public override ChannelReader<RouterMessage> SendCommand(ManagerAddInstance cmd)
        {
            return manager.transport.AddInstance(cmd);
        }
    }

    public class MachineAddInstanceDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/machines/id/{MANAGER_ID}/add_instance";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MachineAddInstanceService(conn, e);
        }
    }
}
