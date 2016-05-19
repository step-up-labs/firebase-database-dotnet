namespace FireBase.Tests.Entities
{
    using Newtonsoft.Json;

    public class Dinosaur
    {
        public Dinosaur(double height, double weight, double length)
        {
            this.Dimensions = new Dimensions { Height = height, Weight = weight, Length = length };
        }

        public Dinosaur()
        {
        }

        [JsonProperty(PropertyName = "ds")]
        public Dimensions Dimensions { get; set; }
    }
}