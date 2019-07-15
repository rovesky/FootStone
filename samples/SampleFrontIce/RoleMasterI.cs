using FootStone.Core;
using FootStone.FrontIce;
using FootStone.GrainInterfaces;
using Ice;
using Orleans;
using System.Threading.Tasks;

namespace SampleFrontIce
{
    public class RoleMasterI : IRoleMasterDisp_, IServantBase
    {
        private Session session;    
        private IClusterClient orleansClient;

        public RoleMasterI(Session session, IClusterClient orleansClient)
        {
            this.session = session;
            this.orleansClient = orleansClient;
        }


        public string GetFacet()
        {
            return nameof(IRoleMasterPrx);
        }
     

        public  void Dispose()
        {
           
        }

        public override async Task<MasterProperty> GetPropertyAsync(Current current = null)
        {
            var playerMaster = orleansClient.GetGrain<IRoleMasterGrain>(session.PlayerId);
            return await playerMaster.GetProperty();
        }

        public override async Task SetPropertyAsync(MasterProperty property, Current current = null)
        {
            var playerMaster = orleansClient.GetGrain<IRoleMasterGrain>(session.PlayerId);
            await playerMaster.SetProperty(property);
        }
    }
}
