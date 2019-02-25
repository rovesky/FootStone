using System.Threading.Tasks;

namespace FootStone.Core
{
    public interface IComponent
    {
        Task Fini();
        Task Init();
    }
}