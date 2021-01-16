using FluentAssertions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;
using yadd.core;

namespace core.unit.tests
{
    public class BaselineRefTests : TestMockDataBase
    {
        [Fact]
        public void InvalidTag_Fails()
        {
            var fs = new MockFileSystem();
            var references = new References(TestRootDir, fs);
            references.Invoking(x => new BaselineRef("$bad!"))
                .Should().Throw<System.Exception>();
        }

        [Fact]
        public void Resolve_ExistingBaseline_ExactMatch_Succeeds()
        {
            var fs = new MockFileSystem();
            var references = new References(TestRootDir, fs);
            references.Invoking(x => new BaselineRef(TestHash1))
                .Should().Throw<System.Exception>();
        }
    }
}
