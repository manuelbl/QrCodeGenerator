using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Internal;

namespace Net.Codecrete.QrCodeGenerator.Test;

public class SegmentCompactionTest
{
    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestCompaction(string text, int version, ECI eci, int expectedLength)
    {
        var utf8Data = eci.GetEncoding().GetBytes(text);
        var segments = SegmentCompaction.BuildSegments(utf8Data, version, eci.Equals(ECI.ShiftJIS));
        var bitLength = DataSegment.GetBitLength(segments, version);

        Assert.Equal(expectedLength, bitLength);
    }

    public static TheoryData<string, int, ECI, int> TestCases()
    {
        var data = new TheoryData<string, int, ECI, int>();
        GenerateTestCases().ForEach(testCase => data.Add(testCase.Text, testCase.Version, testCase.Eci, testCase.ExpectedLength));
        return data;
    }

    private static IEnumerable<TestCase> GenerateTestCases()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        yield return new TestCase("ABC", 10, ECI.UTF8, 32);
        yield return new TestCase("カリ12ゼーシ", 10, ECI.ShiftJIS, 116);
        yield return new TestCase("カリ123456ゼーシ", 10, ECI.ShiftJIS, 129);
        yield return new TestCase(RandomData.MakeString(40, seed: 9117), 13, ECI.UTF8, 354);
        yield return new TestCase(RandomData.MakeString(400, seed: 9117), 7, ECI.UTF8, 4678);
        yield return new TestCase(RandomData.MakeString(4000, seed: 9117), 20, ECI.UTF8, 43687);
        yield return new TestCase(RandomData.MakeString(7813, seed: 9117), 40, ECI.UTF8, 85359);
        yield return new TestCase(RandomData.MakeAlphanumericString(3123, seed: 9117), 17, ECI.UTF8, 17192);
        yield return new TestCase(RandomData.MakeString(2117, seed: 8172), 33, ECI.UTF8, 22842);
        yield return new TestCase(RandomData.MakeAlphanumericString(2632, seed: 3200), 35, ECI.Latin1, 14493);
    }

    private record TestCase(string Text, int Version, ECI Eci, int ExpectedLength);
}