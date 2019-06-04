using System.Threading.Tasks;

namespace FootStone.Game
{
    public interface IFSComponent
    {
        Task Fini();
        Task Init();
    }
}