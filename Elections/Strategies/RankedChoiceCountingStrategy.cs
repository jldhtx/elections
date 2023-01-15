using System.Runtime.CompilerServices;
using Elections;
using Elections.Interfaces;

[assembly: InternalsVisibleTo("Elections.tests")]

public class RankedChoiceCountingStrategy : IBallotCountingStrategy<IRankedBallot>
{
    private record VotingRoundResult(ICandidate Candidate, int TotalVotes, double Percentage);

    // Round `1 - count total, % for all votes
    //
    public ICandidate CountBallots(IReadOnlyList<IRankedBallot> Ballots)
    {
        var results = GetRoundResults(Ballots);
        var winner = MajorityWinner(results);
        var candidateCount = Ballots.SelectMany(b => b.Votes).Select(b => b.Candidate)
                       .Distinct().Count();

        if (IsATie(results))
            return Candidates.NoWinner;

        winner = TryAgain(Ballots, candidateCount, results.Last().Candidate);

        return winner;
    }

    private ICandidate TryAgain(IReadOnlyList<IRankedBallot> Ballots, int candidateCount,
    ICandidate candidateWithLeastVotes)
    {
        int round = 1;
        var winner = Candidates.NoWinner;
        while (winner.Equals(Candidates.NoWinner) && round <= candidateCount)
        {
            var adjustedBallots = AdjustBallots(Ballots, candidateWithLeastVotes).ToList();
            var results = GetRoundResults(adjustedBallots);
            candidateWithLeastVotes = results.Last().Candidate;
            winner = MajorityWinner(results);
            round += 1;
        }

        return winner;
    }

    private bool IsATie(IEnumerable<VotingRoundResult> results)
    {
        var count = results.Count();
        return (results.GroupBy(r => r.TotalVotes)
                        .First().Count() == count);

    }
    internal IEnumerable<IRankedBallot> AdjustBallots(IEnumerable<IRankedBallot> ballots,
                ICandidate candidateWithLeastVotes)
    {
        var candidates = ballots.Where(b => b.Voter is ICandidate);

        return ballots
                    .Where(b => b.Votes.First().Candidate == candidateWithLeastVotes)
                    .Where(b => !(b.Votes.Count == 1)) // Eliminate politicaians :)
                    .Select(v => Promote(v))
                    .Union(ballots
                            .Where(b => !b.Votes.First().Candidate.Equals(candidateWithLeastVotes))
                    );
    }

    private IRankedBallot Promote(IRankedBallot ballot)
    {
        var newVotes = ballot.Votes.Skip(1);
        return new PromotedRankedBallot(newVotes.ToList(), ballot.Voter);
    }
    private record PromotedRankedBallot(IReadOnlyList<IRankedVote> Votes, IVoter Voter) : IRankedBallot;

    private IEnumerable<VotingRoundResult> GetRoundResults(IReadOnlyList<IRankedBallot> ballots)
    {
        return ballots.GroupBy(b => b.Votes.First().Candidate)
             .Select(g => new VotingRoundResult(g.Key, g.Count(), (double)g.Count() / ballots.Count))
             .OrderByDescending(v => v.Percentage).ToList();
    }

    private ICandidate MajorityWinner(IEnumerable<VotingRoundResult> Results)
    {
        if (Results.First().Percentage > 0.50)
            return Results.First().Candidate;
        return Candidates.NoWinner;
    }
}
