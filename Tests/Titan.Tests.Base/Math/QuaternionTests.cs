using NUnit.Framework;

namespace EngineNS.Tests.Math;

[TestFixture]
public class QuaternionTests
{
    const float Tolerance = 1e-6f;

    static void AssertSameRotation(in Quaternion actual, in Quaternion expected)
    {
        var dot = System.Math.Abs(Quaternion.Dot(actual, expected));
        Assert.That(dot, Is.EqualTo(1.0f).Within(Tolerance));
    }

    [Test]
    public void Identity_DefaultIdentity_HasExpectedComponents()
    {
        var identity = Quaternion.Identity;

        Assert.Multiple(() =>
        {
            Assert.That(identity.X, Is.EqualTo(0.0f).Within(Tolerance));
            Assert.That(identity.Y, Is.EqualTo(0.0f).Within(Tolerance));
            Assert.That(identity.Z, Is.EqualTo(0.0f).Within(Tolerance));
            Assert.That(identity.W, Is.EqualTo(1.0f).Within(Tolerance));
            Assert.That(identity.IsIdentity, Is.True);
        });
    }

    [Test]
    public void Multiplication_WithIdentity_PreservesRotation()
    {
        var rotation = Quaternion.RotationAxis(Vector3.UnitZ, MathHelper.PI * 0.5f);

        var rightIdentity = rotation * Quaternion.Identity;
        var leftIdentity = Quaternion.Identity * rotation;

        AssertSameRotation(in rightIdentity, in rotation);
        AssertSameRotation(in leftIdentity, in rotation);
    }

    [TestCase(0.0f)]
    [TestCase(1.0f)]
    public void Slerp_EndpointFactor_ReturnsCorrespondingEndpoint(float factor)
    {
        var start = Quaternion.Identity;
        var end = Quaternion.RotationAxis(Vector3.UnitZ, MathHelper.PI * 0.5f);

        var result = Quaternion.Slerp(in start, in end, factor);
        var expected = factor == 0.0f ? start : end;

        AssertSameRotation(in result, in expected);
    }

    [Test]
    public void Slerp_HalfFactor_ReturnsHalfRotation()
    {
        var start = Quaternion.Identity;
        var end = Quaternion.RotationAxis(Vector3.UnitZ, MathHelper.PI * 0.5f);
        var expected = Quaternion.RotationAxis(Vector3.UnitZ, MathHelper.PI * 0.25f);

        var result = Quaternion.Slerp(in start, in end, 0.5f);

        AssertSameRotation(in result, in expected);
    }

    [Test]
    public void RotationMatrix_QuarterTurnAroundZ_RoundTripsQuaternion()
    {
        var rotation = Quaternion.RotationAxis(Vector3.UnitZ, MathHelper.PI * 0.5f);

        var matrix = Matrix.RotationQuaternion(rotation);
        var doubleMatrix = matrix.AsDMatrix();
        var roundTripped = DMatrix.RotationMatrix(in doubleMatrix);

        Assert.Multiple(() =>
        {
            Assert.That(matrix.M11, Is.EqualTo(0.0f).Within(Tolerance));
            Assert.That(matrix.M12, Is.EqualTo(1.0f).Within(Tolerance));
            Assert.That(matrix.M21, Is.EqualTo(-1.0f).Within(Tolerance));
            Assert.That(matrix.M22, Is.EqualTo(0.0f).Within(Tolerance));
            Assert.That(matrix.M33, Is.EqualTo(1.0f).Within(Tolerance));
            Assert.That(matrix.M44, Is.EqualTo(1.0f).Within(Tolerance));
        });
        AssertSameRotation(in roundTripped, in rotation);
    }
}
