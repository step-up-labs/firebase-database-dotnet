namespace Firebase.Database.Streaming
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using Newtonsoft.Json;

    public class FirebaseSingleObjectCache<T> : FirebaseCache<T>
    {
        private T RootObject;

        private bool hasRootData;

        public override IEnumerable<FirebaseObject<T>> PushData(string path, string data, bool removeEmptyEntries = true)
        {
            if (path.Equals("/"))
            {
                if (hasRootData)
                {
                    JsonConvert.PopulateObject(data, RootObject);
                }
                else
                {
                    RootObject = JsonConvert.DeserializeObject<T>(data);
                    hasRootData = true;
                }
                yield return new FirebaseObject<T>("/", RootObject);
            }
            else
            {
                object obj = RootObject;
                if (hasRootData)
                {
                    var objType = obj.GetType();

                    var pathElements = path.Split(new[] { "/" }, removeEmptyEntries ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);

                    //var setPropertyValue = true;

                    foreach (var element in pathElements)
                    {
                        if (obj is IDictionary dict)
                        {
                            var dictType = dict.GetType();
                            var itemType = dictType.BaseType.GenericTypeArguments[1];
                            objType = itemType;
                            if (dict.Contains(element))
                            {
                                obj = dict[element];
                            }
                            else
                            {
                                obj = Activator.CreateInstance(itemType);
                                dict.Add(element, obj);
                            }
                        }
                        else
                        {
                            var prp = objType.GetProperties().FirstOrDefault(d => d.Name.Equals(element, StringComparison.InvariantCultureIgnoreCase));
                            if (prp != null)
                            {
                                obj = prp.GetValue(obj);
                                objType = obj.GetType();
                            }
                        }

                    }
                    if (!string.IsNullOrEmpty(data))
                    {
                        if (objType.IsPrimitive)
                        {
                            obj = Convert.ChangeType(data, objType);
                        }
                        else
                        {
                            JsonConvert.PopulateObject(data, obj);
                        }
                        yield return new FirebaseObject<T>("/", RootObject);
                    }

                }
            }
        }
    }
}
