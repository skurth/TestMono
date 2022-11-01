namespace TestMono.Network.Packets.ServerToClient;

public class MoveUnitOrderPacket
{
    public int UnitId { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
}