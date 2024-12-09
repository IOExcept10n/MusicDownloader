using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SUSUProgramming.Tests
{
    public class DynamicJsonTests
    {
        [Fact]
        public void TestDynamicJson()
        {
            // Arrange
            const string data = """
{
    "Hello": "World",
    "Key": {
        "A": 1,
        "B": false
    }
}
""";
            // Act
            dynamic test = JsonConvert.DeserializeObject(data)!;
            dynamic nonExistingPath = test.NonExistingPath;
            int? yetAnotherPath = nonExistingPath?.YetAnotherPath;

            // Assert
            Assert.Null(yetAnotherPath);
        }
    }
}
