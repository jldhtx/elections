public interface IVoteResult<T>
{
    T Key { get; }
    int TotalVotes { get; }
    double Percentage { get; }
}


public class VoteCounter<T> where T : notnull
{
    private readonly Dictionary<T, int> counter;

    public VoteCounter(IEnumerable<T> keys)
    {
        counter = new Dictionary<T, int>(
            keys.Select(k => new KeyValuePair<T, int>(k, 0))
        );
    }

    public int KeyCount()
    {
        return counter.Keys.Count;
    }
    public int GetCount(T key)
    {
        if (counter.ContainsKey(key))
            return counter[key];
        throw new KeyNotFoundException(key.ToString());
    }
    public void CountItems(IEnumerable<T> list)
    {
        foreach (var item in list)
        {
            Increment(item);
        }
    }
    public void Increment(T key)
    {
        counter[key] += 1;
    }

    public bool IsTied()
    {
        var distinctCounts = counter.Select(c => c.Value).Distinct();
        return (distinctCounts.Count() == 1);
    }

    public (T Item, int Count, double Percentage) GetHighest()
    {
        var total = counter.Values.Sum(i => i);
        var keyOfMaxValue =
            counter.Aggregate((x, y) => x.Value > y.Value ? x : y).Key; // "a"

        var count = counter[keyOfMaxValue];
        var percentage = count / (double)total;
        return (keyOfMaxValue, count, percentage);
    }

    private bool Majority(T item, int total)
    {
        return (counter[item] / (double)total) > 0.50;
    }
    public T LowestVotes()
    {
        var lowest = counter.Keys.First();

        foreach (var candidate in counter.Keys)
        {
            if (counter[candidate] < counter[lowest])
            {
                lowest = candidate;
            }
        }
        return lowest;
    }

    public void Remove(T item)
    {
        counter.Remove(item);
    }

}