using System.Collections.ObjectModel;
using Elections.Interfaces;
using Elections.Strategies;

namespace Elections.Elections;
public class RankedChoiceElection : Election<IRankedBallot>
{
    private IBallotCountingStrategy<IRankedBallot> strategy = new RankedChoiceCountingStrategy();
    public override IBallotCountingStrategy<IRankedBallot> Strategy
    {
        get => strategy; protected set => strategy = value;
    }

}

