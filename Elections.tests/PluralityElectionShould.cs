using System.Collections.ObjectModel;
using Bogus;
using Elections.Ballots;
using Elections.Elections;
using Elections.Interfaces;
using FluentAssertions;
using Moq;
using Xunit.Abstractions;

namespace Elections.tests;

internal record TestCandidate(int Id, string Name) : ICandidate;
internal record TestVoter(int Id, string Name) : IVoter;
internal record TestBallot(IVote Vote, IVoter Voter) : ISingleVoteBallot;
internal record TestVote(ICandidate Candidate) : IVote;

public class PluralityElectionShould
{
    private readonly ITestOutputHelper helper;
    public PluralityElectionShould(ITestOutputHelper helper)
    {
        this.helper = helper;
    }
    [Fact] // No votes
    public void ReturnNoWinnerWithNoVotes()
    {
        var election = new PluralityElection();
        var candidates = Candidates.Official.ToList();
        var ballots = new ReadOnlyCollection<ISingleVoteBallot>(new List<ISingleVoteBallot>());

        var results = election.Run(ballots, candidates);
        results.Winner.Should().Be(Candidates.NoWinner);
        results.Votes.All(v => v.Value == 0);
    }

    [Fact] // Trivial case
    public void ReturnCorrectOneVoterWithOneCandidateOneVote()
    {
        var election = new PluralityElection();
        var candidates = Candidates.Official.Take(1).ToList();
        var voters = Voters.Create(1, candidates);
        var ballots = SingleVoteBallotFactory.Create(voters, candidates);

        var results = election.Run(ballots, candidates);

        results.Winner.Should().Be(candidates.First());
        results.Votes.Values.First().Should().Be(2);
    }

    [Fact] // A tie?
    public void ReturnATie()
    {
        var election = new PluralityElection();
        var candidates = Candidates.Official.Take(2).ToList();
        var voters = Voters.Create(2, candidates);
        var ballot1 = new TestBallot(new TestVote(candidates.First()), voters.First());
        var ballot2 = new TestBallot(new TestVote(candidates.Last()), voters.Last());
        var ballots = new List<ISingleVoteBallot>() { ballot1, ballot2 };

        var results = election.Run(ballots, candidates);

        results.Votes.Values.First().Should().Be(1);
        results.Votes.Values.Last().Should().Be(1);
        results.Winner.Should().Be(Candidates.NoWinner);
    }

    [Fact] // A tie?
    public void RunRandomElections()
    {
        // PrintResults(RunElection());
        var tasks = new List<Task<IElectionResults>>();
        Enumerable.Range(0, 99).ToList()
            .ForEach(i => tasks.Add(Task.Factory.StartNew(RunElection)));
        Task.WaitAll(tasks.ToArray());
        tasks.ForEach(t =>
        {
            PrintResults(t.Result);
            VerifyResults(t.Result);
        });
    }

    private void VerifyResults(IElectionResults result)
    {
        var topTwo = result.Votes.OrderByDescending(v => v.Value).Take(2);
        if (result.Votes.Count == 1) // Only one candidate
            result.Winner.Should().Be(topTwo.First().Key);
        else if (topTwo.First().Value == topTwo.Last().Value)
            result.Winner.Should().Be(Candidates.NoWinner);
        else
            result.Winner.Should().Be(topTwo.First().Key);
    }

    static void PrintResults(IElectionResults results)
    {

        Console.WriteLine($"========== Results are In ==========");
        Console.WriteLine();
        Console.WriteLine($"Winner is {results?.Winner.Name}");
        results?.Votes.OrderByDescending(v => v.Value).ToList()
            .ForEach(v => Console.WriteLine($"{v.Key.Name} - {v.Value}"));
        Console.WriteLine();
    }
    static IElectionResults RunElection()
    {
        var faker = new Faker();
        int numberOfVoters = faker.Random.Int(1, 10000);
        int numberOfCandidates = faker.Random.Int(1, Candidates.Official.Count);

        var candidates = Candidates.Official.OrderBy(c => Guid.NewGuid())
            .Take(numberOfCandidates).ToList();
        var voters = Voters.Create(numberOfVoters, candidates);
        var ballots = SingleVoteBallotFactory.Create(voters, candidates);

        var election = new PluralityElection();
        return election.Run(ballots, candidates);
    }
    [Fact]
    public void ThrowNullExceptionForCandidates()
    {
        var election = new PluralityElection();
        var ballots = new Mock<IReadOnlyList<ISingleVoteBallot>>();

        var test = () =>
        {
            election.Run(ballots.Object, null);
        };
        test.Should().Throw<ArgumentNullException>();

    }

    [Fact]
    public void ThrowNullExceptionForBallots()
    {
        // Trivial case
        var election = new PluralityElection();
        var candidates = new Mock<IReadOnlyList<ICandidate>>();

        var test = () =>
        {
            election.Run(null, candidates.Object);
        };
        test.Should().Throw<ArgumentNullException>();

    }
}