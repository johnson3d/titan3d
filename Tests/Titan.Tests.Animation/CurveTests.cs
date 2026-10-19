using EngineNS.Animation.Curve;
using NUnit.Framework;

namespace EngineNS.Tests.Animation;

[TestFixture]
public class CurveTests
{
    const float Tolerance = 1e-5f;

    [Test]
    public void FloatCurve_Evaluate_InterpolatesTrackValue()
    {
        var curve = new TtFloatCurve { Track = CreateTrack((0.0f, 0.0f), (1.0f, 100.0f)) };

        var value = curve.Evaluate(0.25f);

        Assert.That(value.FloatValue, Is.EqualTo(25.0f).Within(Tolerance));
    }

    [Test]
    public void Vector3Curve_Evaluate_InterpolatesEachTrack()
    {
        var curve = new TtVector3Curve
        {
            XTrack = CreateTrack((0.0f, 0.0f), (1.0f, 10.0f)),
            YTrack = CreateTrack((0.0f, 0.0f), (1.0f, 20.0f)),
            ZTrack = CreateTrack((0.0f, 0.0f), (1.0f, 30.0f)),
        };

        var value = curve.Evaluate(0.5f).Vector3Value;

        Assert.Multiple(() =>
        {
            Assert.That(value.X, Is.EqualTo(5.0f).Within(Tolerance));
            Assert.That(value.Y, Is.EqualTo(10.0f).Within(Tolerance));
            Assert.That(value.Z, Is.EqualTo(15.0f).Within(Tolerance));
        });
    }

    [TestCase(0.0f, 0.0f)]
    [TestCase(1.0f, 2.0943952f)]
    public void QuaternionCurve_Evaluate_ReturnsEndpointRotation(float time, float expectedRoll)
    {
        var curve = CreateZRotationCurve();

        var value = curve.Evaluate(time).Vector3Value;

        Assert.That(value.Z, Is.EqualTo(expectedRoll).Within(Tolerance));
    }

    [Test]
    [Explicit("Known defect: TtQuaternionCurve uses normalized Lerp instead of constant-speed Slerp.")]
    public void QuaternionCurve_Evaluate_UsesSphericalInterpolation()
    {
        var curve = CreateZRotationCurve();

        var value = curve.Evaluate(0.25f).Vector3Value;

        Assert.That(value.Z, Is.EqualTo(0.5235988f).Within(1e-3f));
    }

    private static TtQuaternionCurve CreateZRotationCurve()
    {
        return new TtQuaternionCurve
        {
            XTrack = CreateTrack((0.0f, 0.0f), (1.0f, 0.0f)),
            YTrack = CreateTrack((0.0f, 0.0f), (1.0f, 0.0f)),
            ZTrack = CreateTrack((0.0f, 0.0f), (1.0f, 0.8660254f)),
            WTrack = CreateTrack((0.0f, 1.0f), (1.0f, 0.5f)),
        };
    }

    private static TtTrack CreateTrack(params (float Time, float Value)[] keyframes)
    {
        var track = new TtTrack();
        foreach (var (time, value) in keyframes)
        {
            var keyframe = new FKeyframe { Time = time, Value = value };
            track.AddKeyframeBack(ref keyframe);
        }
        return track;
    }
}
