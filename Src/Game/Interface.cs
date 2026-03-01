namespace PokerGame;

public class Interface
{
    //RoundEngine RoundEngine;
    Table Table;
    
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
        Console.Write("Board     :  ");
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
            Console.Write($"({Table.Players[i].ChipCount}) :  ");
            foreach (Card c in Table.Players[i].Hand)
                c.PrintCard();
            Console.WriteLine();      
        }
        
    }


}