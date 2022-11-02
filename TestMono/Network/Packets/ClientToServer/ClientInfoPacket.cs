using LiteNetLib.Utils;

namespace TestMono.Network.Packets.ClientToServer;

public class ClientInfoPacket
    : IBasePacket, INetSerializable
{
    public int PacketType { get; set; }
    public long Timestamp { get; set; }
    public string PlayerId { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        PacketType = reader.GetInt();
        Timestamp = reader.GetLong();
        PlayerId = reader.GetString();        
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PacketType);
        writer.Put(Timestamp);
        writer.Put(PlayerId);        
    }
}
