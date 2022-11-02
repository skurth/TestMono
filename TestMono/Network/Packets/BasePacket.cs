using System;

namespace TestMono.Network.Packets;

//public class BasePacket
//{
//    public int PacketType { get; set; }
//    public long Timestamp { get; set; }

//    public BasePacket(PacketType packetType)
//    {
//        PacketType = (int)packetType;        
//    }    
//}

public interface IBasePacket
{
    public int PacketType { get; set; }
    public long Timestamp { get; set; }
}

public enum PacketType
{
    ClientInfoPacket = 1,    
    InitGamePacket = 2,
    MoveUnitRequestPacket = 3,
    MoveUnitOrderPacket = 4
}