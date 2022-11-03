using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TestMono.Helpers;
using TestMono.Network.Packets;
using TestMono.Network.Packets.ClientToServer;
using TestMono.Network.Packets.ServerToClient;

namespace TestMono.Network.Server;

public class Server
{
    private EventBasedNetListener _netListener;
    private NetManager _netManager;    
    private NetSerializer _netSerializer;

    public bool IsRunning => _netManager is not null && _netManager.IsRunning;

    public List<string> MessagesReceived;

    public ServerGameInstance GameInstance { get; set; }
    public int ConnectedPlayersCount { get => _netManager.ConnectedPeersCount; }    

    public Server()
    {
        MessagesReceived = new List<string>();        
    }

    public void Start()
    {
        _netListener = new EventBasedNetListener();
        _netListener.PeerDisconnectedEvent += _netListener_PeerDisconnectedEvent;

        _netManager = new NetManager(_netListener);        

        _netSerializer = new NetSerializer();
        _netSerializer.RegisterNestedType<InitGamePlayerInfo>(() => new InitGamePlayerInfo());

#if DEBUG
        _netManager.DisconnectTimeout = 60000;
#endif

        _netManager.Start(9050);

        _netListener.ConnectionRequestEvent += request =>
        {            
            if (_netManager.ConnectedPeersCount < 10)
                request.Accept();
            else
                request.Reject();
        };

        _netListener.PeerConnectedEvent += peer =>
        {
            Debug.WriteLine("We got connection: {0}", peer.EndPoint);            
            //peer.Send(writer, DeliveryMethod.ReliableOrdered);
        };

        _netListener.NetworkReceiveEvent += (peerFrom, reader, deliveryMethod) =>
        {
            var packetType = (PacketType)reader.PeekInt();

            switch (packetType)
            {
                case PacketType.ClientInfoPacket:
                    {
                        var packet = _netSerializer.Deserialize<ClientInfoPacket>(reader);
                        MessagesReceived.Add($"Client sent InfoPacket: {JsonConvert.SerializeObject(packet)}");
                        GameInstance.AddPlayer(packet.PlayerId, peerFrom);
                        break;
                    }                                    
                case PacketType.MoveUnitRequestPacket:
                    {
                        var packet = _netSerializer.Deserialize<MoveUnitRequestPacket>(reader);
                        MessagesReceived.Add($"Client sent MoveUnitRequestPacket: {JsonConvert.SerializeObject(packet)}");
                        GameInstance.AddPacketFromPlayer(packet, peerFrom);
                        break;
                    }
                case PacketType.ClientStartSyncServerTimestampPacket:
                    {
                        var packet = _netSerializer.Deserialize<ClientStartSyncServerTimestampPacket>(reader);
                        MessagesReceived.Add($"Client sent ClientStartSyncServerTimestampPacket: {JsonConvert.SerializeObject(packet)}");
                        SendServerSyncTimestampResponsePacket(packet, peerFrom);
                        break;
                    }
                default:
                    throw new NotImplementedException($"Unknown PacketType {reader.PeekInt()}");                    
            }            
        };        

        GameInstance = new ServerGameInstance(this);
    }

    private void _netListener_PeerDisconnectedEvent(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.WriteLine($"Client disconnected: {peer.EndPoint}, {disconnectInfo.Reason} ({disconnectInfo.AdditionalData})");
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

    public void SendPacketInitGame(InitGamePacket packet)
    {
        _netManager.SendToAll(_netSerializer.Serialize(packet), DeliveryMethod.ReliableOrdered);
    }

    public void SendMoveUnitOrderPacket(MoveUnitOrderPacket packet)
    {
        _netManager.SendToAll(_netSerializer.Serialize(packet), DeliveryMethod.ReliableOrdered);
    }

    public void SendServerSyncTimestampResponsePacket(ClientStartSyncServerTimestampPacket packetFromClient, NetPeer peerFrom)
    {
        var packet = new ServerSyncTimestampResponsePacket()
        {
            PacketType = (int)PacketType.ServerSyncTimestampResponsePacket,
            Timestamp = TimeUtils.GetCurrentTimestamp(),
            TimestampClient = packetFromClient.Timestamp
        };

        peerFrom.Send(_netSerializer.Serialize(packet), DeliveryMethod.ReliableOrdered);
    }
}
