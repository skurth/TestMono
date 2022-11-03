using LiteNetLib.Utils;

namespace TestMono.Network.Packets.ServerToClient;

public class BuildingSetFoundationOrderPacket
    : IBasePacket
{
    public int PacketType { get; set; }
    public long Timestamp { get; set; }
    public int BuildingId { get; set; }
    public string PlayerId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        PacketType = reader.GetInt();
        Timestamp = reader.GetLong();
        BuildingId = reader.GetInt();
        PlayerId = reader.GetString();
        PositionX = reader.GetFloat();
        PositionY = reader.GetFloat();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PacketType);
        writer.Put(Timestamp);
        writer.Put(BuildingId);
        writer.Put(PlayerId);
        writer.Put(PositionX);
        writer.Put(PositionY);
    }
}