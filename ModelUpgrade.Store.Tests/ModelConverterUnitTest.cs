using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModelUpgrade.Store.Tests
{
    [TestClass]
    public class ModelConverterUnitTest
    {
        [TestMethod]
        public void DataStoreUpgradeTest()
        {
            // Sample data.
            var v1Model = new Version1
            {
                Uid = "TestV1",
                Name = "Test1"
            };

            // Create a model upgrade chain, this chain must from oldest version to latest version.
            var v1UpgradeChain = new MyVersion1To2Upgrade();
            var v2UpgradeChain = new MyVersion2To3Upgrade(v1UpgradeChain);

            // Create a converter.
            var modelSerializer = new MyModelSerializer();
            var converter = new ModelConverter<Version3>(modelSerializer, v2UpgradeChain);

            // Sample data, it's from database.
            var v1DbData = new DataModel(v1Model, modelSerializer.Serialize);

            // Parses your saved data to the v3 model.
            var v3ModelFromConvert = converter.Parse(v1DbData);

            // Parses v3 model to data model for saving.
            var v3DbModel = converter.Parse(v3ModelFromConvert);

            Assert.AreEqual(v1Model.Uid, v3DbModel.Id);
        }
    }

    class MyVersion1To2Upgrade : ModelUpgrade<Version1, Version2>
    {
        protected override Version2 UpgradeFunc(Version1 model) => new Version2
        {
            Id = model.Uid,
            ProjectName = model.Name
        };

        public MyVersion1To2Upgrade() : base(null)
        {
        }
    }

    class MyVersion2To3Upgrade : ModelUpgrade<Version2, Version3>
    {
        protected override Version3 UpgradeFunc(Version2 model) => new Version3
        {
            ProjectId = model.Id,
            ProjectName = model.ProjectName
        };

        public MyVersion2To3Upgrade(params ModelUpgradeChain<Version2>[] nextChains) : base(nextChains)
        {
        }
    }

    class MyModelSerializer : IModelSerializer
    {
        public T Deserialize<T>(string s)
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(s);
        }

        public string Serialize(object model)
        {
            return System.Text.Json.JsonSerializer.Serialize(model);
        }
    }

    class Version1 : IVersionModel
    {
        public string Uid { get; set; }
        public string Name { get; set; }
        public string GetId()
        {
            return Uid;
        }

        public string GetModelName()
        {
            return GetType().FullName;
        }
    }

    class Version2 : IVersionModel
    {
        public string Id { get; set; }
        public string ProjectName { get; set; }
        public string GetId()
        {
            return Id;
        }

        public string GetModelName()
        {
            return GetType().FullName;
        }
    }

    class Version3 : IVersionModel
    {
        public string ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string GetId()
        {
            return ProjectId;
        }

        public string GetModelName()
        {
            return GetType().FullName;
        }
    }
}
