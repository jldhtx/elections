using System.Diagnostics;
using System.Runtime.CompilerServices;
using Elections;
using Elections.Ballots;
using Elections.Interfaces;

[assembly: InternalsVisibleTo("Elections.tests")]

namespace Elections.Strategies;

public class RankedChoiceCountingStrategy : IBallotCountingStrategy<IRankedBallot>
{
    private record VoterRecord(int VoterId, IRankedVote Vote, IEnumerable<IRankedVote> Votes);

    public ICandidate CountBallots(IReadOnlyList<IRankedBallot> Ballots)
    {
        var candidates = GetDistinctCandidates(Ballots);
        var voteCounter = new VoteCounter<ICandidate>(candidates);
        var votesToCount = Ballots // Get rank 1 votes
            .Select(b => new VoterRecord(b.Voter.Id, b.Votes.First(v => v.Rank == 1), b.Votes))
            .ToArray();

        return CountVotes(candidates, voteCounter, votesToCount);

    }

    private ICandidate CountVotes(IEnumerable<ICandidate> candidates,
    VoteCounter<ICandidate> voteCounter, VoterRecord[] votesToCount)
    {
        for (int rank = 1; rank <= candidates.Count(); rank++)
        {
            var winner = GetWinnerForRank(candidates, voteCounter, votesToCount, rank);
            if (voteCounter.IsTied()) // All vote counts the same. No lowest to drop
                return Candidates.NoWinner;
            if (winner != Candidates.NoWinner)
                return winner;
        }
        return Candidates.NoWinner;
    }

    private ICandidate GetWinnerForRank(IEnumerable<ICandidate> candidates,
    VoteCounter<ICandidate> voteCounter, VoterRecord[] votesToCount, int rank)
    {
        votesToCount = NextRank(votesToCount, voteCounter, rank);
        voteCounter.CountItems(votesToCount.Select(v => v.Vote.Candidate));
        var highest = voteCounter.GetHighest();
        if (highest.Percentage > 0.50)
            return highest.Item;
        return Candidates.NoWinner;
    }
    private VoterRecord[] GetLowestCandidateVotes(VoterRecord[] current, ICandidate lowest)
    {
        return current.Where(c => c.Vote.Candidate.Equals(lowest)).ToArray();
    }

    private VoterRecord[] GetNextRankForVoters(VoterRecord[] previous, int rank)
    {
        return previous
            .Select(v => new VoterRecord(v.VoterId, GetVoteForRank(rank, v), v.Votes))
            .Where(v => !v.Vote.Candidate.Equals(Candidates.NoVote))
            .ToArray();
    }

    private VoterRecord[] NextRank(VoterRecord[] votes, VoteCounter<ICandidate> counter, int rank)
    {
        if (rank == 1) return votes; // Don't need to filter out with no previous round
        var lowestCandidate = counter.LowestVotes();
        var votesForLowest = GetLowestCandidateVotes(votes, lowestCandidate);
        counter.Remove(lowestCandidate);
        return GetNextRankForVoters(votesForLowest, rank);
    }


    private IEnumerable<ICandidate> GetDistinctCandidates(
               IReadOnlyList<IRankedBallot> ballots)
    {
        return ballots.SelectMany(b => b.Votes)
                    .Select(v => v.Candidate).Distinct(); ;
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

}
