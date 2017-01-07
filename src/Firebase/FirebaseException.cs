namespace Firebase.Database
{
    using System;

    public class FirebaseException : Exception
    {
        public FirebaseException(string requestUrl, string requestData, string responseData, Exception innerException)
            : base(GenerateExceptionMessage(requestUrl, requestData, responseData), innerException)
        {
            this.RequestUrl = requestUrl;
            this.RequestData = requestData;
            this.ResponseData = responseData;
        }

        /// <summary>
        /// Post data passed to the authentication service.
        /// </summary>
        public string RequestData
        {
            get;
        }

        /// <summary>
        /// Original url of the request.
        /// </summary>
        public string RequestUrl
        {
            get;
        }

        /// <summary>
        /// Response from the authentication service.
        /// </summary>
        public string ResponseData
        {
            get;
        }

        private static string GenerateExceptionMessage(string requestUrl, string requestData, string responseData)
        {
            return $"Exception occured while processing the request.\nUrl: {requestUrl}\nRequest Data: {requestData}\nResponse: {responseData}";
        }
    }
}
