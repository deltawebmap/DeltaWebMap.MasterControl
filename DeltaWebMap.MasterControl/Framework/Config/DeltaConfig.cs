using LibDeltaSystem.CoreNet.NetMessages;
using LibDeltaSystem.Tools;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DeltaWebMap.MasterControl.Framework.Config
{
    public class DeltaConfig
    {
        [JsonIgnore]
        public string _savePath;

        private DeltaConfig(string savePath)
        {
            _savePath = savePath;
        }

        public DeltaConfig()
        {

        }

        public DeltaConfigGeneral general = new DeltaConfigGeneral();
        public DeltaConfigSecrets secrets = new DeltaConfigSecrets();
        public LoginServerConfigHosts hosts = new LoginServerConfigHosts();
        public List<DeltaAdminAccount> admin_credentials = new List<DeltaAdminAccount>();
        public List<DeltaConfigManagerServer> registered_servers = new List<DeltaConfigManagerServer>();

        public void Save()
        {
            File.WriteAllText(_savePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public LoginServerConfig GetRemoteConfig()
        {
            return new LoginServerConfig
            {
                enviornment = general.enviornment,
                steam_cache_expire_minutes = general.steam_cache_expire_minutes,
                firebase_uc_bucket = general.firebase_uc_bucket,
                log = general.log,
                mongodb_connection = general.mongodb_connection,
                steam_api_key = secrets.steam_token_key,
                steam_token_key = general.steam_api_token,
                hosts = hosts
            };
        }

        public static DeltaConfig OpenConfig(string path)
        {
            var cfg = JsonConvert.DeserializeObject<DeltaConfig>(File.ReadAllText(path));
            cfg._savePath = path;
            return cfg;
        }

        public static DeltaConfig NewConfig(string path)
        {
            var cfg = new DeltaConfig(path);

            //Generate secure tokens
            cfg.secrets.steam_token_key = Convert.ToBase64String(SecureStringTool.GenerateSecureRandomBytes(16));
            cfg.secrets.corenet_encryption_key = Convert.ToBase64String(SecureStringTool.GenerateSecureRandomBytes(256));

            return cfg;
        }
    }

    public class DeltaConfigManagerServer
    {
        public string label;
        public string ip_addr;
        public short id;
        public string key; //16 bytes, base64 encoded
    }

    public class DeltaConfigGeneral
    {
        public string enviornment;
        public string mongodb_connection;
        public string steam_api_token;
        public int steam_cache_expire_minutes;
        public string firebase_uc_bucket;
        public bool log;
        public int public_serving_port; //The port the master uses. Public-facing
        public string public_serving_ip; //The serving IP address
        public int admin_session_expire_time;
    }

    public class DeltaConfigSecrets
    {
        public string steam_token_key; //16 bytes, base64 encoded
        public string corenet_encryption_key; //256 bytes, base64 encoded
    }

    public class DeltaAdminAccount
    {
        public string username;
        public string passwordSalt;
        public string passwordHash;
        public DateTime addedAt;
    }
}
