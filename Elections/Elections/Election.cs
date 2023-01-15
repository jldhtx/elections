using Elections.Interfaces;

namespace Elections.Elections;

public abstract class Election<TBallot> : IElection<TBallot> where TBallot : IBallot
{

    public abstract IBallotCountingStrategy<TBallot> Strategy { get; protected set; }

    public ICandidate Run(IReadOnlyList<TBallot> ballots, IReadOnlyList<ICandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(ballots, nameof(ballots));
        ArgumentNullException.ThrowIfNull(candidates, nameof(candidates));

        if (ballots.Count == 0 || candidates.Count == 0)
            return Candidates.NoWinner;
        if (candidates.Count == 1)
            return candidates.First();

        return Strategy.CountBallots(ballots);


    }

    public void SetStrategy(IBallotCountingStrategy<TBallot> strategy)
    {
        this.Strategy = strategy;
    }
}
