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

    public Table(List<Player>? players)
    {
        Players = new List<Player>();
        Pot = 0;
        Dealer = new Dealer();
        RoundEngine = new RoundEngine(this);
        ButtonPosition = 0;
        SmallBlind = 2;
        if(players != null)
            Players = players;
    }
    public void AddPlayer(Player p)
    {
        if(Players.Count < 6)
        {
            Players.Add(p);
        }
        else
            Console.Write("Table full");
    }
    public void RemovePlayer(Player p)
    {
        Players.Remove(p);
    }

    public void TestTable()
    {
        //Player p1 = new Player("Luis", 1, 100, null);
        //Player p2 = new Player("Mom ", 2, 100, null);
        //AddPlayer(p1);
        //AddPlayer(p2);
        //Dealer.DealPlayerCards(Players);
        Dealer.DealBoardCards();
        Dealer.DealBoardCards();
        Dealer.DealBoardCards();
    }
}
