using System.Collections.ObjectModel;
using Elections.Interfaces;

namespace Elections.Elections;

public class PluralityElectionTieException : Exception
{
    public PluralityElectionTieException(string message) : base(message) { }
}
public class PluralityElection : IElection<ISingleVoteBallot>
{
    private record PluralityElectionResults(ICandidate Winner,
        IReadOnlyDictionary<ICandidate, int> Votes) : IElectionResults;

    public IElectionResults Run(IReadOnlyList<ISingleVoteBallot> ballots, IReadOnlyList<ICandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(ballots, nameof(ballots));
        ArgumentNullException.ThrowIfNull(candidates, nameof(candidates));

        var results = new Dictionary<ICandidate, int>
                (ballots.GroupBy(b => b.Vote.Candidate)
                .Select(g => new KeyValuePair<ICandidate, int>(g.Key, g.Count())));

        var winners = results.GroupBy(g => g.Value)
            .OrderByDescending(g => g.Key).FirstOrDefault();

        if (winners == null || winners.Count() > 1)
            return new PluralityElectionResults(Candidates.NoWinner, results);
        return new PluralityElectionResults(winners.First().Key, results);
    }

}
