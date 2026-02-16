using System.Collections;
using System.Drawing;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

namespace PokerGame;

class Program
{
    static void Main()
    {
        Player p1 = new Player("Luis", 1);
        Player p2 = new Player("Mom ", 2);
        Player p3 = new Player("GMan", 3);
        Player p4 = new Player("Monk", 4);
        Player p5 = new Player("Sky ", 5);

        // (initialize names/chips if you want)
        List<Player> players = new List<Player> { p1, p2, p3, p4, p5 };

        Table table = new Table(players);
        //table.Dealer.TestBoard();
        //table.Dealer.TestHand();
        bool flag = true;
        int times = 0;
        while (flag)
        {
            foreach (Player p in players)
                p.Hand.Clear();
            table.Dealer.ClearBoard();

            table.Dealer.DealBoardCards();
            table.Dealer.DealBoardCards();
            table.Dealer.DealBoardCards();
            table.Dealer.DealPlayerCards();


            List<Evaluator.PlayerResult> results = Evaluator.EvaluateBoard(players, table.Dealer.Board);
            var winner = Evaluator.EvaluateWinner(results);


            if (winner[0].Player.Name == "Mom " && winner[0].Evaluation.Rank == 8 && winner[0].Evaluation.PrimaryValue == 14)
                flag = false;
            times++;
        }
        table.PrintTable();
        Console.WriteLine($"Royal flush took {times} rounds.");

    }
}

