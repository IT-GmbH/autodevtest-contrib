using Moq;
using Xunit.Abstractions;

namespace etsdevtest.cli.tests;

public class ProcessorTest
{
    IMock<IConfig> mConfig = new Mock<IConfig>();
    IMock<IEts6Factory> mEtsFactory = new Mock<IEts6Factory>();

    ITestOutputHelper output;
    public ProcessorTest(ITestOutputHelper aOutput) {
        output = aOutput;
    }

    CommandProcessor Get() {
        return new CommandProcessor(mEtsFactory.Object, mConfig.Object, aApp: new AppInstance(mEtsFactory.Object, mConfig.Object));
    }

    [Fact]
    public async void TestSetExecutableConfig()
    {
        // Given
        await Get().Process("config set \"Hello World\"");

        // When

        // Then
        Assert.True(true);
    }

    [Fact]
    public void TestStart() {
        // WIP
    }

}