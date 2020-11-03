using LibDeltaSystem;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DeltaWebMap.MasterControl.WebInterface.BaseServices
{
    public abstract class BaseAuthenticatedAdminInterface : BaseAdminInterface
    {
        public BaseAuthenticatedAdminInterface(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public DateTime ConvertUtcToLocal(DateTime time)
        {
            return time.AddHours(-6);
        }

        public override async Task HandleAdminRequest()
        {
            if(!authenticated)
            {
                await WriteString("Redirecting to login...");
            } else
            {
                await HandleAuthenticatedAdminRequest();
            }
        }

        private bool authenticated;

        public override async Task WriteAdminHeaders()
        {
            //Authenticate
            authenticated = session != null && session.expiry > DateTime.UtcNow;

            //Redirect if needed
            if (!authenticated)
            {
                Redirect("/login?return=" + HttpUtility.UrlEncode(e.Request.Path), false);
            }
        }

        public abstract Task HandleAuthenticatedAdminRequest();
    }
}
