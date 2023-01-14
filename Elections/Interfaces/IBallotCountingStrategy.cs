using Elections.Interfaces;

public interface IBallotCountingStrategy<TBallot> where TBallot : IBallot
{
    ICandidate CountBallots(IReadOnlyList<TBallot> Ballots);
}
