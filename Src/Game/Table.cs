namespace PokerGame;

public class Table
{
    enum RoundState { Preflop, Flop, Turn, River, Reset }
    RoundState roundState;
    public List<Player> Players { get; private set; }
    List<Player> InPlayers { get; set; }
    public Dealer Dealer { get; private set; }
    public RoundEngine RoundEngine {get; set;}
    public int Pot { get; private set; }
    int SmallBlind = 5;
    int BBPosition;
    Player? winner;
    public Table(List<Player> players)
    {
        Players = players;
        InPlayers = new List<Player>(Players);
        roundState = RoundState.Preflop;
        Pot = 0;
        Dealer = new Dealer(Players);
        RoundEngine = new RoundEngine(Players,5);
        BBPosition = 0;
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
    
    
    private void Preflop()
    {
        Dealer.DealPlayerCards();
        foreach(Player player in Players)
            player.outflag = false;
        
        Players[BBPosition].RemoveChips(SmallBlind*2);
        if(BBPosition == 0)
            Players[Players.Count()].RemoveChips(SmallBlind);
        else
            Players[BBPosition-1].RemoveChips(SmallBlind);
        Betting();
        roundState = RoundState.Flop;
    }
    private void Flop()
    {
        Dealer.DealBoardCards();
        Betting();
        roundState = RoundState.Turn;
    }
    private void Turn()
    {
        Dealer.DealBoardCards();
        Betting();
        roundState = RoundState.River;
    }
    private void River()
    {
        Dealer.DealBoardCards();
        Betting();
        roundState = RoundState.Reset;
    }
    private void Betting()
    {
        //loop for when there are still people betting? this aint right
        if(InPlayers.Count > 1)
        {
            foreach (Player p in InPlayers) //I think this control structure needs to be refactored so that it flows with seat position
            {
                p.PromptAction();

                if (p.outflag)
                {
                    InPlayers.Remove(p);
                }
            }
        }

        if (InPlayers.Count == 1)
        {
            winner = InPlayers[0];
            Reset();
        }
    }
    private void Reset()
    {
        //pay winner and reset pot
        if(winner != null)
            winner.AddChips(Pot);
        Pot = 0;
        winner = null;
        //clear hands and board
        
        if (BBPosition++ == Players.Count())
            BBPosition = 0;
        foreach (Player p in Players)
            p.ClearHand();
        Dealer.ClearBoard();

        roundState = RoundState.Preflop;
    }
    public void PrintTable()
    {
        Console.Clear();
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
            Console.Write($"{p.SeatPosition}-{p.Name} :  ");
            
            foreach (Card c in p.Hand)
            {
                c.PrintCard();
            }
            Console.WriteLine();
        }
    }
}
