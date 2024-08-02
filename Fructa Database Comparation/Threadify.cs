using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fructa_Database_Comparation
{
    internal class Threadify
    {
        public static int threadCount = 8;
        public static void Run(Action<int, int> action, int count, int range) 
        {
            int size = range / count;
            int nextRange = 0;
            List<Thread> threads = new List<Thread>();
            Thread thread;
            for (int i = 0; i < count; i++)
            {
                int nextRangeCopy = new int();
                nextRangeCopy = nextRange;
                int endRangeCopy = new int();
                endRangeCopy = Math.Min(nextRange + size, range);

                thread = new Thread(() => { action(nextRangeCopy, endRangeCopy); });
                thread.Start();
                threads.Add(thread);
                nextRange += size;
            }
            thread = new Thread(() => { action(nextRange, range); });
            thread.Start();
            threads.Add(thread);
            for(int i = 0; i < threads.Count; i++)
            {
                threads[i].Join();
            }
        }
    }
}
