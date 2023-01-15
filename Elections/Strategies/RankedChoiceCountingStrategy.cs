using System.Runtime.CompilerServices;
using Elections;
using Elections.Ballots;
using Elections.Interfaces;

[assembly: InternalsVisibleTo("Elections.tests")]

namespace Elections.Strategies;

public class RankedChoiceCountingStrategy : IBallotCountingStrategy<IRankedBallot>
{
    private record VotingRoundResult(ICandidate Candidate, int TotalVotes, double Percentage);


    public ICandidate CountBallots(IReadOnlyList<IRankedBallot> Ballots)
    {
        var candidateCount = Ballots
                      .SelectMany(b => b.Votes).Select(b => b.Candidate)
                     .Distinct().Count();

        var votesToCount = Ballots.Select(b => b.Votes.First());
        var results = GetRoundResults(votesToCount.ToList());
        var winner = MajorityWinner(results);

        if (IsATie(results))
            return Candidates.NoWinner;

        winner = TryAgain(Ballots, candidateCount, results.Last().Candidate);

        return winner;
    }

    private ICandidate TryAgain(IReadOnlyList<IRankedBallot> Ballots, int candidateCount,
    ICandidate candidateWithLeastVotes)
    {
        int rank = 1;
        var winner = Candidates.NoWinner;
        while (winner.Equals(Candidates.NoWinner) && rank <= candidateCount)
        {
            var adjustedVotes = AdjustVotes(Ballots, candidateWithLeastVotes, rank).ToList();
            var results = GetRoundResults(adjustedVotes);
            candidateWithLeastVotes = results.Last().Candidate;
            winner = MajorityWinner(results);
            rank += 1;
        }

        return winner;
    }

    private bool IsATie(IEnumerable<VotingRoundResult> results)
    {
        var count = results.Count();
        return (results.GroupBy(r => r.TotalVotes)
                        .First().Count() == count);

    }
    internal IEnumerable<IRankedVote> AdjustVotes(IEnumerable<IRankedBallot> ballots,
                ICandidate candidateWithLeastVotes, int rank)
    {
        var adjustedVotes = new List<IRankedVote>();
        var votesMinusLowest = ballots
            .Select(b => GetVote(rank - 1, b.Votes))
            .Where(b => !b.Candidate.Equals(candidateWithLeastVotes)
                    && !b.Equals(Candidates.NoVote));

        var nextRankedVote = ballots
            .Where(b => GetVote(rank - 1, b.Votes).Candidate
                    == candidateWithLeastVotes)
            .Select(b => GetVote(rank, b.Votes));

        adjustedVotes.AddRange(votesMinusLowest);
        adjustedVotes.AddRange(nextRankedVote);

        return adjustedVotes;

    }

    private IRankedVote GetVote(int rank, IReadOnlyList<IRankedVote> votes)
    {
        var nextVotes = votes.Skip(rank - 1);
        if (nextVotes.Any())
            return nextVotes.First();
        return RankedBallotFactory.NoVote();


    }
    private IRankedBallot Promote(IRankedBallot ballot)
    {
        var newVotes = ballot.Votes.Skip(1);
        return new PromotedRankedBallot(newVotes.ToList(), ballot.Voter);
    }
    private record PromotedRankedBallot(IReadOnlyList<IRankedVote> Votes, IVoter Voter) : IRankedBallot;

    private IEnumerable<VotingRoundResult> GetRoundResults(IReadOnlyList<IRankedVote> votes)
    {
        return votes.GroupBy(b => b.Candidate)
             .Select(g => new VotingRoundResult(g.Key, g.Count(), (double)g.Count() / votes.Count))
             .OrderByDescending(v => v.Percentage).ToList();
    }

    private ICandidate MajorityWinner(IEnumerable<VotingRoundResult> Results)
    {
        if (Results.First().Percentage > 0.50)
            return Results.First().Candidate;
        return Candidates.NoWinner;
    }
}
