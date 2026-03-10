namespace PokerGame;

class Program
{
    static async Task Main(string[] args)
    {

        if (args.Contains("server"))
        {
            await Server.Start();
        }
        if (args.Contains("client"))
        {
            await Client.Start();
            return;
        }
        /**/
        Player p1 = new Player("Luis", 1, 100, null);
        Player p2 = new Player("Mom ", 2, 100, null);
        Player p3 = new Player("GMan", 3, 100, null);
        Player p4 = new Player("Monk", 4, 100, null);
        Player p5 = new Player("Sky ", 5, 100, null);
        
        // (initialize names/chips if you want)
        List<Player> players = new List<Player> { p1, p2, p3, p4, p5};
        Table table = new Table(players);
        table.RoundEngine = new RoundEngine(table);

        Console.ResetColor();

        await table.RoundEngine.Roundflow();
    }
}

