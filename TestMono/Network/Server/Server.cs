using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TestMono.Network.Packets;

namespace TestMono.Network.Server;

internal class Server
{
    private EventBasedNetListener _netListener;
    private NetManager _netManager;
    private NetPacketProcessor _netPacketProcessor;

    public bool IsRunning => _netManager is not null && _netManager.IsRunning;

    public List<string> MessagesReceived;

    public Server()
    {
        MessagesReceived = new List<string>();
    }

    public void Start()
    {
        _netListener = new EventBasedNetListener();
        _netManager = new NetManager(_netListener);
        _netPacketProcessor = new NetPacketProcessor();        

        _netManager.Start(9050);

        _netListener.ConnectionRequestEvent += request =>
        {
            if (_netManager.ConnectedPeersCount < 10)
                request.AcceptIfKey("SomeConnectionKey");
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
            _netPacketProcessor.ReadAllPackets(reader, peerFrom);   
        };

        _netPacketProcessor.SubscribeReusable<FooPacket>((packet) => {
            MessagesReceived.Add($"Client sent FooPacket: {JsonConvert.SerializeObject(packet)}");
        });
        _netPacketProcessor.SubscribeReusable<PositionPacket>((packet) => {
            MessagesReceived.Add($"Client sent PositionPacket: {JsonConvert.SerializeObject(packet)}");
        });
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

    public void SendTest()
    {
        var writer = new NetDataWriter();
        writer.Put($"This message is from Server (sent at {DateTime.Now})");
        _netManager.SendToAll(writer, DeliveryMethod.ReliableOrdered);
    }

    public void SendTestPacket()
    {
        _netManager.SendToAll(_netPacketProcessor.Write(new FooPacket() { NumberValue = 3, StringValue = "Cat" }), DeliveryMethod.ReliableOrdered);
    }
}
