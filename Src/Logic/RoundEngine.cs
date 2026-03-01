namespace PokerGame;

public class RoundEngine
{
    public enum RoundState { Preflop, Flop, Turn, River, Reset, Showdown, CannotStart}
    public enum PlayerAction  {Call, Raise, Fold, Check, AllIn};

    public int turnIndex{get; set;} // current actioning player
    private int firstToAct;//first player to act in betting round
    private readonly Table Table;
    public RoundState roundState;
    public int CurrentBet { get; set; } // isnt private for easy testing purposes
    public int LastRaiseAmount { get; set; } // size of last raise

    public RoundEngine(Table table)
    {
        roundState = RoundState.CannotStart;

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
        return Table.Players.Where(p => !p.IsFolded && !p.IsAllIn && p.ChipCount > Table.SmallBlind).ToList();
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
        // Only consider players who haven't folded or are all-in
        int targetBet = CurrentBet;
        return active.All(p => p.CurrentBet == targetBet || p.IsAllIn);
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
        sbPlayer.RemoveChips(Table.SmallBlind);
        bbPlayer.RemoveChips(Table.BigBlind);
        Table.Pot += Table.BigBlind+Table.SmallBlind;

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"Small: {sbPlayer.Name} posts {Table.SmallBlind} POT:{Table.Pot}");
        Console.WriteLine($"Big  : {bbPlayer.Name} posts {Table.BigBlind} POT:{Table.Pot}");
        Console.ForegroundColor = ConsoleColor.Gray;

        //Ensure proper minbet logic right after the blinds
        LastRaiseAmount = Table.BigBlind;
        CurrentBet = Table.BigBlind;
    }
    
    private void StartBettingRound(RoundState street)
    {

        Console.WriteLine($"Begin {street} betting round");
        // Reset who has acted
        foreach (var p in Table.Players)
            p.HasActedThisRound = false;

        // Determine first to act
        firstToAct = GetFirstToAct(street); // preflop: UTG after BB, postflop: left of button
        turnIndex = firstToAct;

        while (true)
        {
            var active = GetActivePlayers();

            // Only one player left -> showdown
            if (active.Count == 1)
            {
                Console.WriteLine("Only one active player remaining, going to showdown");
                roundState = RoundState.Showdown;
                return;
            }

            var currentPlayer = Table.Players[turnIndex];

            // Skip folded or all-in players
            if (!currentPlayer.IsFolded && !currentPlayer.IsAllIn)
            {   
                Table.PrintTable();
                ProcessManualAction(currentPlayer);
                currentPlayer.HasActedThisRound = true;
            }

            // Check if betting round is finished:
            // 1. All active players’ bets equal CurrentBet or they are all-in
            // 2. Everyone has acted at least once since last raise
            if (active.All(p => p.CurrentBet == CurrentBet || p.IsAllIn) &&
                active.All(p => p.HasActedThisRound || p.IsAllIn))
            {
                break; // end of betting round
            }

            // Move to next active player
            turnIndex = GetNextActivePlayerIndex(turnIndex);
        }
        var pos = Console.GetCursorPosition();
        Console.WriteLine($"End of {street}.");
        Table.ClearBlock(7,10);
        
        Console.SetCursorPosition(pos.Left, pos.Top);
    }
    private void ResetStreetBets()
    {
        CurrentBet = 0;
        LastRaiseAmount = 0;

        foreach (var p in Table.Players)
        {
            p.CurrentBet = 0;
            p.HasActedThisRound = false;
        }
    }    
    private int GetNextActivePlayerIndex(int idx)
    {
        int count = Table.Players.Count;
        do {
            idx = (idx + 1) % count;
        }
        while (Table.Players[idx].IsFolded || Table.Players[idx].IsAllIn);

        return idx;
    }
    public void AllocatePot()
    {
        Console.WriteLine("Allocating Pot");
        var active = GetActivePlayers();
        if(active.Count == 0) return;
        if(active.Count == 1)// skip eval if just one person left
        {
            Console.WriteLine($"{active[0].Name} wins {Table.Pot} chips. Last standing");
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

        Console.WriteLine($"Pot: {Table.Pot} Winner(s) are ");

        foreach (Evaluator.PlayerResult pr in winners){//add the chips to winners
            Console.Write($"{pr.Player.Name} wins with {pr.Evaluation.Rank}! ");
            Thread.Sleep(10000);
            pr.Player.AddChips(share);
        }
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
        //check if round can start
        if(GetActivePlayers().Count < 2)
            roundState = RoundState.CannotStart;
    }
    public bool CannotStart()
    {
        string[] dots = { ".  ", ".. ", "..." };
        int index = 0;

        var minChipsToPlaceBlinds = Table.Players.Where(p => p.ChipCount >= Table.SmallBlind);
        while (true)
        {
            int playersWithChips = Table.Players.Count(p => p.ChipCount >= Table.SmallBlind);

            if (Table.Players.Count >= 2 && playersWithChips >= 2)
                return false; // Game can start

            Console.Write($"\rWaiting for enough players to continue{dots[index]}");
            index = (index + 1) % dots.Length;
            Thread.Sleep(700);
        }
    }
    public void Roundflow()
    {
        int x = 0;
        while(x < 10)
        {
            switch (roundState)
            {
            case RoundState.CannotStart:
                Console.WriteLine($"{roundState}");
                Table.PrintTable();
                CannotStart();
                roundState = RoundState.Preflop;
                Thread.Sleep(1000);
                break; 
            case RoundState.Preflop:
                Table.Dealer.DealPlayerCards(GetActivePlayers());
                Table.PrintTable();
                PostBlinds();
                roundState = RoundState.Flop;
                StartBettingRound(RoundState.Preflop);
                Thread.Sleep(3000);
                break;
            case RoundState.Flop:
                Table.Dealer.DealBoardCards();
                Table.PrintTable();
                ResetStreetBets();                
                roundState = RoundState.Turn;
                StartBettingRound(RoundState.Flop);
                Thread.Sleep(5000);
                break;
            case RoundState.Turn:
                Table.Dealer.DealBoardCards();
                Table.PrintTable();
                ResetStreetBets();
                roundState = RoundState.River;
                StartBettingRound(RoundState.Turn);
                Thread.Sleep(5000);
                break;
            case RoundState.River:
                Table.Dealer.DealBoardCards();
                Table.PrintTable();
                ResetStreetBets();
                roundState = RoundState.Showdown;
                StartBettingRound(RoundState.River);
                Thread.Sleep(5000);
                break;
            case RoundState.Showdown:
                AllocatePot();
                roundState = RoundState.Reset;
                Thread.Sleep(5000);
                break;
            case RoundState.Reset:
                Console.WriteLine($"{roundState}");
                roundState = RoundState.Preflop;
                Reset();
                Table.PrintTable();
                Thread.Sleep(5000);
                break;
            }
        x++;    
        } 
    }

    private void ProcessManualAction(Player player)
    {
        // Record cursor At the very top of the function

        var startPos = Console.GetCursorPosition();
        int blockHeight = 10; // 10 lines expected for input prompting

        
        // Clear previous block safely
        Table.ClearBlock(startPos.Top, blockHeight);
        Console.SetCursorPosition(startPos.Left, startPos.Top);

        //calculate valid player options
        var options = GetRoundActions(player);
        if (options.Count == 0) return;

        Console.WriteLine($"\n--- ACTION FOR {player.Name} ---");
        Console.WriteLine($"CurrentBet: {CurrentBet} | PlayerBet: {player.CurrentBet} | Chips: {player.ChipCount}");

        //print out options
        for (int i = 0; i < options.Count; i++)
        {
            var o = options[i];
            string raiseRange = o.Action == PlayerAction.Raise ? $" (Min:{o.MinAmount}, Max:{o.MaxAmount})" : "";
            string callAmount = o.Action == PlayerAction.Call ? $" (toCall){o.Amount}" : "";
            Console.WriteLine($"{i}: {o.Action}{callAmount}{raiseRange}");
        }

        // Get user choice
        int choiceIndex = -1;
        while (choiceIndex < 0 || choiceIndex >= options.Count)
        {
            Console.Write("Pick an action index: ");
            string input = Console.ReadLine();
            int.TryParse(input, out choiceIndex);
        }

        var choice = options[choiceIndex];

        // Execute
        switch (choice.Action)
        {
            case PlayerAction.Fold:
                PlayerFold(player);
                Console.WriteLine($"{player.Name} folds. Chips:{player.ChipCount}");
                break;

            case PlayerAction.Call:
                PlayerCall(player);
                if (CurrentBet == player.CurrentBet)
                    Console.WriteLine($"{player.Name} calls {choice.Amount}. Chips:{player.ChipCount} Pot:{Table.Pot}");
                else
                    Console.WriteLine($"{player.Name} checks. Chips:{player.ChipCount} Pot:{Table.Pot}");
                break;

            case PlayerAction.Raise:

                int min = Math.Max(choice.MinAmount, 1);
                int max = choice.MaxAmount;

                if (min > max)
                {
                    PlayerCall(player);
                    Console.WriteLine($"{player.Name} calls instead (raise impossible)");
                    break;
                }

                int raiseAmount = -1;
                while (raiseAmount < min || raiseAmount > max)
                {
                    Console.Write($"Enter raise amount ({min}–{max}): ");
                    int.TryParse(Console.ReadLine(), out raiseAmount);
                }

                PlayerRaise(player, raiseAmount);
                Console.WriteLine($"{player.Name} raises {raiseAmount}. Chips:{player.ChipCount} Pot:{Table.Pot}");
                break;
        }

        // Reset Cursor location for the next option menu
        
        Console.SetCursorPosition(startPos.Left, startPos.Top);
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
                Console.WriteLine($"{player.Name} folds ChipCount:{player.ChipCount}");
                break;

            case PlayerAction.Call: // includes check if toCall = 0
                PlayerCall(player);
                if (CurrentBet == player.CurrentBet)
                    Console.WriteLine($"{player.Name} calls {choice.Amount}.  ChipCount:{player.ChipCount} Pot:{Table.Pot}");
                else
                    Console.WriteLine($"{player.Name} checks. ChipCount:{player.ChipCount} Pot:{Table.Pot} ");
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
                Console.WriteLine($"{player.Name} raises {raiseAmount}. ChipCount{player.ChipCount} Pot:{Table.Pot}");
                break;
        }

        Thread.Sleep(4000); // delay so you can watch step by step
    }
}