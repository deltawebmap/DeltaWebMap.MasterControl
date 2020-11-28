using LibDeltaSystem;
using LibDeltaSystem.WebFramework;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DeltaWebMap.MasterControl.WebInterface.BaseServices
{
    public abstract class BaseAdminInterface : DeltaWebService
    {
        public BaseAdminInterface(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        public const string ACCESS_TOKEN_COOKIE = "delta-admin-access-token";

        public bool renderHeaderBar = true;
        public bool renderContainerHtml = true;

        public AdminSession session;

        public override async Task OnRequest()
        {
            //Set no cache headers
            e.Response.Headers.Add("Cache-Control", "no-store");

            //Get the access token
            if(e.Request.Cookies.ContainsKey(ACCESS_TOKEN_COOKIE))
            {
                //Has cookie
                string token = e.Request.Cookies[ACCESS_TOKEN_COOKIE];
                
                //Authenticate
                lock (Program.admin_sessions)
                {
                    foreach (var s in Program.admin_sessions)
                    {
                        if (s.token == token)
                            session = s;
                    }
                }
            }

            //Allow subclasses to add headers
            await WriteAdminHeaders();

            //Write page header
            if(renderContainerHtml)
                await WriteString("<html translate=\"no\"><head><title>Delta Web Map Admin</title><meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0, minimum-scale=1.0\"><meta name=\"google\" content=\"notranslate\"><style>td {padding-right: 15px; text-align:left;}</style></head><body style=\"font-family: Verdana, Geneva, sans-serif; font-size:15px; color:black;\"><div>");

            //Write top bar
            if (renderHeaderBar)
            {
                await WriteString("<b>DeltaWebMap Admin</b>");
                foreach (var s in Program.web_interface.services)
                {
                    if (s.GetType().IsSubclassOf(typeof(AdminInterfaceHeaderDefinition)))
                    {
                        var ws = (AdminInterfaceHeaderDefinition)s;
                        await WriteString(" - ");
                        await WriteButton(ws.GetTitle(), ws.GetTemplateUrl());
                    }
                }
                if (session != null)
                {
                    await WriteString($"<div style=\"float:right;\">Session Expires in {Math.Round(Math.Max(0, (session.expiry - DateTime.UtcNow).TotalMinutes), 1)} Minutes {CreateButtonHtml("End Now", "/logout")}</div>");
                }
                await WriteString("</div><hr><div>");
            }

            //Run
            try
            {
                await HandleAdminRequest();
            } catch (Exception ex)
            {
                Log("WEB", $"Error while handling web interface request: {ex.Message}{ex.StackTrace}");
                await WriteString($"<div style=\"background-color:#e6e6e6; padding:15px;\"><b style=\"color:red\">There was an error rendering this page.</b><br><br>{HttpUtility.HtmlEncode(ex.Message + ex.StackTrace)}</div>");
            }

            //Write footer
            if(renderContainerHtml)
                await WriteString("</div></body></html>");
        }

        public abstract Task WriteAdminHeaders();

        public abstract Task HandleAdminRequest();

        public Task WriteString(string text)
        {
            return WriteString(text, "text/html", 200);
        }

        public Task WriteButton(string text, string url)
        {
            return WriteString(CreateButtonHtml(text, url));
        }

        public static string CreateButtonHtml(string text, string url)
        {
            return $"<a href=\"{url}\">[{EscapeString(text)}]</a>";
        }

        public static string EscapeString(string s)
        {
            return HttpUtility.HtmlEncode(s);
        }
    }
}
