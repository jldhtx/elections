using Elections;
using Elections.Interfaces;

public class PluralityVoteCountingStrategy : IBallotCountingStrategy<ISingleVoteBallot>
{
    public ICandidate CountBallots(IReadOnlyList<ISingleVoteBallot> ballots)
    {

        var counts = ballots.GroupBy(b => b.Vote.Candidate)
                .OrderByDescending(g => g.Count());
        var winner = counts.First();
        if (counts.Any(c => c.Count() == winner.Count() && !c.Key.Equals(winner.Key)))
            return Candidates.NoWinner;
        return winner.Key;
    }
}
