using System.Collections.Generic;
using EngineNS.IO;
using NUnit.Framework;

namespace EngineNS.Tests.IO;

[TestFixture]
public class SerializerTests
{
    const float Tolerance = 1e-6f;

    [Test]
    public void JsonRoundTrip_NestedObjectAndCollections_PreservesValues()
    {
        var source = new SerializerModel
        {
            A = 2,
            B = 3.0f,
            C = "4",
            D = new SerializerChild { A = 30 },
            Values = new List<int> { 1, 2, 15 },
            Names = new Dictionary<int, string>
            {
                [1] = "a",
                [2] = "b",
            },
        };

        var json = SerializerHelper.SaveAsJson(source);
        var result = SerializerHelper.LoadFromJson<SerializerModel>(json);

        Assert.That(result, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(result.A, Is.EqualTo(source.A));
            Assert.That(result.B, Is.EqualTo(source.B).Within(Tolerance));
            Assert.That(result.C, Is.EqualTo(source.C));
            Assert.That(result.D, Is.Not.Null);
            Assert.That(result.D.A, Is.EqualTo(source.D.A));
            Assert.That(result.Values, Is.EqualTo(source.Values));
            Assert.That(result.Names, Is.EquivalentTo(source.Names));
        });
    }

    public sealed class SerializerModel
    {
        public int A { get; set; }
        public float B { get; set; }
        public string C { get; set; }
        public SerializerChild D { get; set; }
        public List<int> Values { get; set; }
        public Dictionary<int, string> Names { get; set; }
    }

    public sealed class SerializerChild
    {
        public int A { get; set; }
    }
}
