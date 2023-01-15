using Bogus;
using Elections;
using Elections.Ballots;
using Elections.Elections;
using Elections.Interfaces;
using Elections.Strategies;
using FluentAssertions;
using Xunit.Abstractions;

public class PluralityVoteCountingShould
{
    private readonly ITestOutputHelper helper;

    private record TestCandidate(int Id, string Name) : ICandidate;
    private record TestVoter(int Id, string Name) : IVoter;
    private record TestBallot(IVote Vote, IVoter Voter) : ISingleVoteBallot;
    private record TestVote(ICandidate Candidate) : IVote;

    public PluralityVoteCountingShould(ITestOutputHelper helper)
    {
        this.helper = helper;
    }
    [Fact] // Trivial case
    public void ReturnCorrectOneVoterWithOneCandidateOneVote()
    {
        var strategy = new PluralityVoteCountingStrategy();
        var candidates = Candidates.Official.Take(1).ToList();
        var voters = Voters.Create(1, candidates);
        var ballots = SingleVoteBallotFactory.Create(voters, candidates);

        var winner = strategy.CountBallots(ballots);

        winner.Should().Be(candidates.First());
    }

    [Fact] // A tie?
    public void ReturnATie()
    {
        var strategy = new PluralityVoteCountingStrategy();
        var candidates = Candidates.Official.Take(2).ToList();
        var voters = Voters.Create(2, candidates);
        var ballot1 = new TestBallot(new TestVote(candidates.First()), voters.First());
        var ballot2 = new TestBallot(new TestVote(candidates.Last()), voters.Last());
        var ballots = new List<ISingleVoteBallot>() { ballot1, ballot2 };

        var winner = strategy.CountBallots(ballots);
        winner.Should().Be(Candidates.NoWinner);
    }

    private List<ISingleVoteBallot> GenerateFakeBallotsForCandidate(Faker f, int count, ICandidate candidate)
    {
        List<ISingleVoteBallot> ballots = new List<ISingleVoteBallot>();
        Enumerable.Range(1, count).ToList()
            .ForEach(i =>
            {
                ballots.Add(
                    new TestBallot(new TestVote(candidate),
                    new TestVoter(f.IndexFaker, f.Lorem.Word())));
            });
        return ballots;

    }
    [Fact] // A tie?
    public void ReturnATieWithManyCandidates()
    {
        var strategy = new PluralityVoteCountingStrategy();
        var faker = new Faker();
        var candidates = faker.PickRandom(Candidates.Official, 2);
        var otherCandidates = Candidates.Official.Except(candidates);
        var candidate = candidates.First();
        var allBallots = new List<ISingleVoteBallot>();

        GenerateFakeBallotsForCandidate(faker, 100, candidates.First());
        GenerateFakeBallotsForCandidate(faker, 100, candidates.Last());

        otherCandidates.ToList().ForEach(c =>
            allBallots.AddRange(GenerateFakeBallotsForCandidate(faker, 40, c)));

        var winner = strategy.CountBallots(allBallots);
        winner.Should().Be(Candidates.NoWinner);
    }

    [Fact] // A tie?
    public void RunRandomElections()
    {
        // PrintResults(RunElection());
        var tasks = new List<Task>();
        Enumerable.Range(0, 99).ToList()
            .ForEach(i => tasks.Add(Task.Factory.StartNew(RunElection)));
        Task.WaitAll(tasks.ToArray());

    }

    static void RunElection()
    {
        var faker = new Faker();
        int numberOfVoters = faker.Random.Int(1, 100000);
        int numberOfCandidates = faker.Random.Int(1, Candidates.Official.Count);
        var candidates = Candidates.Official.OrderBy(c => Guid.NewGuid())
            .Take(numberOfCandidates).ToList();
        var voters = Voters.Create(numberOfVoters, candidates);
        var ballots = SingleVoteBallotFactory.Create(voters, candidates);
        var strategy = new PluralityVoteCountingStrategy();
        var winner = strategy.CountBallots(ballots);

        var results = new Dictionary<ICandidate, int>(
            ballots.GroupBy(b => b.Vote.Candidate)
            .Select(g => new KeyValuePair<ICandidate, int>(g.Key, g.Count())));
        PrintResults(winner, results, numberOfVoters, numberOfCandidates);

    }

    static void PrintResults(ICandidate winner, Dictionary<ICandidate, int> results,
            int numberOfVoters, int numberOfCandidates)
    {
        Console.WriteLine($"========== Results are In ==========");
        Console.WriteLine();
        Console.WriteLine($"Total votes\t\t {numberOfVoters}");
        Console.WriteLine($"Total candidates\t\t {numberOfCandidates}");
        Console.WriteLine($"Winner is \t\t{winner.Name}");
        results.OrderByDescending(v => v.Value).ToList()
            .ForEach(v => Console.WriteLine($"{v.Key.Name} - {v.Value}"));
        Console.WriteLine();
    }
}