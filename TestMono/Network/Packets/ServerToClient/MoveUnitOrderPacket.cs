using LiteNetLib.Utils;

namespace TestMono.Network.Packets.ServerToClient;

public class MoveUnitOrderPacket 
    : IBasePacket
{
    public int PacketType { get; set; }
    public long Timestamp { get; set; }
    public int UnitId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }

    public void Deserialize(NetDataReader reader)
    {
        PacketType = reader.GetInt();
        Timestamp = reader.GetLong();
        UnitId = reader.GetInt();
        PositionX = reader.GetFloat();
        PositionY = reader.GetFloat();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PacketType);
        writer.Put(Timestamp);
        writer.Put(UnitId);
        writer.Put(PositionX);
        writer.Put(PositionY);
    }
}