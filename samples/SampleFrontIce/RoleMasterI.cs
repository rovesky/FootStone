using FootStone.Core;
using FootStone.FrontIce;
using FootStone.GrainInterfaces;
using Ice;
using System.Threading.Tasks;

namespace SampleFrontIce
{
    public class RoleMasterI : IRoleMasterDisp_, IServantBase
    {
        private SessionI sessionI;     

        public string GetFacet()
        {
            return typeof(IRoleMasterPrx).Name;
        }

        public void setSessionI(SessionI sessionI)
        {
            this.sessionI = sessionI;
        }

        public  void Dispose()
        {
           
        }

        public override async Task<MasterProperty> GetPropertyAsync(Current current = null)
        {
            var playerMaster = Global.OrleansClient.GetGrain<IRoleMasterGrain>(sessionI.PlayerId);
            return await playerMaster.GetProperty();
        }

        public override async Task SetPropertyAsync(MasterProperty property, Current current = null)
        {
            var playerMaster = Global.OrleansClient.GetGrain<IRoleMasterGrain>(sessionI.PlayerId);
            await playerMaster.SetProperty(property);
        }
    }
}
