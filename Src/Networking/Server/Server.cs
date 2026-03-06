using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PokerGame;

class Server
{
    Table? Table;
    TcpListener Listener;
    int ActiveClients;
    int Port;
    public Server(int port)
    {
        ActiveClients = 0;
        Port = port;
        Listener = new TcpListener(IPAddress.Any, port);
        Listener.Start();
        Console.WriteLine($"Server listening on port {port}");

    }
    async Task HandleClient(TcpClient client)
    {
        var stream = client.GetStream();

        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string name = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        
        Console.WriteLine($"{name} connected.");
        ActiveClients++;

        try
        {
            while (true)
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // client disconnected

                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"{name}: {message}");

                // send acknowledgement
                byte[] response = Encoding.UTF8.GetBytes("Received");
                await stream.WriteAsync(response, 0, response.Length);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{name} disconnected unexpectedly: {e.Message}");
        }
        finally
        {
            client.Close();
            ActiveClients--;
            Console.WriteLine($"{name} disconnected. Active clients: {ActiveClients}");
        }
    }
    async Task ClientManager()
    {
        while (true)
        {
            TcpClient client = await Listener.AcceptTcpClientAsync();
            // fire-and-forget handling of this client
            if(ActiveClients < 6)
                _ = HandleClient(client);
        }
    }
    public async Task StartAsync()
    {
        Console.WriteLine("Server starting...");
        await ClientManager();
    }
    public static async Task Start()
    {
        int port = 5000;
        Server server = new Server(port);
        await server.StartAsync();
    }
}
