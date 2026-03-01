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
    
   


}
