using System;
using System.IO;
using FluentAssertions;
using Hanssens.Net;
using Hanssens.Net.Helpers;
using Microsoft.VisualStudio.TestPlatform.Utilities.Helpers;
using Xunit;

namespace LocalStorageTests
{
    public class LocalStorageConfigurationTests
    {
        [Fact(DisplayName = "LocalStorage should not be initializable with null for configuration")]
        public void LocalStorage_Should_Not_Be_Initializable_With_Argument_Null()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var target = new LocalStorage(null);
            });
        }

        [Fact(DisplayName = "LocalStorageConfiguration should respect custom filename")]
        public void LocalStorageConfiguration_Should_Respect_Custom_Filename()
        {
            // arrange - configure localstorage to use a custom filename
            var random_filename = Guid.NewGuid().ToString("N");
            var config = new LocalStorageConfiguration() {
                Filename = random_filename
            };

            // act - store the container
            var storage = new Hanssens.Net.LocalStorage(config);
            storage.Persist();
            var target = FileHelpers.GetLocalStoreFilePath(random_filename);

            // assert
            File.Exists(target).Should().BeTrue();

            // cleanup
            storage.Destroy();
        }

        [Fact(DisplayName = "LocalStorageConfiguration.AutoLoad should load persisted state when enabled", Skip = "TODO")]
        public void LocalStorageConfiguration_AutoLoad_Should_Load_Previous_State_OnLoad()
        {
            throw new NotImplementedException();
        }

        [Fact(DisplayName = "LocalStorageConfiguration.AutoLoad should skip loading persisted state when disabled", Skip = "TODO")]
        public void LocalStorageConfiguration_AutoLoad_Should_Skip_Loading_Previous_State_OnLoad()
        {
            throw new NotImplementedException();
        }

        [Fact(DisplayName = "LocalStorageConfiguration.AutoSave should save changes to disk when enabled", Skip = "TODO")]
        public void LocalStorageConfiguration_AutoSave_Should_Persist_When_Enabled()
        {
            throw new NotImplementedException();
        }

        [Fact(DisplayName = "LocalStorageConfiguration.AutoSave should not save changes to disk when disabled", Skip = "TODO")]
        public void LocalStorageConfiguration_AutoSave_Should_Not_Persist_When_Disabled()
        {
            throw new NotImplementedException();
        }
    }
}
