using System.Runtime.CompilerServices;
using System.Collections;
using Elections;
using Elections.Interfaces;
using Elections.Ballots;

[assembly: InternalsVisibleTo("Elections.tests")]

namespace Elections.Strategies;
public class RankedChoiceFancyCountingStrategy : IBallotCountingStrategy<IRankedBallot>
{
    private record VoterRecord(int VoterId, IRankedVote Vote, IEnumerable<IRankedVote> Votes);

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
        var votesToCount = Ballots // Get rank 1 votes
            .Select(b => new VoterRecord(b.Voter.Id, b.Votes.First(v => v.Rank == 1), b.Votes))
            .ToArray();
        var winner = CountVotes(candidates, votesToCount, voteCounter);
        return GetCandidateByHashCode(winner, candidates);
    }

    private ICandidate GetCandidateByHashCode(int hash, IEnumerable<ICandidate> candidates)
    {
        if (hash == Candidates.NoWinner.GetHashCode()) return Candidates.NoWinner;
        return candidates.First(c => c.GetHashCode() == hash);
    }

    private int CountVotes(IEnumerable<ICandidate> candidates,
    VoterRecord[] votesToCount,
    VoteCounter<int> voteCounter)
    {
        for (int rank = 1; rank <= candidates.Count(); rank++)
        {
            var winner = GetWinnerForRank(candidates, voteCounter, votesToCount, rank);
            if (voteCounter.IsTied()) // All vote counts the same. No lowest to drop
                return Candidates.NoWinner.GetHashCode();
            if (winner != Candidates.NoWinner.GetHashCode())
                return winner;
        }
        return Candidates.NoWinner.GetHashCode();
    }


    private int GetWinnerForRank(IEnumerable<ICandidate> candidates,
    VoteCounter<int> voteCounter, VoterRecord[] votesToCount, int rank)
    {
        votesToCount = NextRank(votesToCount, voteCounter, rank);
        voteCounter.CountItems(votesToCount.Select(v => v.Vote.Candidate.GetHashCode()));
        var highest = voteCounter.GetHighest();
        if (highest.Percentage > 0.50)
            return highest.Item;
        return Candidates.NoWinner.GetHashCode();
    }

    private VoterRecord[] NextRank(VoterRecord[] votes, VoteCounter<int> counter, int rank)
    {
        if (rank == 1) return votes; // Don't need to filter out with no previous round
        var lowestCandidate = counter.LowestVotes();
        var votesForLowest = GetLowestCandidateVotes(votes, lowestCandidate);
        counter.Remove(lowestCandidate);
        return GetNextRankForVoters(votesForLowest, rank);
    }

    private VoterRecord[] GetNextRankForVoters(VoterRecord[] previous, int rank)
    {
        return previous
            .Select(v => new VoterRecord(v.VoterId, GetVoteForRank(rank, v), v.Votes))
            .Where(v => !v.Vote.Candidate.Equals(Candidates.NoVote))
            .ToArray();
    }


    private IRankedVote GetVoteForRank(int rank, VoterRecord v)
    {
        var nextVotes = v.Votes
            .OrderBy(v => v.Rank)
            .Skip(rank - 1);
        if (nextVotes.Any())
            return nextVotes.First();
        return RankedBallotFactory.NoVote();
    }

    private VoterRecord[] GetLowestCandidateVotes(VoterRecord[] current, int lowest)
    {
        return current.Where(c => c.Vote.Candidate.GetHashCode().Equals(lowest)).ToArray();
    }

}