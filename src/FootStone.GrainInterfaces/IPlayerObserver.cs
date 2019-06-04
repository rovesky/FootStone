using Orleans;
using System;
using System.Collections.Generic;
using System.Text;

namespace FootStone.Core.GrainInterfaces
{
    public interface IPlayerObserver : IGrainObserver
    {
        void HpChanged(int hp);

        void LevelChanged(Guid playerId, int newLevel);
    }
}
