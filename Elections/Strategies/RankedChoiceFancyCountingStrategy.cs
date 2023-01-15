using System.Runtime.CompilerServices;
using System.Collections;
using Elections;
using Elections.Interfaces;


[assembly: InternalsVisibleTo("Elections.tests")]

namespace Elections.Strategies;
public class RankedChoiceFancyCountingStrategy : IBallotCountingStrategy<IRankedBallot>
{
    private IEnumerable<ICandidate> GetDistinctCandidates(
            IReadOnlyList<IRankedBallot> ballots)
    {
        return ballots.SelectMany(b => b.Votes)
                    .Select(v => v.Candidate).Distinct(); ;
    }

    public ICandidate CountBallots(IReadOnlyList<IRankedBallot> Ballots)
    {
        // Initialize an empty dictionary of the 
        // candidates in the ifeld. 
        var candidates = GetDistinctCandidates(Ballots);
        var voteCounter = new VoteCounter<int>(candidates.Select(c => c.GetHashCode()));
        var winner = CountRanks(Ballots, voteCounter);
        return GetCandidateByHashCode(winner, candidates);
    }

    private ICandidate GetCandidateByHashCode(int hash, IEnumerable<ICandidate> candidates)
    {
        if (hash == Candidates.NoWinner.GetHashCode())
            return Candidates.NoWinner;
        return candidates.First(c => c.GetHashCode() == hash);
    }

    private int CountRanks(IReadOnlyList<IRankedBallot> ballots,
    VoteCounter<int> voteCounter)
    {
        var rank = 1;
        int candidateCount = voteCounter.KeyCount();
        var noWinner = Candidates.NoWinner.GetHashCode();
        while (rank <= candidateCount)
        {
            voteCounter.CountItems(GetVotesByRank(ballots, rank));
            if (voteCounter.IsTied())
                return noWinner;
            var highesTVotes = voteCounter.GetHighest();
            if (highesTVotes.Percentage > 0.50) return highesTVotes.Item;
            var lowest = voteCounter.LowestVotes();
            voteCounter.Remove(lowest);
            rank++;
            ballots = AdjustBallots(ballots, rank, lowest);
        }

        return noWinner;
    }

    private IReadOnlyList<IRankedBallot> AdjustBallots(IReadOnlyList<IRankedBallot> ballots, int rank, int lowest)
    {
        // Get the votes at Rank-1 that voted for the lowest ranked candidate
        // Then retrieve their next item
        var offset = rank - 1;
        return ballots
                .Where(b => b.Votes.Any(v => v.Rank == offset
                        && v.Candidate.GetHashCode() == lowest))
                        .ToList();

    }
    private int GetCandidateWithLeastVotes(Dictionary<int, int> voteCounter)
    {
        var lowest = voteCounter.Keys.First();

        foreach (var candidate in voteCounter.Keys)
        {
            if (voteCounter[candidate] < voteCounter[lowest])
            {
                lowest = candidate;
            }
        }
        return lowest;
    }
    private void DropLowest(Dictionary<int, int> voteCounter,
    int lowest)
    {
        voteCounter.Remove(lowest);
    }




    private int GetWinner(Dictionary<int, int> counter, int voteCount)
    {
        foreach (var candidate in counter.Keys)
        {
            double percentageOfVote = counter[candidate] / (double)voteCount;
            if (percentageOfVote >= .50) return candidate;
        }
        return Candidates.NoWinner.GetHashCode();
    }
    private int[] GetVotesByRank(IReadOnlyList<IRankedBallot> Ballots, int rank)
    {
        return Ballots
                .Select(b => b.Votes.FirstOrDefault(v => v.Rank == rank))
                .Where(v => v != null)
                .Select(v => v.Candidate.GetHashCode()).ToArray();

    }
    private void Count(Dictionary<int, int> counter, int[] votes)
    {
        for (int i = 0; i < votes.Length; i++)
        {
            counter[votes[i]] += 1;
        }
    }
}