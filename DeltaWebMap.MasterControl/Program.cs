using DeltaWebMap.MasterControl.CLI;
using DeltaWebMap.MasterControl.Framework;
using DeltaWebMap.MasterControl.Framework.Config;
using DeltaWebMap.MasterControl.Framework.IOServer;
using DeltaWebMap.MasterControl.WebInterface;
using DeltaWebMap.MasterControl.WebInterface.Services;
using DeltaWebMap.MasterControl.WebInterface.Services.Machines;
using DeltaWebMap.MasterControl.WebInterface.Services.Machines.Manage;
using DeltaWebMap.MasterControl.WebInterface.Services.Machines.Manage.Instances;
using LibDeltaSystem;
using LibDeltaSystem.Tools;
using LibDeltaSystem.WebFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DeltaWebMap.MasterControl
{
    class Program
    {
        public const byte APP_VERSION_MAJOR = 0;
        public const byte APP_VERSION_MINOR = 6;

        public static DeltaConfig cfg;
        public static DeltaConnection delta;
        public static MasterControlServer server;
        public static DeltaWebServer web_interface;
        public static List<AdminSession> admin_sessions;
        public static List<DeltaManagerServer> servers;
        
        static void Main(string[] args)
        {
            //Get the config path, otherwise launch in CLI mode
            if(args.Length != 1)
            {
                new DeltaCLI().Run();
                return;
            }

            //Open config file
            cfg = DeltaConfig.OpenConfig(args[0]);

            //Init delta
            delta = new DeltaConnection(-1, -1, APP_VERSION_MAJOR, APP_VERSION_MINOR, DeltaCoreNetServerType.MASTER_CONTROL);
            delta.InitOffline(cfg.GetRemoteConfig(), new int[0]);

            //Init others
            admin_sessions = new List<AdminSession>();
            servers = new List<DeltaManagerServer>();
            foreach (var s in cfg.registered_servers)
                servers.Add(new DeltaManagerServer(s));

            //Init underlying server
            server = new MasterControlServer(delta, new IPEndPoint(IPAddress.Any, cfg.general.public_serving_port));

            //Init web interface server
            web_interface = new DeltaWebServer(delta, cfg.general.admin_interface_port);
            web_interface.AddService(new LoginDefinition());
            web_interface.AddService(new MachineListDefinition());
            web_interface.AddService(new MachineEnrollDefinition());
            web_interface.AddService(new MachineStatusDefinition());
            web_interface.AddService(new MachineAddPackageDefinition());
            web_interface.AddService(new MachineAddVersionDefinition());
            web_interface.AddService(new MachineAddInstanceDefinition());
            web_interface.AddService(new MachineUpdateInstanceDefinition());
            web_interface.AddService(new MachineDestroyInstanceDefinition());
            web_interface.AddService(new MachineDeleteVersionDefinition());
            web_interface.AddService(new MachineAddSiteDefinition());
            web_interface.AddService(new MachineAssignSiteDefinition());
            web_interface.AddService(new MachineRebootInstanceDefinition());
            web_interface.AddService(new MachinePingInstanceDefinition());
            web_interface.AddService(new MachineInstanceManageDefinition());

            //Run
            web_interface.RunAsync().GetAwaiter().GetResult();
        }

        public static DeltaManagerServer CreateManagerServer(string label, IPAddress address)
        {
            //Prepare
            DeltaConfigManagerServer configEntry;
            lock (cfg.registered_servers)
            {
                //Create key
                byte[] key = SecureStringTool.GenerateSecureRandomBytes(16);

                //Create ID
                short id;
                Random rand = new Random();
                do
                {
                    id = (short)rand.Next(257, short.MaxValue - 1);
                } while (cfg.registered_servers.Where(x => x.id == id).Count() != 0);

                //Create config entry
                configEntry = new DeltaConfigManagerServer
                {
                    label = label,
                    ip_addr = address.ToString(),
                    id = id,
                    key = Convert.ToBase64String(key)
                };

                //Add
                cfg.registered_servers.Add(configEntry);
                cfg.Save();
            }

            //Create instance and add it
            DeltaManagerServer server = new DeltaManagerServer(configEntry);
            lock (servers)
                servers.Add(server);

            return server;
        }
    }
}
