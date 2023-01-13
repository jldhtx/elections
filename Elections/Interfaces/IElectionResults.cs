using System.Collections.Generic;

namespace Elections.Interfaces;

public interface IElectionResults
{
    ICandidate Winner { get; }

    IReadOnlyDictionary<ICandidate, int> Votes { get; }

}