using System.Collections;
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
    private Deck deck;
    private List<Player> Players;
    public List<Card> Board { get; private set; }
    public Dealer(List<Player> players)
    {
        deck = NewDeck();
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
            player.AddCard(deck.Deal());
            }
        }
        
    }

    public void DealBoardCard()
    {
        if(Board.Count == 0)
        {
           Board.Add(deck.Deal());
           Board.Add(deck.Deal());
           Board.Add(deck.Deal());
        }
        else if(Board.Count == 3)
            Board.Add(deck.Deal());
        else if(Board.Count == 4)
            Board.Add(deck.Deal());    
        else
            Console.Write("Board full!");
    }

    public void ClearBoard()
    {
        Board.Clear();
        deck = NewDeck();
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
    private void RemoveChips(int chips)
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
    enum RoundState { Preflop,Flop, Turn, River, Reset }
    RoundState roundState;
    public List<Player> Players { get; private set; }
    List<Player> InPlayers { get; set; }
    Dealer Dealer { get; set; }
    public int Pot { get; private set; }
    Player? winner;
    public Table(List<Player> players)
    {
        Players = players;
        InPlayers = new List<Player>(Players);
        roundState = RoundState.Preflop;
        Pot = 0;
        Dealer = new Dealer(Players);
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
        Betting();
        roundState = RoundState.Turn;
    }
    private void Flop()
    {
        Dealer.DealBoardCard();
        Betting();
        roundState = RoundState.River;
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
        foreach (Player p in Players)
            p.ClearHand();
        Dealer.ClearBoard();

        roundState = RoundState.Preflop;
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
        List<Player> players = new List<Player> { p1, p2, p3 };
        
        Table table = new Table(players);
        table.Dealer.DealPlayerCards();
        table.Dealer.DealBoardCard();
        table.Dealer.DealBoardCard();
        table.Dealer.DealBoardCard();

        
        //print
        Console.Clear();

        //print board
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        Console.Write("Board  :  ");
        foreach (Card c in table.Dealer.Board)
        {
            c.PrintCard();
        }
        Console.WriteLine();

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

