using FluentAssertions;
using Translumo.Infrastructure.Collections;
using Xunit;

namespace Translumo.Infrastructure.Tests.Collections;

public class LimitedDictionaryTests
{
    [Fact]
    public void Constructor_WithValidCapacity_CreatesInstance()
    {
        // Arrange & Act
        var dictionary = new LimitedDictionary<string, int>(5, null);

        // Assert
        dictionary.Capacity.Should().Be(5);
        dictionary.Count.Should().Be(0);
        dictionary.BackupThresholdRatio.Should().Be(2);
    }

    [Fact]
    public void Constructor_WithInvalidCapacity_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => new LimitedDictionary<string, int>(0, null));
        Assert.Throws<ArgumentException>(() => new LimitedDictionary<string, int>(-1, null));
    }

    [Fact]
    public void Add_WithinCapacity_AddsItems()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);

        // Act
        dictionary.Add("key1", 1);
        dictionary.Add("key2", 2);

        // Assert
        dictionary.Count.Should().Be(2);
        dictionary["key1"].Should().Be(1);
        dictionary["key2"].Should().Be(2);
    }

    [Fact]
    public void Add_DuplicateKey_ThrowsArgumentException()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);
        dictionary.Add("key1", 1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => dictionary.Add("key1", 2));
    }

    [Fact]
    public void Indexer_Set_UpdatesExistingValue()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);
        dictionary.Add("key1", 1);

        // Act
        dictionary["key1"] = 10;

        // Assert
        dictionary["key1"].Should().Be(10);
        dictionary.Count.Should().Be(1);
    }

    [Fact]
    public void Indexer_Set_AddsNewValue()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);

        // Act
        dictionary["key1"] = 1;

        // Assert
        dictionary["key1"].Should().Be(1);
        dictionary.Count.Should().Be(1);
    }

    [Fact]
    public void Add_ExceedsCapacity_SwapsDictionaries()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(2, null);
        dictionary.Add("key1", 1);
        dictionary.Add("key2", 2);

        // Act - This should trigger dictionary swap
        dictionary.Add("key3", 3);

        // Assert
        dictionary.Count.Should().Be(1); // Only the new item should be in the main dictionary
        dictionary["key3"].Should().Be(3);
        dictionary.ContainsKey("key1").Should().BeFalse();
        dictionary.ContainsKey("key2").Should().BeFalse();
    }

    [Fact]
    public void BackupThreshold_AddsToBackupDictionary()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(4, null)
        {
            BackupThresholdRatio = 2 // Backup threshold = 4/2 = 2
        };

        // Act
        dictionary.Add("key1", 1);
        dictionary.Add("key2", 2); // Should trigger backup
        dictionary.Add("key3", 3); // Should also be in backup

        // Assert
        dictionary.Count.Should().Be(3);
        // We can't directly test backup dictionary, but we can test behavior
        dictionary.ContainsKey("key1").Should().BeTrue();
        dictionary.ContainsKey("key2").Should().BeTrue();
        dictionary.ContainsKey("key3").Should().BeTrue();
    }

    [Fact]
    public void Remove_ExistingKey_RemovesFromBothDictionaries()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(4, null);
        dictionary.Add("key1", 1);
        dictionary.Add("key2", 2);
        dictionary.Add("key3", 3);

        // Act
        var result = dictionary.Remove("key2");

        // Assert
        result.Should().BeTrue();
        dictionary.Count.Should().Be(2);
        dictionary.ContainsKey("key2").Should().BeFalse();
    }

    [Fact]
    public void Remove_NonExistentKey_ReturnsFalse()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);
        dictionary.Add("key1", 1);

        // Act
        var result = dictionary.Remove("nonexistent");

        // Assert
        result.Should().BeFalse();
        dictionary.Count.Should().Be(1);
    }

    [Fact]
    public void TryGetValue_ExistingKey_ReturnsTrue()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);
        dictionary.Add("key1", 42);

        // Act
        var result = dictionary.TryGetValue("key1", out var value);

        // Assert
        result.Should().BeTrue();
        value.Should().Be(42);
    }

    [Fact]
    public void TryGetValue_NonExistentKey_ReturnsFalse()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);

        // Act
        var result = dictionary.TryGetValue("nonexistent", out var value);

        // Assert
        result.Should().BeFalse();
        value.Should().Be(0); // Default value for int
    }

    [Fact]
    public void Clear_RemovesAllItems()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);
        dictionary.Add("key1", 1);
        dictionary.Add("key2", 2);

        // Act
        dictionary.Clear();

        // Assert
        dictionary.Count.Should().Be(0);
        dictionary.ContainsKey("key1").Should().BeFalse();
        dictionary.ContainsKey("key2").Should().BeFalse();
    }

    [Fact]
    public void Capacity_Set_ClearsAndUpdatesCapacity()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);
        dictionary.Add("key1", 1);
        dictionary.Add("key2", 2);

        // Act
        dictionary.Capacity = 5;

        // Assert
        dictionary.Capacity.Should().Be(5);
        dictionary.Count.Should().Be(0); // Should be cleared
    }

    [Fact]
    public void Capacity_SetInvalidValue_ThrowsArgumentException()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => dictionary.Capacity = 0);
        Assert.Throws<ArgumentException>(() => dictionary.Capacity = -1);
    }

    [Fact]
    public void Keys_ReturnsCorrectKeys()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);
        dictionary.Add("key1", 1);
        dictionary.Add("key2", 2);

        // Act
        var keys = dictionary.Keys;

        // Assert
        keys.Should().Contain("key1");
        keys.Should().Contain("key2");
        keys.Count.Should().Be(2);
    }

    [Fact]
    public void Values_ReturnsCorrectValues()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);
        dictionary.Add("key1", 10);
        dictionary.Add("key2", 20);

        // Act
        var values = dictionary.Values;

        // Assert
        values.Should().Contain(10);
        values.Should().Contain(20);
        values.Count.Should().Be(2);
    }

    [Fact]
    public void IsReadOnly_ReturnsFalse()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);

        // Act & Assert
        dictionary.IsReadOnly.Should().BeFalse();
    }

    [Fact]
    public void GetEnumerator_IteratesOverItems()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(3, null);
        dictionary.Add("key1", 1);
        dictionary.Add("key2", 2);

        // Act
        var items = new List<KeyValuePair<string, int>>();
        foreach (var item in dictionary)
        {
            items.Add(item);
        }

        // Assert
        items.Should().HaveCount(2);
        items.Should().Contain(new KeyValuePair<string, int>("key1", 1));
        items.Should().Contain(new KeyValuePair<string, int>("key2", 2));
    }

    [Fact]
    public void BackupThresholdRatio_CanBeModified()
    {
        // Arrange
        var dictionary = new LimitedDictionary<string, int>(10, null);

        // Act
        dictionary.BackupThresholdRatio = 5;

        // Assert
        dictionary.BackupThresholdRatio.Should().Be(5);
    }

    [Fact]
    public void WithCustomEqualityComparer_UsesComparer()
    {
        // Arrange
        var comparer = StringComparer.OrdinalIgnoreCase;
        var dictionary = new LimitedDictionary<string, int>(3, comparer);

        // Act
        dictionary.Add("KEY1", 1);
        dictionary.Add("key2", 2);

        // Assert
        dictionary.ContainsKey("key1").Should().BeTrue(); // Case insensitive
        dictionary.ContainsKey("KEY2").Should().BeTrue(); // Case insensitive
        dictionary["key1"].Should().Be(1);
        dictionary["KEY2"].Should().Be(2);
    }
}