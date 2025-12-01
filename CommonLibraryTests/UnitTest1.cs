using System.Reflection;
using System.Xml.Linq;
using CommonLibrary;
using Xunit;
using Xunit.Sdk;

namespace CommonLibraryTests
{
    public class ConfigSerializationUnitTests : IDisposable
    {
        private readonly ConfigSerilization serializer;
        string _configPath = Pathing.ExecutablePath + "unit_test_config.xml";

        public ConfigSerializationUnitTests()
        {
            // Constructor replaces [SetUp]
            serializer = new ConfigSerilization("unit_test_config.xml", true);
            Console.WriteLine("Pathing.Project        " + _configPath);

            // Ensure clean file before each test
            if (File.Exists(_configPath))
            {
                File.Delete(_configPath);
            }
        }

        [Fact]
        public void WriteAndReadXmlConfigTest()
        {
            Logging log = new Logging(LogLevel.TRACE);
            log.Trace(MethodBase.GetCurrentMethod().ToString());

            // 1. Create test object
            var original = new TestConfig
            {
                Name = "UnitTest",
                Value = 999,
                Nested = new NestedConfig
                {
                    IsEnabled = true,
                    Description = "Nested object for unit test"
                }
            };

            serializer.WriteXmlConfig(original);
            Assert.True(File.Exists(_configPath), "XML file should exist after serialization");
            string xmlContent = File.ReadAllText(_configPath);
            Assert.Contains("<Name>UnitTest</Name>", xmlContent);
            Assert.Contains("<Description>Nested object for unit test</Description>", xmlContent);

            var deserialized = serializer.ReadXmlConfig<TestConfig>();

            Assert.NotNull(deserialized);
            Assert.Equal(original.Name, deserialized.Name);
            Assert.Equal(original.Value, deserialized.Value);
            Assert.NotNull(deserialized.Nested);
            Assert.Equal(original.Nested.IsEnabled, deserialized.Nested.IsEnabled);
            Assert.Equal(original.Nested.Description, deserialized.Nested.Description);
        }

        [Fact]
        public void XmlSerializer_WritesCorrectElementValues()
        {
            // Arrange
            var original = new TestConfig
            {
                Name = "UnitTest",
                Value = 999,
                Nested = new NestedConfig
                {
                    IsEnabled = true,
                    Description = "Nested object for unit test"
                }
            };

            // Act
            serializer.WriteXmlConfig(original);

            // Assert
            var xml = XDocument.Load(_configPath);

            Assert.Equal("UnitTest", xml.Root?.Element("Name")?.Value);
            Assert.Equal("999", xml.Root?.Element("Value")?.Value);

            var nested = xml.Root?.Element("Nested");
            Assert.Equal("true", nested?.Element("IsEnabled")?.Value);
            Assert.Equal("Nested object for unit test", nested?.Element("Description")?.Value);
        }

        [Fact]
        public void XmlSerializer_WritesRequiredXmlElements()
        {
            // Arrange
            var original = new TestConfig
            {
                Name = "UnitTest",
                Value = 999,
                Nested = new NestedConfig
                {
                    IsEnabled = true,
                    Description = "Nested object for unit test"
                }
            };

            // Act
            serializer.WriteXmlConfig(original);

            // Assert
            var xml = XDocument.Load(_configPath);
            var root = xml.Root;

            Assert.NotNull(root?.Element("Name"));
            Assert.NotNull(root?.Element("Value"));

            var nested = root?.Element("Nested");
            Assert.NotNull(nested);
            Assert.NotNull(nested?.Element("IsEnabled"));
            Assert.NotNull(nested?.Element("Description"));
        }

        [Fact]
        public void XmlSerializer_RoundTripPreservesValues()
        {
            // Arrange
            var original = new TestConfig
            {
                Name = "UnitTest",
                Value = 999,
                Nested = new NestedConfig
                {
                    IsEnabled = true,
                    Description = "Nested object for unit test"
                }
            };

            // Act
            serializer.WriteXmlConfig(original);
            var reloaded = serializer.ReadXmlConfig<TestConfig>();

            // Assert
            Assert.Equal(original.Name, reloaded.Name);
            Assert.Equal(original.Value, reloaded.Value);
            Assert.Equal(original.Nested.IsEnabled, reloaded.Nested.IsEnabled);
            Assert.Equal(original.Nested.Description, reloaded.Nested.Description);
        }


        public void Dispose()
        {
            if (File.Exists(_configPath))
            {
                // File.Delete(serializer._defaultFileLocation);
            }
        }
    }

    public class GeneralTests
    {
        private readonly Configuration _config;
        private readonly Logging _log;

        public GeneralTests()
        {
            _config = new Configuration();
            _log = new Logging(LogLevel.FATAL);
            _log.Trace("Test!");
        }

        [Fact]
        public void PrintPaths()
        {
            Console.WriteLine("Pathing.Project        " + Pathing.ProjectFile);
            Console.WriteLine("Pathing.Solution       " + Pathing.SolutionFile);
            Console.WriteLine("Pathing.Executable     " + Pathing.ExecutableFile);
            Console.WriteLine("Pathing.DllFile        " + Pathing.DllFile);
            Console.WriteLine("Pathing.ExecutablePath " + Pathing.ExecutablePath);
            Console.WriteLine("Pathing.XmlFile        " + Pathing.XmlFile);
            Console.WriteLine("Pathing.XmlFilePath    " + Pathing.XmlFilePath);

            Assert.True(true); // xUnit has no Assert.Pass(), just assert something
        }

        [Fact]
        public void IsProjectDir()
        {
            foreach (string dFile in Directory.GetFiles(Pathing.ProjectPath))
            {
                if (Path.GetExtension(dFile).Equals(".csproj", StringComparison.OrdinalIgnoreCase))
                {
                    return; // Test passes
                }
            }
            throw new XunitException("No .csproj file found in project directory");
        }

        [Fact]
        public void IsSolutionDir()
        {
            foreach (string dFile in Directory.GetFiles(Pathing.SolutionPath))
            {
                if (Path.GetExtension(dFile).Equals(".sln", StringComparison.OrdinalIgnoreCase))
                {
                    return; // Test passes
                }
            }
            throw new XunitException("No .sln file found in project directory");
        }
    }
}
/*
| Assertion                                             | Purpose                        |
| ----------------------------------------------------- | ------------------------------ |
| `Assert.AreEqual(expected, actual)`                   | Check equality.                |
| `Assert.AreNotEqual(notExpected, actual)`             | Check inequality.              |
| `Assert.IsTrue(condition)`                            | Condition must be true.        |
| `Assert.IsFalse(condition)`                           | Condition must be false.       |
| `Assert.IsNull(obj)`                                  | Object must be null.           |
| `Assert.IsNotNull(obj)`                               | Object must not be null.       |
| `Assert.Throws<TException>(delegate)`                 | Verify an exception is thrown. |
| `Assert.That(actual, Is.EqualTo(expected))`           | Constraint-based style.        |
| `Assert.That(actual, Is.Not.Null.And.GreaterThan(0))` | Combined constraints.          |
*/
