using System.Threading.Tasks;

namespace PagarMe.Generic
{
    public static class TaskExtension
    {
        public static T WaitResult<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }
    }
}
