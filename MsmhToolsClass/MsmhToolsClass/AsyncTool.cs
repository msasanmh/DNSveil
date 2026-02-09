using System.Diagnostics;

namespace MsmhToolsClass;

public static class AsyncTool
{
    private static readonly TaskFactory TaskFactory = new(CancellationToken.None, TaskCreationOptions.None, TaskContinuationOptions.None, TaskScheduler.Default);

    public static void RunSync(Func<Task> task)
    {
		try
		{
			TaskFactory.StartNew(task).Unwrap().GetAwaiter().GetResult();
		}
		catch (Exception ex)
		{
            Debug.WriteLine("AsyncTool RunSync Void: " + ex.Message);
        }
    }

    public static T? RunSync<T> (Func<Task<T>> task)
    {
        try
        {
            return TaskFactory.StartNew(task).Unwrap().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            Debug.WriteLine("AsyncTool RunSync T: " + ex.Message);
            return default;
        }
    }

}