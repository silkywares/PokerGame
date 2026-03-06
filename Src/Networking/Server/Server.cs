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
            var client = listener.AcceptTcpClient();
            var stream = client.GetStream();

            
            byte[] buffer = new byte[1024]; // reuse buffer for this client
            
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            string name = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.WriteLine($"{name} connected.");

            try
            {
                while (true) // handle messages from this client
                {
                    bytesRead = stream.Read(buffer, 0, buffer.Length);

                    if (bytesRead == 0) // client disconnected normally
                    {
                        Console.WriteLine($"{name} disconnected normally.");
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($"{name}: ");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine(message);

                    // send response
                    byte[] responseBytes = Encoding.UTF8.GetBytes("Received");
                    stream.Write(responseBytes, 0, responseBytes.Length);
                    
                    if (message.Trim().ToLower() == "end")
                        break; // client wants to end
                }
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;

                Console.WriteLine($"{name} disconnected unexpectedly: " + e.Message);
                Console.ForegroundColor = ConsoleColor.White;

            }
            finally
            {
                // ensure the client is always closed
                Console.ForegroundColor = ConsoleColor.White;
                stream.Close();
                client.Close();
            }
        }
        
    }
}