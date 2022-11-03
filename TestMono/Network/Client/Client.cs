using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestMono.Helpers;
using TestMono.Network.Packets;
using TestMono.Network.Packets.ClientToServer;
using TestMono.Network.Packets.ServerToClient;

namespace TestMono.Network.Client;

public class Client
{
    public string PlayerId { get; init; }

    private EventBasedNetListener _netListener;
    private NetManager _netManager;    
    private NetSerializer _netSerializer;

    public bool IsRunning => _netManager is not null && _netManager.IsRunning;

    public List<string> MessagesReceived;

    public ClientGameInstance GameInstance { get; set; }

    public Client()
    {
        PlayerId = Guid.NewGuid().ToString().Substring(0, 4);
        MessagesReceived = new List<string>();
    }

    public void Connect()
    {
        _netListener = new EventBasedNetListener();
        _netManager = new NetManager(_netListener);        
        _netSerializer = new NetSerializer();
        _netSerializer.RegisterNestedType<InitGamePlayerInfo>(() => new InitGamePlayerInfo());

#if DEBUG
        //_netManager.SimulateLatency = true;
        //_netManager.SimulationMinLatency = 1000;
        //_netManager.SimulationMaxLatency = 3000;        
        //_netManager.SimulatePacketLoss = true;
        //_netManager.SimulationPacketLossChance = 50;        

        _netManager.DisconnectTimeout = 60000;
#endif

        _netManager.Start();
        _netManager.Connect("localhost", 9050, "TestMono");

        _netListener.PeerConnectedEvent += (server) =>
        {
            SendClientInfoPacket();
        };

        _netListener.NetworkReceiveEvent += (server, reader, channel, deliveryMethod) =>
        {
            var packetType = (PacketType)reader.PeekInt();

            switch (packetType)
            {
                case PacketType.InitGamePacket:
                    {
                        var packet = _netSerializer.Deserialize<InitGamePacket>(reader);
                        MessagesReceived.Add($"Server sent InitGamePacket: {JsonConvert.SerializeObject(packet)}");
                        var sceneInfo = ClientGameInstance.CreateGameSceneInfo(packet, ApplicationType.Client);
                        Game1.CurrentGame.ScenesManager.CreateGameScene(sceneInfo, GameInstance, null);
                        break;
                    }
                case PacketType.MoveUnitOrderPacket:
                    {
                        var packet = _netSerializer.Deserialize<MoveUnitOrderPacket>(reader);
                        MessagesReceived.Add($"Server sent MoveUnitOrderPacket: {JsonConvert.SerializeObject(packet)}");
                        GameInstance.MoveUnitOrder(packet);
                        break;
                    }
                case PacketType.MoveUnitStopOrderPacket:
                    {
                        var packet = _netSerializer.Deserialize<MoveUnitStopOrderPacket>(reader);
                        MessagesReceived.Add($"Server sent MoveUnitStopOrderPacket: {JsonConvert.SerializeObject(packet)}");
                        GameInstance.MoveUnitStopOrder(packet);
                        break;
                    }
                case PacketType.BuildingSetFoundationOrderPacket:
                    {
                        var packet = _netSerializer.Deserialize<BuildingSetFoundationOrderPacket>(reader);
                        MessagesReceived.Add($"Server sent BuildingSetFoundationOrderPacket: {JsonConvert.SerializeObject(packet)}");
                        GameInstance.BuildingSetFoundationOrder(packet);
                        break;
                    }
                case PacketType.BuildingSetBuiltOrderPacket:
                    {
                        var packet = _netSerializer.Deserialize<BuildingSetBuiltOrderPacket>(reader);
                        MessagesReceived.Add($"Server sent BuildingSetBuiltOrderPacket: {JsonConvert.SerializeObject(packet)}");
                        GameInstance.BuildingSetBuiltOrder(packet);
                        break;
                    }
                case PacketType.ServerSyncTimestampResponsePacket:
                    {
                        var packet = _netSerializer.Deserialize<ServerSyncTimestampResponsePacket>(reader);
                        MessagesReceived.Add($"Server sent ServerSyncTimestampResponsePacket: {JsonConvert.SerializeObject(packet)}");
                        GameInstance.SyncServerTimestamp(packet);
                        break;
                    }
                default:
                    throw new NotImplementedException($"Unknown PacketType {reader.PeekInt()}");
            }            
        };                

        GameInstance = new ClientGameInstance(this);
    }

    public void PollEvents()
    {
        if (!IsRunning)
            return;

        _netManager.PollEvents();
    }

    public string GetLatestMessagesReceived()
    {
        var sb = new StringBuilder();

        var latestMessages = MessagesReceived.TakeLast(5);
        foreach (var message in latestMessages)
        {
            sb.AppendLine(message);
        }

        return sb.ToString();
    }

    public void SendClientInfoPacket()
    {
        var packet = new ClientInfoPacket()
        {
            PacketType = (int)PacketType.ClientInfoPacket,
            Timestamp = TimeUtils.GetCurrentTimestamp(),
            PlayerId = PlayerId
        };
        
        _netManager.SendToAll(_netSerializer.Serialize(packet), DeliveryMethod.ReliableOrdered);
    }

    public void SendMoveUnitRequestPacket(MoveUnitRequestPacket packet)
    {
        _netManager.SendToAll(_netSerializer.Serialize(packet), DeliveryMethod.ReliableOrdered);
    }

    public void SendBuildingSetFoundationRequestPacket(BuildingSetFoundationRequestPacket packet)
    {
        _netManager.SendToAll(_netSerializer.Serialize(packet), DeliveryMethod.ReliableOrdered);
    }

    public void SendSyncServerTimestampRequestPacket(ClientStartSyncServerTimestampPacket packet)
    {
        _netManager.SendToAll(_netSerializer.Serialize(packet), DeliveryMethod.ReliableOrdered);
    }


}
