using NUnit.Framework;

namespace EngineNS.Tests.Math;

[TestFixture]
public class Vector3Tests
{
    const float Tolerance = 1e-6f;

    static void AssertVector(Vector3 actual, Vector3 expected)
    {
        Assert.Multiple(() =>
        {
            Assert.That(actual.X, Is.EqualTo(expected.X).Within(Tolerance));
            Assert.That(actual.Y, Is.EqualTo(expected.Y).Within(Tolerance));
            Assert.That(actual.Z, Is.EqualTo(expected.Z).Within(Tolerance));
        });
    }

    [Test]
    public void Addition_TwoVectors_ReturnsComponentSum()
    {
        var result = new Vector3(1.0f, -2.0f, 3.0f) + new Vector3(4.0f, 5.0f, -6.0f);

        AssertVector(result, new Vector3(5.0f, 3.0f, -3.0f));
    }

    [Test]
    public void Subtraction_TwoVectors_ReturnsComponentDifference()
    {
        var result = new Vector3(5.0f, 3.0f, -3.0f) - new Vector3(4.0f, 5.0f, -6.0f);

        AssertVector(result, new Vector3(1.0f, -2.0f, 3.0f));
    }

    [Test]
    public void Dot_OrthogonalVectors_ReturnsZero()
    {
        var result = Vector3.Dot(Vector3.UnitX, Vector3.UnitY);

        Assert.That(result, Is.EqualTo(0.0f).Within(Tolerance));
    }

    [Test]
    public void Cross_UnitAxes_ReturnsRightHandedAxis()
    {
        var result = Vector3.Cross(Vector3.UnitX, Vector3.UnitY);

        AssertVector(result, Vector3.UnitZ);
    }

    [Test]
    public void Normalize_NonUnitVector_ReturnsOriginalLengthAndUnitVector()
    {
        var value = new Vector3(3.0f, 4.0f, 0.0f);

        var originalLength = value.Normalize();

        Assert.Multiple(() =>
        {
            Assert.That(originalLength, Is.EqualTo(5.0f).Within(Tolerance));
            Assert.That(value.Length(), Is.EqualTo(1.0f).Within(Tolerance));
        });
        AssertVector(value, new Vector3(0.6f, 0.8f, 0.0f));
    }

    [Test]
    public void Length_KnownVector_ReturnsEuclideanMagnitude()
    {
        var value = new Vector3(2.0f, -3.0f, 6.0f);

        Assert.That(value.Length(), Is.EqualTo(7.0f).Within(Tolerance));
    }
}
