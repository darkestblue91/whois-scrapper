using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


/*

Extensions

Task domain WHOIS scraping into different threads trying to do that in order (ForAllInApproximateOrder)

*/

namespace whois_scrapper
{
    public static class Extensions {

        public static int MAX_SUPPORTED_DOP = 512;

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
