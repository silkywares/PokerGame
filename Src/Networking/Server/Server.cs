using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace PokerGame;


public class TableDTO
{
    public int RecevingPlayerSeatPos { get; set; } = new();
    public List<Card> Board { get; set; } = new();
    public int Pot { get; set; } = new();
    public RoundEngine.RoundState roundState { get; set; } = new();
    public int TurnIndex { get; set; } = new();
    public List<Card> Hand { get; set; } = new();
    public List<PlayerInfo> Players { get; set; } = new();
    public int ButtonPosition {get; set;} = new();
    
}
public class PlayerInfo
{
    public string Name { get; set; }
    public int ChipCount { get; set; }
    public bool IsFolded { get; set; }
    public bool IsAllIn { get; set; }
}

class Server
{
    Table ServerTable = new Table(null);
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

    }

    async Task ClientManager()
    {
        ///Manages all incoming client connections and fires off a new task for each one
        while (true)
        {
            TcpClient client = await Listener.AcceptTcpClientAsync();

            var stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string name = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            if(ActiveClients <= 6)
            {
                //Creates Player connection and player obejct, connects them then adds it to the table 
                //need to change ActiveClients variable as Player.seat index so that it takes the lowest seatposition that is currently avaialable
                PlayerConnection playerConnection = new PlayerConnection(client);
                Player player = new Player(name, ActiveClients, 100, playerConnection);
                
                //send player handshake
                SendHandshake(player);

                //add to table
                ServerTable.AddPlayer(player);

                //start handling client
                _ = HandleClient(player);   
            }
            else
                Console.WriteLine($"{name} cannot connect to the server. ActiveClients > 6");
                
        }
    }
    async Task HandleClient(Player player)
    {
        // local var stream is assigned to playerConnection stream
        var stream = player.PlayerConnection!.stream; 

        byte[] buffer = new byte[1024];
        int bytesRead;
        
        //TODO verify user and password.

        Console.WriteLine($"{player.Name} connected.");
        SendClientTable(player);
        ActiveClients++;

        try
        {
            while (true)
            {
                bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break; // client disconnected

                //receive client message
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine($"{player.Name}: {message}");

                // send acknowledgement
                await SendAck(player);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"{player.Name} disconnected unexpectedly: {e.Message}");
        }
        finally
        {
            player.PlayerConnection!.client.Close();
            ServerTable.RemovePlayer(player);
            ActiveClients--;
            Console.WriteLine($"{player.Name} disconnected. Active clients: {ActiveClients}");
        }
    }
    async Task SendAck(Player player, string message = "Received")
    {
        byte[] payload = Encoding.UTF8.GetBytes(message);
        byte[] framedMessage = new byte[1 + 4 + payload.Length];
        framedMessage[0] = (byte)MessageType.Ack;
        Array.Copy(BitConverter.GetBytes(payload.Length), 0, framedMessage, 1, 4);
        Array.Copy(payload, 0, framedMessage, 5, payload.Length);

        await player.PlayerConnection!.stream.WriteAsync(framedMessage, 0, framedMessage.Length);
    }
    void SendHandshake(Player player)
    {
        byte[] payload = Encoding.UTF8.GetBytes(player.Name);
        byte[] message = new byte[1 + 4 + payload.Length];
        message[0] = (byte)MessageType.Handshake;
        Array.Copy(BitConverter.GetBytes(payload.Length), 0, message, 1, 4);
        Array.Copy(payload, 0, message, 5, payload.Length);

        _ = player.PlayerConnection!.stream.WriteAsync(message, 0, message.Length);
    }
    public void QueryPlayerAction(Player player, List<RoundEngine.PlayerAction> actions)
    {
        ///Send the player the avaialable actions from RoundEngine.OfferPlayerActions
        
        var response = SendPlayerActions(player, actions);
        if (ValidateAction(actions, response) ? true : false);
        //update the roundEngine       
    }
    int SendPlayerActions(Player player, List<RoundEngine.PlayerAction> actions)
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
        var tableState = new
        {
            Players = ServerTable.Players.Select(p => new { p.Name, p.ChipCount, p.IsFolded, p.IsAllIn }).ToList(),
            RecevingPlayerSeatPos = player.SeatPosition,
            RoundState = ServerTable.RoundEngine.roundState,
            ButtonPosition = ServerTable.ButtonPosition,
            Board = ServerTable.Dealer.Board,     
            Pot = ServerTable.Pot,
            Hand = player.Hand,
            TurnIndex = ServerTable.RoundEngine.turnIndex

        };


        //message will send 1 byte first to indicate message type, 
        // then 4 bytes for the JSON length, then the JSON message
        string json = JsonSerializer.Serialize(tableState);
        byte[] data = Encoding.UTF8.GetBytes(json);
            
        byte[] lengthPrefix = BitConverter.GetBytes(data.Length);
        byte[] message = new byte[1 + 4 + data.Length]; //(message type[1] + message length[4] + message[n])

        message[0] = (byte)MessageType.TableState; //set message type
        Array.Copy(lengthPrefix, 0, message, 1, 4);//set message length
        Array.Copy(data, 0, message, 5, data.Length);//set message

        //DEBUGING
        //Console.WriteLine("JSON being sent:");
        //Console.WriteLine(json);

        _ = player.PlayerConnection!.stream.WriteAsync(message, 0, message.Length);
    }
    async Task PrintPlayersLoop()
    {
        int lastActiveClients = ActiveClients;
        while (true)
        {
            await Task.Delay(500);

            if (ActiveClients != lastActiveClients)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine("---- Players at Table ----");
                foreach (var p in ServerTable.Players)
                {
                    Console.WriteLine($"{p.Name}");
                    SendClientTable(p);
                }
                Console.WriteLine("--------------------------");
                Console.ForegroundColor = ConsoleColor.Gray;
                lastActiveClients = ActiveClients;
            }
        }
    }

    
    
    
    public async Task StartAsync(int port)
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("Server starting...");
        Console.WriteLine($"Server listening on port {port}");
        Console.ForegroundColor = ConsoleColor.Gray;
        _ = PrintPlayersLoop();   // fire and forget
        await ClientManager();
    }
    public static async Task Start()
    {
        int port = 5000;
        Server server = new Server(port);
        server.ServerTable.TestTable(); //sets test table state
        await server.StartAsync(port);
    }
}
