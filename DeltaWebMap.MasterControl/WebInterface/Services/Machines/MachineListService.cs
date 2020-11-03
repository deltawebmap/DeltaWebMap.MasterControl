using DeltaWebMap.MasterControl.Framework;
using DeltaWebMap.MasterControl.WebInterface.BaseServices;
using LibDeltaSystem;
using LibDeltaSystem.CoreNet.NetMessages.Master;
using LibDeltaSystem.WebFramework;
using LibDeltaSystem.WebFramework.Extras.HTMLTable;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.MasterControl.WebInterface.Services.Machines
{
    public class MachineListService : BaseAuthenticatedAdminInterface
    {
        public MachineListService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        private static HTMLTableGenerator<DeltaManagerServer> table = new HTMLTableGenerator<DeltaManagerServer>(
            new List<string>() { "ID", "Label", "Address", "Host", "Lib", "App", "Status", "Actions" },
            (DeltaManagerServer s) =>
            {
                //Request the status of this
                int ping = 0;
                ManagerStatusMessage status = null;
                if(s.transport != null)
                {
                    DateTime start = DateTime.UtcNow;
                    status = s.transport.RequestStatus().GetAwaiter().GetResult();
                    ping = (int)(DateTime.UtcNow - start).TotalMilliseconds;
                }

                //Prepare
                string hostname = "-";
                string libVersion = "-";
                string appVersion = "-";
                string statusText = "Offline";
                string statusColor = "#F54842";
                string statusExtra = "";
                string actions = CreateButtonHtml("Manage", "/machines/id/" + s.id + "/");
                if (status != null)
                {
                    hostname = status.host_name;
                    libVersion = "v" + status.version_lib_major + "." + status.version_lib_minor;
                    appVersion = "v" + status.version_app_major + "." + status.version_app_minor;
                    statusText = "Online";
                    statusColor = "#43e438";
                    statusExtra = $" {ping}ms";
                }
                return new List<string>() { s.id.ToString("X4"), s.label, s.address.ToString(), hostname, libVersion, appVersion, $"<span style=\"color:white; background-color:{statusColor}; padding: 0 5px; border-radius: 5px;\">{statusText}</span>" + statusExtra, actions };
            });

        public override async Task HandleAuthenticatedAdminRequest()
        {
            //Write table
            await WriteString(table.GenerateTable(Program.servers));

            //Write footer
            await WriteString($"<hr><b>{Program.servers.Count} listed</b> - {CreateButtonHtml("Enroll New", "/machines/enroll")}");
        }
    }

    public class MachineListDefinition : AdminInterfaceHeaderDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/machines/";
        }

        public override string GetTitle()
        {
            return "Machines";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MachineListService(conn, e);
        }
    }
}
