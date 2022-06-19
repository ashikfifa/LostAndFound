using System.Threading;
using System.Threading.Tasks;

namespace LostAndFound.Service.Workers
{
    public interface IImageMatchingWorker
    {
        Task DoWork(CancellationToken cancellationToken);
    }
}