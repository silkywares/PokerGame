namespace PokerGame;

public class Dealer
{
    
    private Deck Deck;//change back to private
    public List<Card> Board { get; private set; }
    public Dealer()
    {
        Deck = NewDeck();
        Board = new List<Card>();
    }
    private Deck NewDeck()
    {
        Deck deck = new Deck();
        deck.Shuffle();
        return deck;
    }
    public void DealPlayerCards(List<Player> players)
    {
        for(int i=0; i<2; i++)
        {
            foreach(Player p in players)
            {
            p.AddCard(Deck.Deal());
            }
        }
    }
    public void DealBoardCards()
    {
        if(Board.Count == 0)
        {
           Board.Add(Deck.Deal());
           Board.Add(Deck.Deal());
           Board.Add(Deck.Deal());
        }
        else if(Board.Count == 3)
            Board.Add(Deck.Deal());
        else if(Board.Count == 4)
            Board.Add(Deck.Deal());    
        else
            Console.Write("Board full!");
    }
    public void ClearBoard(List<Player> players)
    {
        Board.Clear();
        foreach (Player p in players)
            p.Hand.Clear();
        Deck = NewDeck();
    }

}
