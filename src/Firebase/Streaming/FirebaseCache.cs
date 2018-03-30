namespace Firebase.Database.Streaming
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Firebase.Database.Http;

    using Newtonsoft.Json;

    /// <summary>
    /// The firebase cache.
    /// </summary>
    /// <typeparam name="T"> Type of top-level entities in the cache. </typeparam>
    public class FirebaseCache<T> : IEnumerable<FirebaseObject<T>>
    {
        private readonly IDictionary<string, T> dictionary;
        private readonly bool isDictionaryType;

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebaseCache{T}"/> class.
        /// </summary>
        public FirebaseCache() 
            : this(new Dictionary<string, T>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FirebaseCache{T}"/> class and populates it with existing data.
        /// </summary>
        /// <param name="existingItems"> The existing items. </param>
        public FirebaseCache(IDictionary<string, T> existingItems)
        {
            this.dictionary = existingItems;
            this.isDictionaryType = typeof(IDictionary).GetTypeInfo().IsAssignableFrom(typeof(T).GetTypeInfo());
        }

        /// <summary>
        /// The push data.
        /// </summary>
        /// <param name="path"> The path of incoming data, separated by slash. </param>  
        /// <param name="data"> The data in json format as returned by firebase. </param>  
        /// <returns> Collection of top-level entities which were affected by the push. </returns>
        public IEnumerable<FirebaseObject<T>> PushData(string path, string data, bool removeEmptyEntries = true)
        {
            object obj = this.dictionary;
            Action<object> primitiveObjSetter = null;
            Action objDeleter = null;

            var pathElements = path.Split(new[] { "/" }, removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

            // first find where we should insert the data to
            foreach (var element in pathElements)
            {
                if (obj is IDictionary)
                {
                    // if it's a dictionary, then it's just a matter of inserting into it / accessing existing object by key
                    var dictionary = obj as IDictionary;
                    var valueType = obj.GetType().GenericTypeArguments[1];

                    primitiveObjSetter = (d) => dictionary[element] = d;
                    objDeleter = () => dictionary.Remove(element);

                    if (dictionary.Contains(element))
                    {
                        obj = dictionary[element];
                    }
                    else
                    {
                        dictionary[element] = this.CreateInstance(valueType);
                        obj = dictionary[element];
                    }
                }
                else
                {
                    // if it's not a dictionary, try to find the property of current object with the matching name
                    var objParent = obj;
                    var property = objParent
                        .GetType()
                        .GetRuntimeProperties()
                        .First(p => p.Name.Equals(element, StringComparison.OrdinalIgnoreCase) || element == p.GetCustomAttribute<JsonPropertyAttribute>()?.PropertyName);

                    objDeleter = () => property.SetValue(objParent, null);
                    primitiveObjSetter = (d) => property.SetValue(objParent, d);
                    obj = property.GetValue(obj);
                    if (obj == null)
                    {
                        obj = this.CreateInstance(property.PropertyType);
                        property.SetValue(objParent, obj);
                    }
                }
            }

            // if data is null (=empty string) delete it
            if (string.IsNullOrWhiteSpace(data) || data == "null")
            {
                var key = pathElements[0];
                var target = this.dictionary[key];

                objDeleter();

                yield return new FirebaseObject<T>(key, target);
                yield break;
            }

            // now insert the data
            if (obj is IDictionary && !this.isDictionaryType)
            {
                // insert data into dictionary and return it as a collection of FirebaseObject
                var dictionary = obj as IDictionary;
                var valueType = obj.GetType().GenericTypeArguments[1];
                var objectCollection = data.GetObjectCollection(valueType);

                foreach (var item in objectCollection)
                {
                    dictionary[item.Key] = item.Object;

                    // top level dictionary changed
                    if (!pathElements.Any())
                    {
                        yield return new FirebaseObject<T>(item.Key, (T)item.Object);
                    }
                }

                // nested dictionary changed
                if (pathElements.Any())
                {
                    this.dictionary[pathElements[0]] = this.dictionary[pathElements[0]];
                    yield return new FirebaseObject<T>(pathElements[0], this.dictionary[pathElements[0]]);
                }
            }
            else
            {
                // set the data on a property of the given object
                var valueType = obj.GetType();

                // firebase sends strings without double quotes
                var targetObject = valueType == typeof(string) ? data.ToString() : JsonConvert.DeserializeObject(data, valueType);

                if ((valueType.GetTypeInfo().IsPrimitive || valueType == typeof(string)) && primitiveObjSetter != null)
                {
                    // handle primitive (value) types separately
                    primitiveObjSetter(targetObject);
                }
                else
                {
                    JsonConvert.PopulateObject(data, obj);
                }

                this.dictionary[pathElements[0]] = this.dictionary[pathElements[0]];
                yield return new FirebaseObject<T>(pathElements[0], this.dictionary[pathElements[0]]);
            }
        }

        private object CreateInstance(Type type)
        {
            if (type == typeof(string))
            {
                return string.Empty;
            }
            else
            {
                return Activator.CreateInstance(type);
            }
        }

        #region IEnumerable

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public IEnumerator<FirebaseObject<T>> GetEnumerator()
        {
            return this.dictionary.Select(p => new FirebaseObject<T>(p.Key, p.Value)).GetEnumerator();
        }

        #endregion
    }
}
