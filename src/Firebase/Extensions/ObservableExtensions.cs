namespace Firebase.Database.Extensions
{
    using System;
    using System.Reactive.Linq;

    public static class ObservableExtensions
    {
        /// <summary>
        /// Returns a cold observable which retries (re-subscribes to) the source observable on error until it successfully terminates. 
        /// </summary>
        /// <param name="source">The source observable.</param>
        /// <param name="dueTime">How long to wait between attempts.</param>
        /// <param name="retryOnError">A predicate determining for which exceptions to retry. Defaults to all</param>
        /// <returns>
        /// A cold observable which retries (re-subscribes to) the source observable on error up to the 
        /// specified number of times or until it successfully terminates.
        /// </returns>
        public static IObservable<T> RetryAfterDelay<T, TException>(
            this IObservable<T> source,
            TimeSpan dueTime,
            Func<TException, bool> retryOnError)
            where TException: Exception
        {
            int attempt = 0;

            return Observable.Defer(() =>
            {
                return ((++attempt == 1) ? source : source.DelaySubscription(dueTime))
                    .Select(item => new Tuple<bool, T, Exception>(true, item, null))
                    .Catch<Tuple<bool, T, Exception>, TException>(e => retryOnError(e)
                        ? Observable.Throw<Tuple<bool, T, Exception>>(e)
                        : Observable.Return(new Tuple<bool, T, Exception>(false, default(T), e)));
            })
            .Retry()
            .SelectMany(t => t.Item1
                ? Observable.Return(t.Item2)
                : Observable.Throw<T>(t.Item3));
        }
    }
}
