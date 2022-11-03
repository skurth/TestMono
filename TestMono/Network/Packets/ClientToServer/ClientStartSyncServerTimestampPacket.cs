using LiteNetLib.Utils;

namespace TestMono.Network.Packets.ClientToServer;

public class ClientStartSyncServerTimestampPacket
    : IBasePacket, INetSerializable
{
    public int PacketType { get; set; }
    public long Timestamp { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        PacketType = reader.GetInt();
        Timestamp = reader.GetLong();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PacketType);
        writer.Put(Timestamp);
    }
}
