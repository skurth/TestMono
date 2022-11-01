using System.Collections.Generic;
using TestMono.GameObjects;
using TestMono.GameObjects.Map;
using TestMono.Helpers;
using TestMono.Network.Packets.ClientToServer;
using TestMono.Network.Packets.ServerToClient;
using TestMono.Scenes;

namespace TestMono.Network.Client;

public class ClientGameInstance
{
    public Client Client { get; init; }

    public ClientGameInstance(Client client)
    {
        Client = client;
    }

    public static CreateGameSceneInfo CreateGameSceneInfo(InitGamePacket packet, ApplicationType appType)
    {
        var map = new Map();
        map.InitMap(packet.MapTilesX, packet.MapTilesY);

        var players = new List<Player>();
        Player localPlayer = null;

        foreach (var playerInfo in packet.PlayerInfos)
        {
            bool authorized = false;
            if (appType == ApplicationType.Client)
            {
                var client = ApplicationManager.Client;
                authorized = client.PlayerId == playerInfo.Id;
            }

            var startUnitPosition = Map.GetPositionFromTiles(playerInfo.StartUnitPositionTileX, playerInfo.StartUnitPositionTileY);

            var player = new Player(playerInfo.Id,
                                    authorized,
                                    playerInfo.StartUnitId,
                                    startUnitPosition,
                                    PlayerUtils.PlayerColors[playerInfo.ColorIdx]);
            players.Add(player);

            if (player.Authorized)
                localPlayer = player;
        }

        var sceneInfo = new CreateGameSceneInfo()
        {
            AppType = appType,
            Map = map,
            Players = players,
            LocalPlayer = localPlayer
        };        

        return sceneInfo;
    }

    public void MoveUnitRequest(int unitId, float endPositionX, float endPositionY)
    {
        var packet = new MoveUnitRequestPacket()
        {
            UnitId = unitId,
            PositionX = endPositionX,
            PositionY = endPositionY
        };

        Client.SendMoveUnitRequestPacket(packet);
    }

    public void MoveUnitOrder(MoveUnitOrderPacket packet)
    {
        var gameScene = Game1.CurrentGame.ScenesManager.CurrentScene as GameScene;
        gameScene.MoveUnitOrder(packet.UnitId, packet.PositionX, packet.PositionY);
    }
}

public class CreateGameSceneInfo
{
    public ApplicationType AppType { get; set; }
    public Map Map { get; set; }
    public List<Player> Players { get; set; }
    public Player LocalPlayer { get; set; }
}
