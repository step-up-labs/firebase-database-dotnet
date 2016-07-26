namespace Firebase.Database.Query
{
    using System;

    using Firebase.Database.Offline;

    /// <summary>
    /// Firebase query which references the child of current node.
    /// </summary>
    public class ChildQuery : FirebaseQuery
    {
        private readonly Func<string> pathFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent.  </param>
        /// <param name="pathFactory"> The path to the child node.  </param>
        /// <param name="client"> The owner. </param>
        public ChildQuery(FirebaseQuery parent, Func<string> pathFactory, FirebaseClient client)
            : base(parent, client)
        {
            this.pathFactory = pathFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildQuery"/> class.
        /// </summary>
        /// <param name="client"> The client. </param>
        /// <param name="pathFactory"> The path to the child node.  </param>
        public ChildQuery(FirebaseClient client, Func<string> pathFactory)
            : this(null, pathFactory, client)
        {
        }

        /// <summary>
        /// The as offline database.
        /// </summary>
        /// <typeparam name="T"> Type of elements. </typeparam>
        /// <param name="filenameModifier"> Custom string which will get appended to the file name. </param>
        /// <param name="elementRoot"> Optional custom root element of received json items. </param>
        /// <param name="streamChanges"> Specifies whether changes should be streamed from the server. </param> 
        /// <param name="pullEverythingOnStart"> Specifies if everything should be pull from the online storage on start. It only makes sense when <see cref="streamChanges"/> is set to true. </param>
        /// <returns> The <see cref="RealtimeDatabase{T}"/>. </returns>
        public RealtimeDatabase<T> AsRealtimeDatabase<T>(string filenameModifier, string elementRoot = "", bool streamChanges = true, bool pullEverythingOnStart = false) where T : class
        {
            return new RealtimeDatabase<T>(this, elementRoot, this.Client.OfflineDatabaseFactory, filenameModifier, streamChanges, pullEverythingOnStart);
        }

        /// <summary>
        /// Build the url segment of this child.
        /// </summary>
        /// <param name="child"> The child of this child. </param>
        /// <returns> The <see cref="string"/>. </returns>
        protected override string BuildUrlSegment(FirebaseQuery child)
        {
            var s = this.pathFactory();

            if (s != string.Empty && !s.EndsWith("/"))
            {
                s += '/';
            }

            if (!(child is ChildQuery))
            {
                return s + ".json";
            }

            return s;
        }
    }
}
