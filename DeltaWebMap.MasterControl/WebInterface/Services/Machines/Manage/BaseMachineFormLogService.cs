using DeltaWebMap.MasterControl.WebInterface.Extras;
using LibDeltaSystem;
using LibDeltaSystem.CoreNet.IO;
using LibDeltaSystem.WebFramework.Extras.HTMLForm;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DeltaWebMap.MasterControl.WebInterface.Services.Machines.Manage
{
    public abstract class BaseMachineFormLogService<T> : BaseMachineManageService
    {
        public BaseMachineFormLogService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public abstract HTMLFormProcessor<T> GetForm();

        public abstract ChannelReader<RouterMessage> SendCommand(T cmd);

        public override async Task HandleManagerServer()
        {
            //Get the form
            var form = GetForm();

            //Handle
            if (GetMethod() != LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.POST)
            {
                await WriteString(form.BuildHTML());
            }
            else
            {
                //Get form
                T cmd = (T)Activator.CreateInstance(typeof(T));
                await form.ProcessResponse(cmd, e);

                //Run
                bool ok = await LogRenderer.RenderLogs(SendCommand(cmd), this);

                //If successful, write JS to redirect us back
                if (ok)
                    await WriteString($"<script>window.location = '/machines/id/{manager.id}/';</script>");
            }
        }
    }
}
