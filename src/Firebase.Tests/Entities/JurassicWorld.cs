using System.Collections.Generic;

namespace FireBase.Tests.Entities
{
    public class JurassicWorld
    {
        public JurassicWorld()
        {
            this.Dinosaurs = new Dictionary<string, Dinosaur>();
        }

        public Dictionary<string, Dinosaur> Dinosaurs { get; set; }
    }
}
