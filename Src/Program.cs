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
        List<Evaluator.PlayerResult> winners = new List<Evaluator.PlayerResult>();
        Table table = new Table(players);
        //table.Dealer.TestBoard();
        //table.Dealer.TestHand();

        bool flag = true;
        int times = 0;
        long dotcount =0;
        while (flag)
        {
            foreach (Player p in players)
                p.Hand.Clear();
            table.Dealer.ClearBoard();

            table.Dealer.DealBoardCards();
            //table.Dealer.DealBoardCards();
            //table.Dealer.DealBoardCards();
            table.Dealer.DealPlayerCards();


            List<Evaluator.PlayerResult> results = Evaluator.EvaluateBoard(players, table.Dealer.Board);
            var winner = Evaluator.EvaluateWinner(results);
            winners = winner;
            if (dotcount > 1000000)
            {
                Console.Write(".");
                dotcount = 0;
            }

            if (winners.Count == 4)
                flag = false;
            times++;
            dotcount++;
        }
        table.PrintTable();
        Evaluator.HandRank rank = (Evaluator.HandRank)winners[0].Evaluation.Rank;
        Console.WriteLine($"Tie took {times:N0} rounds. With {rank}");
        foreach (Evaluator.PlayerResult pr in winners)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{pr.Player.Name}");
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
}

