using FootStone.GrainInterfaces;
using Ice;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FootStone.Core.FrontIce
{
    public class RoleMasterI : IRoleMasterDisp_, IServantBase
    {
        private SessionI sessionI;

        public RoleMasterI(SessionI session)
        {
            this.sessionI = session;
        }

        public void Dispose()
        {
           
        }

        public override async Task<MasterProperty> GetPropertyAsync(Current current = null)
        {
            try
            {
                var playerMaster = Global.Instance.OrleansClient.GetGrain<IRoleMasterGrain>(sessionI.PlayerId);

                return await playerMaster.GetProperty();
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
        }

        public override async Task SetPropertyAsync(MasterProperty property, Current current = null)
        {
            try
            {
                var playerMaster = Global.Instance.OrleansClient.GetGrain<IRoleMasterGrain>(sessionI.PlayerId);

                await playerMaster.SetProperty(property);
            }
            catch (System.Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw ex;
            }
        }
    }
}
