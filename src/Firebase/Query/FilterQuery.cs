namespace Firebase.Database.Query 
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Represents a firebase filtering query, e.g. "?LimitToLast=10".
    /// </summary>
    public class FilterQuery : ParameterQuery 
    {
        private readonly Func<string> valueFactory;
        private readonly Func<double> doubleValueFactory;
        private readonly Func<long> longValueFactory;
        private readonly Func<bool> boolValueFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent. </param>
        /// <param name="filterFactory"> The filter. </param>
        /// <param name="valueFactory"> The value for filter. </param>
        /// <param name="client"> The owning client. </param>  
        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<string> valueFactory, FirebaseClient client)
            : base(parent, filterFactory, client)
        {
            this.valueFactory = valueFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent. </param>
        /// <param name="filterFactory"> The filter. </param>
        /// <param name="valueFactory"> The value for filter. </param>
        /// <param name="client"> The owning client. </param>
        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<double> valueFactory, FirebaseClient client)
            : base(parent, filterFactory, client)
        {
            this.doubleValueFactory = valueFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent. </param>
        /// <param name="filterFactory"> The filter. </param>
        /// <param name="valueFactory"> The value for filter. </param>
        /// <param name="client"> The owning client. </param>
        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<long> valueFactory, FirebaseClient client)
            : base(parent, filterFactory, client)
        {
            this.longValueFactory = valueFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent. </param>
        /// <param name="filterFactory"> The filter. </param>
        /// <param name="valueFactory"> The value for filter. </param>
        /// <param name="client"> The owning client. </param>
        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<bool> valueFactory, FirebaseClient client)
            : base(parent, filterFactory, client)
        {
            this.boolValueFactory = valueFactory;
        }

        /// <summary>
        /// The build url parameter.
        /// </summary>
        /// <param name="child"> The child. </param> 
        /// <returns> Url parameter part of the resulting path. </returns> 
        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            if (this.valueFactory != null)
            {
                if(this.valueFactory() == null)
                {
                    return $"null";
                }
                return $"\"{this.valueFactory()}\"";
            }
            else if (this.doubleValueFactory != null)
            {
                return this.doubleValueFactory().ToString(CultureInfo.InvariantCulture);
            }
            else if (this.longValueFactory != null)
            {
                return this.longValueFactory().ToString();
            }
            else if (this.boolValueFactory != null)
            {
                return $"{this.boolValueFactory().ToString().ToLower()}";
            }

            return string.Empty;
        }
    }
}
