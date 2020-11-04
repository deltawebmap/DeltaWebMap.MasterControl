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
    public class MachineAddSiteService : BaseMachineFormLogService<ManagerAddSite>
    {
        public MachineAddSiteService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        private static HTMLFormProcessor<ManagerAddSite> form = new HTMLFormProcessor<ManagerAddSite>("Create Site");

        public override HTMLFormProcessor<ManagerAddSite> GetForm()
        {
            return form;
        }

        public override ChannelReader<RouterMessage> SendCommand(ManagerAddSite cmd)
        {
            return manager.transport.AddSite(cmd);
        }
    }

    public class MachineAddSiteDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/machines/id/{MANAGER_ID}/add_site";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MachineAddSiteService(conn, e);
        }
    }
}
