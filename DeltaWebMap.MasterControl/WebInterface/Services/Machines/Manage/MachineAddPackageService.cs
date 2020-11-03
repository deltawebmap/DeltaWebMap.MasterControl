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
    public class MachineAddPackageService : BaseMachineFormLogService<ManagerAddPackage>
    {
        public MachineAddPackageService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        private static HTMLFormProcessor<ManagerAddPackage> form = new HTMLFormProcessor<ManagerAddPackage>("Create Instance");

        public override HTMLFormProcessor<ManagerAddPackage> GetForm()
        {
            return form;
        }

        public override ChannelReader<RouterMessage> SendCommand(ManagerAddPackage cmd)
        {
            return manager.transport.AddPackage(cmd);
        }
    }

    public class MachineAddPackageDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/machines/id/{MANAGER_ID}/add_package";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MachineAddPackageService(conn, e);
        }
    }
}
