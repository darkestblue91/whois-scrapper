using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace whois_scrapper
{
    static class Extensions
    {
        public static void ForAllInApproximateOrder<TSource>(this ParallelQuery<TSource> source, int threadsNumber, Action<TSource> action)
        {
            Partitioner.Create(source)
                       .AsParallel()
                       .AsOrdered()
                       .WithDegreeOfParallelism(threadsNumber)
                       .ForAll(e => action(e));
        }
    }
}
