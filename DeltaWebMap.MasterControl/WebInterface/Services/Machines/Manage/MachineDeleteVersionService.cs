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
    public class MachineDeleteVersionService : BaseMachineFormLogService<ManagerDeleteVersion>
    {
        public MachineDeleteVersionService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        private static HTMLFormProcessor<ManagerDeleteVersion> form = new HTMLFormProcessor<ManagerDeleteVersion>("Delete Version");

        public override HTMLFormProcessor<ManagerDeleteVersion> GetForm()
        {
            return form;
        }

        public override ChannelReader<RouterMessage> SendCommand(ManagerDeleteVersion cmd)
        {
            return manager.transport.DeleteVersion(cmd);
        }
    }

    public class MachineDeleteVersionDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/machines/id/{MANAGER_ID}/delete_version";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MachineDeleteVersionService(conn, e);
        }
    }
}
