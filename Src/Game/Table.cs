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
    public Table(List<Player> players)
    {
        Players = players;
        Pot = 0;
        Dealer = new Dealer();
        RoundEngine = new RoundEngine(this);
        ButtonPosition = 0;
        SmallBlind = 2;
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
    
    public void ShowColoredPlayerOrder(int pos)
    {
        if(Players.Count >= pos)//check highlighted index is within player size
        {
            int highlightPos = pos*3-3;
            Console.Write("p1 p2 p3 p4 p5 p6");
            var cursorPos = Console.GetCursorPosition();
            Console.SetCursorPosition(cursorPos.Left-17+highlightPos, cursorPos.Top);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.DarkMagenta;
            Console.Write($"p{pos}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.SetCursorPosition(0, cursorPos.Top+1);  
        }
        
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

        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("Board  :  ");
        foreach (Card c in Dealer.Board)
        {
            c.PrintCard();
        }
        Console.WriteLine();
        
        //print players
        foreach (Player p in Players)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"{p.Name}({p.ChipCount}) :  ");
            
            foreach (Card c in p.Hand)
            {
                c.PrintCard();
            }
            Console.WriteLine();
        }
        
        

        //var currentplayer = RoundEngine.turnIndex;
        //ShowColoredPlayerOrder(RoundEngine.turnIndex);
    }
}
