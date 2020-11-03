using DeltaWebMap.MasterControl.Framework;
using DeltaWebMap.MasterControl.WebInterface.BaseServices;
using LibDeltaSystem;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.MasterControl.WebInterface.Services.Machines.Manage
{
    public abstract class BaseMachineManageService : BaseAuthenticatedAdminInterface
    {
        public BaseMachineManageService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        private short managerId;
        public DeltaManagerServer manager;

        public override async Task HandleAuthenticatedAdminRequest()
        {
            //Get manager
            manager = null;
            lock(Program.servers)
            {
                foreach (var s in Program.servers)
                {
                    if (s.id == managerId)
                        manager = s;
                }
            }

            //Check if we found it
            if(manager == null)
            {
                await WriteString("<p style=\"color:red\">Could not find that manager server.</p>");
            } else
            {
                await HandleManagerServer();
            }
        }

        public abstract Task HandleManagerServer();

        public override Task<bool> SetArgs(Dictionary<string, string> args)
        {
            managerId = short.Parse(args["MANAGER_ID"]);
            return base.SetArgs(args);
        }
    }
}
