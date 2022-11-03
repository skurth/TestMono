using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMono.TestCode
{
    internal class ServerMultithread
    {
        //private List<GameInstance> _gameInstances = new();
        //private void btnTest_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    var gameInstance1 = new GameInstance()
        //    {
        //        Name = "Game Instance 1",
        //        ID = Guid.NewGuid(),
        //        Players = new List<string>()
        //    {
        //        "Spieler 1", "Spieler 2"
        //    }
        //    };

        //    gameInstance1.Thread = new Thread(StartGameInstance);
        //    gameInstance1.Thread.Start(gameInstance1);
        //    _gameInstances.Add(gameInstance1);

            //var gameInstance2 = new GameInstance()
            //{
            //    Name = "Game Instance 2",
            //    ID = Guid.NewGuid(),
            //    Players = new List<string>()
            //    {
            //        "Spieler 3", "Spieler 4"
            //    }
            //};

            //gameInstance2.Thread = new Thread(StartGameInstance);
            //gameInstance2.Thread.Start(gameInstance2);
            //_gameInstances.Add(gameInstance2);


            //var listenerThread = new Thread(StartListenerThread);
            //listenerThread.Start();
        //}

        //public const int TICKS_PER_SEC = 1;
        //public const float MS_PER_TICK = 1000 / TICKS_PER_SEC;
        //private void StartGameInstance(object data)
        //{
        //    var gameInstance = (GameInstance)data;

        //    // While (genauer als PeriodicTimer)
        //    DateTime _lastLoop = DateTime.Now;
        //    DateTime _nextLoop = _lastLoop.AddMilliseconds(MS_PER_TICK);

        //    var lastRun = DateTime.Now;

        //    while (true)
        //    {
        //        var now = DateTime.Now;
        //        while (_nextLoop < now)
        //        {
        //            Debug.WriteLine($"{DateTime.Now.ToString("O")} {gameInstance.Name} Tick (Diff: {(now - lastRun).TotalMilliseconds})");
        //            lastRun = now;

        //            _lastLoop = _nextLoop;
        //            _nextLoop = _nextLoop.AddMilliseconds(MS_PER_TICK);

        //            List<Packet> packetsToProceed;
        //            lock (gameInstance.PacketsLock)
        //            {
        //                packetsToProceed = new List<Packet>(gameInstance.Packets);
        //                gameInstance.Packets.Clear();
        //            }

        //            foreach (var packet in packetsToProceed)
        //            {
        //                Debug.WriteLine($"{DateTime.Now.ToString("o")} {gameInstance.Name} Packet {packet.TestString}");
        //            }

        //            //Thread.Sleep(5200);

        //            if (_nextLoop > DateTime.Now)
        //            {
        //                //Vermutlich gesünder fürs System (CPU), allerdings wird ein Tick dadurch ungenauer (paar ms)
        //                //Thread.Sleep(_nextLoop - DateTime.Now);
        //            }
        //        }
        //    }

        //    //PeriodicTimer
        //    //var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(TICKS_PER_SEC * 1000));

        //    //var lastRun = DateTime.Now;
        //    //while (await timer.WaitForNextTickAsync())
        //    //{
        //    //    var now = DateTime.Now;
        //    //    Debug.WriteLine($"{DateTime.Now.ToString("O")} {gameInstance.Name} Tick (Diff: {(now - lastRun).TotalMilliseconds})");
        //    //    lastRun = now;

        //    //    List<Packet> packetsToProceed;
        //    //    lock (gameInstance.PacketsLock)
        //    //    {
        //    //        packetsToProceed = new List<Packet>(gameInstance.Packets);
        //    //        gameInstance.Packets.Clear();
        //    //    }

        //    //    foreach (var packet in packetsToProceed)
        //    //    {
        //    //        Debug.WriteLine($"{DateTime.Now.ToString("O")} {gameInstance.Name} Packet {packet.TestString}");
        //    //    }

        //    //    Thread.Sleep(500);
        //    //}

        //}

        //private void StartListenerThread()
        //{
        //    var random = new Random();

        //    while (true)
        //    {
        //        var gameInstanceIdx = random.Next(0, 2);
        //        var gameInstance = _gameInstances[gameInstanceIdx];

        //        lock (gameInstance.PacketsLock)
        //        {
        //            gameInstance.Packets.Add(new Packet()
        //            {
        //                TestInt = gameInstanceIdx,
        //                TestString = "Hi from client: " + Guid.NewGuid().ToString()
        //            });
        //        }

        //        Thread.Sleep(5000);
        //    }
        //}

        //private class GameInstance
        //{
        //    public string Name { get; set; }
        //    public Thread Thread { get; set; }
        //    public Guid ID { get; set; }
        //    public List<string> Players { get; set; }
        //    public List<Packet> Packets { get; set; } = new();
        //    public object PacketsLock { get; set; } = new();
        //}

        //private class Packet
        //{
        //    public int TestInt { get; set; }
        //    public string TestString { get; set; }
        //}
    }
}
