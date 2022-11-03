using LiteNetLib;
using System;
using System.Collections.Generic;
using System.Linq;
using TestMono.GameObjects.Map;
using TestMono.Helpers;
using TestMono.Network.Client;
using TestMono.Network.Packets;
using TestMono.Network.Packets.ServerToClient;

namespace TestMono.Network.Server;

public class ServerGameInstance
{    
    public Server Server { get; set; }    

    public int ConnectedPlayersCount { get => Server.ConnectedPlayersCount; }
    public bool CanStartGame { get => ConnectedPlayersCount >= 1; }

    public List<PlayerServerInfo> Players { get; set; }

    public ClientGameInstance ClientGameInstance { get; set; }

    public List<PlayerPacket> PlayerPackets { get; set; }

    public ServerGameInstance(Server server)
    {
        Server = server;
        Players = new List<PlayerServerInfo>();
        ClientGameInstance = new ClientGameInstance(null);
        PlayerPackets = new List<PlayerPacket>();
    }

    public void AddPlayer(string playerId, NetPeer peer)
    {
        Players.Add(new PlayerServerInfo() { Id = playerId, Peer = peer });
    }

    public void InitGame()
    {
        // Generate Map
        var r = new Random();
        int tilesX = r.Next(Map._MAP_MIN_TILES_X, Map._MAP_MAX_TILES_X);
        int tilesY = r.Next(Map._MAP_MIN_TILES_Y, Map._MAP_MAX_TILES_Y);

        var packet = new InitGamePacket()
        {
            PacketType = (int)PacketType.InitGamePacket,
            Timestamp = TimeUtils.GetCurrentTimestamp(),
            MapTilesX = tilesX,
            MapTilesY = tilesY
        };

        // Generate Players
        int playerIdx = 0;
        int unitIdx = 0;
        foreach (var player in Players)
        {
            var startUnitPositionTileX = r.Next(0, tilesX);
            var startUnitPositionTileY = r.Next(0, tilesY);
            packet.PlayerInfos.Add(new InitGamePlayerInfo()
            {
                Id = player.Id,
                Idx = playerIdx++,
                StartUnitId = unitIdx++,
                StartUnitPositionTileX = startUnitPositionTileX,
                StartUnitPositionTileY = startUnitPositionTileY,
                ColorIdx = playerIdx
            });
        }

        Server.SendPacketInitGame(packet);

        // Server simulates Client        
        var sceneInfo = ClientGameInstance.CreateGameSceneInfo(packet, ApplicationType.Server);
        Game1.CurrentGame.ScenesManager.CreateGameScene(sceneInfo, ClientGameInstance, this);
    }

    public void AddPacketFromPlayer(IBasePacket packet, NetPeer peerFrom)
    {
        var playerInfo = Players.FirstOrDefault(x => x.Peer == peerFrom);
        if (playerInfo is null)
        {
            throw new Exception("PlayerInfo not found");
        }

        PlayerPackets.Add(new PlayerPacket() { PlayerInfo = playerInfo, Packet = packet });
    }

    public class PlayerServerInfo
    {
        public string Id { get; set; }
        public NetPeer Peer { get; set; }
    }
    
    public class PlayerPacket
    {
        public PlayerServerInfo PlayerInfo { get; set; }
        public IBasePacket Packet { get; set; }
        public bool Handled { get; set; } = false;
    }

}
