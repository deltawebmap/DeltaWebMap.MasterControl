using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DeltaWebMap.MasterControl.WebInterface.Services.Machines.Manage.Instances
{
    public class MachineInstanceManageService : BaseMachineManageService
    {
        public MachineInstanceManageService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public long instanceId;

        public override Task<bool> SetArgs(Dictionary<string, string> args)
        {
            instanceId = long.Parse(args["INSTANCE_ID"]);
            return base.SetArgs(args);
        }

        public override async Task HandleManagerServer()
        {
            //Begin logging segment
            await WriteString("<u>Log</u>");

            //Subscribe to logging event
            manager.transport.OnInstanceLog += Transport_OnInstanceLog;

            //Wait for page load to be ancelled
            await AwaitCancel();

            //Unsubscribe
            manager.transport.OnInstanceLog -= Transport_OnInstanceLog;
        }

        private void Transport_OnInstanceLog(long instanceId, DeltaLogLevel logLevel, string topic, string message)
        {
            //Make sure this matches our instance
            if (this.instanceId != instanceId)
                return;

            //Determine color
            string color = "black";
            switch(logLevel)
            {
                case DeltaLogLevel.Debug: color = "black"; break;
                case DeltaLogLevel.Low: color = "#0394fc"; break;
                case DeltaLogLevel.Medium: color = "#fca903"; break;
                case DeltaLogLevel.High: color = "#fa3737"; break;
                case DeltaLogLevel.Alert: color = "#fa37ea"; break;
            }

            //Log
            WriteString($"<div style=\"display:flex;margin-top:5px;\"><div style=\"margin-right: 10px;white-space: nowrap;\"><span style=\"background-color:{color};color:white;border-radius: 5px;padding: 3px 6px;\">{logLevel.ToString().ToUpper()}</span> <u style=\"color:{color};\">{HttpUtility.HtmlEncode(topic)}</u></div><div>{HttpUtility.HtmlEncode(message)}</div></div>");
        }
    }

    public class MachineInstanceManageDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/machines/id/{MANAGER_ID}/instances/{INSTANCE_ID}";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MachineInstanceManageService(conn, e);
        }
    }
}
