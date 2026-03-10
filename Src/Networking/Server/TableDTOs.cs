using PokerGame;

namespace PokerGame.DTOs;

public class TableDTO
{
    public int RecevingPlayerSeatPos { get; set; } = new();
    public List<Card> Board { get; set; } = new();
    public int Pot { get; set; } = new();
    public RoundState roundState { get; set; } = new();
    public int TurnIndex { get; set; } = new();
    public List<Card> Hand { get; set; } = new();
    public List<PlayerInfo> Players { get; set; } = new();
    public int ButtonPosition { get; set; } = new();
}

public class PlayerInfo
{
    public string Name { get; set; }
    public int ChipCount { get; set; }
    public bool IsFolded { get; set; }
    public bool IsAllIn { get; set; }
}

public class ActionsDTO
{
    public List<PlayerActionOptionsDTO> Options { get; set; } = new();
}
public class PlayerActionOptionsDTO
{
    public PlayerAction Action { get; set; } = new();   // or enum if client knows it
    public int Amount { get; set; }
    public int MinAmount { get; set; }
    public int MaxAmount { get; set; }
}