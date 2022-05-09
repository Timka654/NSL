using NSL.ConfigurationEngine.Providers.Json;
using NUnit.Framework;
using System.IO;

namespace ConfigurationEngine.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestJsonLoading()
        {
            var manager = new TestConfigurationManager(Path.Combine("Data", "jsonConfig.json"), new LoadingProvider());

            manager.ReloadWithDefaults();

            if (manager.GetAllValues().Count != 20)
                Assert.Fail();

            Assert.Pass();
        }

        [Test]
        public void TestJsonSave()
        {
            var manager = new TestConfigurationManager(Path.Combine("Data", "jsonConfig.json"), new LoadingProvider());

            manager.ReloadWithDefaults();
            manager.SaveData();

            Assert.Pass();
        }

        [Test]
        public void TestJsonLoadingInvalidPath()
        {
            var manager = new TestConfigurationManager(Path.Combine("Data", "jsonConfig_noex.json"), new LoadingProvider());

            if (manager.ReloadWithDefaults())
                Assert.Fail();

            Assert.Pass();
        }

        [Test]
        public void TestJsonLoadingNoProvider()
        {
            var manager = new TestConfigurationManager(Path.Combine("Data", "jsonConfig.json"), null);
            try
            {
                manager.ReloadWithDefaults();

                Assert.Fail();
            }
            catch (System.Exception)
            {
                Assert.Pass();
            }
        }
    }
}