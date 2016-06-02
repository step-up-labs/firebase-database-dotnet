namespace Firebase.Database.Tests.Entities
{
    using System.Collections.Generic;

    public class JurassicWorld
    {
        public JurassicWorld()
        {
            this.Dinosaurs = new Dictionary<string, Dinosaur>();
        }

        public Dictionary<string, Dinosaur> Dinosaurs { get; set; }
    }
}
