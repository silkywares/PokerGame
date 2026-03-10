using System.Net.Sockets;

namespace PokerGame;

public class Player
{
    public String Name;
    public int ChipCount { get; private set; }
    public int CurrentBet { get; set; }
    public int SeatPosition;
    public List<Card> Hand { get; private set; }
    public bool IsFolded { get; set; }
    public bool IsAllIn {get; set;}
    public bool HasActedThisRound {get; set;}
    public PlayerConnection? PlayerConnection;

    public Player(string name, int seat, int chips, PlayerConnection? playerConnection)
    {
        Hand = new List<Card>();
        Name = name;
        SeatPosition = seat;
        ChipCount = chips;

        if(playerConnection != null)
            PlayerConnection = playerConnection;
    }
    

    public void AddCard(Card card)
    {
        Hand.Add(card);
    }
    public void ClearHand()
    {
        Hand.Clear();
    }

    public void AddChips(int chips)
    {
        ChipCount += chips;
    }
    public void RemoveChips(int chips)
    {
        if(chips >= ChipCount)
            ChipCount = 0;
        else
            ChipCount -= chips;
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
    
    public static string RName()
    {
        TestNames[] values = (TestNames[])Enum.GetValues(typeof(TestNames));
        var rand = new Random();
        TestNames name = values[rand.Next(values.Length)];
        return name.ToString();
    }
}