namespace NetTest
{
    internal class Program
    {
        static Dictionary<int, string> dic = new Dictionary<int, string>();
        static List<Task> tasks = new List<Task>();
        static int key = 0;
        static async Task Main(string[] args)
        {
            CancellationTokenSource srcToken = new CancellationTokenSource();
            SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1,1);
            for (int i = 0; i < 10; i++)
            {
                dic.Add(i, i.ToString());
            }
            foreach (var i in dic)
            {
                tasks.Add(Task.Run(() =>
                {
                    Thread.Sleep(1000 * (i.Key + 1));
                    Console.WriteLine($"task:{i.Value} task线程ID：{Thread.CurrentThread.ManagedThreadId}");
                    semaphoreSlim.Release();
                }, srcToken.Token));    //这个cancellToken，只cancell未开始的Task
            }
            for (int i = 0; i < 10; i++)
            {
                try
                {
                    Console.Out.WriteLine($"semaphoreSlim.CurrentCount: {semaphoreSlim.CurrentCount}");
                    //Console.WriteLine($"for前:{i} 线程ID：{Thread.CurrentThread.ManagedThreadId}");
                    //超时会释放锁，进入后序代码执行，不会报错停止执行
                    await semaphoreSlim.WaitAsync(100000, srcToken.Token);
                    Console.Out.WriteLine($"semaphoreSlim.CurrentCount: {semaphoreSlim.CurrentCount}");

                    key++;
                    Console.WriteLine($"for后:{i} 线程ID：{Thread.CurrentThread.ManagedThreadId}, key: {key}");
                    if (i > 3) throw new Exception("拿到锁线程异常");   //拿到锁的线程异常退出，如果没有Release，后序变成死锁了
                    if (i > 5) srcToken.Cancel();
                    semaphoreSlim.Release(); //拿到锁工作完后，必须要释放，其它线路程释放或自已释放
                    //semaphoreSlim.Release(); //超过最大数量限制会报错
                    //semaphoreSlim.Release();
                    //semaphoreSlim.Release();
                    //semaphoreSlim.Release();
                    Console.Out.WriteLine($"semaphoreSlim.CurrentCount: {semaphoreSlim.CurrentCount}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"异常：{i} 线程ID: {Thread.CurrentThread.ManagedThreadId}, key: {key}");
                    Console.WriteLine(ex.ToString());
                    //throw;
                }
                finally
                {
                    Console.WriteLine($"finally：{i} 线程ID: {Thread.CurrentThread.ManagedThreadId}, key: {key}");
                    //导致没锁住
                    //semaphoreSlim.Release();
                }
            }

            Console.WriteLine($"主线路程ID：{Thread.CurrentThread.ManagedThreadId}");

            Console.ReadKey();
        }
    }
}
