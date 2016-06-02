namespace Firebase.Database
{
    using System;
    using System.Text;

    /// <summary>
    /// Offline key generator which mimics the official Firebase generators. 
    /// Credit: https://github.com/bubbafat/FirebaseSharp/blob/master/src/FirebaseSharp.Portable/FireBasePushIdGenerator.cs
    /// </summary>
    public class FirebaseKeyGenerator 
    {
        // Modeled after base64 web-safe chars, but ordered by ASCII.
        private const string PushCharsString = "-0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ_abcdefghijklmnopqrstuvwxyz";
        private static readonly char[] PushChars;
        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);

        private static readonly Random random = new Random();
        private static readonly byte[] lastRandChars = new byte[12];

        // Timestamp of last push, used to prevent local collisions if you push twice in one ms.
        private static long lastPushTime;

        static FirebaseKeyGenerator()
        {
            PushChars = Encoding.UTF8.GetChars(Encoding.UTF8.GetBytes(PushCharsString));
        }

        /// <summary>
        /// Returns next firebase key based on current time.  
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>. </returns>
        public static string Next()
        {
            // We generate 72-bits of randomness which get turned into 12 characters and
            // appended to the timestamp to prevent collisions with other clients. We store the last
            // characters we generated because in the event of a collision, we'll use those same
            // characters except "incremented" by one.
            var id = new StringBuilder(20);
            var now = (long)(DateTimeOffset.Now - Epoch).TotalMilliseconds;
            var duplicateTime = now == lastPushTime;
            lastPushTime = now;

            var timeStampChars = new char[8];
            for (int i = 7; i >= 0; i--)
            {
                var index = (int)(now % PushChars.Length);
                timeStampChars[i] = PushChars[index];
                now = (long)Math.Floor((double)now / PushChars.Length);
            }

            if (now != 0)
            {
                throw new Exception("We should have converted the entire timestamp.");
            }

            id.Append(timeStampChars);

            if (!duplicateTime)
            {
                for (int i = 0; i < 12; i++)
                {
                    lastRandChars[i] = (byte)random.Next(0, PushChars.Length);
                }
            }
            else
            {
                // If the timestamp hasn't changed since last push, use the same random number,
                // except incremented by 1.
                var lastIndex = 11;
                for (; lastIndex >= 0 && lastRandChars[lastIndex] == PushChars.Length - 1; lastIndex--)
                {
                    lastRandChars[lastIndex] = 0;
                }

                lastRandChars[lastIndex]++;
            }

            for (int i = 0; i < 12; i++)
            {
                id.Append(PushChars[lastRandChars[i]]);
            }

            if (id.Length != 20)
            {
                throw new Exception("Length should be 20.");
            }

            return id.ToString();
        }
    }
}
