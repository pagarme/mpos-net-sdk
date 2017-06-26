using System;
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

        public static async Task<Boolean> SetTimeout(this Task task, Int32 timeout)
        {
            var firstFinished = await Task.WhenAny(task, Task.Delay(timeout));
            return firstFinished == task;
        }
    }
}
