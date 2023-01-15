using Bogus;
using Elections;
using Elections.Ballots;
using Elections.Interfaces;
using Elections.Strategies;
using FluentAssertions;

public class RankedChoiceCountingShould
{
    private record TestCandidate(int Id, string Name) : ICandidate;
    private record TestVoter(int Id, string Name) : IVoter;
    private record TestBallot(IReadOnlyList<IRankedVote> Votes, IVoter Voter) : IRankedBallot;
    private record TestVote(ICandidate Candidate, int Rank) : IRankedVote;

    [Fact]
    public void ReturnATieWithTwoCandidatesAndTwoVotes()
    {
        var faker = new Faker();
        var strategy = new RankedChoiceCountingStrategy();
        var candidates = faker.PickRandom(Candidates.Official, 2).ToArray();
        var A = candidates[0];
        var B = candidates[1];

        var ballots = new List<IRankedBallot>();
        ballots.AddRange(GenerateFakeBallotsForCandidates(faker, 2, new[] { A, B }));
        ballots.AddRange(GenerateFakeBallotsForCandidates(faker, 2, new[] { B, A }));

        var winner = strategy.CountBallots(ballots.ToList());

        winner.Should().Be(Candidates.NoWinner);
    }

    [Fact]
    public void ReturnCorrectWinnerWithExampleFromBallotPedia()
    {
        // https://ballotpedia.org/Ranked-choice_voting_(RCV)
        // 1025 total votes
        // First round
        // A - 475
        // B - 300
        // C - 175
        // D - 75
        var faker = new Faker();
        var candidates = faker.PickRandom(Candidates.Official, 4).ToArray();
        var strategy = new RankedChoiceCountingStrategy();

        var A = candidates[0];
        var B = candidates[1];
        var C = candidates[2];
        var D = candidates[3];

        var ballots = new List<IRankedBallot>();

        ballots.AddRange(GenerateFakeBallotsForCandidates(faker, 475, new[] { A, B, C, D }));
        ballots.AddRange(GenerateFakeBallotsForCandidates(faker, 300, new[] { B, A, C, D }));
        ballots.AddRange(GenerateFakeBallotsForCandidates(faker, 175, new[] { C, B, A, D }));
        ballots.AddRange(GenerateFakeBallotsForCandidates(faker, 25, new[] { D, B, A, C }));
        ballots.AddRange(GenerateFakeBallotsForCandidates(faker, 50, new[] { D, A, B, C }));

        var winner = strategy.CountBallots(ballots);

        winner.Should().Be(A);
    }

    [Fact]
    public void AdjustsVotesCorrectly()
    {

        var faker = new Faker();
        var candidates = faker.PickRandom(Candidates.Official, 2).ToArray();
        var A = candidates[0];
        var B = candidates[1];
        var ballots = new List<IRankedBallot>();
        ballots.AddRange(GenerateFakeBallotsForCandidates(faker, 200, new[] { A, B }));
        ballots.AddRange(GenerateFakeBallotsForCandidates(faker, 100, new[] { B, A }));

        var strategy = new RankedChoiceCountingStrategy();
        var adjustedBallots = strategy.AdjustVotes(ballots, B, 2);
        adjustedBallots.Where(vote => vote.Candidate.Equals(A))
            .Should().HaveCount(300);


    }
    private List<IRankedBallot> GenerateFakeBallotsForCandidates(Faker f, int count, ICandidate[] candidates)
    {
        List<IRankedBallot> ballots = new List<IRankedBallot>();
        Enumerable.Range(1, count).ToList()
            .ForEach(i =>
            {
                var votes = candidates
                    .Select((c, i) => new TestVote(c, i + 1))
                    .ToList();

                ballots.Add(
                    new TestBallot(votes, new TestVoter(f.IndexFaker, f.Lorem.Word())));
            });
        return ballots;

    }

    [Fact]
    public void RunWithRandomData()
    {
        const int numVoters = 100_000;
        var voters = Voters.Create(numVoters, Candidates.Official);
        var ballots = RankedBallotFactory.Create(voters, Candidates.Official);

        var strategy = new RankedChoiceCountingStrategy();
        var winner = strategy.CountBallots(ballots);
        winner.Should().NotBe(Candidates.NoWinner);
    }
}