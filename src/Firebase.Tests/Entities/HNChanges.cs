namespace Firebase.Database.Tests.Entities
{
    using System.Collections.Generic;

    public class HNChanges
    {
        public HNChanges()
        {
            this.items = new List<long>();
            this.profiles = new List<string>();
        }

        public IList<long> items { get; set; }
        public IList<string> profiles { get; set; }
    }
}
