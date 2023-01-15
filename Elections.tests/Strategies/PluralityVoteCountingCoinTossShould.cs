using Bogus;
using Elections;
using Elections.Ballots;
using Elections.Elections;
using Elections.Interfaces;
using FluentAssertions;
using Xunit.Abstractions;

public class PluralityVoteCountingCoinTossShould
{
    private readonly ITestOutputHelper helper;

    private record TestCandidate(int Id, string Name) : ICandidate;
    private record TestVoter(int Id, string Name) : IVoter;
    private record TestBallot(IVote Vote, IVoter Voter) : ISingleVoteBallot;
    private record TestVote(ICandidate Candidate) : IVote;

    public PluralityVoteCountingCoinTossShould(ITestOutputHelper helper)
    {
        this.helper = helper;
    }
    [Fact] // Trivial case
    public void ReturnCorrectOneVoterWithOneCandidateOneVote()
    {
        var strategy = new PluralityVoteCountingStrategyFlipACoinTieBreaker();
        var candidates = Candidates.Official.Take(1).ToList();
        var voters = Voters.Create(1, candidates);
        var ballots = SingleVoteBallotFactory.Create(voters, candidates);
        var winner = strategy.CountBallots(ballots);

        winner.Should().Be(candidates.First());
    }

    [Fact]
    public void NotReturnATie()
    {
        var strategy = new PluralityVoteCountingStrategyFlipACoinTieBreaker();
        var candidates = Candidates.Official.Take(2).ToList();
        var voters = Voters.Create(2, candidates);
        var ballot1 = new TestBallot(new TestVote(candidates.First()), voters.First());
        var ballot2 = new TestBallot(new TestVote(candidates.Last()), voters.Last());
        var ballots = new List<ISingleVoteBallot>() { ballot1, ballot2 };

        var winner = strategy.CountBallots(ballots);
        winner.Should().NotBe(Candidates.NoWinner);
    }
}