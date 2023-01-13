using Elections;
using Elections.Interfaces;
using Elections.Ballots;
using Elections.Elections;
using System.Diagnostics;
using System.Reflection;

const int numVoters = 100_000;
var voters = Voters.Create(numVoters, Candidates.Official);

RunSimpleElection(voters);

RunRankedChoiceElection(voters);

static void RunSimpleElection(IReadOnlyList<IVoter> voters)
{
    var ballots = SingleVoteBallotFactory.Create(voters, Candidates.Official);
    RunElection<PluralityElection, ISingleVoteBallot>(ballots);
}

static void RunRankedChoiceElection(IReadOnlyList<IVoter> voters)
{
    var ballots = RankedBallotFactory.Create(voters, Candidates.Official);
    RunElection<RankedChoiceElection, IRankedBallot>(ballots);
}

static void RunElection<TElection, TBallot>(IReadOnlyList<TBallot> ballots)
    where TElection : IElection<TBallot>, new()
    where TBallot : IBallot
{
    var stopwatch = Stopwatch.StartNew();
    Console.WriteLine($"========== {typeof(TElection).Name} ==========");
    Console.WriteLine();

    try
    {
        var election = new TElection();
        var results = election.Run(ballots, Candidates.Official);
        Console.WriteLine(FormatMessage($"Winner is {results?.Winner.Name}"));
    }
    catch (Exception ex)
    {
        Console.WriteLine(FormatMessage(ex.ToString()));
    }

    Console.WriteLine();
    Console.WriteLine($"============================================");
    Console.WriteLine();
    Console.WriteLine();

    string FormatMessage(string prefix)
        => $"{prefix} [{stopwatch!.Elapsed.TotalMilliseconds} ms]";
}