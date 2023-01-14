
using Elections.Interfaces;

public class PluralityVoteCountingStrategyFlipACoinTieBreaker : IBallotCountingStrategy<ISingleVoteBallot>
{
    public ICandidate CountBallots(IReadOnlyList<ISingleVoteBallot> ballots)
    {
        throw new Exception();
    }
}