using System.Collections.Generic;

namespace Elections.Interfaces;

public interface IElection<TBallot>
    where TBallot : IBallot
{
    IElectionResults Run(IReadOnlyList<TBallot> ballots, IReadOnlyList<ICandidate> candidates);
}