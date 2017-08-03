namespace Infrastructure.Workers
{
    public interface IWorker
    {
        /// <summary>
        /// start the worker unblocking
        /// </summary>
        void Start();

        /// <summary>
        /// request stop the worker,then waiting until stopped
        /// </summary>
        void Stop();

        /// <summary>
        /// wait until worker stopped by itself
        /// </summary>
        void WaitForExit();
    }
}