using DeltaWebMap.MasterControl.WebInterface.BaseServices;
using LibDeltaSystem;
using LibDeltaSystem.Tools;
using LibDeltaSystem.WebFramework;
using LibDeltaSystem.WebFramework.Extras.HTMLForm;
using LibDeltaSystem.WebFramework.Extras.HTMLForm.Attributes.Types;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.MasterControl.WebInterface.Services
{
    public class LoginService : BaseAdminInterface
    {
        public LoginService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
        }

        private static HTMLFormProcessor<LoginForm> form = new HTMLFormProcessor<LoginForm>("Login");

        public override async Task HandleAdminRequest()
        {
            //If we successfully authenticated, drop out here. We're redirecting anyways
            if (authenticationSuccess)
                return;

            //Build base message
            string message = null;
            if (e.Request.Query.ContainsKey("return"))
                message = "Sign in to access that page.";
            if (session != null)
                message = "Your session has expired. Please sign in again.";
            if (!authenticationSuccess && authenticationAttempted)
                message = "The username and password you entered were incorrect.";

            //Render form
            await WriteString((message == null ? "" : $"<p style=\"color:red;\">{message}</p>") + form.BuildHTML());
        }

        private bool authenticationSuccess;
        private bool authenticationAttempted;

        public override async Task WriteAdminHeaders()
        {
            //Since we need to set cookies, we'll do the log in process here before any data is written.
            authenticationAttempted = GetMethod() == LibDeltaSystem.WebFramework.Entities.DeltaCommonHTTPMethod.POST;
            if (authenticationAttempted)
                authenticationSuccess = await TryAuthenticate();
        }

        private async Task<bool> TryAuthenticate()
        {
            //Read form
            LoginForm data = new LoginForm();
            await form.ProcessResponse(data, e);

            //Authenticate
            Framework.Config.DeltaAdminAccount authenticatedUser = null;
            foreach (var u in Program.cfg.admin_credentials)
            {
                if (u.username == data.username)
                {
                    //Check password
                    bool ok = PasswordTool.AuthenticateHashedPassword(data.password, Convert.FromBase64String(u.passwordHash), Convert.FromBase64String(u.passwordSalt));
                    if (ok)
                        authenticatedUser = u;
                }
            }

            //Check if passed
            if (authenticatedUser != null)
            {
                //Correct creds! Create a session
                AdminSession session = new AdminSession
                {
                    expiry = DateTime.UtcNow.AddMinutes(Program.cfg.general.admin_session_expire_time),
                    token = SecureStringTool.GenerateSecureString(24),
                    username = authenticatedUser.username
                };

                //Set session cookie
                e.Response.Cookies.Append(ACCESS_TOKEN_COOKIE, session.token);

                //Add to sessions
                Program.admin_sessions.Add(session);

                //Redirect back
                string returnTo = "/";
                if (e.Request.Query.ContainsKey("return"))
                    returnTo = e.Request.Query["return"];
                Redirect(returnTo, false);

                return true;
            }
            else
            {
                //Failed
                return false;
            }
        }

        class LoginForm
        {
            [FormElementTypeText("Username", "Username", "user")]
            public string username { get; set; }
            [FormElementTypePassword("Password", "pass")]
            public string password { get; set; }
        }
    }

    public class LoginDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/login";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new LoginService(conn, e);
        }
    }
}
