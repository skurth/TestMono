using LiteNetLib.Utils;
using System.Collections.Generic;

namespace TestMono.Network.Packets.ServerToClient;

public class InitGamePacket
{
    public int MapTilesX { get; set; }
    public int MapTilesY { get; set; }

    public List<InitGamePlayerInfo> PlayerInfos { get; set; } = new List<InitGamePlayerInfo>();
}

public class InitGamePlayerInfo : INetSerializable
{
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

    public int Idx { get; set; }
    public string Id { get; set; }
    public int StartUnitId { get; set; }
    public int StartUnitPositionTileX { get; set; }
    public int StartUnitPositionTileY { get; set; }
    public int ColorIdx { get; set; }
}