using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Drizzle.Common.Extensions;

//ref: https://www.youtube.com/watch?v=O1Tx-k4Vao0
public static class TaskExtensions
{
    //public async static void Await<T>(this Task<T> task, Action<T> completedCallBack, Action<Exception> errorCallBack)
    //{
    //    try
    //    {
    //        T result = await task;
    //        completedCallBack?.Invoke(result);
    //    }
    //    catch (Exception ex)
    //    {
    //        errorCallBack?.Invoke(ex);
    //    }
    //}

    public async static void Await(this Task task, Action completedCallBack, Action<Exception> errorCallBack)
    {
        try
        {
            await task;
            completedCallBack?.Invoke();
        }
        catch (Exception ex)
        {
            errorCallBack?.Invoke(ex);
        }
    }
}
