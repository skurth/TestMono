using LiteNetLib.Utils;

namespace TestMono.Network.Packets.ServerToClient;

public class BuildingSetBuiltOrderPacket
    : IBasePacket
{
    public int PacketType { get; set; }
    public long Timestamp { get; set; }
    public int BuildingId { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        PacketType = reader.GetInt();
        Timestamp = reader.GetLong();
        BuildingId = reader.GetInt();        
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PacketType);
        writer.Put(Timestamp);
        writer.Put(BuildingId);        
    }
}