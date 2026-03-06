using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Pokergame;

class Client
{
    static TcpClient? client;
    static NetworkStream? stream;
    static string? message;
    static void ConnectToServer(string name)
    {
        client = new TcpClient("127.0.0.1", 5000);
        stream = client.GetStream();
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine("Connected to Server.");

        byte[] nameBytes = Encoding.UTF8.GetBytes(name);
        stream.Write(nameBytes, 0, nameBytes.Length);
    }
    static void SendMessage(string message)
    {
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);
        stream!.Write(messageBytes, 0, messageBytes.Length);
    }
    public static void Start()
    {
        Console.WriteLine("Starting Client...");
        ConnectToServer("Luis");
        var x = 0;
        while (true)
        {
            SendMessage(x.ToString());
            Thread.Sleep(1000);
            x++;
        }
    }

}