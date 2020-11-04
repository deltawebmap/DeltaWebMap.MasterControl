using DeltaWebMap.MasterControl.WebInterface.Extras;
using LibDeltaSystem;
using LibDeltaSystem.CoreNet.NetMessages.Master;
using LibDeltaSystem.CoreNet.NetMessages.Master.Entities;
using LibDeltaSystem.WebFramework;
using LibDeltaSystem.WebFramework.Extras.HTMLForm;
using LibDeltaSystem.WebFramework.Extras.HTMLTable;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DeltaWebMap.MasterControl.WebInterface.Services.Machines.Manage
{
    public class MachineStatusService : BaseMachineManageService
    {
        public MachineStatusService(DeltaConnection conn, HttpContext e) : base(conn, e)
        {
            packagesTable = new HTMLTableGenerator<NetManagerPackage>(
                new List<string>() { "Name", "Latest Version", "Actions" },
                (NetManagerPackage s) =>
                {
                    string name = s.name;
                    string latestVersion = s.latest_version;
                    string actions = GenerateFormBtnHtml("add_version", "Update", new KeyValuePair<string, string>("package_name", s.name)) + " " + GenerateFormBtnHtml("add_instance", "New Instance", new KeyValuePair<string, string>("package_name", s.name), new KeyValuePair<string, string>("count", "1"));
                    return new List<string>() { name, latestVersion, actions };
                });
            versionsTable = new HTMLTableGenerator<NetManagerVersion>(
                new List<string>() { "ID", "Package Name", "Time", "Uses", "Actions" },
                (NetManagerVersion s) =>
                {
                    string id = s.id;
                    string packageName = s.package_name;
                    DateTime timeLocal = ConvertUtcToLocal(s.time);
                    string time = timeLocal.ToShortDateString() + " " + timeLocal.ToLongTimeString();
                    string actions = GenerateFormBtnHtml("delete_version", "Delete", new KeyValuePair<string, string>("version_id", id));

                    //Count uses
                    int usesCount = 0;
                    foreach(var i in instanceList)
                    {
                        if (i.version_id == s.id)
                            usesCount++;
                    }
                    string uses = usesCount == 0 ? "<span style=\"color:red;\">0</span>" : usesCount.ToString();

                    return new List<string>() { id, packageName, time, uses, actions };
                });
            instancesTable = new HTMLTableGenerator<NetManagerInstance>(
                new List<string>() { "ID", "Package Name", "Version", "Site", "Status", "Actions" },
                (NetManagerInstance s) =>
                {
                    string id = s.id.ToString();
                    string packageName = s.package_name;
                    string versionId = s.version_id;
                    string statusColor = "#F54842";
                    string statusText = "Unknown";
                    string actions = GenerateFormBtnHtml("update_instance", "Update", new KeyValuePair<string, string>("instance_id", id)) + " " + GenerateFormBtnHtml("destroy_instance", "Destroy", new KeyValuePair<string, string>("instance_id", id));

                    //Search for package this belongs to so we can check status
                    foreach (var p in packageList)
                    {
                        if(p.name == packageName)
                        {
                            if(p.latest_version == s.version_id)
                            {
                                //Up to date
                                statusColor = "#43e438";
                                statusText = "Online";
                            } else
                            {
                                //Outdated
                                statusColor = "#ec9d17";
                                statusText = "Outdated Version";
                            }
                        }
                    }

                    //Generate sites list
                    string sites = $"<form method=\"post\" action=\"assign_site\"><input type=\"hidden\" value=\"{s.id}\" name=\"instance_id\"><select name=\"site_id\">";
                    foreach(var si in sitesList)
                        if(si.id == s.site_id)
                            sites += $"<option value=\"{si.id}\">{si.site_domain}</option>";
                    sites += $"<option value=\"{s.site_id}\"><i>No Assigned Site</i></option>";
                    foreach (var si in sitesList)
                        if (si.id != s.site_id)
                            sites += $"<option value=\"{si.id}\">{si.site_domain}</option>";
                    sites += "</select></form>";

                    return new List<string>() { id, packageName, versionId, sites, $"<span style=\"color:white; background-color:{statusColor}; padding: 0 5px; border-radius: 5px;\">{statusText}</span>", actions };
                });
            sitesTable = new HTMLTableGenerator<NetManagerSite>(
                new List<string>() { "Domain", "Protocol", "SSL Expires In", "Instances", "Actions" },
                (NetManagerSite s) =>
                {
                    //string name = s.name;
                    //string latestVersion = s.latest_version;
                    //string actions = GenerateFormBtnHtml("add_version", "Update", new KeyValuePair<string, string>("package_name", s.name)) + " " + GenerateFormBtnHtml("add_instance", "New Instance", new KeyValuePair<string, string>("package_name", s.name), new KeyValuePair<string, string>("count", "1"));
                    //Count uses
                    int usesCount = 0;
                    foreach (var i in instanceList)
                    {
                        if (i.site_id == s.id)
                            usesCount++;
                    }
                    return new List<string>() { s.site_domain, s.proto, Math.Floor((s.cert_expiry - DateTime.UtcNow).TotalDays) + " days", usesCount == 0 ? "<span style=\"color:red;\">0</span>" : usesCount.ToString(), "" };
                });
        }

        private HTMLTableGenerator<NetManagerPackage> packagesTable;
        private HTMLTableGenerator<NetManagerVersion> versionsTable;
        private HTMLTableGenerator<NetManagerInstance> instancesTable;
        private HTMLTableGenerator<NetManagerSite> sitesTable;

        private List<NetManagerPackage> packageList;
        private List<NetManagerVersion> versionList;
        private List<NetManagerInstance> instanceList;
        private List<NetManagerSite> sitesList;

        public override async Task HandleManagerServer()
        {
            packageList = await manager.transport.GetPackageList();
            await MakePackageList();
            versionList = await manager.transport.GetVersionList();
            instanceList = await manager.transport.GetInstanceList();
            sitesList = await manager.transport.GetSiteList();
            await MakeSiteList();
            await MakeVersionList();
            await MakeInstanceList();
        }

        private async Task MakePackageList()
        {
            //Write header
            await WriteString("<u>Packages</u> - " + CreateButtonHtml("Add Package", "add_package") + "<br><br>");

            //Write table
            await WriteString(packagesTable.GenerateTable(packageList));

            //Write footer
            await WriteString("<hr>");
        }

        private async Task MakeVersionList()
        {
            //Write header
            await WriteString("<u>Versions</u><br><br>");

            //Write table
            await WriteString(versionsTable.GenerateTable(versionList));

            //Write footer
            await WriteString("<hr>");
        }

        private async Task MakeInstanceList()
        {
            //Write header
            await WriteString("<u>Instances</u><br><br>");

            //Write table
            await WriteString(instancesTable.GenerateTable(instanceList));

            //Write footer
            await WriteString("<hr>");
        }

        private async Task MakeSiteList()
        {
            //Write header
            await WriteString("<u>Sites</u> - " + CreateButtonHtml("Add Site", "add_site") + "<br><br>");

            //Write table
            await WriteString(sitesTable.GenerateTable(sitesList));

            //Write footer
            await WriteString("<hr>");
        }

        private static string GenerateFormBtnHtml(string url, string btnText, params KeyValuePair<string, string>[] values)
        {
            string html = $"<form style=\"display:inline-block; margin:0;\" method=\"post\" action=\"{url}\">";
            foreach (var v in values)
                html += $"<input type=\"hidden\" name=\"{v.Key}\" id=\"{v.Key}\" value=\"{v.Value}\">";
            html += $"<input type=\"submit\" value=\"{btnText}\"></form>";
            return html;
        }
    }

    public class MachineStatusDefinition : DeltaWebServiceDefinition
    {
        public override string GetTemplateUrl()
        {
            return "/machines/id/{MANAGER_ID}/";
        }

        public override DeltaWebService OpenRequest(DeltaConnection conn, HttpContext e)
        {
            return new MachineStatusService(conn, e);
        }
    }
}
