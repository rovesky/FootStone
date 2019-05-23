using Orleans;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.GrainInterfaces
{
    public interface IRoleMasterGrain : IGrainWithGuidKey
    {
        Task SetProperty(MasterProperty property);
        Task<MasterProperty> GetProperty();

    }
}
