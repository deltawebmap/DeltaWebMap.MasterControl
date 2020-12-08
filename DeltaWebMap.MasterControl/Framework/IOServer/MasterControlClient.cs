using DeltaWebMap.MasterControl.Framework.Entities;
using LibDeltaSystem.CoreNet;
using LibDeltaSystem.CoreNet.IO;
using LibDeltaSystem.CoreNet.IO.Server;
using LibDeltaSystem.CoreNet.NetMessages.Master;
using LibDeltaSystem.CoreNet.NetMessages.Master.Entities;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace DeltaWebMap.MasterControl.Framework.IOServer
{
    public delegate void MasterControlClientInstanceLogEventArgs(long instanceId, LibDeltaSystem.DeltaLogLevel logLevel, string topic, string message);
    
    public class MasterControlClient : ServerRouterSession
    {
        public MasterControlClient(IServerRouterIO server, Socket sock) : base(server, sock)
        {
        }

        public bool authenticated;
        public DeltaManagerServer server;
        public event MasterControlClientInstanceLogEventArgs OnInstanceLog;

        private const int LOG_HEADER_LEN = 8;

        public void DispatchInstanceLogEvent(byte[] payload)
        {
            //If there are no subsribers, abort
            if (OnInstanceLog == null)
                return;

            //Parse
            long instanceId = BitConverter.ToInt64(payload, 0);
            int logLevel = BitConverter.ToInt32(payload, LOG_HEADER_LEN + 0);
            int topicLen = BitConverter.ToInt32(payload, LOG_HEADER_LEN + 4);
            int messageLen = BitConverter.ToInt32(payload, LOG_HEADER_LEN + 8 + topicLen);
            string topic = Encoding.UTF8.GetString(payload, LOG_HEADER_LEN + 8, topicLen);
            string msg = Encoding.UTF8.GetString(payload, LOG_HEADER_LEN + 8 + topicLen + 4, messageLen);

            //Dispatch
            OnInstanceLog.Invoke(instanceId, (LibDeltaSystem.DeltaLogLevel)logLevel, topic, msg);
        }

        public Task<ManagerStatusMessage> RequestStatus()
        {
            return RequestGetObject<ManagerStatusMessage>(MasterConnectionOpcodes.OPCODE_MASTER_STATUS);
        }

        public async Task<InstancePingResult> PingInstance(long instanceId)
        {
            var c = SendMessageGetResponseChannel(MasterConnectionOpcodes.OPCODE_MASTER_PING_INSTANCE, BitConverter.GetBytes(instanceId));
            var m = await c.ReadAsync();
            return new InstancePingResult
            {
                status = (InstanceStatusResult)m.payload[0],
                reserved = m.payload[1],
                lib_version_major = m.payload[2],
                lib_version_minor = m.payload[3],
                app_version_major = m.payload[4],
                app_version_minor = m.payload[5],
                initial_instance_ping_ms = BitConverter.ToUInt16(m.payload, 6)
            };
        }

        public ChannelReader<RouterMessage> AddPackage(ManagerAddPackage cmd)
        {
            return SendMessageGetResponseChannelSerialized(MasterConnectionOpcodes.OPCODE_MASTER_M_ADDPACKAGE, cmd);
        }

        public ChannelReader<RouterMessage> AddVersion(ManagerAddVersion cmd)
        {
            return SendMessageGetResponseChannelSerialized(MasterConnectionOpcodes.OPCODE_MASTER_M_ADDVERSION, cmd);
        }

        public ChannelReader<RouterMessage> AddInstance(ManagerAddInstance cmd)
        {
            return SendMessageGetResponseChannelSerialized(MasterConnectionOpcodes.OPCODE_MASTER_M_ADDINSTANCE, cmd);
        }

        public Task<List<NetManagerPackage>> GetPackageList()
        {
            return RequestGetObject<List<NetManagerPackage>>(MasterConnectionOpcodes.OPCODE_MASTER_M_LISTPACKAGES);
        }

        public Task<List<NetManagerVersion>> GetVersionList()
        {
            return RequestGetObject<List<NetManagerVersion>>(MasterConnectionOpcodes.OPCODE_MASTER_M_LISTVERSIONS);
        }

        public Task<List<NetManagerInstance>> GetInstanceList()
        {
            return RequestGetObject<List<NetManagerInstance>>(MasterConnectionOpcodes.OPCODE_MASTER_M_LISTINSTANCES);
        }

        public ChannelReader<RouterMessage> UpdateInstanceVersion(ManagerUpdateInstance cmd)
        {
            return SendMessageGetResponseChannelSerialized(MasterConnectionOpcodes.OPCODE_MASTER_M_UPDATEINSTANCE, cmd);
        }

        public ChannelReader<RouterMessage> DestroyInstance(ManagerUpdateInstance cmd)
        {
            return SendMessageGetResponseChannelSerialized(MasterConnectionOpcodes.OPCODE_MASTER_M_DESTROYINSTANCE, cmd);
        }
        
        public ChannelReader<RouterMessage> DeleteVersion(ManagerDeleteVersion cmd)
        {
            return SendMessageGetResponseChannelSerialized(MasterConnectionOpcodes.OPCODE_MASTER_M_DELETEVERSION, cmd);
        }

        public ChannelReader<RouterMessage> AddSite(ManagerAddSite cmd)
        {
            return SendMessageGetResponseChannelSerialized(MasterConnectionOpcodes.OPCODE_MASTER_M_ADDSITE, cmd);
        }

        public Task<List<NetManagerSite>> GetSiteList()
        {
            return RequestGetObject<List<NetManagerSite>>(MasterConnectionOpcodes.OPCODE_MASTER_M_LISTSITES);
        }

        public ChannelReader<RouterMessage> AssignSite(ManagerAssignSite cmd)
        {
            return SendMessageGetResponseChannelSerialized(MasterConnectionOpcodes.OPCODE_MASTER_M_ASSIGNSITE, cmd);
        }

        public ChannelReader<RouterMessage> RebootInstance(ManagerRebootInstance cmd)
        {
            return SendMessageGetResponseChannelSerialized(MasterConnectionOpcodes.OPCODE_MASTER_REBOOT_INSTANCE, cmd);
        }
    }
}