using DeltaWebMap.MasterControl.Framework.Config;
using LibDeltaSystem;
using LibDeltaSystem.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeltaWebMap.MasterControl.CLI
{
    public class DeltaCLI
    {
        public DeltaConfig cfg;
        public bool unsaved;
        
        public void Run()
        {
            //Write header
            CLITools.PrintText("DeltaWebMap MasterControl CLI (C) DeltaWebMap, RomanPort 2020", ConsoleColor.White, ConsoleColor.Blue);
            CLITools.PrintText($"App Version {Program.APP_VERSION_MAJOR}.{Program.APP_VERSION_MINOR} - Lib Version {DeltaConnection.LIB_VERSION_MAJOR}.{DeltaConnection.LIB_VERSION_MINOR}", ConsoleColor.Blue);
            CLITools.PrintText("You're seeing this CLI because you ran this program without any arguments.");
            CLITools.PrintText("To run the master control server normally, pass in the path to the config file.\n");
            CLITools.PrintText("Type your command. Type \"help\" for a list of commands.");
            
            //Loop
            while(true)
            {
                //Read command
                string[] args = CLITools.PromptString("help").Split(' ');
                string cmd = args[0];

                //Respond
                switch(cmd)
                {
                    case "help": PrintHelp(); break;
                    case "quit": if (ConfirmUnsavedExit()) { return; } else { break; }

                    case "new": CmdNewConfig(); break;
                    case "open": CmdLoadConfig(); break;
                    case "save": CmdSaveConfig(); break;

                    case "edit_cfg": CfgEditConfig(); break;

                    case "superuser_add": CmdSuperuserAdd(); break;
                    case "superuser_list": CmdSuperuserList(); break;
                    case "superuser_remove": CmdSuperuserRemove(); break;
                    default: CLITools.PrintText("Unknown command. Type \"help\" for a list of commands.", ConsoleColor.Red); break;
                }
            }
        }

        private void PrintHelp()
        {
            CLITools.PrintText("DeltaWebMap CLI Help", ConsoleColor.Blue);
            CLITools.PrintText("<- CLI Tools ->", ConsoleColor.Cyan);
            CLITools.PrintText("[help] - Shows this menu.");
            CLITools.PrintText("[quit] - Quits to the command line.");
            CLITools.PrintText("<- File Tools ->", ConsoleColor.Cyan);
            CLITools.PrintText("[new] - Creates a new Delta Web Map config file.");
            CLITools.PrintText("[open] - Opens an existing Delta Web Map config file.");
            CLITools.PrintText("[save] - Saves the current config file to disk.");
            CLITools.PrintText("<- Editing Tools ->", ConsoleColor.Cyan);
            CLITools.PrintText("[edit_cfg] - Edits the general config.");
            CLITools.PrintText("<- Superuser Tools ->", ConsoleColor.Cyan);
            CLITools.PrintText("[superuser_add] - Adds a new superuser.");
            CLITools.PrintText("[superuser_list] - Lists the current superusers.");
            CLITools.PrintText("[superuser_remove] - Removes a superuser by username.");
        }

        private bool ConfirmUnsavedExit()
        {
            //Return true if we're saved
            if (cfg == null || !unsaved)
                return true;

            //Confirm
            CLITools.PrintText("You have unsaved changes. Run the \"save\" command to commit them.", ConsoleColor.Yellow);
            CLITools.PrintText("Are you sure you want to exit without saving these changes?");
            return CLITools.PromptSelect("DON'T EXIT", "EXIT") == "EXIT";
        }

        private bool EnsureSessionExists()
        {
            //Make sure we have a session
            if (cfg != null)
                return true;

            //We don't
            CLITools.PrintText("You haven't created or loaded a session. Run the \"new\" or \"open\" command first.", ConsoleColor.Red);
            return false;
        }

        private void CmdNewConfig()
        {
            //Prompt install location
            string installPath = CLITools.PromptFormTextInput("New Config - Specify Path", "Type the filename to save as. This will be a JSON file.");

            //Verify
            if (!Path.IsPathFullyQualified(installPath))
            {
                CLITools.PrintText("ERROR: The install path you specified is invalid.", ConsoleColor.Red);
                return;
            }
            if (File.Exists(installPath))
            {
                CLITools.PrintText("ERROR: The file path you specified already exists.", ConsoleColor.Red);
                return;
            }

            //Make
            cfg = DeltaConfig.NewConfig(installPath);
            unsaved = true;

            //Write ack
            CLITools.PrintText("Configuration was successfully created.", ConsoleColor.Green);
        }

        private void CmdLoadConfig()
        {
            //Prompt install location
            string installPath = CLITools.PromptFormTextInput("Load Config - Specify Path", "Type the filename to load.");

            //Verify
            if (!File.Exists(installPath))
            {
                CLITools.PrintText("ERROR: The file path you specified already exists.", ConsoleColor.Red);
                return;
            }

            //Load
            cfg = DeltaConfig.OpenConfig(installPath);
            unsaved = false;

            //Write ack
            CLITools.PrintText("Configuration was successfully loaded.", ConsoleColor.Green);
        }

        private void CmdSaveConfig()
        {
            //Ensure session
            if (!EnsureSessionExists())
                return;

            //Make a copy
            if(File.Exists(cfg._savePath))
            {
                CLITools.PrintText("Backing up...");
                if (File.Exists(cfg._savePath + ".bak"))
                    File.Delete(cfg._savePath + ".bak");
                File.Copy(cfg._savePath, cfg._savePath + ".bak");
            }

            //Save
            CLITools.PrintText("Saving...");
            cfg.Save();
            CLITools.PrintText("Saved to disk successfully.", ConsoleColor.Green);
            unsaved = false;
        }

        private void CfgEditConfig()
        {
            //Prompt various
            cfg.general.enviornment = CLITools.PromptFormTextInput("Make Config - Enviornment", "Type an enviornment label. Database collections will have this name.", "prod");
            cfg.general.mongodb_connection = CLITools.PromptFormTextInput("Make Config - MongoDB Connection", "Type the MongoDB connection string. It will begin with \"mongodb://\".");
            cfg.general.steam_api_token = CLITools.PromptFormTextInput("Make Config - Steam API Key", "Type your Steam API key. You can obtain it at https://steamcommunity.com/dev/apikey.");
            cfg.general.steam_cache_expire_minutes = CLITools.PromptFormIntInput("Make Config - Steam API Cache Time", "This is the amount of time, in minutes, to cache Steam profiles.", "512");
            cfg.general.firebase_uc_bucket = CLITools.PromptFormTextInput("Make Config - Firebase UC Bucket", "Type the name of your Firebase bucket that you will use for storing user content.");
            cfg.general.log = CLITools.PromptFormSelect("Make Config - Enable Logging", "Would you like to enable logging? This may slow down the program.", "DISABLED", "ENABLED") == "ENABLED";
            cfg.general.public_serving_port = CLITools.PromptFormIntInput("Make Config - Serving Port", "This is the port that manager servers will connect on. This should be open.", "43199");
            cfg.general.public_serving_ip = CLITools.PromptFormTextInput("Make Config - Serving IP", "This is the address that manager servers will connect on. This should be open.", "10.0.1.13");
            cfg.general.admin_session_expire_time = CLITools.PromptFormIntInput("Make Config - Admin Session Time", "This is how long, in minutes, admin sessions are valid for.", "15");
            cfg.general.admin_interface_port = CLITools.PromptFormIntInput("Make Config - Admin Web Interface Port", "Select a port to run the admin interface on. This should be public facing.", "80");
            cfg.general.sub_configs_directory = CLITools.PromptFormTextInput("Make Config - Sub Configs Directory", "Choose a directory to load sub-configs from. This could be empty. End with a slash.", "");

            //Prompt hosts
            cfg.hosts.master = CLITools.PromptFormTextInput("Make Config - Hosts - Master API", "Type the hostname of the master API.", "https://deltamap.net");
            cfg.hosts.echo = CLITools.PromptFormTextInput("Make Config - Hosts - Echo API", "Type the hostname of the Echo API.", "https://echo-content.deltamap.net");
            cfg.hosts.assets_icon = CLITools.PromptFormTextInput("Make Config - Hosts - Icon Assets API", "Type the hostname of the icon assets API.", "https://icon-assets.deltamap.net");
            cfg.hosts.packages = CLITools.PromptFormTextInput("Make Config - Hosts - Packages API", "Type the hostname of the packages API.", "https://charlie-packages.deltamap.net");

            //Update
            unsaved = true;
        }

        private void CmdSuperuserAdd()
        {
            //Ensure session
            if (!EnsureSessionExists())
                return;
            
            //Prompt username + password
            string username = CLITools.PromptFormTextInput("Add Superuser - Username", "Superusers have full access to the admin control panel. Type the username.");
            string password = CLITools.PromptFormTextInput("Add Superuser - Password", "Type the password to the account. This will be salted+hashed and stored locally.");

            //Make sure we don't already have a user with this
            foreach(var u in cfg.admin_credentials)
            {
                if(u.username == username)
                {
                    CLITools.PrintText("ERROR: A user already exists with that username.", ConsoleColor.Red);
                    return;
                }
            }

            //Generate
            CLITools.PrintText("Generating...");
            byte[] hash = PasswordTool.HashPassword(password, out byte[] salt);

            //Add
            cfg.admin_credentials.Add(new DeltaAdminAccount
            {
                username = username,
                passwordSalt = Convert.ToBase64String(salt),
                passwordHash = Convert.ToBase64String(hash),
                addedAt = DateTime.UtcNow
            });
            unsaved = true;

            //Ack
            CLITools.PrintText($"Added user \"{username}\"!", ConsoleColor.Green);
        }

        private void CmdSuperuserList()
        {
            //Ensure session
            if (!EnsureSessionExists())
                return;

            //List all
            foreach (var su in cfg.admin_credentials)
            {
                CLITools.PrintText($"* {su.username} (added {su.addedAt.ToShortDateString()} {su.addedAt.ToLongTimeString()} UTC)");
            }
            CLITools.PrintText(cfg.admin_credentials.Count + " superusers", ConsoleColor.Green);
        }

        private void CmdSuperuserRemove()
        {
            //Ensure session
            if (!EnsureSessionExists())
                return;

            //Prompt username + password
            string username = CLITools.PromptFormTextInput("Remove Superuser - Username", "Type the username of the superuser you want to remove.");

            //Find
            DeltaAdminAccount account = null;
            foreach (var u in cfg.admin_credentials)
            {
                if (u.username == username)
                {
                    account = u;
                }
            }

            //Remove
            if(account != null)
            {
                cfg.admin_credentials.Remove(account);
                unsaved = true;
                CLITools.PrintText($"Removed user \"{username}\".", ConsoleColor.Green);
            } else
            {
                CLITools.PrintText("Could not find that user. No superusers were removed.", ConsoleColor.Red);
            }
        }
    }
}
