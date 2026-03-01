using System.Collections;
using System.Drawing;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace PokerGame;

class Program
{
    static void Main()
    {
        Player p1 = new Player("Luis", 1, 100);
        Player p2 = new Player("Mom ", 2, 100);
        Player p3 = new Player("GMan", 3, 100);
        Player p4 = new Player("Monk", 4, 100);
        Player p5 = new Player("Sky ", 5, 100);

        // (initialize names/chips if you want)
        List<Player> players = new List<Player> { p1, p2, p3, p4, p5};
        Table table = new Table(players);
        table.RoundEngine = new RoundEngine(table);

        table.RoundEngine.Roundflow();

    }
}

