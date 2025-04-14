using System.Collections.Generic;

namespace Firebase.Database
{
    /// <summary>
    /// Values that can be used to instruct the Firebase Database to take action based on server value
    /// </summary>
    public static class ServerValue
    {
        private const string SERVER_VALUE_KEY = ".sv";
        private const string TIME_STAMP_KEY = "timestamp";

        /// <summary>
        /// Increments or decreases a number in server side
        /// e.g.: valueInFirebaseDatabase = 3
        /// Sending { "valueInFirebaseDatabase", ServerValue.Increment(-1) } 
        /// results in valueInFirebaseDatabase = 2
        /// </summary>
        /// <param name="value">the amount you want to increment or decrease(negative values)</param>
        /// <returns></returns>
        public static Dictionary<string, object> Increment(double value)
            => new Dictionary<string, object> { { SERVER_VALUE_KEY, new { increment = value } } };
            
        /// <summary>
        /// Saves the server time as unix timestamp at the moment of the request
        /// </summary>
        public static Dictionary<string, object> TimeStamp
            => new Dictionary<string, object> { { SERVER_VALUE_KEY, TIME_STAMP_KEY } };
    }
}
