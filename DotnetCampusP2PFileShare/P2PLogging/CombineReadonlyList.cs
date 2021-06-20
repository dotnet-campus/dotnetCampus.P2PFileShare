using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DotnetCampusP2PFileShare.P2PLogging
{
    /// <summary>
    /// 合并多个只读数组
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CombineReadonlyList<T> : IReadOnlyList<T>
    {
        public CombineReadonlyList(params IReadOnlyList<T>[] source)
        {
            Source = source;
        }

        public IReadOnlyList<T>[] Source { get; }

        public IEnumerator<T> GetEnumerator()
        {
            return Source.SelectMany(readOnlyList => readOnlyList).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count => Source.Sum(temp => temp.Count);

        public T this[int index]
        {
            get
            {
                var n = index;
                var source = Source;

                foreach (var list in source)
                {
                    if (n < list.Count)
                    {
                        return list[n];
                    }

                    n -= list.Count;
                }

                throw new IndexOutOfRangeException();
            }
        }
    }
}