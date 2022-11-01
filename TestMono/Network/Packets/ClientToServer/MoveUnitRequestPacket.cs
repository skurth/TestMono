namespace TestMono.Network.Packets.ClientToServer;

public class MoveUnitRequestPacket
{
    public int UnitId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
}