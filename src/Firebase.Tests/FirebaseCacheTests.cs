namespace FireBase.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Firebase;
    using Firebase.Streaming;

    using FireBase.Tests.Entities;

    using FluentAssertions;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using Newtonsoft.Json;

    [TestClass]
    public class FirebaseCacheTests
    {
        [TestMethod]
        public void InitialPushOfEntireDictionaryToEmptyCache()
        {
            // this should simulate first time connection is made and all data is returned in a batch in a form of a dictionary
            var cache = new FirebaseCache<Dinosaur>();
            var dinosaurs = @"{
  ""lambeosaurus"": {
    ""ds"": {
      ""height"" : 2,
      ""length"" : 2,
      ""weight"": 2
    }
  },
  ""stegosaurus"": {
    ""ds"": {
      ""height"" : 3,
      ""length"" : 3,
      ""weight"" : 3
    }
  }
}";

            var entities = cache.PushData("/", dinosaurs).ToList();
            var expectation = new[]
            {
                new FirebaseObject<Dinosaur>("lambeosaurus", new Dinosaur(2, 2, 2)),
                new FirebaseObject<Dinosaur>("stegosaurus", new Dinosaur(3, 3, 3))
            };

            entities.ShouldAllBeEquivalentTo(expectation);
        }

        [TestMethod]
        public void NewTopLevelItemInsertedToNonEmptyCache()
        {
            // this should simulate when connection had already been established with some data populated when new top-level item arrived
            var dinosaurs = new List<FirebaseObject<Dinosaur>>(new[]
            {
                new FirebaseObject<Dinosaur>("lambeosaurus", new Dinosaur(2, 2, 2)),
                new FirebaseObject<Dinosaur>("stegosaurus", new Dinosaur(3, 3, 3))
            });

            var cache = new FirebaseCache<Dinosaur>(dinosaurs.ToDictionary(f => f.Key, f => f.Object));
            var trexData = @"{
    ""ds"": {
      ""height"" : 4,
      ""length"" : 4,
      ""weight"": 4
    }
}";

            var entities = cache.PushData("/trex", trexData).ToList();
            var trex = new FirebaseObject<Dinosaur>("trex", new Dinosaur(4, 4, 4));

            entities.Should().HaveCount(1);
            entities.First().ShouldBeEquivalentTo(trex);
        }

        [TestMethod]
        public void SecondLevelItemWhichBelongsToExistingObjectChanged()
        {
            // this should simulate when some data of an existing object changed
            var dinosaurs = new List<FirebaseObject<Dinosaur>>(new[]
            {
                new FirebaseObject<Dinosaur>("lambeosaurus", new Dinosaur(2, 2, 2)),
                new FirebaseObject<Dinosaur>("stegosaurus", new Dinosaur(3, 3, 3))
            });

            var cache = new FirebaseCache<Dinosaur>(dinosaurs.ToDictionary(f => f.Key, f => f.Object));

            var stegosaurusds = @"
{
  ""height"" : 4,
  ""length"" : 4,
  ""weight"": 4
}";

            var entities = cache.PushData("/stegosaurus/ds", stegosaurusds).ToList();
            var stegosaurus = new FirebaseObject<Dinosaur>("stegosaurus", new Dinosaur(4, 4, 4));

            entities.Should().HaveCount(1);
            entities.First().ShouldBeEquivalentTo(stegosaurus);
        }

        [TestMethod]
        public void PrimitiveItemWhichBelongsToExistingObjectChanged()
        {
            // this should simulate when some primitive data of an existing object changed
            var dinosaurs = new List<FirebaseObject<Dinosaur>>(new[]
            {
                new FirebaseObject<Dinosaur>("lambeosaurus", new Dinosaur(2, 2, 2)),
                new FirebaseObject<Dinosaur>("stegosaurus", new Dinosaur(3, 3, 3))
            });

            var cache = new FirebaseCache<Dinosaur>(dinosaurs.ToDictionary(f => f.Key, f => f.Object));

            var height = "4";

            var entities = cache.PushData("/stegosaurus/ds/height", height).ToList();
            var stegosaurus = new FirebaseObject<Dinosaur>("stegosaurus", new Dinosaur(4, 3, 3));

            entities.Should().HaveCount(1);
            entities.First().ShouldBeEquivalentTo(stegosaurus);
        }

        [TestMethod]
        public void ObjectWithDictionaryPropertyInserted()
        {
            var jurassicPrague = new FirebaseObject<JurassicWorld>("jurassicPrague", new JurassicWorld());
            jurassicPrague.Object.Dinosaurs.Add("lambeosaurus", new Dinosaur(2, 2, 2));
            jurassicPrague.Object.Dinosaurs.Add("stegosaurus", new Dinosaur(3, 3, 3));

            var cache = new FirebaseCache<JurassicWorld>();
            var jurassicPragueJson = @"
{ 
  ""jurassicPrague"": {
      ""dinosaurs"" : {
          ""lambeosaurus"": {
            ""ds"": {
              ""height"" : 2,
              ""length"" : 2,
              ""weight"": 2
            }
          },
          ""stegosaurus"": {
            ""ds"": {
              ""height"" : 3,
              ""length"" : 3,
              ""weight"" : 3
            }
          }
        }
    }
}";
            var entities = cache.PushData("/", jurassicPragueJson).ToList();

            entities.Should().HaveCount(1);
            entities.First().ShouldBeEquivalentTo(jurassicPrague);
        }

        [TestMethod]
        public void ObjectWithinDictionaryChanged()
        {
            var jurassicPrague = new FirebaseObject<JurassicWorld>("jurassicPrague", new JurassicWorld());
            jurassicPrague.Object.Dinosaurs.Add("lambeosaurus", new Dinosaur(2, 2, 2));
            jurassicPrague.Object.Dinosaurs.Add("stegosaurus", new Dinosaur(3, 3, 3));

            var cache = new FirebaseCache<JurassicWorld>(new Dictionary<string, JurassicWorld>()
                { { jurassicPrague.Key, jurassicPrague.Object } }
            );

            var stegosaurusds = @"
{ 
    ""height"" : 4,
    ""length"" : 4,
    ""weight"": 4
}";
            var entities = cache.PushData("/jurassicPrague/dinosaurs/stegosaurus/ds", stegosaurusds).ToList();

            entities.Should().HaveCount(1);
            entities.First().ShouldBeEquivalentTo(jurassicPrague);
            jurassicPrague.Object.Dinosaurs["stegosaurus"].Dimensions.ShouldBeEquivalentTo(new Dimensions { Height = 4, Length = 4, Weight = 4 });
        }

        [TestMethod]
        public void ItemChangesInDictionaryOfPrimitiveBooleans()
        {
            var cache = new FirebaseCache<bool>();
            var boolDictionary = new [] { new FirebaseObject<bool>("a", true), new FirebaseObject<bool>("b", true), new FirebaseObject<bool>("c", false) };
            var bools = @"
{ 
    ""a"" : true,
    ""b"" : true,
    ""c"": false
}";

            var entities = cache.PushData("/", bools).ToList();
            entities.ShouldBeEquivalentTo(boolDictionary);

            entities = cache.PushData("/d", "true").ToList();
            entities.First().ShouldBeEquivalentTo(new FirebaseObject<bool>("d", true));

            entities = cache.PushData("/c", "true").ToList();
            entities.First().ShouldBeEquivalentTo(new FirebaseObject<bool>("c", true));
        }

        [TestMethod]
        public void ItemChangesInDictionaryOfPrimitiveStrings()
        {
            var cache = new FirebaseCache<string>();
            var stringDictionary = new[] { new FirebaseObject<string>("a", "a"), new FirebaseObject<string>("b", "b"), new FirebaseObject<string>("c", "c") };
            var strings = @"
{ 
    ""a"" : ""a"",
    ""b"" : ""b"",
    ""c"": ""c""
}";

            var entities = cache.PushData("/", strings).ToList();
            entities.ShouldBeEquivalentTo(stringDictionary);

            entities = cache.PushData("/d", @"""d""").ToList();
            entities.First().ShouldBeEquivalentTo(new FirebaseObject<string>("d", "d"));

            entities = cache.PushData("/c", @"""cc""").ToList();
            entities.First().ShouldBeEquivalentTo(new FirebaseObject<string>("c", "cc"));
        }

        [TestMethod]
        public void ItemDeletedInDictionary()
        {
            var cache = new FirebaseCache<Dinosaur>();
            var dinosaurs = @"{
  ""lambeosaurus"": {
    ""ds"": {
      ""height"" : 2,
      ""length"" : 2,
      ""weight"": 2
    }
  },
  ""stegosaurus"": {
    ""ds"": {
      ""height"" : 3,
      ""length"" : 3,
      ""weight"" : 3
    }
  }
}";

            var entities = cache.PushData("/", dinosaurs).ToList();

            // delete top level item from dictionary
            entities = cache.PushData("/stegosaurus", " ").ToList();

            cache.Count().ShouldBeEquivalentTo(1);
            entities.ShouldAllBeEquivalentTo(new[] { new FirebaseObject<Dinosaur>("stegosaurus", new Dinosaur(3, 3, 3)) });

            // delete a property - it should be set to null
            entities = cache.PushData("/lambeosaurus/ds", " ").ToList();

            cache.Count().ShouldBeEquivalentTo(1);
            entities.ShouldAllBeEquivalentTo(new[] { new FirebaseObject<Dinosaur>("lambeosaurus", new Dinosaur { Dimensions = null }) });
        }

        [TestMethod]
        public void ItemPatchedInDictionary()
        {
            var cache = new FirebaseCache<Dinosaur>();
            var dinosaurs = @"{
  ""lambeosaurus"": {
    ""ds"": {
      ""height"" : 2,
      ""length"" : 2,
      ""weight"": 2
    }
  },
  ""stegosaurus"": {
    ""ds"": {
      ""height"" : 3,
      ""length"" : 3,
      ""weight"" : 3
    }
  }
}";

            var patch = @"
{
    ""height"" : 8,
}
";

            var entities = cache.PushData("/", dinosaurs).ToList();

            // delete top level item from dictionary
            entities = cache.PushData("/stegosaurus/ds", patch).ToList();

            entities.First().ShouldBeEquivalentTo(new FirebaseObject<Dinosaur>("stegosaurus", new Dinosaur(8, 3, 3)));
        }
    }
}
