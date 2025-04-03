using System;
using System.Text.Json;
using System.Collections.Generic;
using OpenFeature.Model;
using Xunit;
using Hyphen.OpenFeature.Provider;

namespace Hyphen.OpenFeature.Provider.Tests
{
    public class HyphenUtilsTests
    {
        [Fact]
        public void ConvertJsonElementToValue_String_ReturnsValue()
        {
            var json = "\"test\"";
            var element = JsonDocument.Parse(json).RootElement;

            var result = HyphenUtils.ConvertJsonElementToValue(element);

            Assert.Equal("test", result.AsString);
        }

        [Fact]
        public void ConvertJsonElementToValue_Number_ReturnsValue()
        {
            var json = "123";
            var element = JsonDocument.Parse(json).RootElement;

            var result = HyphenUtils.ConvertJsonElementToValue(element);

            Assert.Equal(123, result.AsInteger);
        }

        [Fact]
        public void ConvertJsonElementToValue_Boolean_ReturnsValue()
        {
            var json = "true";
            var element = JsonDocument.Parse(json).RootElement;

            var result = HyphenUtils.ConvertJsonElementToValue(element);

            Assert.True(result.AsBoolean);
        }

        [Fact]
        public void ConvertJsonElementToValue_Array_ReturnsValue()
        {
            var json = "[\"test\", 123, true]";
            var element = JsonDocument.Parse(json).RootElement;

            var result = HyphenUtils.ConvertJsonElementToValue(element);

            var list = result.AsList;
            Assert.Equal("test", list[0].AsString!);
            Assert.Equal(123, list[1].AsInteger);
            Assert.True(list[2].AsBoolean);
        }

        [Fact]
        public void ConvertJsonElementToValue_Object_ReturnsValue()
        {
            var json = "{\"key1\": \"value1\", \"key2\": 123}";
            var element = JsonDocument.Parse(json).RootElement;

            var result = HyphenUtils.ConvertJsonElementToValue(element);

            var structure = result.AsStructure;
            Assert.Equal("value1", structure["key1"].AsString!);
            Assert.Equal(123, structure["key2"].AsInteger);
        }

        [Fact]
        public void ConvertJsonElementToValue_Null_ReturnsValue()
        {
            var json = "null";
            var element = JsonDocument.Parse(json).RootElement;

            var result = HyphenUtils.ConvertJsonElementToValue(element);

            Assert.True(result.IsNull);
        }

        [Fact]
        public void ConvertValueToObject_String_ReturnsObject()
        {
            var value = new Value("test");

            var result = HyphenUtils.ConvertValueToObject(value);

            Assert.Equal("test", result);
        }

        [Fact]
        public void ConvertValueToObject_Number_ReturnsObject()
        {
            var value = new Value(123);

            var result = HyphenUtils.ConvertValueToObject(value);
            Assert.Equal(123, result);
        }

        [Fact]
        public void ConvertValueToObject_Boolean_ReturnsObject()
        {
            var value = new Value(true);

            var result = HyphenUtils.ConvertValueToObject(value);

            Assert.True((bool)result!);
        }

        [Fact]
        public void ConvertValueToObject_List_ReturnsObject()
        {
            var list = new List<Value> { new Value("test"), new Value(123), new Value(true) };
            var value = new Value(list);

            var result = HyphenUtils.ConvertValueToObject(value);

            var resultList = (List<object?>)result!;
            Assert.Equal("test", resultList[0]);
            Assert.Equal(123, resultList[1]);
            Assert.True((bool)resultList[2]!);
        }

        [Fact]
        public void ConvertValueToObject_Structure_ReturnsObject()
        {
            var structure = Structure.Builder()
                .Set("key1", new Value("value1"))
                .Set("key2", new Value(123))
                .Build();
            var value = new Value(structure);

            var result = HyphenUtils.ConvertValueToObject(value);

            var resultDict = (Dictionary<string, object?>)result!;
            Assert.Equal("value1", resultDict["key1"]);
            Assert.Equal(123, resultDict["key2"]);
        }

        [Fact]
        public void ConvertValueToObject_Null_ReturnsObject()
        {
            var value = new Value();

            var result = HyphenUtils.ConvertValueToObject(value);

            Assert.Null(result);
        }
    }
}
