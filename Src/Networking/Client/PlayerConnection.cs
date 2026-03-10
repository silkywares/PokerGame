using System.Net;
using System.Net.Sockets;
using System.Text;

namespace PokerGame;

public class PlayerConnection
{ 
    public TcpClient client;
    public NetworkStream stream;

    public PlayerConnection(TcpClient client)
    {
        this.client = client;
        stream = client.GetStream();
    }
    public async Task<string> Receive()
    {
        byte[] buffer = new byte[1024];
        int bytesRead = await stream.ReadAsync(buffer);
        return Encoding.UTF8.GetString(buffer,0,bytesRead);
    }

    public async Task Send(string message)
    {
        byte[] data = Encoding.UTF8.GetBytes(message);
        await stream.WriteAsync(data);
    }

}