using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FootStone.Client
{
    public interface IFSClient
    {
        /// <summary>
        /// Starts this silo.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the work performed.</returns>
        Task StartAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Stops this silo.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="Task"/> representing the work performed.</returns>
        /// <remarks>
        /// A stopped silo cannot be restarted.
        /// If the provided <paramref name="cancellationToken"/> is canceled or becomes canceled during execution, the silo will terminate ungracefully.
        /// </remarks>
        Task StopAsync(CancellationToken cancellationToken = default(CancellationToken));


        
        Task<IFSSession> CreateSession(string ip, int port, string id);

        void Update();
    }
}
