using DeltaWebMap.MasterControl.Framework.Entities;
using LibDeltaSystem;
using LibDeltaSystem.CoreNet.NetMessages.Master;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DeltaWebMap.MasterControl.WebInterface.Services.Machines.Manage
{
    public class MachinePingInstanceService : BaseMachineManageService
    {
        public MachinePingInstanceService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
            renderHeaderBar = false;
            renderContainerHtml = false;
        }

        private const string COLOR_RED = "#ff4f4f";
        private const string COLOR_ORANGE = "#ffb94f";
        private const string COLOR_BLUE = "#445ef2";
        private const string COLOR_GREEN = "#52f244";

        public override async Task HandleManagerServer()
        {
            //Get the ID
            if(!e.Request.Query.ContainsKey("instance_id"))
            {
                await WriteErrorBubble("Error [4]");
                return;
            }
            if(!long.TryParse(e.Request.Query["instance_id"], out long instanceId))
            {
                await WriteErrorBubble("Error [3]");
                return;
            }

            //Ping
            InstancePingResult pingData;
            try
            {
                var t = manager.transport.PingInstance(instanceId);
                if (await Task.WhenAny(t, Task.Delay(8000)) != t)
                {
                    //Timed out
                    await WriteErrorBubble("Command Timed Out");
                    return;
                } else
                {
                    pingData = t.Result;
                }
            } catch
            {
                await WriteErrorBubble("Error [1]");
                return;
            }

            //Switch status
            switch(pingData.status)
            {
                case InstanceStatusResult.ONLINE:
                    await WriteBubbleContent(COLOR_GREEN, "Online", pingData.initial_instance_ping_ms + " ms");
                    break;
                case InstanceStatusResult.NOT_CONNECTED:
                    await WriteBubbleContent(COLOR_RED, "Not Connected", "");
                    break;
                case InstanceStatusResult.PING_TIMED_OUT:
                    await WriteBubbleContent(COLOR_ORANGE, "Ping Timed Out", "");
                    break;
                case InstanceStatusResult.PING_FAILED:
                    await WriteBubbleContent(COLOR_ORANGE, "Ping Failed", "");
                    break;
                default:
                    await WriteErrorBubble("Error [2]");
                    break;
            }
        }

        private async Task WriteBubbleContent(string bubbleColor, string bubbleText, string sideText)
        {
            await WriteString("<html><head><title>Status</title></head><body style=\"font-family: Verdana, Geneva, sans-serif;color:black;background:white;display:flex;height:21px;width:150px;line-height:21px;font-size: medium;margin:0;padding:0;\">");
            await WriteString($"<div style=\"color:white;background-color:{bubbleColor};padding: 0px 7px;border-radius: 5px; flex-grow: 1; text-align: center;\">{HttpUtility.HtmlEncode(bubbleText)}</div>");
            if (sideText.Length > 0)
                await WriteString("<div style=\"margin-left:8px;\">" + sideText + "</div>");
            await WriteString("</body></html>");
        }

        private async Task WriteErrorBubble(string text)
        {
            await WriteBubbleContent(COLOR_RED, text.ToUpper(), "");
        }
    }

    public class MachinePingInstanceDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/machines/id/{MANAGER_ID}/ping_instance";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MachinePingInstanceService(conn, e);
        }
    }
}
