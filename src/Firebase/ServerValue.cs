using System.Collections.Generic;

namespace Oktoberfest.Functions.Tests
{
    /// <summary>
    /// Values that can be used to instruct the Firebase Database to take action based on server value
    /// </summary>
    public static class ServerValue
    {
        private const string SERVER_VALUE_KEY = ".sv";
        private const string TIME_STAMP_KEY = "timestamp";

        /// <summary>
        /// Increments a number in server side
        /// </summary>
        /// <param name="value">the amount you want to increment</param>
        /// <returns></returns>
        public static Dictionary<string, object> Increment(uint value)
            => IncrementOrDecrease(value);

        /// <summary>
        /// Decreases a number in server side
        /// </summary>
        /// <param name="value">the amount you want to decrease</param>
        /// <returns></returns>
        public static Dictionary<string, object> Decrement(uint value)
            => IncrementOrDecrease(value * -1);

        /// <summary>
        /// Saves the server time at the moment of the request
        /// </summary>
        public static Dictionary<string, object> TimeStamp 
            => new Dictionary<string, object> { { SERVER_VALUE_KEY, TIME_STAMP_KEY } };


        private static Dictionary<string, object> IncrementOrDecrease(long value)
            => new Dictionary<string, object> { { SERVER_VALUE_KEY, new { increment = value } } };
    }
}
