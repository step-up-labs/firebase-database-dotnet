namespace Firebase
{
    public class FirebaseObject<T>
    {
        internal FirebaseObject(string key, T obj)
        {
            this.Key = key;
            this.Object = obj;
        }

        public string Key
        {
            get;
        }

        public T Object
        {
            get;
        }
    }
}
