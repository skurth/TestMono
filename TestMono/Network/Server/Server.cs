using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TestMono.Network.Packets;
using TestMono.Network.Packets.ClientToServer;
using TestMono.Network.Packets.ServerToClient;

namespace TestMono.Network.Server;

public class Server
{
    private EventBasedNetListener _netListener;
    private NetManager _netManager;
    private NetPacketProcessor _netPacketProcessor;

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
        _netManager = new NetManager(_netListener);
        _netPacketProcessor = new NetPacketProcessor();

        _netPacketProcessor.RegisterNestedType<InitGamePlayerInfo>(() => new InitGamePlayerInfo());
        //_netPacketProcessor.RegisterNestedType<MoveUnitRequestPacket>(() => new MoveUnitRequestPacket());

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
            //var writer = new NetDataWriter();
            //writer.Put($"Hello client Nr {_netManager.ConnectedPeersCount}!");
            //peer.Send(writer, DeliveryMethod.ReliableOrdered);
        };

        _netListener.NetworkReceiveEvent += (peerFrom, reader, deliveryMethod) =>
        {
            var packetType = (PacketType)reader.PeekInt(); //packetType = -132163544654 ??

            switch (packetType)
            {
                case PacketType.ClientInfoPacket:
                    var packet = reader.Get<ClientInfoPacket>();
                    MessagesReceived.Add($"Client sent InfoPacket: {JsonConvert.SerializeObject(packet)}");
                    GameInstance.AddPlayer(packet.PlayerId, peerFrom);
                    // ??
                    break;
                case PacketType.InitGamePacket:
                    // ??
                    break;
                case PacketType.MoveUnitRequestPacket:
                    // ??
                    break;
                case PacketType.MoveUnitOrderPacket:
                    // ??
                    break;
                default:
                    break;
            }

            //_netPacketProcessor.ReadAllPackets(reader);
        };

        //_netPacketProcessor.SubscribeReusable<ClientInfoPacket, NetPeer>((packet, peerFrom) =>
        //{
        //    MessagesReceived.Add($"Client sent InfoPacket: {JsonConvert.SerializeObject(packet)}");

        //    GameInstance.AddPlayer(packet.PlayerId, peerFrom);
        //});

        _netPacketProcessor.SubscribeNetSerializable<MoveUnitRequestPacket, NetPeer>((packet, peerFrom) => {
            MessagesReceived.Add($"Client sent MoveUnitRequestPacket: {JsonConvert.SerializeObject(packet)}");            

            GameInstance.AddPacketFromPlayer(packet, peerFrom);            
        });

        GameInstance = new ServerGameInstance(this);
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
        _netManager.SendToAll(_netPacketProcessor.Write(packet), DeliveryMethod.ReliableOrdered);
    }

    public void SendMoveUnitOrderPacket(MoveUnitOrderPacket packet)
    {
        _netManager.SendToAll(_netPacketProcessor.Write(packet), DeliveryMethod.ReliableOrdered);
    }
}
