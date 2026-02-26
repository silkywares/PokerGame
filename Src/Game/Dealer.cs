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
    public void TestBoard()
    {
        Board.Clear();
        Board.Add(new Card(Rank.Four, Suit.Diamonds));
        Board.Add(new Card(Rank.Four, Suit.Diamonds));
        Board.Add(new Card(Rank.Four, Suit.Diamonds));
        Board.Add(new Card(Rank.Four, Suit.Diamonds));
        Board.Add(new Card(Rank.Two, Suit.Diamonds));
    }
    public void TestHand(List<Player> players)
    {
        players[0].Hand.Clear();
        players[0].Hand.Add(new Card(Rank.Eight, Suit.Diamonds));
        players[0].Hand.Add(new Card(Rank.Two, Suit.Diamonds));
    }
    public void TestOneMillion(List<Player> players)
    {
       int[] handRankCounts = new int[10]; 

        int simulations = 1_000_000;

        for (int i = 0; i < simulations; i++)
        {
            
            DealPlayerCards(players);
            DealBoardCards();
            DealBoardCards();
            DealBoardCards();

            foreach (var p in players)
            {
                var eval = Evaluator.EvaluatePlayer(p, Board);

                // check for Royal Flush
                if (eval.Rank == 8 && eval.PrimaryValue == 14) // Ace-high straight flush
                {
                    handRankCounts[9]++; // Royal Flush
                }
                else
                {
                    handRankCounts[eval.Rank]++;
                }
            }

            ClearBoard(players);
        }

        // Print results
        Console.WriteLine("Simulation results after 1,000,000 deals:");
        string[] rankNames = { "High Card", "One Pair", "Two Pair", "Three of a Kind", "Straight",
                            "Flush", "Full House", "Four of a Kind", "Straight Flush", "Royal Flush" };

        for (int rank = 0; rank < handRankCounts.Length; rank++)
        {
            Console.WriteLine($"{rankNames[rank]}: {handRankCounts[rank]}");
        }
 
    }
}
