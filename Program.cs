using System.Collections;
using System.Drawing;
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
    public List<Player> Players;
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
        foreach (Player p in Players)
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
        public int Rank { get; set; }      // e.g. StraightFlush, FullHouse, etc.
        public int PrimaryValue { get; set; }   // e.g. rank of the trips in a full house
        public int SecondaryValue { get; set; } // e.g. rank of the pair in a full house
        public int Kicker { get; set; }  // remaining high cards sorted
    }
    public static List<PlayerResult> EvaluateBoard(List<Player> players, List<Card> board)
    {
        var results = new List<PlayerResult>();

        foreach (var p in players)
        {
            var evaluation = EvaluatePlayer(p, board);
            results.Add(new PlayerResult(p) { Evaluation = evaluation });
            //Console.WriteLine($"{p.Name}-{evaluation.Rank}");
        }

        return results;
    }
    public static HandEvaluation EvaluatePlayer(Player p, List<Card> board)
    {
        var cards = p.Hand.Concat(board).ToList();

        var rankGroups = cards.GroupBy(c => c.Rank)
                            .OrderByDescending(g => g.Count())
                            .ThenByDescending(g => g.Key)
                            .ToList();

        var suitGroups = cards.GroupBy(c => c.Suit)
                            .ToList();
        var kicker = 0;


        if (IsStraightFlush(suitGroups, out var sfHigh, out kicker))
            return new HandEvaluation { Rank = 8, PrimaryValue = sfHigh };

        if (IsFourOfAKind(rankGroups, out var quad, out kicker))
            return new HandEvaluation { Rank = 7, PrimaryValue = quad, Kicker = kicker};

        if (IsFullHouse(rankGroups, out var trips, out var pair, out kicker))
            return new HandEvaluation { Rank = 6, PrimaryValue = trips, SecondaryValue = pair };

        if (IsFlush(suitGroups, out var flushHigh))
            return new HandEvaluation { Rank = 5, PrimaryValue = flushHigh };

        if (IsStraight(cards, out var straightHigh, out kicker))
            return new HandEvaluation { Rank = 4, PrimaryValue = straightHigh };

        if (IsThreeOfAKind(rankGroups, out var tripsRank, out kicker))
            return new HandEvaluation { Rank = 3, PrimaryValue = tripsRank };

        if (IsTwoPair(rankGroups, out var highPair, out var lowPair, out kicker))
            return new HandEvaluation { Rank = 2, PrimaryValue = highPair, SecondaryValue = lowPair };

        if (IsOnePair(rankGroups, out var pairRank, out kicker))
            return new HandEvaluation { Rank = 1, PrimaryValue = pairRank };

        HighCard(cards, out int highCard, out kicker);
        return new HandEvaluation 
        { 
            Rank = 0, // 0 = High Card
            PrimaryValue = highCard, 
            Kicker = kicker
        };
    }
    private static bool IsFourOfAKind(List<IGrouping<Rank, Card>> rankGroups, out int quadRank, out int kicker)
    {
        quadRank = 0;
        kicker = 0;
        var quad = rankGroups.FirstOrDefault(g => g.Count() == 4);
        if (quad == null)
            return false;

        quadRank = (int)quad.Key;

        //TODO inspect this for accuracy
        kicker = rankGroups
            .Where(g => g.Count() != 4)
            .Max(g => (int)g.Key);

        return true;
    }
    private static bool IsStraightFlush(List<IGrouping<Suit, Card>> suitGroups, out int high, out int kicker)
    {
        high = 0;
        kicker = 0;
        //TODO this aint done at all
        foreach (var group in suitGroups)
        {
            if (IsStraight(group.ToList(), out high, out kicker))
                return true;
        }
        return false;
    }
    private static bool IsStraight(List<Card> cards, out int high, out int kicker)
    {

        var ranks = cards.Select(c => (int)c.Rank) .Distinct() .OrderBy(r => r) .ToList(); 
        kicker = 0;
        high = 0;

        if (ranks.Contains(14))
            ranks.Insert(0, 1);// add a 1 if Ace 

        for (int i = 0; i <= ranks.Count - 5; i++) 
        { 
            if (ranks[i + 4] - ranks[i] == 4) 
            { 
                high = ranks[i+4]; 
                //TODO assign kicker
                return true;
            } 
        }
        return false;     
    }
    private static bool IsFullHouse(List<IGrouping<Rank, Card>> rankGroups, out int trips, out int pair, out int kicker)
    {
        trips = 0;
        pair = 0;
        kicker = 0;
        var tripRank = rankGroups.FirstOrDefault(g => g.Count() == 3);
        var pairRank = rankGroups.FirstOrDefault( g => g.Count() ==2);
        if(tripRank != null && pairRank != null)
        {
            trips = (int)tripRank.Key;
            pair = (int)pairRank.Key;
            //TODO assign kicker
            return true;
        }
        return false;
    }
    private static bool IsFlush(List<IGrouping<Suit, Card>> suitGroups, out int high)
    {
        high = 0;
        var largestGroup = suitGroups.OrderByDescending(g => g.Count()).FirstOrDefault()!;
        if( largestGroup.Count() >= 5)
        {
            high = largestGroup.Max(c => (int)c.Rank);
            //TODO assign kicker
            return true;
        }
        return false;
    }
    private static bool IsThreeOfAKind(List<IGrouping<Rank, Card>> rankGroups, out int tripRank , out int kicker)
    {
        kicker = 0;
        tripRank = 0;
        var trips = rankGroups.FirstOrDefault(g => g.Count() == 3);
        if (trips == null)
            return false;

        tripRank = (int)trips.Key;

        //TODO inspect this for accuracy
        kicker = rankGroups
            .Where(g => g.Count() != 3)
            .Max(g => (int)g.Key);

        return true;

    }
    private static bool IsTwoPair(List<IGrouping<Rank,Card>> rankGroups, out int highPair, out int lowPair, out int kicker)
    {
        highPair = 0;
        lowPair = 0;
        kicker = 0;

        var pairGroups = rankGroups.Where(g => g.Count() == 2).ToList();

        if (pairGroups.Count >= 2)
        {
            highPair = (int)pairGroups[0].Key;
            lowPair = (int)pairGroups[1].Key;
            return true;
        }
        //TODO kicker logic
        return false;
    }
    private static bool IsOnePair(List<IGrouping<Rank, Card>> rankGroups, out int pairRank , out int kicker)
    {
        kicker = 0;
        pairRank = 0;
        var pair = rankGroups.FirstOrDefault(g => g.Count() == 2);
        if (pair == null)
            return false;

        pairRank = (int)pair.Key;

        //TODO inspect this for accuracy
        kicker = rankGroups
            .Where(g => g.Count() != 2)
            .Max(g => (int)g.Key);
        return true;
    }    
    private static bool HighCard(List<Card>cards, out int highCard, out int kicker)
    {
        kicker = 0;
        highCard = (int)cards[0].Rank;
        kicker   = (int)cards[1].Rank;
        return true;
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
        Player p1 = new Player("Luis",1);
        Player p2 = new Player("Arny",2);
        Player p3 = new Player("GMan",3);
        Player p4 = new Player("Monkers",4);
        Player p5 = new Player("Nigamus",5);

        // (initialize names/chips if you want)
        List<Player> players = new List<Player> { p1, p2, p3 };
        
        Table table = new Table(players);
        //table.Dealer.TestBoard();
        //table.Dealer.TestHand();
        


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
        /*print players
        foreach (Player p in players)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"{p.SeatPosition}-{p.Name} :  ");
            
            foreach (Card c in p.Hand)
            {
                c.PrintCard();
            }
            Console.WriteLine();
        }*/
        //Evaluator.EvaluateBoard(players,table.Dealer.Board);
    

    // Initialize counters for each hand rank (0 = High Card, 1 = One Pair, ..., 8 = Straight Flush)
    // Initialize counters for each hand rank
// 0 = High Card, 1 = One Pair, ..., 8 = Straight Flush, 9 = Royal Flush
int[] handRankCounts = new int[10]; 

int simulations = 1_000_000;

for (int i = 0; i < simulations; i++)
{
    
    table.Dealer.DealPlayerCards();
    table.Dealer.DealBoardCard();
    table.Dealer.DealBoardCard();
    table.Dealer.DealBoardCard();

    foreach (var player in table.Players)
    {
        var eval = Evaluator.EvaluatePlayer(player, table.Dealer.Board);

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

    table.Dealer.ClearBoard();
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

