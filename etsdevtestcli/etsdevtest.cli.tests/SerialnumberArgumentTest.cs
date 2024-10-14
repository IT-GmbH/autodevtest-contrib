
namespace etsdevtest.cli.tests;

public class SerialnumberArgumentTest
{
    [Fact]
    public void TestStringToHex()
    {
        Assert.Equal(new SerialnumberArgument().StringToHex("00FD10F1C921"), new byte[] { 0x00, 0xFD, 0x10, 0xF1, 0xC9, 0x21 });

        Assert.Throws<ArgumentException>(() =>
        {
            new SerialnumberArgument().StringToHex("00FD10F1C9");
        });

        Assert.Throws<FormatException>(() =>
        {
            new SerialnumberArgument().StringToHex("00FD10F1C9X1");
        });
    }
}