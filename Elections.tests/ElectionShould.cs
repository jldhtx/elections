using System.Collections.ObjectModel;
using Bogus;
using Elections.Ballots;
using Elections.Elections;
using Elections.Interfaces;
using FluentAssertions;
using Moq;
using Xunit.Abstractions;

namespace Elections.tests;


public class ElectionShould
{


    private readonly ITestOutputHelper helper;

    public ElectionShould(ITestOutputHelper helper)
    {
        this.helper = helper;
    }
    private record TestVote(ICandidate Candidate) : IVote;
    private record TestVoter(int Id, string Name) : IVoter;
    private record TestBallot(IVote Vote, IVoter Voter) : ISingleVoteBallot;

    [Fact] // No votes
    public void ReturnNoWinnerWithNoCandidatesNoVotes()
    {
        var election = new PluralityElection();
        var candidates = new List<ICandidate>();
        var ballots = new ReadOnlyCollection<ISingleVoteBallot>(new List<ISingleVoteBallot>());
        var winner = election.Run(ballots, candidates);

        winner.Should().Be(Candidates.NoWinner);
        helper.WriteLine("No Candidates, No Votes, No Winner!");

    }
    [Fact] // No votes
    public void ReturnWinnerWithOnlyOneCandidate()
    {
        var election = new PluralityElection();
        var candidates = Candidates.Official.Take(1).ToList();
        var ballot = new TestBallot(
                new TestVote(candidates[0]),
                new TestVoter(123, "abc"));

        var ballots = new ReadOnlyCollection<ISingleVoteBallot>(
            new List<ISingleVoteBallot>() { ballot });

        var winner = election.Run(ballots, candidates);
        winner.Should().Be(candidates[0]);
    }

    [Fact] // One candidate = always the winner
    public void ReturnNoWinnerWithNoVotes()
    {
        var election = new PluralityElection();
        var candidates = Candidates.Official.Take(1).ToList();
        var ballots = new ReadOnlyCollection<ISingleVoteBallot>(new List<ISingleVoteBallot>());

        var winner = election.Run(ballots, candidates);
        winner.Should().Be(Candidates.NoWinner);
    }

    [Fact] // One candidate = always the winner
    public void DefaultTheStrategyForRankedChoice()
    {
        var election = new RankedChoiceElection();
        election.Strategy.Should().NotBeNull();
    }

    [Fact] // One candidate = always the winner
    public void DefaultTheStrategyForPlurality()
    {
        var election = new PluralityElection();
        election.Strategy.Should().NotBeNull();
    }

    [Fact] // No votes
    public void SetsStrategyCorrectly()
    {
        var election = new PluralityElection();
        var strategy = new Mock<IBallotCountingStrategy<ISingleVoteBallot>>();
        election.SetStrategy(strategy.Object);
        election.Strategy.Should().Be(strategy.Object);
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