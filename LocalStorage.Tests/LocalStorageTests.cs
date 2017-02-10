using FluentAssertions;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LocalStorage.Tests.Stubs;
using Xunit;

namespace LocalStorage.Tests
{
    public class LocalStorageTests
    {
        [Fact(DisplayName = "LocalStorage should be initializable")]
        public void LocalStorage_Should_Be_Initializable()
        {
            var target = new LocalStorage();
            target.Should().NotBeNull();
        }

        [Fact(DisplayName = "LocalStorage.Store() should persist simple string")]
        public void LocalStorage_Store_Should_Persist_Simple_String()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var expectedValue = "I-AM-GROOT";
            var storage = new LocalStorage();

            // act
            storage.Store(key, expectedValue);
            storage.Persist();

            // assert
            var target = storage.Get(key);
            target.Should().BeOfType<string>();
            target.Should().Be(expectedValue);
        }

        [Fact(DisplayName = "LocalStorage.Store() should persist simple DateTime as struct")]
        public void LocalStorage_Store_Should_Persist_Simple_DateTime_As_Struct()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var expectedValue = DateTime.Now;
            var storage = new LocalStorage();

            // act
            storage.Store(key, expectedValue);
            storage.Persist();

            // assert
            var target = storage.Get<DateTime>(key);
            target.Should().Be(expectedValue);
        }

        [Fact(DisplayName = "LocalStorage.Store() should persist and retrieve correct type")]
        public void LocalStorage_Store_Should_Persist_And_Retrieve_Correct_Type()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            var value = (double) 42.4m;
            var storage = new LocalStorage();

            // act
            storage.Store(key, value);
            storage.Persist();

            // assert
            var target = storage.Get<double>(key);
            target.Should().Be(value);
        }

        [Fact(DisplayName = "LocalStorage.Store() should persist multiple values")]
        public void LocalStorage_Store_Should_Persist_Multiple_Values()
        {
            // arrange - create multiple values, of different types
            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();
            var key3 = Guid.NewGuid().ToString();
            var value1 = "It was the best of times, it was the worst of times.";
            var value2 = DateTime.Now;
            var value3 = Int32.MaxValue;
            var storage = new LocalStorage();

            // act
            storage.Store(key1, value1);
            storage.Store(key2, value2);
            storage.Store(key3, value3);
            storage.Persist();

            // assert
            var target1 = storage.Get<string>(key1);
            var target2 = storage.Get<DateTime>(key2);
            var target3 = storage.Get<int>(key3);

            target1.Should().Be(value1);
            target2.Should().Be(value2);
            target3.Should().Be(value3);
        }

        [Fact(DisplayName = "LocalStorage.Clear() should clear all content")]
        public void LocalStorage_Clear_Should_Clear_All_Content()
        {
            // arrange - make sure something is stored in the LocalStorage
            var storage = new LocalStorage();
            var filepath = Helpers.GetLocalStoreFilePath();
            var key = Guid.NewGuid().ToString();
            var value = Guid.NewGuid();
            storage.Store(key, value);
            storage.Persist();

            // act - clear the store
            storage.Clear();

            // assert - open the file here and make sure the contents are empty
            var target = File.ReadAllText(filepath);
            target.Should().BeNullOrEmpty();
        }

        [Fact(DisplayName = "LocalStorage.Persist() should leave previous entries intact")]
        public void LocalStorage_Persist_Should_Leave_Previous_Entries_Intact()
        {
            // arrange - add an arbitrary item and persist
            var storage = new LocalStorage();
            var key1 = Guid.NewGuid().ToString();
            var value1 = "Some kind of monster";
            storage.Store(key1, value1);
            storage.Persist();

            // act - add a second item
            var key2 = Guid.NewGuid().ToString();
            var value2 = "Some kind of monster";
            storage.Store(key2, value2);
            storage.Persist();

            // assert - prove that both items remain intact
            var target1 = storage.Get<string>(key1);
            var target2 = storage.Get<string>(key2);
            target1.Should().Be(value1);
            target2.Should().Be(value2);
        }

        [Fact(DisplayName = "LocalStorage should remain intact between multiple instances")]
        public void LocalStorage_Should_Remain_Intact_Between_Multiple_Instances()
        {
            // arrange - add an arbitrary item and persist
            var storage1 = new LocalStorage();
            var key1 = Guid.NewGuid().ToString();
            var value1 = "Robert Baratheon";
            storage1.Store(key1, value1);
            storage1.Persist();

            // act - create a second instance of the LocalStorage,
            // and persist some more stuff
            var storage2 = new LocalStorage();
            var key2 = Guid.NewGuid().ToString();
            var value2 = "Ned Stark";
            storage2.Store(key2, value2);
            storage2.Persist();

            // assert - prove that entries from both instances still exist
            var storage3 = new LocalStorage();
            var target1 = storage3.Get<string>(key1);
            var target2 = storage3.Get<string>(key2);
            target1.Should().Be(value1);
            target2.Should().Be(value2);
        }

        [Fact(DisplayName = "LocalStorage should support unicode")]
        public void LocalStorage_Store_Should_Support_Unicode()
        {
            // arrange
            var key = Guid.NewGuid().ToString();
            const string expectedValue = "Juliën's Special Characters: ~!@#$%^&*()œōøęsæ";
            var storage = new LocalStorage();

            // act
            storage.Store(key, expectedValue);
            storage.Persist();

            // assert
            var target = storage.Get(key);
            target.Should().BeOfType<string>();
            target.Should().Be(expectedValue);
        }

        [Fact(DisplayName = "LocalStorage should perform decently with large collections")]
        public void LocalStorage_Should_Perform_Decently_With_Large_Collections()
        {
            // arrange - create a larger collection (100K records)
            var stopwatch = Stopwatch.StartNew();
            var storage = new LocalStorage();
            for (var i = 0; i < 100000; i++)
                storage.Store(Guid.NewGuid().ToString(), i);

            storage.Persist();

            // act - create new instance (e.g. load the larger collection from disk)
            var target = new LocalStorage();
            target.Clear();
            stopwatch.Stop();

            // assert - make sure the entire operation is done in < 1sec. (psychological boundry, if you will)
            stopwatch.ElapsedMilliseconds.Should().BeLessOrEqualTo(1000);
        }

        [Fact(DisplayName = "LocalStorage.Query() should cast to a collection")]
        public void LocalStorage_Query_Should_Cast_Response_To_Collection()
        {
            // arrange - persist a collection to storage
            var collection = CarFactory.Create();
            var storage = new LocalStorage();
            var key = Guid.NewGuid().ToString();
            var expected_amount = collection.Count();
            storage.Store(key, collection);

            // act - fetch directly as a collection, passing along a where-clause
            var target = storage.Query<Car>(key);

            // assert
            target.Should().NotBeNull();
            target.Count().Should().Be(expected_amount);
        }

        [Fact(DisplayName = "LocalStorage.Query() should respect a provided predicate")]
        public void LocalStorage_Query_Should_Respect_Provided_Predicate()
        {
            // arrange - persist a collection to storage
            var collection = CarFactory.Create();
            var storage = new LocalStorage();
            var key = Guid.NewGuid().ToString();
            var expected_brand = "BMW";
            var expected_amount = collection.Count(c => c.Brand == expected_brand);
            storage.Store(key, collection);

            // act - fetch directly as a collection, passing along a where-clause
            var target = storage.Query<Car>(key, c => c.Brand == expected_brand);

            // assert
            target.Should().NotBeNull();
            target.Count().Should().Be(expected_amount);
            target.All(c => c.Brand == expected_brand);
        }

        [Fact]
        public void LocalStorage_AsEnumerable_Should_Cast_IEnumerable()
        {
            var storage = new LocalStorage();
        }
    }
}
