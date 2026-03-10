using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

using PokerGame.DTOs;

namespace PokerGame;


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
        //TESTING

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
    
    static async Task SendMessageAsync<T>(Player player, MessageType type, T dto)
    {
        ///Generalized Message serialization and framing
        string json = JsonSerializer.Serialize(dto);
        byte[] payload = Encoding.UTF8.GetBytes(json);
        byte[] lengthPrefix = BitConverter.GetBytes(payload.Length);
        byte[] message = new byte[1 + 4 + payload.Length];

        message[0] = (byte)type;
        Array.Copy(lengthPrefix, 0, message, 1, 4);
        Array.Copy(payload, 0, message, 5, payload.Length);

        await player.PlayerConnection!.stream.WriteAsync(message, 0, message.Length);
    }
    async Task SendAck(Player player, string message = "Received")
    {
        await SendMessageAsync(player, MessageType.Ack, message);
    }
    async void SendHandshake(Player player)
    {
        await SendMessageAsync(player, MessageType.Handshake, player.Name);
    }

    async void SendActions(Player player, List<RoundEngine.PlayerActionOption> actions)
    {
        ///Send the player the avaialable actions from RoundEngine.OfferPlayerActions
        ActionsDTO dto = new ActionsDTO
        {
            Options = actions.Select(a => new PlayerActionOptionsDTO
            {
                Action = a.Action,
                Amount = a.Amount,
                MinAmount = a.MinAmount,
                MaxAmount = a.MaxAmount
            }).ToList()
        };
        await SendMessageAsync(player, MessageType.ActionRequest, dto);
    }
    bool ValidateAction(List<RoundEngine.PlayerActionOption> sentAction, int action)
    {
        /// Takes in the sent action list to the player and checks that the returned action choice from the player is among the sent actions.
        return false;
    }

    async void SendClientTable(Player player)
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

        await SendMessageAsync(player, MessageType.TableState, tableState);
    }
    async Task PrintPlayersLoop()
    {
        int lastActiveClients = ActiveClients;


var testOptions = new List<RoundEngine.PlayerActionOption>
{
    new RoundEngine.PlayerActionOption
    {
        Action = PlayerAction.Fold,
        Amount = 0,
        MinAmount = 0,
        MaxAmount = 0
    },
    new RoundEngine.PlayerActionOption
    {
        Action = PlayerAction.Call,
        Amount = 50,
        MinAmount = 50,
        MaxAmount = 50
    },
    new RoundEngine.PlayerActionOption
    {
        Action = PlayerAction.Raise,
        Amount = 100,
        MinAmount = 100,
        MaxAmount = 200
    },
    new RoundEngine.PlayerActionOption
    {
        Action = PlayerAction.Check,
        Amount = 0,
        MinAmount = 0,
        MaxAmount = 0
    }
};
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
                    SendActions(p, testOptions);
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
