namespace PokerGame;

public enum Suit { Hearts, Diamonds, Clubs, Spades }
public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }
public enum RoundState { Preflop, Flop, Turn, River, Reset, Showdown, CannotStart}

public enum MessageType : byte { Handshake = 1, TableState = 2,  ActionRequest = 3, ActionResponse = 4, Chat = 5, Ack = 6}
public enum PlayerAction : byte  {Call = 1, Raise = 2, Fold = 3, Check = 4 , AllIn = 5};

public enum TestNames { Luis, Dean, Mike, Zane, Raul, Blad, Chid, Spuf, Reib, Lars}
