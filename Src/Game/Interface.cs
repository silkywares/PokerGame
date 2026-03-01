namespace PokerGame;

public class Interface
{
    //RoundEngine RoundEngine;
    Table Table;
    int colonOffset = 9;
    int chipOffset = 6;
    int chatOffset = 40;
    
    public Interface(Table table)
    {
        Table = table;
    }
    public void ClearBlock(int top, int height)
    {
        top = Math.Max(0, Math.Min(top, Console.BufferHeight - 1));
        for (int i = 0; i < height; i++)
        {
            int line = top + i;
            if (line >= Console.BufferHeight) break;
            Console.SetCursorPosition(0, line);
            Console.Write(new string(' ', Console.WindowWidth));
        }
    }
     public void PrintTable()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"***{Table.RoundEngine.roundState}*** Pot: {Table.Pot}");
        Console.ForegroundColor = ConsoleColor.Gray;

        //draw board and board cards
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("Board    : ");
        foreach (Card c in Table.Dealer.Board)
            c.PrintCard();

        Console.WriteLine();
        
        //print players
        for (int i = 0; i < Table.Players.Count; i++)
        {
            // Highlight current player directly
            if (i == Table.RoundEngine.turnIndex && !Table.Players[i].IsFolded)
                Console.BackgroundColor = ConsoleColor.DarkMagenta;
            else if (Table.Players[i].IsFolded)
                Console.ForegroundColor = ConsoleColor.DarkGray;
            else
                Console.ForegroundColor = ConsoleColor.Blue;

            Console.Write($"{Table.Players[i].Name}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Blue;
            var cursorPos = Console.GetCursorPosition();
            Console.SetCursorPosition(chipOffset, cursorPos.Top);
            Console.Write($"{Table.Players[i].ChipCount}");
            Console.SetCursorPosition(colonOffset, cursorPos.Top);
            Console.Write(": ");
            foreach (Card c in Table.Players[i].Hand)
                c.PrintCard();
            Console.WriteLine();      
        }

        //print chat
        

        
    }
    public void WaitForSpacebar()
    {
        Console.WriteLine("\nPress SPACE to continue...");
        ConsoleKey key;
        do
        {
            var keyInfo = Console.ReadKey(intercept: true); // intercept: true prevents it from printing
            key = keyInfo.Key;
        } while (key != ConsoleKey.Spacebar);
    }
    public void PrintPlayerOptions(Player player, List<RoundEngine.PlayerActionOption> options)
    {
        Console.WriteLine($"\n--- ACTION FOR {player.Name} ---");
        Console.WriteLine($"CurrentBet: {Table.RoundEngine.CurrentBet} | PlayerBet: {player.CurrentBet} | Chips: {player.ChipCount}");

        //print out options
        for (int i = 0; i < options.Count; i++)
        {
            var o = options[i];
            string raiseRange = o.Action == RoundEngine.PlayerAction.Raise ? $" (Min:{o.MinAmount}, Max:{o.MaxAmount})" : "";
            string callAmount = o.Action == RoundEngine.PlayerAction.Call ? $" {o.Amount}" : "";
            Console.WriteLine($"{i}: {o.Action}{callAmount}{raiseRange}");
        }
    }


    public void PopulateChat()
    {
        Console.SetCursorPosition(chatOffset,1);
        Console.Write("--Chat--");
    }
}