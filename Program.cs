using System.Collections;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;

enum Suit { Hearts, Diamonds, Clubs, Spades }
enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }
class Card
{
    public Suit Suit {get;}
    public Rank Rank {get;}
    public Card(Rank rank, Suit suit)
    {
        Rank = rank;
        Suit = suit;
    }
    public override string ToString()
    {
        string suitSymbol = Suit switch
        {
            Suit.Hearts => "♥",
            Suit.Diamonds => "♦",
            Suit.Clubs => "♣",
            Suit.Spades => "♠",
            _ => "?"
        };

        string RankSymbol = Rank switch
        {
            Rank.Two   => "2",
            Rank.Three => "3",
            Rank.Four  => "4",
            Rank.Five  => "5",
            Rank.Six   => "6",
            Rank.Seven => "7",
            Rank.Eight => "8",
            Rank.Nine  => "9",
            Rank.Ten   => "T",
            Rank.Jack  => "J",
            Rank.Queen => "Q",
            Rank.King  => "K",
            Rank.Ace   => "A",
            _ => "?"
        };

        return $"[{RankSymbol}{suitSymbol}]";
    }
    public void PrintCard()
    {
        switch (Suit)
                {
                case Suit.Hearts:
                case Suit.Diamonds:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.White;
                    break;
                }
        Console.Write(ToString());
        Console.ResetColor();
    
    }
}
class Deck
{
    private List<Card> cards;
    public Deck()
    {
        cards = new List<Card>();
        // populate the deck
        foreach (Suit suit in Enum.GetValues(typeof(Suit)))
        {
            foreach (Rank rank in Enum.GetValues(typeof(Rank)))
            {
                cards.Add(new Card(rank, suit));
            }
        }
    }

    public void Shuffle()
    {
        Random rng = new Random();
        int n = cards.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            var temp = cards[k];
            cards[k] = cards[n];
            cards[n] = temp;
        }
    }
    
    public Card Deal()
    {
        if (cards.Count == 0) return null; // or throw exception
        Card top = cards[0];
        cards.RemoveAt(0);
        return top;
    }
}
class Dealer
{
    private Deck Deck;//change back to private
    private List<Player> Players;
    public List<Card> Board { get; private set; }
    public Dealer(List<Player> players)
    {
        Deck = NewDeck();
        Players = players;
        Board = new List<Card>();
    }
    private Deck NewDeck()
    {
        Deck deck = new Deck();
        deck.Shuffle();
        return deck;
    }
    public void DealPlayerCards()
    {
        for(int i=0; i<2; i++)
        {
            foreach(Player player in Players)
            {
            player.AddCard(Deck.Deal());
            }
        }
    }
    public void DealBoardCard()
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
    public void ClearBoard()
    {
        Board.Clear();
        Deck = NewDeck();
    }
    public void TestBoard()
    {
        Board.Clear();
        Board.Add(new Card(Rank.Four, Suit.Diamonds));
        Board.Add(new Card(Rank.Ace, Suit.Hearts));
        Board.Add(new Card(Rank.Five, Suit.Spades));
        Board.Add(new Card(Rank.Four, Suit.Clubs));
        Board.Add(new Card(Rank.Four, Suit.Diamonds));
    }
    public void TestHand()
    {
        Players[0].Hand.Clear();
        Players[0].Hand.Add(new Card(Rank.Eight, Suit.Diamonds));
        Players[0].Hand.Add(new Card(Rank.Two, Suit.Diamonds));
    }
}
class Evaluator
{
    public class PlayerResult
    {
        public Player Player { get; set; }
        public HandEvaluation Evaluation { get; set; }
        public PlayerResult(Player player)
        {
            Player = player;
            Evaluation = new HandEvaluation();
        }
    }
    public class HandEvaluation
    {
        public Rank Rank { get; set; }      // e.g. StraightFlush, FullHouse, etc.
        public int PrimaryValue { get; set; }   // e.g. rank of the trips in a full house
        public int SecondaryValue { get; set; } // e.g. rank of the pair in a full house
        public List<int> Kickers { get; set; }  // remaining high cards sorted
    }
    public List<PlayerResult> EvaluateBoard(List<Player>players,List<Card>board)
    {
        List<PlayerResult> PlayerResults = new List<PlayerResult>();
        foreach (Player p in players)
        {
            PlayerResult player = new PlayerResult(p);
            PlayerResults.Add(player);
        }
        //checkflush
        //checkstraight
        CheckPair(players, board);
        return PlayerResults;
    }
    void CheckFlush(List<Player>players,List<Card>board)
    {
        
    }
    void CheckStraight(List<Player>players,List<Card>board)
    {
        
    }

    public static void CheckPair(List<Player> players, List<Card> board)
    {
        // Combine player hand + board into one list
        var comp = new List<Card>();
        comp.AddRange(players[0].Hand);
        comp.AddRange(board);

        // Group by rank
        var groups = comp
            .GroupBy(c => c.Rank)
            .Select(g => new { Rank = g.Key, Count = g.Count() })
            .Where(g => g.Count >= 2) // keep only actual pairs/trips/quads/etc
            .OrderByDescending(g => g.Count)
            .ToList();

        // Now you can inspect results:
        foreach (var g in groups)
            Console.WriteLine($"{g.Rank}: {g.Count}");
    }
}
class Player
{
    public String Name;
    public int ChipCount { get; private set; }
    public short SeatPosition;
    public List<Card> Hand { get; private set; }
    public void AddCard(Card card)
    {
        Hand.Add(card);
    }
    public void ClearHand()
    {
        Hand.Clear();
    }
    public Player(string name, short seat)
    {
        Hand = new List<Card>();
        Name = name;
        SeatPosition = seat;
    }
    public bool outflag = true;
    public void AddChips(int chips)
    {
        ChipCount += chips;
    }
    public void RemoveChips(int chips)
    {
        if(chips >= ChipCount)
            ChipCount -= chips;
        else
            ChipCount = 0;
    }
    public void BetChips(int chips)
    {
        
    }
    public void PromptAction()
    {
        //bet
        //call
        //check
        //fold
    }
    
}
class Table
{
    enum RoundState { Preflop, Flop, Turn, River, Reset }
    RoundState roundState;
    public List<Player> Players { get; private set; }
    List<Player> InPlayers { get; set; }
    public Dealer Dealer { get; private set; }
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
    
    private void Roundflow()
    {
        while(Players.Count > 1)
        {
            switch (roundState)
            {
            case RoundState.Preflop:
                Preflop();
                break;
            case RoundState.Flop:
                Flop();
                break;
            case RoundState.Turn:
                Turn();
                break;
            case RoundState.River:
                River();
                break;
            case RoundState.Reset:
                Reset();
                break;
            }
        } 
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
        Dealer.DealBoardCard();
        Betting();
        roundState = RoundState.Turn;
    }
    private void Turn()
    {
        Dealer.DealBoardCard();
        Betting();
        roundState = RoundState.River;
    }
    private void River()
    {
        Dealer.DealBoardCard();
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
}
class Program
{
    static void Main()
    {
        Player p1 = new Player("Asdf",1);
        Player p2 = new Player("Zxcv",2);
        Player p3 = new Player("Qwer",3);
        Player p4 = new Player("Sdfg",4);
        Player p5 = new Player("Tyui",5);

        // (initialize names/chips if you want)
        List<Player> players = new List<Player> { p1 };
        
        Table table = new Table(players);
        // table.Dealer.DealPlayerCards();
        // table.Dealer.DealBoardCard();
        // table.Dealer.DealBoardCard();
        // table.Dealer.DealBoardCard();
        table.Dealer.TestBoard();
        table.Dealer.TestHand();

        //print board
        {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("Board  :  ");
        foreach (Card c in table.Dealer.Board)
        {
            c.PrintCard();
        }
        Console.WriteLine();
        }
        //print players
        foreach (Player p in players)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"{p.SeatPosition}-{p.Name} :  ");
            
            foreach (Card c in p.Hand)
            {
                c.PrintCard();
            }
            Console.WriteLine();
        }
        Evaluator.CheckPair(players,table.Dealer.Board);
        
    }
}

 /*
          __________
         /          \
        |            |
        |            |
        |            |
        |            |        
        |            |
         \__________/


        
     */       

