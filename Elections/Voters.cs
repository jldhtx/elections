using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elections.Interfaces;

namespace Elections;

public static class Voters
{
    public static IReadOnlyList<IVoter> Create(int voterCount, IReadOnlyList<ICandidate> candidates)
    {
        var totalVoters = voterCount + candidates.Count;
        var maxCandidateId = candidates.Max(c => c.Id);
        var voters = Enumerable.Range(maxCandidateId + 1, voterCount).Select(CreateVoter).ToList();
        voters.AddRange(candidates);
        return voters;
    }



    private static IVoter CreateVoter(int id)
    {
        return new Voter(id, $"Voter {id}");
    }

    private record Voter(int Id, string Name) : IVoter;
}
