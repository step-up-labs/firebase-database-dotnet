namespace Firebase.Database.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Linq;

    public static class ObservableExtensions
    {
        public static IObservable<TSource> RetryAfterDelay<TSource>(this IObservable<TSource> source, TimeSpan dueTime)
        {
            return RepeateInfinite(source, dueTime).Catch();
        }

        private static IEnumerable<IObservable<TSource>> RepeateInfinite<TSource>(IObservable<TSource> source, TimeSpan dueTime)
        {
            // Don't delay the first time        
            yield return source;

            while (true)
                yield return source.DelaySubscription(dueTime);
        }
    }
}
