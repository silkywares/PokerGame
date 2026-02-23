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
        List<Player> players = new List<Player> { p1, p2, p3, p4, p5 };
        List<Evaluator.PlayerResult> winners = new List<Evaluator.PlayerResult>();
        Table table = new Table(players);
        //table.Dealer.TestBoard();
        //table.Dealer.TestHand();

        foreach (Player p in players)
            p.Hand.Clear();
        table.Dealer.ClearBoard();

        //table.Dealer.DealBoardCards();
        //table.Dealer.DealBoardCards();
        //table.Dealer.DealBoardCards();
        table.Dealer.DealPlayerCards();

            

        List<Evaluator.PlayerResult> results = Evaluator.EvaluateBoard(players, table.Dealer.Board);
        var winner = Evaluator.EvaluateWinner(results);
        //winners = winner;
        
        table.PrintTable();
        table.RoundEngine.CurrentBet = 20;
        table.RoundEngine.OfferPlayerActions(p1);
    }
}

