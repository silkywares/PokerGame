namespace PokerGame;

public class RoundEngine
{
    public enum RoundState { Preflop, Flop, Turn, River, Reset, Showdown}
    public enum PlayerAction  {Call, Raise, Fold, Check, AllIn};

    public int turnIndex{get; set;} // current actioning player
    private int firstToAct;//first player to act in betting round
    private readonly Table Table;
    public RoundState roundState;
    public int CurrentBet { get; set; } // isnt private for easy testing purposes
    public int LastRaiseAmount { get; set; } // size of last raise

    public RoundEngine(Table table)
    {
        roundState = RoundState.Preflop;
        Table = table;
    }
    
    public class PlayerActionOption
    {
        public PlayerAction Action;
        public int Amount;    // for Call
        public int MinAmount; // for Raise/Bet
        public int MaxAmount; // for Raise/Bet

    }
    public List<PlayerActionOption> GetRoundActions(Player player)
    {
        //this function checks the game state and assembles a list of valid actions a user can take
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
                minRaise = Table.BigBlind;
            else
                minRaise = Math.Max(LastRaiseAmount, Table.BigBlind);

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
        player.IsFolded = true;
        player.CurrentBet = 0;
    }
    public void PlayerCall(Player player)
    {
        int toCall = CurrentBet - player.CurrentBet;
        if(toCall <= 0) return; // nothing to call

        int chipsToPay = Math.Min(player.ChipCount, toCall); // handle short stack
        player.RemoveChips(chipsToPay);
        Table.Pot += chipsToPay;
        player.CurrentBet += chipsToPay;

        if(player.ChipCount == 0)
            player.IsAllIn = true;
    }
    public void PlayerRaise(Player player, int raiseAmount)
    {
        
        int toCall = CurrentBet - player.CurrentBet;
        int totalAmount = toCall + raiseAmount;

        if (raiseAmount <= 0) return;
        if(totalAmount >= player.ChipCount)
        {
            // Treat as all-in if raise exceeds chips
            totalAmount = player.ChipCount;
            player.IsAllIn = true;
        }

        player.RemoveChips(totalAmount);
        player.CurrentBet += totalAmount;
        Table.Pot += totalAmount;

        // Update CurrentBet for the table
        CurrentBet = Math.Max(CurrentBet, player.CurrentBet);

        // Update LastRaiseAmount (difference above previous bet)
        LastRaiseAmount = Math.Max(LastRaiseAmount, totalAmount - toCall);
    }
    
    private List<Player> GetActivePlayers()
    {
        return Table.Players.Where(p => !p.IsFolded && !p.IsAllIn).ToList();
    }
    private int GetFirstToAct(RoundState street)
    {
        //this function determines the first acting player for each specific round of betting
        int playerCount = Table.Players.Count;

        switch (street)
        {
            case RoundState.Preflop:
                // Preflop → first to act is player **after big blind**
                int bbPos = (Table.ButtonPosition + 2) % playerCount;
                return (bbPos + 1) % playerCount;

            case RoundState.Flop:
            case RoundState.Turn:
            case RoundState.River:
                // Postflop → first to act is player **after button**
                return (Table.ButtonPosition + 1) % playerCount;

            default:
                throw new InvalidOperationException("Invalid street for first-to-act computation");
        }
    }
    public bool AllBetsEqual()
    {
        var active = GetActivePlayers();
        int bet = active[0].CurrentBet;
        return active.All(p => p.CurrentBet == bet); // this LINQ call returns true if predicate is true for all list memebers (all p.currentBet == bet)
    }
    public void PostBlinds()
    {
        //get blind positions
        var SBPosition = (Table.ButtonPosition + 1) % Table.Players.Count; // 1
        var BBPosition = (Table.ButtonPosition + 2) % Table.Players.Count; // 2 
        
        //assign SB & BB to the player at that index
        var sbPlayer = Table.Players[SBPosition];
        var bbPlayer = Table.Players[BBPosition];

        //take the blinds
        PlayerRaise(sbPlayer, Table.SmallBlind);
        PlayerRaise(bbPlayer, Table.BigBlind);

        //Ensure proper minbet logic right after the blinds
        LastRaiseAmount = Table.BigBlind;
        CurrentBet = Table.BigBlind;
    }
    
    private void ResetStreetBets()
    {
        CurrentBet = 0;
        LastRaiseAmount = 0;

        foreach (var p in Table.Players)
            p.CurrentBet = 0;
    }
    private void StartBettingRound(RoundState street)
    {

        firstToAct = GetFirstToAct(street); // computes index based on button/BB
        turnIndex = firstToAct; 

        while(true)
        {
            var active = GetActivePlayers();
            if (active.Count == 1)
            {
                roundState = RoundState.Showdown;
                return;// all else folded
            }
            if (AllBetsEqual()) 
            {
                break; // normal end of betting round
            }

            var currentPlayer = active[turnIndex % active.Count];
            //OfferPlayerActions(currentPlayer);
            ProcessRandomAction(currentPlayer);
            turnIndex++;
        }
    }
    public void AllocatePot()
    {
        var active = GetActivePlayers();
        if(active.Count == 0) return;
        if(active.Count == 1)// skip eval if just one person left
        {
            active[0].AddChips(Table.Pot);
            Table.Pot = 0;
            return;
        }
        var results = Evaluator.EvaluateBoard(active, Table.Dealer.Board);
        var winners = Evaluator.EvaluateWinner(results);
        if (winners.Count == 0)
        {
            Console.WriteLine("No winners detected! Pot remains: " + Table.Pot);
            return; // prevent divide by zero
        }
        int share = Table.Pot / winners.Count;

        foreach (Evaluator.PlayerResult pr in winners)//add the chips to winners
            pr.Player.AddChips(share);
    }  
    public void Reset()
    {
        Table.Dealer.ClearBoard(Table.Players);
        Table.ButtonPosition = (Table.ButtonPosition + 1) % Table.Players.Count; // rotate button
        Table.Pot = 0;
        CurrentBet = 0;
        LastRaiseAmount = 0;

        foreach(var p in Table.Players)
        {
        p.CurrentBet = 0;
        p.IsFolded = false;
        p.IsAllIn = false;
        }
    }

    public void Roundflow()
    {
        while(true)
        {

            switch (roundState)
            {
            case RoundState.Preflop:
                Table.Dealer.DealPlayerCards(Table.Players);
                ResetStreetBets();
                PostBlinds();
                Console.WriteLine($"{roundState} {Table.Pot}");
                StartBettingRound(RoundState.Preflop);
                roundState = RoundState.Flop;
                break;
            case RoundState.Flop:
                Table.Dealer.DealBoardCards();
                Console.WriteLine($"{roundState} {Table.Pot}");
                ResetStreetBets();
                StartBettingRound(RoundState.Flop);
                roundState = RoundState.Turn;
                break;
            case RoundState.Turn:
                Table.Dealer.DealBoardCards();
                Console.WriteLine($"{roundState} {Table.Pot}");
                ResetStreetBets();
                StartBettingRound(RoundState.Turn);
                roundState = RoundState.River;
                break;
            case RoundState.River:
                Table.Dealer.DealBoardCards();
                Console.WriteLine($"{roundState} {Table.Pot}");
                ResetStreetBets();
                StartBettingRound(RoundState.River);
                roundState = RoundState.Showdown;
                break;
            case RoundState.Showdown:
                Console.WriteLine($"{roundState} {Table.Pot}");
                AllocatePot();
                roundState = RoundState.Reset;
                break;
            case RoundState.Reset:
                Console.WriteLine($"{roundState} {Table.Pot}");
                Reset();
                roundState = RoundState.Preflop;
                break;
            }
        } 
    }
    
    private void ProcessRandomAction(Player player)
    {
        var options = GetRoundActions(player);
        if (options.Count == 0) return;

        var rnd = new Random();
        var choice = options[rnd.Next(options.Count)];

        switch (choice.Action)
        {
            case PlayerAction.Fold:
                PlayerFold(player);
                Console.WriteLine($"{player.Name} folds pc{player.ChipCount}");
                break;

            case PlayerAction.Call: // includes check if toCall = 0
                PlayerCall(player);
                if (CurrentBet == player.CurrentBet)
                    Console.WriteLine($"{player.Name} calls {choice.Amount}  pc{player.ChipCount} !{Table.Pot}");
                else
                    Console.WriteLine($"{player.Name} checks pc{player.ChipCount} {Table.Pot} ");
                break;

            case PlayerAction.Raise: // includes all-in if player doesn't have enough chips
                // Prevent invalid raise windows
                int min = Math.Max(choice.MinAmount, 1);  // a raise must be > 0
                int max = choice.MaxAmount;

                if (min > max)
                {
                    // Player can’t raise → fallback to call or check
                    PlayerCall(player);
                    Console.WriteLine($"{player.Name} calls instead (raise impossible)");
                    break;
                }

                // Pick a real raise amount (never zero)
                int raiseAmount = rnd.Next(min, max + 1);

                PlayerRaise(player, raiseAmount);
                Console.WriteLine($"{player.Name} raises {raiseAmount} pc{player.ChipCount} {Table.Pot}");
                break;
        }

        Thread.Sleep(1500); // delay so you can watch step by step
    }
}