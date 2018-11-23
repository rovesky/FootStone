using Orleans.Concurrency;
using System.Collections.Generic;

namespace FootStone.GrainInterfaces
{
    [Immutable]
    public class MonsterInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public List<long> KilledBy { get; set; }
    }
}
