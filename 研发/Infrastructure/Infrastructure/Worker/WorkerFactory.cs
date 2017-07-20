using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Workers
{
    public class WorkerFactory
    {
        static List<IWorker> _workers = new List<IWorker>();
        public static void AddWorker(IWorker worker, bool runImmediately = false)
        {
            if (_workers.Contains(worker))
                throw new Exception("该Worker已经加入");

            _workers.Add(worker);
            if (runImmediately) worker.Start();
        }

        public static void StartAll()
        {
            Parallel.ForEach(_workers, w => w.Start());
        }

        public static void StopAll()
        {
            Parallel.ForEach(_workers, w => w.Stop());
        }
    }
}
