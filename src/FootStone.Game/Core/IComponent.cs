using System.Threading.Tasks;

namespace FootStone.Game
{
    public interface IComponent
    {
        Task Fini();
        Task Init();
    }
}