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

    private Dictionary<int, int> EmptyVoteCounter(
        IEnumerable<ICandidate> candidates)
    {
        return new Dictionary<int, int>(
                    candidates.Select(c =>
                        new KeyValuePair<int, int>(c.GetHashCode(), 0))
                );
    }
    public ICandidate CountBallots(IReadOnlyList<IRankedBallot> Ballots)
    {
        // Initialize an empty dictionary of the 
        // candidates in the ifeld. 
        var candidates = GetDistinctCandidates(Ballots);
        var voteCounter = EmptyVoteCounter(candidates);

        var winner = CountRanks(Ballots, voteCounter);
        return GetCandidateByHashCode(winner, candidates);
    }

    private ICandidate GetCandidateByHashCode(int hash, IEnumerable<ICandidate> candidates)
    {
        return candidates.First(c => c.GetHashCode() == hash);
    }

    private int CountRanks(IReadOnlyList<IRankedBallot> ballots,
    Dictionary<int, int> voteCounter)
    {
        var rank = 1;
        int candidateCount = voteCounter.Keys.Count;
        var noWinner = Candidates.NoWinner.GetHashCode();
        int winner = noWinner;

        while (rank <= candidateCount &&
                       winner == noWinner)
        {
            Count(voteCounter, GetVotesByRank(ballots, rank));
            winner = GetWinner(voteCounter, ballots.Count);
            if (winner != noWinner)
                return winner;
            var lowest = GetCandidateWithLeastVotes(voteCounter);
            DropLowest(voteCounter, lowest);
            rank++;
            ballots = AdjustBallots(ballots, rank, lowest);
        }

        return winner;
    }

    private IReadOnlyList<IRankedBallot> AdjustBallots(IReadOnlyList<IRankedBallot> ballots, int rank, int lowest)
    {
        var offset = rank - 1;
        return ballots
                .Where(b =>
                    b.Votes.Skip(offset - 1)
                        .Take(1).First()
                        .Candidate.GetHashCode() == lowest)
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
                .SelectMany(b => b.Votes.Skip(rank - 1).Take(1))
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