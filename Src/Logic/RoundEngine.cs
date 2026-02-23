namespace PokerGame;

public class RoundEngine
{
    public enum RoundState { Preflop, Flop, Turn, River, Reset }
    public enum PlayerAction  {Call, Raise, Fold, Check, AllIn};
    RoundState roundState;
    List<Player>? Players= new List<Player>();
    List<Player>? Winners= new List<Player>();
    Player[] seats = new Player[6]; // 6 seats at the table
    bool[] folded = new bool[6]; 
    public int Pot;
    public int CurrentBet;
    public int LastRaiseAmount { get; set; } // size of last raise
    int SB;
    int BB;

    public RoundEngine(List<Player>? players, int smallBlind)
    {
        roundState = RoundState.Preflop;
        SB = smallBlind;
        BB = SB*2;
        Pot = 0;

        if(players != null)
            for (int i = 0; i < players.Count; i++)
            {
                seats[i] = players[i]; 
                seats[i].SeatPosition = i;
            }
    }

    public class PlayerActionOption
    {
        
        public PlayerAction Action;
        public int MinAmount; // for Raise/Bet
        public int MaxAmount; // for Raise/Bet
        public int Amount;    // for Call

    }
    public List<PlayerActionOption> GetRoundActions(Player player)
    {
        List<PlayerActionOption> actions = new List<PlayerActionOption>();
        int toCall = CurrentBet - player.CurrentBet;

        //fold
        actions.Add(new PlayerActionOption{Action = PlayerAction.Fold});
        //check
        if(toCall == 0)
            actions.Add(new PlayerActionOption{Action = PlayerAction.Check}); 
        //call 
        if(player.ChipCount >= toCall)
            actions.Add(new PlayerActionOption{Action = PlayerAction.Call, Amount = CurrentBet-player.CurrentBet});
        //raise
        if (player.ChipCount > toCall)
        {
            int minRaise;
            // First bet of the round → minRaise = BigBlind
            if (CurrentBet == 0)
                minRaise = BB;
            else
                minRaise = LastRaiseAmount; // track last raise difference

            // Ensure minRaise is not more than player chips
            if (minRaise <= player.ChipCount)
            {
                int maxRaise = player.ChipCount; // all-in is max
                actions.Add(new PlayerActionOption
                {
                    Action = PlayerAction.Raise,
                    MinAmount = minRaise,
                    MaxAmount = maxRaise
                });
            }
        }

        return actions;
    }
    public void OfferPlayerActions(Player player)
    {
        List<PlayerActionOption> actionOption = GetRoundActions(player);
        foreach(PlayerActionOption act in actionOption)
            Console.WriteLine($"{player.Name}-{act.Action}");
    }
    public void PlayerFold(Player player)
    {
        folded[player.SeatPosition] = true;
        
    }
    public void PlayerCall(Player player)
    {
        player.RemoveChips(CurrentBet);
    }
    public void PlayerRaise(Player player, int chips)
    {
        Pot += chips;
        player.RemoveChips(chips);
        CurrentBet = chips;

    }
    public void PlayerCheck(Player player){

    }



    

    private void Roundflow()
    {
        while(Players.Count > 1)
        {
            switch (roundState)
            {
            case RoundState.Preflop:
                //Preflop();
                break;
            case RoundState.Flop:
                //Flop();
                break;
            case RoundState.Turn:
                //Turn();
                break;
            case RoundState.River:
                //River();
                break;
            case RoundState.Reset:
                //Reset();
                break;
            }
        } 
    }
    
}