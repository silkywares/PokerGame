namespace PokerGame;

public class Table
{
    public RoundEngine RoundEngine {get; set;}
    public List<Player> Players { get; private set; }
    public Dealer Dealer { get; private set; }
    public int Pot { get; set; }
    public int ButtonPosition{get; set;}
    public int SmallBlind { get; }
    public int BigBlind => SmallBlind * 2;

    public int playerLine;
    public Table(List<Player> players)
    {
        Players = players;
        Pot = 0;
        Dealer = new Dealer();
        RoundEngine = new RoundEngine(this);
        ButtonPosition = 0;
        SmallBlind = 2;
        playerLine = Players.Count +2;
    }
    void AddPlayer(Player p)
    {
        if(Players.Count < 6)
        {
            Players.Add(p);
        }
        else
            Console.Write("Table full");
    }
    
    public void UpdatePot()
    {
        Console.SetCursorPosition(8, 5);
    }
    public void PrintTable()
    {


        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.WriteLine($"***{RoundEngine.roundState}*** Pot: {Pot}");
        Console.ForegroundColor = ConsoleColor.Gray;

        //draw board and board cards
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("Board     :  ");
        foreach (Card c in Dealer.Board)
            c.PrintCard();

        Console.WriteLine();
        
        //print players
        for (int i = 0; i < Players.Count; i++)
        {
            // Highlight current player directly
            if (i == RoundEngine.turnIndex && !Players[i].IsFolded)
                Console.BackgroundColor = ConsoleColor.DarkMagenta;
            else if (Players[i].IsFolded)
                Console.ForegroundColor = ConsoleColor.DarkGray;
            else
                Console.ForegroundColor = ConsoleColor.Blue;

            Console.Write($"{Players[i].Name}");
            Console.ResetColor();
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"({Players[i].ChipCount}) :  ");
            foreach (Card c in Players[i].Hand)
                c.PrintCard();
            Console.WriteLine();      
        }
        
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
}
