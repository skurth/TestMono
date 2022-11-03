using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TestMono.GameObjects;
using TestMono.GameObjects.Map;
using TestMono.Helpers;
using TestMono.Network.Packets;
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
            PacketType = (int)PacketType.MoveUnitRequestPacket,
            Timestamp = TimeUtils.GetCurrentTimestamp(),
            UnitId = unitId,
            PositionX = endPositionX,
            PositionY = endPositionY
        };

        Debug.WriteLine($"{DateTime.Now} Client to Server: {JsonConvert.SerializeObject(packet)}");

        Client.SendMoveUnitRequestPacket(packet);
    }    

    public void MoveUnitOrder(MoveUnitOrderPacket packet)
    {
        var gameScene = Game1.CurrentGame.ScenesManager.CurrentScene as GameScene;
        gameScene.MoveUnitOrder(packet.UnitId, packet.PositionX, packet.PositionY);
    }

    public void MoveUnitStopOrder(MoveUnitStopOrderPacket packet)
    {
        var gameScene = Game1.CurrentGame.ScenesManager.CurrentScene as GameScene;
        gameScene.MoveUnitStopOrder(packet.UnitId, packet.PositionX, packet.PositionY);
    }

    public void BuildingSetFoundationRequest(string playerId, float endPositionX, float endPositionY)
    {
        var packet = new BuildingSetFoundationRequestPacket()
        {
            PacketType = (int)PacketType.BuildingSetFoundationRequestPacket,
            Timestamp = TimeUtils.GetCurrentTimestamp(),
            PlayerId = playerId,
            PositionX = endPositionX,
            PositionY = endPositionY
        };

        Debug.WriteLine($"{DateTime.Now} Client to Server: {JsonConvert.SerializeObject(packet)}");

        Client.SendBuildingSetFoundationRequestPacket(packet);
    }

    public void BuildingSetFoundationOrder(BuildingSetFoundationOrderPacket packet)
    {
        var gameScene = Game1.CurrentGame.ScenesManager.CurrentScene as GameScene;
        gameScene.BuildingSetFoundationOrder(packet.BuildingId, packet.PlayerId, packet.PositionX, packet.PositionY);
    }

    public void BuildingSetBuiltOrder(BuildingSetBuiltOrderPacket packet)
    {
        var gameScene = Game1.CurrentGame.ScenesManager.CurrentScene as GameScene;
        gameScene.BuildingSetBuiltOrder(packet.BuildingId);
    }

    public void SyncServerTimestampRequest()
    {
        var packet = new ClientStartSyncServerTimestampPacket()
        {
            PacketType = (int)PacketType.ClientStartSyncServerTimestampPacket,
            Timestamp = TimeUtils.GetCurrentTimestamp(),
        };

        Debug.WriteLine($"{DateTime.Now} Client to Server: {JsonConvert.SerializeObject(packet)}");

        Client.SendSyncServerTimestampRequestPacket(packet);
    }

    public void SyncServerTimestamp(ServerSyncTimestampResponsePacket packet)
    {
        var gameScene = Game1.CurrentGame.ScenesManager.CurrentScene as GameScene;

        // calculate the time taken from the packet to be sent from the client and then for the server to return it //
        var roundTrip = (int)(TimeUtils.GetCurrentTimestamp() - packet.TimestampClient);
        var latency = roundTrip / 2; // the latency is half the round-trip time
        // calculate the server-delta from the server time minus the current time
        int serverDelta = (int)(packet.Timestamp - TimeUtils.GetCurrentTimestamp());
        var timeDelta = serverDelta + latency; // the time-delta is the server-delta plus the latency

        var timestampInfo = new TimestampInfo()
        {
            ServerTimestamp = packet.Timestamp,
            Latency = latency,
            ServerDelta = serverDelta,
            TimeDelta = timeDelta
        };

        gameScene.SetTimestampInfo(timestampInfo);
    }

}

public class CreateGameSceneInfo
{
    public ApplicationType AppType { get; set; }
    public Map Map { get; set; }
    public List<Player> Players { get; set; }
    public Player LocalPlayer { get; set; }
}
