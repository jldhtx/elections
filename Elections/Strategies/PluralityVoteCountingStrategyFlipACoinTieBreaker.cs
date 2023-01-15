
using Elections.Interfaces;

public class PluralityVoteCountingStrategyFlipACoinTieBreaker : IBallotCountingStrategy<ISingleVoteBallot>
{
    public ICandidate CountBallots(IReadOnlyList<ISingleVoteBallot> ballots)
    {
        var counts = ballots.GroupBy(b => b.Vote.Candidate)
                 .OrderByDescending(g => g.Count());

        var firstWinner = counts.First();
        var allWinners = counts.Where(c => c.Count() == firstWinner.Count());

        if (allWinners.Count() == 1) // Only one candidate had the highest count
            return firstWinner.Key;


        return allWinners.ToArray()[Random.Shared.Next(0, allWinners.Count() - 1)].Key;
    }
}