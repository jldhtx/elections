﻿using System.Collections.ObjectModel;
using Elections.Interfaces;

namespace Elections.Elections;

public sealed class PluralityElection : Election<ISingleVoteBallot>
{
    private IBallotCountingStrategy<ISingleVoteBallot> strategy =
        new PluralityVoteCountingStrategy();
    public override IBallotCountingStrategy<ISingleVoteBallot> Strategy
    {
        get => strategy;
        protected set => strategy = value;
    }


}
