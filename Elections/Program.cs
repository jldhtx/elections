using Elections;
using Elections.Interfaces;
using Elections.Ballots;
using Elections.Elections;
using System.Diagnostics;
using System.Reflection;
using Elections.Strategies;

const int numVoters = 100_000;
var voters = Voters.Create(numVoters, Candidates.Official);

RunSimpleElection(voters);

RunRankedChoiceElection(voters);

RunSimpleElectionCoinFlip(voters);

RunFancyRankedChoiceElection(voters);

static void RunSimpleElection(IReadOnlyList<IVoter> voters)
{
    var ballots = SingleVoteBallotFactory.Create(voters, Candidates.Official);
    RunElection<PluralityElection, PluralityVoteCountingStrategy, ISingleVoteBallot>(ballots);
}

static void RunSimpleElectionCoinFlip(IReadOnlyList<IVoter> voters)
{
    var ballots = SingleVoteBallotFactory.Create(voters, Candidates.Official);
    RunElection<PluralityElection, PluralityVoteCountingStrategyFlipACoinTieBreaker, ISingleVoteBallot>(ballots);
}

static void RunRankedChoiceElection(IReadOnlyList<IVoter> voters)
{
    var ballots = RankedBallotFactory.Create(voters, Candidates.Official);
    RunElection<RankedChoiceElection, RankedChoiceCountingStrategy, IRankedBallot>(ballots);
}

static void RunFancyRankedChoiceElection(IReadOnlyList<IVoter> voters)
{
    var ballots = RankedBallotFactory.Create(voters, Candidates.Official);
    RunElection<RankedChoiceElection, RankedChoiceFancyCountingStrategy, IRankedBallot>(ballots);
}

static void RunElection<TElection, TStrategy, TBallot>(IReadOnlyList<TBallot> ballots)
    where TElection : IElection<TBallot>, new()
    where TBallot : IBallot
    where TStrategy : IBallotCountingStrategy<TBallot>, new()
{
    var stopwatch = Stopwatch.StartNew();
    var header = $"========== {typeof(TElection).Name} - {typeof(TStrategy).Name} ==========";
    Console.WriteLine(header);
    Console.WriteLine();

    try
    {
        var election = new TElection();
        var strategy = new TStrategy();
        election.SetStrategy(strategy);
        var winner = election.Run(ballots, Candidates.Official);

        Console.WriteLine(FormatMessage($"Winner is {winner.Name}"));
    }
    catch (Exception ex)
    {
        Console.WriteLine(FormatMessage(ex.ToString()));
    }

    Console.WriteLine();
    Console.WriteLine($"{new string('=', header.Length)}");
    Console.WriteLine();
    Console.WriteLine();

    string FormatMessage(string prefix)
        => $"{prefix} [{stopwatch!.Elapsed.TotalMilliseconds} ms]";
}