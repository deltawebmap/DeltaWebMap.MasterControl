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
    public class MachineUpdateInstanceService : BaseMachineFormLogService<ManagerUpdateInstance>
    {
        public MachineUpdateInstanceService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        private static HTMLFormProcessor<ManagerUpdateInstance> form = new HTMLFormProcessor<ManagerUpdateInstance>("Update");

        public override HTMLFormProcessor<ManagerUpdateInstance> GetForm()
        {
            return form;
        }

        public override ChannelReader<RouterMessage> SendCommand(ManagerUpdateInstance cmd)
        {
            return manager.transport.UpdateInstanceVersion(cmd);
        }
    }

    public class MachineUpdateInstanceDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/machines/id/{MANAGER_ID}/update_instance";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MachineUpdateInstanceService(conn, e);
        }
    }
}
