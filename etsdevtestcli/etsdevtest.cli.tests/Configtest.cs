
using Moq;
using Xunit.Abstractions;

namespace etsdevtest.cli.tests;

public class ConfigTest : IDisposable
{
    Mock<IEts6Factory> mEtsFactory = new Mock<IEts6Factory>();

    Config mConfig;

    ITestOutputHelper output;

    public ConfigTest(ITestOutputHelper aOutput)
    {
        output = aOutput;
        mConfig = new Config(mEtsFactory.Object);
        mConfig.Clear();
    }

    public void Dispose()
    {
        mConfig.Clear();
    }

    [Fact]
    public void TestConfigGetDescription()
    {
        Assert.Equal("projectstore", mConfig.GetString(IConfig.Types.ProjectStore));
        Assert.Equal("executable", mConfig.GetString(IConfig.Types.ExecutablePath));
    }

    [Fact]
    public void TestSetAndGetConfig()
    {
        // Given
        string expected = "value";

        // When
        mConfig.Set(IConfig.Types.ProjectStore, expected);
        string actual = mConfig.Get(IConfig.Types.ProjectStore);

        // Then
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void TestClear()
    {
        mConfig.Set(IConfig.Types.ProjectStore, "cool");
        mConfig.Clear();

        Assert.Equal(String.Empty, mConfig.Get(IConfig.Types.ProjectStore));
    }

    [Fact]
    public void TestGetDefault()
    {
        // Given
        string defaultValue = "Hello";

        // When
        string actual = mConfig.Get(IConfig.Types.ExecutablePath, defaultValue);

        // Then
        Assert.Equal(defaultValue, actual);
    }

    [Fact]
    public void TestGetEts6DefaultValues()
    {
        string expected = "ActualEts6Value";
        mEtsFactory.SetupGet(x => x.ProjectStore).Returns(expected);

        mConfig = new Config(mEtsFactory.Object);

        output.WriteLine($"mEtsFactory.Object.ProjectStore: '{mEtsFactory.Object.ProjectStore}'");

        Assert.Equal(expected, mConfig.Get(IConfig.Types.ProjectStore));
    }
}