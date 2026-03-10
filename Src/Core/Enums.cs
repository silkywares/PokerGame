namespace PokerGame;

public enum Suit { Hearts, Diamonds, Clubs, Spades }
public enum Rank { Two = 2, Three, Four, Five, Six, Seven, Eight, Nine, Ten, Jack, Queen, King, Ace }
public enum TestNames { Luis, Dean, Mike, Zane, Raul, Blad}
enum MessageType : byte { TableState = 1, Chat = 2, ActionRequest = 3, Handshake = 4,Ack =5}