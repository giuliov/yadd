using FluentAssertions;
using System.IO.Abstractions.TestingHelpers;
using Xunit;
using yadd.core;

namespace core.unit.tests
{
    public class ObjectIdsTests : TestMockDataBase
    {
        string pathToIdFile = @"c:\Temp\id_test";

        [Fact]
        public void Hash_Equals_Input()
        {
            var id = new BaselineId(TestHash1);

            id.Hash.Should().Be(TestHash1);
        }

        [Fact]
        public void Write_Succeeds()
        {
            var fs = new MockFileSystem();
            var id = new BaselineId(TestHash1);

            id.Write(pathToIdFile, fs);

            fs.FileExists(pathToIdFile).Should().BeTrue();
            fs.GetFile(pathToIdFile).TextContents.Should().Be(TestHash1);
        }

        [Fact]
        public void Read_ExistingFile_Succeeds()
        {
            var fs = new MockFileSystem();
            fs.AddFile(pathToIdFile, new MockFileData(TestHash1));

            var id = ObjectId.Read<BaselineId>(pathToIdFile, fs);

            id.Should().NotBeNull();
            id.Hash.Should().Be(TestHash1);
        }

        [Fact]
        public void Read_NonExistingFile_IsNull()
        {
            var fs = new MockFileSystem();

            var id = ObjectId.Read<BaselineId>(pathToIdFile, fs);

            id.Should().BeNull();
        }

        [Fact]
        public void Read_NonExistingDir_IsNull()
        {
            var fs = new MockFileSystem();
            fs.Directory.Delete(@"c:\Temp");

            var id = ObjectId.Read<BaselineId>(pathToIdFile, fs);

            id.Should().BeNull();
        }
    }
}
