using EngineNS.Animation.Curve;
using NUnit.Framework;

namespace EngineNS.Tests.Animation;

[TestFixture]
public class TrackEvaluateTests
{
    const float Tolerance = 1e-6f;

    [Test]
    public void Evaluate_InsideSegment_ReturnsLinearInterpolation()
    {
        var track = CreateTrack((0.0f, 10.0f), (2.0f, 30.0f));
        var cache = new FTrackCache();

        var value = track.Evaluate(0.5f, ref cache);

        Assert.That(value, Is.EqualTo(15.0f).Within(Tolerance));
    }

    [TestCase(-1.0f, 10.0f)]
    [TestCase(3.0f, 30.0f)]
    public void Evaluate_OutsideRange_ReturnsNearestEndpoint(float time, float expected)
    {
        var track = CreateTrack((0.0f, 10.0f), (2.0f, 30.0f));
        var cache = new FTrackCache();

        var value = track.Evaluate(time, ref cache);

        Assert.That(value, Is.EqualTo(expected).Within(Tolerance));
    }

    [TestCase(-1.0f)]
    [TestCase(0.5f)]
    [TestCase(2.0f)]
    public void Evaluate_SingleKeyframe_ReturnsItsValue(float time)
    {
        var track = CreateTrack((0.5f, 42.0f));
        var cache = new FTrackCache();

        var value = track.Evaluate(time, ref cache);

        Assert.That(value, Is.EqualTo(42.0f).Within(Tolerance));
    }

    [Test]
    [Explicit("Known defect: TtTrack.Evaluate indexes an empty keyframe list.")]
    public void Evaluate_EmptyTrack_ReturnsDefaultValue()
    {
        var track = new TtTrack();
        var cache = new FTrackCache();

        var value = track.Evaluate(0.5f, ref cache);

        Assert.That(value, Is.Zero);
    }

    [Test]
    [Explicit("Known defect: AddKeyframeBack does not preserve the ordering required by BinarySearch.")]
    public void AddKeyframeBack_UnsortedInput_EvaluatesInTimeOrder()
    {
        var track = CreateTrack((1.0f, 10.0f), (0.0f, 0.0f), (2.0f, 20.0f));
        var cache = new FTrackCache();

        var value = track.Evaluate(0.5f, ref cache);

        Assert.That(value, Is.EqualTo(5.0f).Within(Tolerance));
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
