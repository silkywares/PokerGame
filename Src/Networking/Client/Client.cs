using System.Net.Sockets;
using System.Text;
using PokerGame;
using System.Text.Json;

namespace Pokergame;

class Client
{
    static TcpClient? client;
    static NetworkStream? stream;

    static void ConnectToServer(string name)
    {
        try
        {
            client = new TcpClient("127.0.0.1", 5000);
            stream = client.GetStream();
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine($"Connected to Server as {name}.");

            // Send framed handshake
            byte[] payload = Encoding.UTF8.GetBytes(name);
            byte[] message = new byte[1 + 4 + payload.Length];
            message[0] = (byte)MessageType.Handshake;
            Array.Copy(BitConverter.GetBytes(payload.Length), 0, message, 1, 4);
            Array.Copy(payload, 0, message, 5, payload.Length);

            stream.Write(message, 0, message.Length);
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Could not connect: {e.Message}");
            Environment.Exit(1);
        }
    }
    static async Task SendMessage(string message)
    {
        if (stream == null || client == null || !client.Connected) return;

        byte[] payload = Encoding.UTF8.GetBytes(message);
        byte[] framedMessage = new byte[1 + 4 + payload.Length];
        framedMessage[0] = (byte)MessageType.Chat;
        Array.Copy(BitConverter.GetBytes(payload.Length), 0, framedMessage, 1, 4);
        Array.Copy(payload, 0, framedMessage, 5, payload.Length);

        await stream.WriteAsync(framedMessage, 0, framedMessage.Length);
    }
    static async Task ReceiveMessage()
    {
        byte[] typeBuffer = new byte[1];
        await ReadExactAsync(stream!, typeBuffer, 1);
        MessageType type = (MessageType)typeBuffer[0];

        byte[] lengthBytes = new byte[4];
        await ReadExactAsync(stream!, lengthBytes, 4);
        int messageLength = BitConverter.ToInt32(lengthBytes, 0);

        byte[] payload = new byte[messageLength];
        await ReadExactAsync(stream!, payload, messageLength);

        string json = Encoding.UTF8.GetString(payload);

        switch (type)
        {
            case MessageType.Handshake:
                Console.WriteLine($"Handshake confirmed.");
                break;
            case MessageType.Ack:
                Console.WriteLine($"Server ACK: {json}");
                break;
            case MessageType.TableState:
                var tableState = JsonSerializer.Deserialize<TableDTO>(json) 
                    ?? throw new Exception("Invalid table state received");
                Console.WriteLine("Recieved DTO");
                Interface.PrintTable(tableState);
                break;
            case MessageType.Chat:
                // handle chat
                break;
            case MessageType.ActionRequest:
                //handle action request
                break;
        }
    }
    static async Task ReadExactAsync(NetworkStream stream, byte[] buffer, int length)
    {
        int totalRead = 0;

        while (totalRead < length)
        {
            int bytesRead = await stream.ReadAsync(buffer, totalRead, length - totalRead);

            if (bytesRead == 0)
                throw new Exception("Disconnected");

            totalRead += bytesRead;
        }
    }
    static async Task ReceiveLoop()
    {
        try
        {
            while (true)
            {
                await ReceiveMessage();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Disconnected from server: {e.Message}");
        }
    }
    public static async Task Start()
    {
        //re-defines the Cancel program operations so client properly shuts down
        Console.CancelKeyPress += (sender, e) =>
        {
            Console.WriteLine("Ctrl+C pressed, disconnecting...");
            Disconnect();
            e.Cancel = true; // prevent immediate exit so cleanup runs
        };
        Console.Clear();
        Console.WriteLine("Starting Client...");
        ConnectToServer(Player.RName());
        
        _ = ReceiveLoop();
        await Task.Delay(-1);
    }
    static void Disconnect()
    {
        try { stream?.Close(); } catch { }
        try { client?.Close(); } catch { }
        Console.WriteLine("Disconnected from server.");
        Environment.Exit(0);
    }
}