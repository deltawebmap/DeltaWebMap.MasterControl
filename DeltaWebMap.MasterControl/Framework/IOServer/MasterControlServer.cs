using LibDeltaSystem;
using LibDeltaSystem.CoreNet;
using LibDeltaSystem.CoreNet.IO;
using LibDeltaSystem.CoreNet.IO.Server;
using LibDeltaSystem.CoreNet.IO.Transports;
using LibDeltaSystem.CoreNet.NetMessages;
using LibDeltaSystem.Entities;
using LibDeltaSystem.Tools;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace DeltaWebMap.MasterControl.Framework.IOServer
{
    public class MasterControlServer
    {
        private ServerRouterIO<MasterControlClient> io;
        private IDeltaLogger logger;

        public MasterControlServer(IDeltaLogger logger, IPEndPoint listenEndpoint)
        {
            this.logger = logger;
            io = new ServerRouterIO<MasterControlClient>(logger, new UnencryptedTransport(), new MinorMajorVersionPair(Program.APP_VERSION_MAJOR, Program.APP_VERSION_MINOR), listenEndpoint, (IServerRouterIO server, Socket sock) =>
            {
                return new MasterControlClient(server, sock);
            });
            io.OnClientConnected += Io_OnClientConnected;
            io.OnClientDropped += Io_OnClientDropped;
            io.OnClientMessage += Io_OnClientMessage;
        }

        private void Io_OnClientMessage(MasterControlClient session, RouterMessage msg)
        {
            if (session.authenticated)
            {
                //Authenticated commands
                if (msg.opcode == MasterConnectionOpcodes.OPCODE_MASTER_CONFIG)
                    HandleRequestConfigCommand(session, msg);
                else
                    logger.Log("Io_OnClientMessage", $"Client {session.GetDebugName()} sent an unknown command.", DeltaLogLevel.Debug);
            }
            else
            {
                //Unauthenticated commands
                if (msg.opcode == MasterConnectionOpcodes.OPCODE_MASTER_LOGIN)
                    HandleLoginCommand(session, msg);
                else
                    logger.Log("Io_OnClientMessage", $"Client {session.GetDebugName()} sent unauthenticated command that was not LOGIN.", DeltaLogLevel.Debug);
            }
        }

        private void Io_OnClientDropped(MasterControlClient session)
        {
            logger.Log("Io_OnClientConnected", $"Dropped client {session.GetDebugName()}", DeltaLogLevel.Low);
            if (session.server != null)
                session.server.DisconnectTransport();
        }

        private void Io_OnClientConnected(MasterControlClient session)
        {
            logger.Log("Io_OnClientConnected", $"Connected client {session.GetDebugName()}", DeltaLogLevel.Low);
        }

        private void HandleLoginCommand(MasterControlClient session, RouterMessage msg)
        {
            //Get details
            int loginId = BitConverter.ToInt16(msg.payload, 0);
            byte[] loginKey = new byte[16];
            Array.Copy(msg.payload, 2, loginKey, 0, 16);

            //Search for a server
            DeltaManagerServer server = null;
            lock(Program.servers)
            {
                foreach(var s in Program.servers)
                {
                    if(s.id == loginId)
                    {
                        //Authenticate key
                        if (BinaryTool.CompareBytes(s.key, loginKey))
                            server = s;
                    }
                }
            }

            //Check if it was successful
            if(server != null)
            {
                //Set
                server.SetTransport(session);
                session.authenticated = true;

                //Log
                logger.Log("HandleLoginCommand", $"Client {session.GetDebugName()} successfully logged in as server ID {server.id}.", DeltaLogLevel.Low);
            } else
            {
                //Failed
                logger.Log("HandleLoginCommand", $"Client {session.GetDebugName()} attempted login, but failed.", DeltaLogLevel.Low);
            }
        }

        private void HandleRequestConfigCommand(MasterControlClient session, RouterMessage msg)
        {
            msg.RespondJson(new LoginServerConfig
            {
                enviornment = Program.cfg.general.enviornment,
                mongodb_connection = Program.cfg.general.mongodb_connection,
                steam_api_key = Program.cfg.general.steam_api_token,
                steam_cache_expire_minutes = Program.cfg.general.steam_cache_expire_minutes,
                firebase_uc_bucket = Program.cfg.general.firebase_uc_bucket,
                log = Program.cfg.general.log,
                steam_token_key = Program.cfg.secrets.steam_token_key,
                hosts = Program.cfg.hosts
            }, true);
        }
    }
}
