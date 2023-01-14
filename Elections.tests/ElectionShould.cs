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
    public void ReturnNoWinnerWithNoVotes()
    {
        var election = new PluralityElection();
        var candidates = Candidates.Official.ToList();

        var ballots = new ReadOnlyCollection<ISingleVoteBallot>(new List<ISingleVoteBallot>());

        var winner = election.Run(ballots, candidates);
        winner.Should().Be(Candidates.NoWinner);
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