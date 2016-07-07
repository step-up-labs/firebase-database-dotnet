namespace Firebase.Database
{
    using System;
    using System.Collections.ObjectModel;

    using Firebase.Database.Streaming;

    /// <summary>
    /// Extensions for <see cref="IObservable{T}"/>.
    /// </summary>
    public static class ObservableExtensions
    {
        /// <summary>
        /// Starts observing on given firebase observable and propagates event into an <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="observable"> The observable. </param>
        /// <typeparam name="T"> Type of entity. </typeparam>
        /// <returns> The <see cref="ObservableCollection{T}"/>. </returns> 
        public static ObservableCollection<T> AsObservableCollection<T>(this IObservable<FirebaseEvent<T>> observable)
        {
            var collection = new ObservableCollection<T>();

            observable.Subscribe(f =>
            {
                if (f.EventType == FirebaseEventType.InsertOrUpdate)
                {
                    var i = collection.IndexOf(f.Object);
                    if (i >= 0)
                    {
                        collection.RemoveAt(i);
                    }

                    collection.Add(f.Object);
                }
                else
                {
                    collection.Remove(f.Object);
                }
            });

            return collection;
        }
    }
}
