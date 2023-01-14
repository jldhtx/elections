using System.Collections.Generic;

namespace Elections.Interfaces;

public interface IElection<TBallot>
    where TBallot : IBallot
{

    void SetStrategy(IBallotCountingStrategy<TBallot> strategy);
    ICandidate Run(IReadOnlyList<TBallot> ballots, IReadOnlyList<ICandidate> candidates
             );
}