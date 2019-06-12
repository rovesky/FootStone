using Ice;
using System;
using System.Threading.Tasks;

namespace FootStone.FrontIce
{
    public class FSInterceptor : DispatchInterceptor,IDisposable
    {
        private Ice.Object servant;
        private Logger logger;

        public FSInterceptor(Ice.Object servant, Logger logger)
        {
            this.servant = servant;
            this.logger = logger;
        }                   

        public async override Task<OutputStream> dispatch(Request request)
        {         
            try
            {              
               //  logger.print("dispatch begein:" + request.getCurrent().operation);        
                var ret = await servant.ice_dispatch(request);
               //  logger.print("dispatch end:" + request.getCurrent().operation);
                return ret;
            }
            catch(Ice.Exception e)
            {
                logger.trace("",e.ToString());
                throw e;
            }
            catch (System.Exception e)
            {
                logger.error(e.ToString());
                throw e;
            }
        }

        public void Dispose()
        {
            IDisposable dis = servant as IDisposable;
            if(dis != null)
                dis.Dispose();
        }
      
    }
}
