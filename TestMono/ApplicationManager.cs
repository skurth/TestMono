using TestMono.Network.Client;
using TestMono.Network.Server;

namespace TestMono;

internal static class ApplicationManager
{
    public static ApplicationType AppType { get; set; }

    public static Client Client { get; set; }
    public static Server Server { get; set; }

    public static bool IsRunning { get => Client is not null || Server is not null; }

    public static void OnClientStartRequested()
    {
        AppType = ApplicationType.Client;
        Client = new Client();
        Client.Connect();
    }

    public static void OnServerStartRequested()
    {
        AppType = ApplicationType.Server;
        Server = new Server();
        Server.Start();
    }

    public static void PollEvents()
    {
        if (!IsRunning)
            return;

        Server?.PollEvents();
        Client?.PollEvents();
    }   
}

public enum ApplicationType
{
    None = 0,
    Server = 1,
    Client = 2
}
