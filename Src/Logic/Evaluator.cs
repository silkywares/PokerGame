using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PokerGame;

public class Evaluator
{
    static string[] rankNames = { "High Card", "One Pair", "Two Pair", "Three of a Kind", "Straight",
                            "Flush", "Full House", "Four of a Kind", "Straight Flush", "Royal Flush" };

    public enum HandRank { HighCard, OnePair, TwoPair, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush, RoyalFlush }

    /*
    High Card → kicker(s) matter (top 4 remaining cards)

    One Pair → top 3 remaining cards are kickers

    Two Pair → 1 kicker (fifth card outside the two pairs)

    Three of a Kind → 2 kickers (highest remaining cards)

    Straight → kicker usually not needed (high card of straight determines it)

    Flush → top 5 suited cards matter (primary = highest, secondary = next highest, etc.)

    !Full House → no kicker (trips + pair fully determine hand)

    !Four of a Kind → 1 kicker (fifth card outside quads)

    Straight Flush / Royal Flush → kicker irrelevant
    */
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
        public List<Card>? Leftovers;
    }
    public static List<PlayerResult> EvaluateBoard(List<Player> players, List<Card> board)
    {
        var results = new List<PlayerResult>();

        foreach (var p in players)
        {
            var evaluation = EvaluatePlayer(p, board);
            results.Add(new PlayerResult(p) { Evaluation = evaluation });
        }
        return results;
    }
    public static HandEvaluation EvaluatePlayer(Player p, List<Card> board)
    {
        var cards = p.Hand.Concat(board)
                            .OrderByDescending(c => c.Rank)
                            .ToList();

        var rankGroups = cards.GroupBy(c => c.Rank)
                            .OrderByDescending(g => g.Count())
                            .ThenByDescending(g => g.Key)
                            .ToList();

        var suitGroups = cards.GroupBy(c => c.Suit)
                            .ToList();
        

        if (IsStraightFlush(suitGroups, out var sfHigh))
            return new HandEvaluation { Rank = (int)HandRank.StraightFlush, PrimaryValue = sfHigh};

        if (IsFourOfAKind(rankGroups, cards, out var quad, out var leftovers))
            return new HandEvaluation { Rank = (int)HandRank.FourOfAKind, PrimaryValue = quad, Leftovers = leftovers};

        if (IsFullHouse(rankGroups, out var trips, out var pair))
            return new HandEvaluation { Rank = (int)HandRank.FullHouse, PrimaryValue = trips, SecondaryValue = pair, Leftovers = leftovers};

        if (IsFlush(suitGroups, out var flushHigh))
            return new HandEvaluation { Rank = (int)HandRank.Flush, PrimaryValue = flushHigh};

        if (IsStraight(cards, out var straightHigh))
            return new HandEvaluation { Rank = (int)HandRank.Straight, PrimaryValue = straightHigh};

        if (IsThreeOfAKind(rankGroups,cards, out var tripsRank, out leftovers))
            return new HandEvaluation { Rank = (int)HandRank.ThreeOfAKind, PrimaryValue = tripsRank, Leftovers = leftovers};

        if (IsTwoPair(rankGroups, cards, out var highPair, out var lowPair, out leftovers))
            return new HandEvaluation { Rank = (int)HandRank.TwoPair, PrimaryValue = highPair, SecondaryValue = lowPair, Leftovers = leftovers};

        if (IsOnePair(rankGroups, cards, out var pairRank, out leftovers))
            return new HandEvaluation { Rank = (int)HandRank.OnePair, PrimaryValue = pairRank, Leftovers = leftovers};

        HighCard(cards, out int highCard, out leftovers);
        return new HandEvaluation
        {
            Rank = (int)HandRank.HighCard,
            PrimaryValue = highCard,
            Leftovers = leftovers,
        };
    }
    public static List<PlayerResult> EvaluateWinner(List<PlayerResult> results)
    {
        var topRank = results.Max(r => r.Evaluation.Rank);

        // Filter players with the top rank
        var topPlayers = results
            .Where(r => r.Evaluation.Rank == topRank)
            .ToList();
            
        if( topPlayers.Count>1)
            topPlayers = RankTieBreak(topPlayers);

        return topPlayers;
    }
    public static List<PlayerResult> RankTieBreak(List<PlayerResult> results)
    {
        var topPlayers = results;
        var maxPrimary = 0;
        var maxSecondary = 0;

        switch (results[0].Evaluation.Rank)
        {
            case (int)HandRank.StraightFlush:
                maxPrimary = results.Max(p => p.Evaluation.PrimaryValue);
                topPlayers = topPlayers
                    .Where(c => c.Evaluation.PrimaryValue == maxPrimary)
                    .ToList();
                break;
            case (int)HandRank.FourOfAKind:
                maxPrimary = results.Max(p => p.Evaluation.PrimaryValue);
                topPlayers = topPlayers
                    .Where(c => c.Evaluation.PrimaryValue == maxPrimary)
                    .ToList();
                break;
            case (int)HandRank.Straight:
                maxPrimary = results.Max(p => p.Evaluation.PrimaryValue);
                topPlayers = topPlayers
                    .Where(c => c.Evaluation.PrimaryValue == maxPrimary)
                    .ToList();
                break;
            case (int)HandRank.FullHouse: // eval second pair
                maxPrimary = results.Max(p => p.Evaluation.PrimaryValue);
                maxSecondary = results.Max(p => p.Evaluation.SecondaryValue);
                topPlayers = topPlayers
                    .Where(c => c.Evaluation.PrimaryValue == maxPrimary)
                    .ToList();
                topPlayers = topPlayers
                    .Where(c => c.Evaluation.SecondaryValue == maxSecondary)
                    .ToList();
                break;
            case (int)HandRank.ThreeOfAKind:
                maxPrimary = results.Max(p => p.Evaluation.PrimaryValue);
                topPlayers = topPlayers
                    .Where(c => c.Evaluation.PrimaryValue == maxPrimary)
                    .ToList();
                break;
            case (int)HandRank.Flush:
                maxPrimary = results.Max(p => p.Evaluation.PrimaryValue);
                topPlayers = topPlayers
                    .Where(c => c.Evaluation.PrimaryValue == maxPrimary)
                    .ToList();
                break;
            case (int)HandRank.TwoPair:
                maxPrimary = results.Max(p => p.Evaluation.PrimaryValue);
                maxSecondary = results.Max(p => p.Evaluation.SecondaryValue);
                topPlayers = topPlayers
                    .Where(c => c.Evaluation.PrimaryValue == maxPrimary)
                    .ToList();
                topPlayers = topPlayers
                    .Where(c => c.Evaluation.SecondaryValue == maxSecondary)
                    .ToList();
                break;
            case (int)HandRank.OnePair:
                maxPrimary = results.Max(p => p.Evaluation.PrimaryValue);
                topPlayers = topPlayers
                    .Where(c => c.Evaluation.PrimaryValue == maxPrimary)
                    .ToList();
                break;
            case (int)HandRank.HighCard:
                maxPrimary = results.Max(p => p.Evaluation.PrimaryValue);
                topPlayers = topPlayers
                    .Where(c => c.Evaluation.PrimaryValue == maxPrimary)
                    .ToList();
                break;
        }

        if (topPlayers.Count > 1)
            topPlayers = LeftoverTieBreak(topPlayers);

        return topPlayers;
    }
    public static List<PlayerResult> LeftoverTieBreak(List<PlayerResult> results)
    {
        var winners = new List<PlayerResult>(results);

        if(winners[0].Evaluation.Leftovers == null)
            return winners;

        int kickerCount = winners[0].Evaluation.Leftovers!.Count;

        for (int i = 0; i < kickerCount; i++)
        {
            // Find the highest kicker at position i
            int best = winners.Max(p => (int)p.Evaluation.Leftovers![i].Rank);

            // Omit players whose kicker at this position is lower
            winners = winners
                .Where(p => (int)p.Evaluation.Leftovers![i].Rank == best)
                .ToList();

            if (winners.Count <= 1)
                break;
        }

        return winners;
    }
    private static bool IsFourOfAKind(List<IGrouping<Rank, Card>> rankGroups, List<Card> cards, out int quadRank, out List<Card>? leftovers)
    {
        quadRank = 0;
        leftovers = null;
        var quad = rankGroups.FirstOrDefault(g => g.Count() == 4);
        if (quad == null)
            return false;

        quadRank = (int)quad.Key;
        int quadRankLocal = quadRank;

        leftovers = cards
                .Where(c => (int)c.Rank != quadRankLocal)
                .OrderByDescending(c => c.Rank)
                .Take(1)
                .ToList();

        return true;
    }
    private static bool IsStraightFlush(List<IGrouping<Suit, Card>> suitGroups, out int high)
    {
        high = 0;
        foreach (var group in suitGroups)
            if (IsStraight(group.ToList(), out high))
                return true;
        return false;
    }
    private static bool IsStraight(List<Card> cards, out int high)
    {
        high = 0;
        var ranks = cards
            .Select(c => (int)c.Rank)
            .Distinct()
            .OrderBy(r => r)
            .ToList();
        
        if (ranks.Contains(14))
            ranks.Insert(0, 1);// add a 1 if Ace 

        for (int i = 0; i <= ranks.Count - 5; i++)
        {
            if (ranks[i + 4] - ranks[i] == 4)
            {
                high = ranks[i + 4];
                var straightSet = ranks.GetRange(i, 5).ToHashSet();
                return true;
            }
        }
        return false;
    }
    private static bool IsFullHouse(List<IGrouping<Rank, Card>> rankGroups, out int trips, out int pair)
    {
        trips = 0;
        pair = 0;
        var tripRank = rankGroups.FirstOrDefault(g => g.Count() == 3);
        var pairRank = rankGroups.FirstOrDefault(g => g.Count() == 2);

        if (tripRank != null && pairRank != null)
        {
            trips = (int)tripRank.Key;
            pair = (int)pairRank.Key;
            return true;
        }
        return false;
    }
    private static bool IsFlush(List<IGrouping<Suit, Card>> suitGroups, out int high)
    {
        high = 0;
        var largestGroup = suitGroups.OrderByDescending(g => g.Count()).FirstOrDefault()!;
        if (largestGroup.Count() >= 5)
        {
            high = largestGroup.Max(c => (int)c.Rank);

            return true;
        }
        return false;
    }
    private static bool IsThreeOfAKind(List<IGrouping<Rank, Card>> rankGroups, List<Card> cards, out int tripRank, out List<Card>? leftovers)
    {
        tripRank = 0;
        leftovers = null;

        var trips = rankGroups.FirstOrDefault(g => g.Count() == 3);
        if (trips == null)
            return false;

        tripRank = (int)trips.Key;

        // Assign out variable to local to use in lambda
        var tripRankLocal = tripRank;

        // Remaining cards outside the trips
        leftovers = cards
            .Where(c => (int)c.Rank != tripRankLocal)
            .OrderByDescending(c => c.Rank)
            .Take(2)
            .ToList(); 
        return true;
    }
    private static bool IsTwoPair(List<IGrouping<Rank, Card>> rankGroups, List<Card> cards, out int highPair, out int lowPair, out List<Card>? leftovers)
    {
        highPair = 0;
        lowPair = 0;
        leftovers = null;

        var pairs = rankGroups.Where(g => g.Count() == 2)
                            .Select(g => (int)g.Key)
                            .OrderByDescending(r => r)
                            .ToList();

        if (pairs.Count >= 2)
        {
            highPair = pairs[0];
            lowPair = pairs[1];
            int highPairLocal = highPair;
            int lowPairLocal = lowPair;

            leftovers = cards
                .Where(c => (int)c.Rank != highPairLocal && (int)c.Rank != lowPairLocal)   // keep only cards NOT in the pair
                .OrderByDescending(c => c.Rank)        // sort high → low
                .Take(1)                               // one-pair uses 3 kickers
                .ToList();
            return true;
        }

        return false;
    }
    private static bool IsOnePair(List<IGrouping<Rank, Card>> rankGroups, List<Card> cards, out int pairRank, out List<Card>? leftovers)
    {
        leftovers = null;
        pairRank = 0;
        var pair = rankGroups.FirstOrDefault(g => g.Count() == 2);
        if (pair == null)
            return false;

        pairRank = (int)pair.Key;
        int pairRankLocal = pairRank;

        leftovers = cards
            .Where(c => (int)c.Rank != pairRankLocal)   // keep only cards NOT in the pair
            .OrderByDescending(c => c.Rank)        // sort high → low
            .Take(3)                               // one-pair uses 3 kickers
            .ToList();
        return true;
    }
    private static bool HighCard(List<Card> cards, out int highCard, out List<Card> leftovers)
    {
        highCard = (int)cards[0].Rank;

        leftovers = cards
            .Skip(1)       // skip the high card
            .Take(4)       // take the remaining cards as kickers
            .ToList();
        return true;
    }
}
