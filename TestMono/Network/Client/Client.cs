using LiteNetLib;
using LiteNetLib.Utils;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestMono.Network.Packets;

namespace TestMono.Network.Client;

internal class Client
{
    private EventBasedNetListener _netListener;
    private NetManager _netManager;
    private NetPacketProcessor _netPacketProcessor;

    public bool IsRunning => _netManager is not null && _netManager.IsRunning;

    public List<string> MessagesReceived = new List<string>();


    public Client()
    {

    }

    public void Connect()
    {
        _netListener = new EventBasedNetListener();
        _netManager = new NetManager(_netListener);
        _netPacketProcessor = new NetPacketProcessor();

        _netManager.Start();
        _netManager.Connect("localhost", 9050, "SomeConnectionKey");

        _netListener.NetworkReceiveEvent += (server, reader, deliveryMethod) =>
        {
            //var msg = dataReader.GetString(100);
            //MessagesReceived.Add(msg);            
            //dataReader.Recycle();

            _netPacketProcessor.ReadAllPackets(reader, server);
        };

        _netPacketProcessor.SubscribeReusable<FooPacket>((packet) => {
            MessagesReceived.Add($"Server sent FooPacket: {JsonConvert.SerializeObject(packet)}");
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
        //NetClient.SendToAll
    }

    public void SendTestPacket()
    {
        //_netManager.SendToAll(_netPacketProcessor.Write(new FooPacket() { NumberValue = 3, StringValue = "Hi from Client" }), DeliveryMethod.ReliableOrdered);
        _netManager.SendToAll(_netPacketProcessor.Write(new PositionPacket() { UnitId = 1, MoveToX = 2, MoveToY = 3}), DeliveryMethod.ReliableOrdered);
    }

}
