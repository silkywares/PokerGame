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
    public void AddCard(Card card)
    {
        Hand.Add(card);
    }
    public void ClearHand()
    {
        Hand.Clear();
    }
    public Player(string name, int seat, int chips)
    {
        Hand = new List<Card>();
        Name = name;
        SeatPosition = seat;
        ChipCount = chips;
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
    
}
