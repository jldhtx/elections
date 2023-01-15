using Bogus;
using Elections;
using Elections.Interfaces;
using FluentAssertions;

public class VoteCounterShould
{

    [Fact]
    public void ReturnIsTiedFourWaySplit()
    {
        var faker = new Faker();
        var candidates = faker.PickRandom(Candidates.Official, 4).ToList();
        var listToCount = new List<ICandidate>();
        var counter = new VoteCounter<ICandidate>(candidates);
        Enumerable.Range(1, 100).ToList()
            .ForEach(i =>
            {
                listToCount.Add(candidates[0]);
                listToCount.Add(candidates[1]);
                listToCount.Add(candidates[2]);
                listToCount.Add(candidates[3]);

            });

        counter.CountItems(listToCount);
        counter.IsTied().Should().BeTrue();
    }

    [Fact]
    public void CountCorrectly()
    {
        var faker = new Faker();
        var candidates = faker.PickRandom(Candidates.Official, 2).ToList();
        var listToCount = new List<ICandidate>();
        var counter = new VoteCounter<ICandidate>(candidates);
        Enumerable.Range(1, 100).ToList()
            .ForEach(i => listToCount.Add(candidates.First()));
        Enumerable.Range(1, 50).ToList()
       .ForEach(i => listToCount.Add(candidates.Last()));

        counter.CountItems(listToCount);
        counter.GetCount(candidates.First()).Should().Be(100);
        counter.GetCount(candidates.Last()).Should().Be(50);
    }

    [Fact]
    public void RemoveCorrectly()
    {
        var faker = new Faker();
        var candidates = faker.PickRandom(Candidates.Official, 2).ToList();
        var listToCount = new List<ICandidate>();
        var counter = new VoteCounter<ICandidate>(candidates);
        Enumerable.Range(1, 100).ToList()
            .ForEach(i => listToCount.Add(candidates.First()));
        Enumerable.Range(1, 50).ToList()
       .ForEach(i => listToCount.Add(candidates.Last()));

        counter.CountItems(listToCount);
        counter.Remove(candidates.Last());

        var keyNotFound = () =>
        {
            counter.GetCount(candidates.Last());
        };
        keyNotFound.Should().Throw<KeyNotFoundException>();
    }

    [Fact]
    public void ConstructCorrectly()
    {
        var faker = new Faker();
        var candidates = faker.PickRandom(Candidates.Official, 2);
        var counter = new VoteCounter<ICandidate>(candidates);
        counter.Should().NotBeNull();
    }
    [Fact]
    public void ThrowIfNullValuesPassedToConstructor()
    {
        var test = () =>
        {
            var counter = new VoteCounter<ICandidate>(null);
        };

        test.Should().Throw<ArgumentNullException>();
    }
    [Fact]
    public void ThrowKeyNotFound()
    {
        var test = () =>
        {
            var faker = new Faker();
            var candidates = faker.PickRandom(Candidates.Official, 2);
            var counter = new VoteCounter<ICandidate>(candidates);
            var count = counter.GetCount(Candidates.Empty);
        };

        test.Should().Throw<KeyNotFoundException>();
    }
}