using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PokerGame;

class Server
{
    List<Table> TableList = new List<Table>();
    List<Player>? Players;
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
        
        //verify user and password.

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
            {
                _ = HandleClient(client);   
            }
                
        }
    }
    

    void QueryPlayerAction(NetworkStream stream, List<RoundEngine.PlayerAction> actions)
    {
        ///Send the player the avaialable actions from RoundEngine.OfferPlayerActions
        
        var response = SendPlayerActions(stream, actions);
        if (ValidateAction(actions, response) ? true : false);
        //update the roundEngine       
    }
    int SendPlayerActions(NetworkStream stream, List<RoundEngine.PlayerAction> actions)
    {
        ///Send the player the avaialable actions from RoundEngine.OfferPlayerActions
        int clientResponse = -1;
        //stream.WriteAsync();
        return clientResponse;
        
    }
    bool ValidateAction(List<RoundEngine.PlayerAction> sentAction, int action)
    {
        /// Takes in the sent action list to the player and checks that the returned action choice from the player is among the sent actions.
        
        return false;
    }


    
    void SendClientTable(Player player)
    {
        ///Sends the client the updated table state minus the other players cards
        /*
        table.board
        that players cards
        each players 
            chipcount
            round move
        */
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
