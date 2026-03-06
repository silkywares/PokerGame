using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PokerGame;

class Server
{

    public static void Start()
    {
        Console.WriteLine("Starting Server...");
        int port = 5000;

        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();

        Console.WriteLine($"Server listening on port {port}");

        while (true)
        {
            TcpClient client = listener.AcceptTcpClient(); //find someone trying to connect and designates them as listener
            Console.WriteLine("Client connected");

            NetworkStream stream = client.GetStream();

            byte[] message = Encoding.UTF8.GetBytes("Connected to Poker Server\n");
            stream.Write(message, 0, message.Length);

            client.Close();
        }
    }
}