using Elections.Interfaces;

namespace Elections.Ballots;

public static class RankedBallotFactory
{

    public static IRankedVote NoVote()
    {
        return new RankedChoiceVote(Candidates.NoVote, 0);
    }
    public static IReadOnlyList<IRankedBallot> Create(IReadOnlyList<IVoter> voters, IReadOnlyList<ICandidate> candidates)
    {
        return voters.Select(x => CreateBallot(x, candidates)).ToList();
    }

    private static RankedChoiceBallot CreateBallot(IVoter voter, IReadOnlyList<ICandidate> candidates)
    {
        if (voter is ICandidate candidate)
            return CreateCandidateBallot(voter, candidate);

        return CreateVoterBallot(voter, candidates);
    }

    private static RankedChoiceBallot CreateCandidateBallot(IVoter voter, ICandidate candidate)
    {
        return new RankedChoiceBallot(voter, new[] { new RankedChoiceVote(candidate, 1) });
    }

    private static RankedChoiceBallot CreateVoterBallot(IVoter voter, IReadOnlyList<ICandidate> candidates)
    {
        var candidateCount = GetCandidateCount(candidates);
        var availableCandidates = candidates.ToList();
        var votes = new List<RankedChoiceVote>();

        for (int i = 0; i < candidateCount; i++)
        {
            var voteCandidate = Candidates.SelectRandom(availableCandidates);
            availableCandidates.Remove(voteCandidate);
            votes.Add(new RankedChoiceVote(voteCandidate, i + 1));
        }

        return new RankedChoiceBallot(voter, votes);
    }

    private static int GetCandidateCount(IReadOnlyList<ICandidate> candidates)
    {
        return Random.Shared.Next() % candidates.Count + 1;
    }

    private record RankedChoiceBallot(IVoter Voter, IReadOnlyList<IRankedVote> Votes) : IRankedBallot;

    private record RankedChoiceVote(ICandidate Candidate, int Rank) : IRankedVote;
}
