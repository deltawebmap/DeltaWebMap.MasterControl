using DeltaWebMap.MasterControl.WebInterface.BaseServices;
using LibDeltaSystem;
using LibDeltaSystem.Entities.RouterServer;
using LibDeltaSystem.WebFramework;
using LibDeltaSystem.WebFramework.Extras.HTMLForm;
using LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DeltaWebMap.MasterControl.WebInterface.Services.Machines
{
    public class MachineEnrollService : BaseAuthenticatedAdminInterface
    {
        public MachineEnrollService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        private static HTMLFormProcessor<EnrollForm> form = new HTMLFormProcessor<EnrollForm>("Create");

        public override async Task HandleAuthenticatedAdminRequest()
        {
            if(GetMethod() != LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.POST)
            {
                //Show form
                await WriteString("<u>Enroll New Machine</u><p>Credentials will be generated for the new machine.</p>");
                await WriteString(form.BuildHTML());
            } else
            {
                //Handle form
                EnrollForm data = new EnrollForm();
                await form.ProcessResponse(data, e);

                //Add new
                var server = Program.CreateManagerServer(data.label, IPAddress.Parse(data.ip));

                //Make config
                RouterServerConfig config = new RouterServerConfig
                {
                    id = server.id,
                    auth_key = server.key,
                    label = server.label,
                    master_ip = Program.cfg.general.public_serving_ip,
                    master_port = Program.cfg.general.public_serving_port
                };

                //Write response
                await WriteString("<p>Server is ready to be deployed. Save the config file below and point the manager server to it.</p><div style=\"background-color:#e6e6e6; padding:15px;\">" + HttpUtility.HtmlEncode(JsonConvert.SerializeObject(config, Formatting.Indented)) + "</div>");
            }
        }

        class EnrollForm
        {
            [FormElementTypeText("Label", "User-Defined Name", "label")]
            public string label { get; set; }
            [FormElementTypeText("IP Address", "192.168.100.1", "ip")]
            public string ip { get; set; }
        }
    }

    public class MachineEnrollDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/machines/enroll";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MachineEnrollService(conn, e);
        }
    }
}
