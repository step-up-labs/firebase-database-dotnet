namespace Firebase.Database.Query
{
    using System;

    /// <summary>
    /// Represents a firebase ordering query, e.g. "?OrderBy=Foo".
    /// </summary>
    public class OrderQuery : ParameterQuery
    {
        private readonly Func<string> propertyNameFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderQuery"/> class.
        /// </summary>
        /// <param name="parent"> The query parent. </param>
        /// <param name="propertyNameFactory"> The property name. </param>
        /// <param name="client"> The owning client. </param>
        public OrderQuery(ChildQuery parent, Func<string> propertyNameFactory, FirebaseClient client)
            : base(parent, () => "orderBy", client)
        {
            this.propertyNameFactory = propertyNameFactory;
        }

        /// <summary>
        /// The build url parameter.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <returns> The <see cref="string"/>. </returns>
        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return $"\"{this.propertyNameFactory()}\"";
        }
    }
}
