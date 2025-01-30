using Xunit;
using FluentAssertions;

namespace MyJsonParser
{
    public class JsonParserTests
    {
        [Fact]
        public void Brackets_Are_Valid_Json()
        {
            string filePath = @"TestSteps/Step1/valid.json";

            string content = File.ReadAllText(filePath);

            JsonParser.TryParse(content, out var result).Should().BeTrue();
            result.Should().NotBeNull();
        }

        [Fact]
        public void Empty_String_Is_Invalid_Json()
        {
            string filePath = @"TestSteps/Step1/invalid.json";

            string content = File.ReadAllText(filePath);

            JsonParser.TryParse(content, out var result).Should().BeFalse();
            result.Should().BeNull();
        }

        [Theory]
        [InlineData("12 ", 12)]
        [InlineData(" 34", 34)]
        [InlineData(" 56 ", 56)]
        public void ParseNumber(string valueType, int expected)
        {
            JsonParser.TryParse(valueType, out var result).Should().BeFalse();
        }

        [Theory]
        [InlineData("{\"key\": test}")]
        [InlineData("{\"key\": tree }")]
        [InlineData("{\"key\": temp}")]
        [InlineData("{\"key\": famin}")]
        [InlineData("{\"key\": feast}")]
        [InlineData("{\"key\": note}")]
        [InlineData("{\"key\": nllu}")]
        public void ParseInvalidTokens(string valueType)
        {
            JsonParser.TryParse(valueType, out var result).Should().BeFalse();
        }

        [Fact]
        public void ParseNull()
        {
            JsonParser.TryParse("{\"key\": null}", out var result).Should().BeTrue();
            result.As<Dictionary<string, object?>>()["key"].Should().BeNull();

            JsonParser.TryParse("{\"key\": null }", out var res2).Should().BeTrue();
            res2.As<Dictionary<string, object?>>()["key"].Should().BeNull();
        }

        [Theory]
        [InlineData("{\"key\": true}", true)]
        [InlineData("{\"key\": true }", true)]
        [InlineData("{\"key\": false}", false)]
        [InlineData("{\"key\": false }", false)]
        public void ParseBoolean(string valueType, bool expected)
        {
            JsonParser.TryParse(valueType, out var result).Should().BeTrue();
            result.As<Dictionary<string, object?>>()["key"].Should().Be(expected);
        }

        [Theory]
        [InlineData("[\"hello\", \"world\"]", new string[] { "hello", "world" })]
        [InlineData("[\"hello\"]", new string[] { "hello"})]
        [InlineData("[\"hello\", 123, null, true, false]", new object[] { "hello", 123, null, true, false })]
        public void ParseArray(string array, object?[] expected)
        {
            JsonParser.TryParse(array, out var result).Should().BeTrue();
            for (int i = 0; i < expected.Length; i++)
            {
                result.As<object[]>()[i].Should().Be(expected[i]);
            }
        }

        [Theory]
        [InlineData("TestSteps/Step2/valid.json", 1)]
        [InlineData("TestSteps/Step2/valid2.json", 2)]
        public void ParseValidJsonStep2(string filePath, int expectedCount)
        {
            string content = File.ReadAllText(filePath);

            JsonParser.TryParse(content, out var result).Should().BeTrue();
            result.Should().NotBeNull();

            result.As<Dictionary<string, object?>>()["key"].Should().Be("value");

            for (int i = 1; i < expectedCount; i++)
            {
                result.As<Dictionary<string, object?>>()[$"key{expectedCount}"].Should().Be("value");
            }
        }

        [Theory]
        [InlineData("TestSteps/Step2/invalid.json")]
        [InlineData("TestSteps/Step2/invalid2.json")]
        public void ParseInValidJsonStep2(string filePath)
        {
            string content = File.ReadAllText(filePath);

            JsonParser.TryParse(content, out var result).Should().BeFalse();
        }

        [Fact]
        public void ParseValidJsonStep3()
        {
            string content = File.ReadAllText(@"TestSteps/Step3/valid.json");

            JsonParser.TryParse(content, out var result).Should().BeTrue();

            result.As<Dictionary<string, object?>>()["key1"].Should().Be(true);
            result.As<Dictionary<string, object?>>()["key2"].Should().Be(false);
            result.As<Dictionary<string, object?>>()["key3"].Should().Be(null);
            result.As<Dictionary<string, object?>>()["key4"].Should().Be("value");
            result.As<Dictionary<string, object?>>()["key5"].Should().Be(101);         
        }

        [Fact]
        public void ParseInValidJsonStep3()
        {
            string content = File.ReadAllText(@"TestSteps/Step3/invalid.json");

            JsonParser.TryParse(content, out var result).Should().BeFalse();
        }

        [Fact]
        public void ParseValid1JsonStep4()
        {
            string content = File.ReadAllText(@"TestSteps/Step4/valid.json");

            JsonParser.TryParse(content, out var result).Should().BeTrue();

            result.As<Dictionary<string, object?>>()["key"].Should().Be("value");
            result.As<Dictionary<string, object?>>()["key-n"].Should().Be(101);
            result.As<Dictionary<string, object?>>()["key-o"].Should().NotBeNull();
            result.As<Dictionary<string, object?>>()["key-o"].Should().BeOfType<Dictionary<string, object?>?>();
            result.As<Dictionary<string, object?>>()["key-l"].Should().BeOfType<object?[]>();
        }
        
        [Fact]
        public void ParseValid2JsonStep4()
        {
            string content = File.ReadAllText(@"TestSteps/Step4/valid2.json");

            JsonParser.TryParse(content, out var result).Should().BeTrue();

            result.As<Dictionary<string, object?>>()["key"].Should().Be("value");
            result.As<Dictionary<string, object?>>()["key-n"].Should().Be(101);
            result.As<Dictionary<string, object?>>()["key-o"].Should().NotBeNull();
            result.As<Dictionary<string, object?>>()["key-o"].Should().BeOfType<Dictionary<string, object?>?>();
            result.As<Dictionary<string, object?>>()["key-o"]?.As<Dictionary<string, object?>>()["inner key"].Should().BeOfType<string>();
            result.As<Dictionary<string, object?>>()["key-o"]?.As<Dictionary<string, object?>>()["inner key"].Should().Be("inner value");

            result.As<Dictionary<string, object?>>()["key-l"].Should().BeOfType<object?[]>();
            result.As<Dictionary<string, object?>>()["key-l"].As<object?[]>()[0].Should().Be("list value");
        }

        [Fact]
        public void ParseInValidJsonStep4()
        {
            string content = File.ReadAllText(@"TestSteps/Step4/invalid.json");

            JsonParser.TryParse(content, out var result).Should().BeFalse();
        }

        [Theory]
        [InlineData("TestSteps/Step6/valid.json")]
        [InlineData("TestSteps/Step6/valid2.json")]
        public void ParseValidJsonStep6(string filePath)
        {
            string content = File.ReadAllText(filePath);

            JsonParser.TryParse(content, out var result).Should().BeTrue();
            result.Should().NotBeNull();
        }

        [Theory]
        [InlineData("TestSteps/Step5/fail1.json")]
        [InlineData("TestSteps/Step5/fail2.json")]
        [InlineData("TestSteps/Step5/fail3.json")]
        [InlineData("TestSteps/Step5/fail4.json")]
        [InlineData("TestSteps/Step5/fail5.json")]
        [InlineData("TestSteps/Step5/fail6.json")]
        [InlineData("TestSteps/Step5/fail7.json")]
        [InlineData("TestSteps/Step5/fail8.json")]
        [InlineData("TestSteps/Step5/fail9.json")]
        [InlineData("TestSteps/Step5/fail10.json")]
        [InlineData("TestSteps/Step5/fail11.json")]
        [InlineData("TestSteps/Step5/fail12.json")]
        [InlineData("TestSteps/Step5/fail13.json")]
        [InlineData("TestSteps/Step5/fail14.json")]
        [InlineData("TestSteps/Step5/fail15.json")]
        [InlineData("TestSteps/Step5/fail16.json")]
        [InlineData("TestSteps/Step5/fail17.json")]
        [InlineData("TestSteps/Step5/fail18.json")]
        [InlineData("TestSteps/Step5/fail19.json")]
        [InlineData("TestSteps/Step5/fail20.json")]
        [InlineData("TestSteps/Step5/fail21.json")]
        [InlineData("TestSteps/Step5/fail22.json")]
        [InlineData("TestSteps/Step5/fail23.json")]
        [InlineData("TestSteps/Step5/fail24.json")]
        [InlineData("TestSteps/Step5/fail25.json")]
        [InlineData("TestSteps/Step5/fail26.json")]
        [InlineData("TestSteps/Step5/fail27.json")]
        [InlineData("TestSteps/Step5/fail28.json")]
        [InlineData("TestSteps/Step5/fail29.json")]
        [InlineData("TestSteps/Step5/fail30.json")]
        [InlineData("TestSteps/Step5/fail31.json")]
        [InlineData("TestSteps/Step5/fail32.json")]
        [InlineData("TestSteps/Step5/fail33.json")]
        public void ParseInValidJsonStep5(string filePath)
        {
            string content = File.ReadAllText(filePath);

            JsonParser.TryParse(content, out var result).Should().BeFalse();
        }

        [Theory]
        [InlineData("TestSteps/Step5/pass1.json")]
        [InlineData("TestSteps/Step5/pass2.json")]
        [InlineData("TestSteps/Step5/pass3.json")]
        public void ParseValidJsonStep5(string filePath)
        {
            string content = File.ReadAllText(filePath);

            JsonParser.TryParse(content, out var result).Should().BeTrue();
        }





    }
}
