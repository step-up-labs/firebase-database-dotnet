namespace Firebase.Query
{
    /// <summary>
    /// Query extensions providing linq like syntax for firebase server methods.
    /// </summary>
    public static class QueryExtensions
    {
        /// <summary>
        /// References a sub child of the existing node.
        /// </summary>
        /// <param name="node"> The child. </param>
        /// <param name="path"> The path of sub child. </param>
        /// <returns> The <see cref="ChildQuery"/>. </returns>
        public static ChildQuery Child(this ChildQuery node, string path)
        {
            return new ChildQuery(path, node);
        }

        /// <summary>
        /// Order data by given <see cref="propertyName"/>. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <param name="propertyName"> The property name. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public static OrderQuery OrderBy(this ChildQuery child, string propertyName)
        {
            return new OrderQuery(child, propertyName);
        }

        /// <summary>
        /// Order data by $key. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public static OrderQuery OrderByKey(this ChildQuery child)
        {
            return child.OrderBy("$key");
        }

        /// <summary>
        /// Order data by $value. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public static OrderQuery OrderByValue(this ChildQuery child)
        {
            return child.OrderBy("$value");
        }

        /// <summary>
        /// Order data by $priority. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public static OrderQuery OrderByPriority(this ChildQuery child)
        {
            return child.OrderBy("$priority");
        }

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <see cref="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery StartAt(this ParameterQuery child, string value)
        {
            return new FilterQuery(child, "startAt", value);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <see cref="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EndAt(this ParameterQuery child, string value)
        {
            return new FilterQuery(child, "endAt", value);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <see cref="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EqualTo(this ParameterQuery child, string value)
        {
            return new FilterQuery(child, "equalTo", value);
        }

        /// <summary>
        /// Instructs firebase to send data greater or equal to the <see cref="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery StartAt(this ParameterQuery child, double value)
        {
            return new FilterQuery(child, "startAt", value);
        }

        /// <summary>
        /// Instructs firebase to send data lower or equal to the <see cref="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EndAt(this ParameterQuery child, double value)
        {
            return new FilterQuery(child, "endAt", value);
        }

        /// <summary>
        /// Instructs firebase to send data equal to the <see cref="value"/>. This must be preceded by an OrderBy query.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="value"> Value to start at. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery EqualTo(this ParameterQuery child, double value)
        {
            return new FilterQuery(child, "equalTo", value);
        }

        /// <summary>
        /// Limits the result to first <see cref="count"/> items.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="count"> Number of elements. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery LimitToFirst(this ParameterQuery child, int count)
        {
            return new FilterQuery(child, "limitToFirst", count);
        }

        /// <summary>
        /// Limits the result to last <see cref="count"/> items.
        /// </summary>
        /// <param name="child"> Current node. </param>
        /// <param name="count"> Number of elements. </param>
        /// <returns> The <see cref="FilterQuery"/>. </returns>
        public static FilterQuery LimitToLast(this ParameterQuery child, int count)
        {
            return new FilterQuery(child, "limitToLast", count);
        }
    }
}
