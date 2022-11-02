using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestMono.Network.Packets;
using TestMono.Network.Packets.ClientToServer;
using TestMono.Network.Packets.ServerToClient;

namespace TestMono.Network.Client;

public class Client
{
    public string PlayerId { get; init; }

    private EventBasedNetListener _netListener;
    private NetManager _netManager;
    private NetPacketProcessor _netPacketProcessor;

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
        _netPacketProcessor = new NetPacketProcessor();

        _netPacketProcessor.RegisterNestedType<InitGamePlayerInfo>(() => new InitGamePlayerInfo());
        _netPacketProcessor.RegisterNestedType<MoveUnitRequestPacket>(() => new MoveUnitRequestPacket());

        //_netManager.SimulateLatency = true;
        //_netManager.SimulationMinLatency = 1000;
        //_netManager.SimulationMaxLatency = 3000;
        //_netManager.SimulationPacketLossChance = 99;

        _netManager.Start();
        _netManager.Connect("localhost", 9050, "TestMono");

        _netListener.PeerConnectedEvent += (server) =>
        {
            SendClientInfoPacket();
        };

        _netListener.NetworkReceiveEvent += (server, reader, deliveryMethod) =>
        {
            _netPacketProcessor.ReadAllPackets(reader, server);
        };                

        _netPacketProcessor.SubscribeReusable<InitGamePacket>((packet) => {
            MessagesReceived.Add($"Server sent InitGamePacket: {JsonConvert.SerializeObject(packet)}");

            var sceneInfo = ClientGameInstance.CreateGameSceneInfo(packet, ApplicationType.Client);
            Game1.CurrentGame.ScenesManager.CreateGameScene(sceneInfo, GameInstance, null);
        });

        _netPacketProcessor.SubscribeReusable<MoveUnitOrderPacket, NetPeer>((packet, peerFrom) => {
            MessagesReceived.Add($"Client sent InfoPacket: {JsonConvert.SerializeObject(packet)}");

            GameInstance.MoveUnitOrder(packet);            
        });

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
        //_netManager.SendToAll(_netPacketProcessor.Write(new ClientInfoPacket() { 
        //    PacketType = (int)PacketType.ClientInfoPacket, 
        //    Timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds(), 
        //    PlayerId = PlayerId }), DeliveryMethod.ReliableOrdered);

        var packet = new ClientInfoPacket()
        {
            PacketType = (int)PacketType.ClientInfoPacket,
            Timestamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds(),
            PlayerId = PlayerId
        };

        var writer = new NetDataWriter();
        writer.Put(packet);
        _netManager.SendToAll(writer, DeliveryMethod.ReliableOrdered);
    }

    public void SendMoveUnitRequestPacket(MoveUnitRequestPacket packet)
    {
        _netManager.SendToAll(_netPacketProcessor.Write(packet), DeliveryMethod.ReliableOrdered);
    }

}
