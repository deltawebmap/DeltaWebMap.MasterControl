using LibDeltaSystem.CoreNet.IO;
using LibDeltaSystem.CoreNet.NetMessages.Master;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Web;

namespace DeltaWebMap.MasterControl.WebInterface.Extras
{
    /// <summary>
    /// This endpoint shows a log output from an external server. Waits until it completes to finish
    /// </summary>
    public static class LogRenderer
    {
        public static async Task<bool> RenderLogs(ChannelReader<RouterMessage> channel, DeltaWebService service)
        {
            //Send CSS
            await service.WriteString("<style>.logMore {display: none;} .logBtn:checked + div + .logMore { display: block; }</style>", "text/html", 200);

            //Loop
            bool exitOk = false;
            while (!service.e.RequestAborted.IsCancellationRequested)
            {
                //Read next
                var msg = await channel.ReadAsync(service.e.RequestAborted);

                //Check if ended
                if (service.e.RequestAborted.IsCancellationRequested)
                    break;

                //Deserialize message
                MasterCommandLogOpcode op = DeserializeMessage(msg.payload, out int code, out string[] strings);

                //Get HTML
                string html;
                if (op == MasterCommandLogOpcode.LOG)
                {
                    html = $"<div><div style=\"float:left; clear:both; margin-right:5px;\"><b>[{strings[0]}]</b></div>{strings[1]}</div>";
                }
                else if (op == MasterCommandLogOpcode.LOG_CLI_BEGIN)
                {
                    html = $"<div style=\"background-color: #e6e6e6;\"><input style=\"margin: 3px 5px; vertical-align: top;\" class=\"logBtn\" type=\"checkbox\"><div style=\"display: inline-block;\"><b>(Log)</b> {strings[1]}</div><div style=\"margin-left:25px;\" class=\"logMore\">";
                }
                else if (op == MasterCommandLogOpcode.LOG_CLI_MESSAGE)
                {
                    html = $"<div>{strings[0]}</div>";
                }
                else if (op == MasterCommandLogOpcode.LOG_CLI_END)
                {
                    html = "</div></div>";
                } else if (op == MasterCommandLogOpcode.FINISHED_SUCCESS || op == MasterCommandLogOpcode.FINISHED_FAIL)
                {
                    string color = op == MasterCommandLogOpcode.FINISHED_SUCCESS ? "#31c301" : "#ff4336";
                    html = $"<div style=\"color:{color}; font-weight:800; margin: 10px 0;\">{strings[0]}</div>";
                } else
                {
                    html = "INVALID_LOG";
                }

                //Write
                await service.WriteString(html, "text/html", 200);

                //Check if we're done
                exitOk = op == MasterCommandLogOpcode.FINISHED_SUCCESS;
                if (op == MasterCommandLogOpcode.FINISHED_FAIL || op == MasterCommandLogOpcode.FINISHED_SUCCESS)
                    break;
            }

            return exitOk;
        }

        private static MasterCommandLogOpcode DeserializeMessage(byte[] data, out int code, out string[] strings)
        {
            MasterCommandLogOpcode op = (MasterCommandLogOpcode)BitConverter.ToInt16(data, 0);
            short stringCount = BitConverter.ToInt16(data, 2);
            code = BitConverter.ToInt32(data, 4);
            strings = new string[stringCount];
            int pos = 8;
            for(int i = 0; i<stringCount; i++)
            {
                int len = BitConverter.ToInt32(data, pos);
                pos += 4;
                strings[i] = HttpUtility.HtmlEncode(Encoding.UTF8.GetString(data, pos, len));
                pos += len;
            }
            return op;
        }
    }
}
