using LiteNetLib.Utils;
using System.Collections.Generic;

namespace TestMono.Network.Packets.ServerToClient;

public class InitGamePacket
    : IBasePacket, INetSerializable
{
    public int PacketType { get; set; }
    public long Timestamp { get; set; }
    public int MapTilesX { get; set; }
    public int MapTilesY { get; set; }

    public List<InitGamePlayerInfo> PlayerInfos { get; set; } = new List<InitGamePlayerInfo>();

    public void Deserialize(NetDataReader reader)
    {
        PacketType = reader.GetInt();
        Timestamp = reader.GetLong();
        MapTilesX = reader.GetInt();
        MapTilesY = reader.GetInt();        
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(PacketType);
        writer.Put(Timestamp);
        writer.Put(MapTilesX);
        writer.Put(MapTilesY);        
    }
}

public class InitGamePlayerInfo : INetSerializable
{
    public int Idx { get; set; }
    public string Id { get; set; }
    public int StartUnitId { get; set; }
    public int StartUnitPositionTileX { get; set; }
    public int StartUnitPositionTileY { get; set; }
    public int ColorIdx { get; set; }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Idx);
        writer.Put(Id);
        writer.Put(StartUnitId);
        writer.Put(StartUnitPositionTileX);
        writer.Put(StartUnitPositionTileY);
        writer.Put(ColorIdx);
    }

    public void Deserialize(NetDataReader reader)
    {
        Idx = reader.GetInt();
        Id = reader.GetString();
        StartUnitId = reader.GetInt();
        StartUnitPositionTileX = reader.GetInt();
        StartUnitPositionTileY = reader.GetInt();
        ColorIdx = reader.GetInt();
    }
}