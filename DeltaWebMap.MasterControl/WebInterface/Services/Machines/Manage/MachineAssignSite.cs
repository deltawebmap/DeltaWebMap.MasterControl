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
    public class MachineAssignSiteService : BaseMachineFormLogService<ManagerAssignSite>
    {
        public MachineAssignSiteService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        private static HTMLFormProcessor<ManagerAssignSite> form = new HTMLFormProcessor<ManagerAssignSite>("Assign Instance to Site");

        public override HTMLFormProcessor<ManagerAssignSite> GetForm()
        {
            return form;
        }

        public override ChannelReader<RouterMessage> SendCommand(ManagerAssignSite cmd)
        {
            return manager.transport.AssignSite(cmd);
        }
    }

    public class MachineAssignSiteDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/machines/id/{MANAGER_ID}/assign_site";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MachineAssignSiteService(conn, e);
        }
    }
}
