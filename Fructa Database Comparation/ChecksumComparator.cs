using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fructa_Database_Comparation
{
    internal class ChecksumComparator
    {
        private List<int> _indiciesMissing;
        private List<int> _list2;
        private ConcurrentDictionary<int, bool> _map;
        private int _threads;
        private List<int> _list1;

        public ChecksumComparator(int threads)
        {
            _threads = threads;
        }

        public List<int> Compare(List<int> list1, List<int> list2)
        {
            _list1 = list1;
            _list2 = list2;
            _indiciesMissing = new List<int>();

            _map = new ConcurrentDictionary<int, bool>(_threads, list1.Count);

            Threadify.Run(ConstructMap1, 1, _list2.Count);
            Threadify.Run(ConstructMap2, 1, _list1.Count);

            Threadify.Run(CheckForMissing, _threads, _list2.Count); ;

            return _indiciesMissing;
        }

        private void ConstructMap1(int rangeStart, int rangeEnd)
        {
            for (int i = rangeStart; i < rangeEnd; ++i) _map[_list2[i]] = false;
        }
        private void ConstructMap2(int rangeStart, int rangeEnd)
        {
            for (int i = rangeStart; i < rangeEnd; ++i) _map[_list1[i]] = true;
        }

        private void CheckForMissing(int rangeStart, int rangeEnd)
        {
            for (int i = rangeStart; i < rangeEnd; ++i)
            {
                if (_map[_list2[i]]) continue;

                lock (_indiciesMissing)
                {
                    _indiciesMissing.Add(_list2[i]);
                }


            }
        }
    }
}
