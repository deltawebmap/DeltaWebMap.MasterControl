using DeltaWebMap.MasterControl.WebInterface.BaseServices;
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
    public class MachineRebootInstanceService : BaseMachineFormLogService<ManagerRebootInstance>
    {
        public MachineRebootInstanceService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        private static HTMLFormProcessor<ManagerRebootInstance> form = new HTMLFormProcessor<ManagerRebootInstance>("Reboot Instance");

        public override HTMLFormProcessor<ManagerRebootInstance> GetForm()
        {
            return form;
        }

        public override ChannelReader<RouterMessage> SendCommand(ManagerRebootInstance cmd)
        {
            return manager.transport.RebootInstance(cmd);
        }
    }

    public class MachineRebootInstanceDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/machines/id/{MANAGER_ID}/reboot_instance";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MachineRebootInstanceService(conn, e);
        }
    }
}
