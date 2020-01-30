using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.GamePlay.Camera.Modifier
{
    public enum WaveType
    {
        Sine,
        Perlin,
    }
    public enum InitialPhaseType
    {
        Zero,
        Random,
    }
    [Rtti.MetaClass]
    public class Oscillation : IO.Serializer.Serializer
    {
        [Rtti.MetaData]
        [Editor.Editor_NoCategoryAttribute]
        public float Amplitude { get; set; } = 0;
        [Rtti.MetaData]
        [Editor.Editor_NoCategoryAttribute]
        public float Frequency { get; set; } = 0;
        [Rtti.MetaData]
        [Editor.Editor_NoCategoryAttribute]
        public InitialPhaseType InitialPhaseType { get; set; } = InitialPhaseType.Zero;
        [Rtti.MetaData]
        [Editor.Editor_NoCategoryAttribute]
        public WaveType WaveType { get; set; } = WaveType.Sine;
        [Rtti.MetaData]
        [Editor.Editor_NoCategoryAttribute]
        public float Offset { get; set; } = 0;
        float mInitialPhase = 0;
        public Oscillation()
        {

        }
        public Oscillation(float amplitude, float frequency, float offset, InitialPhaseType initialOffset, WaveType waveType)
        {
            Amplitude = amplitude;
            Frequency = frequency;
            InitialPhaseType = initialOffset;
            Offset = offset;
            WaveType = waveType;
            if (initialOffset == InitialPhaseType.Random)
            {
                mInitialPhase = 0;
            }
            else
            {
                mInitialPhase = 0;
            }
        }
        public float GetValue(float time)
        {
            if (WaveType == WaveType.Sine)
            {
                return Amplitude * MathHelper.Sin(time * MathHelper.PI * 2 * Frequency + mInitialPhase) + Offset;
            }
            if (WaveType == WaveType.Perlin)
            {
                return Amplitude * MathHelper.Sin(time * MathHelper.PI * 2 * Frequency + mInitialPhase) + Offset;
            }
            return 0;
        }
        float mLastValue = 0;
        public float GetDeltaValue(float time)
        {
            float value = 0;
            if (WaveType == WaveType.Sine)
            {
                value = Amplitude * MathHelper.Sin(time * MathHelper.PI * 2 * Frequency + mInitialPhase) + Offset;
            }
            if (WaveType == WaveType.Perlin)
            {
                value = Amplitude * MathHelper.Sin(time * MathHelper.PI * 2 * Frequency + mInitialPhase) + Offset;
            }
            var delta = value - mLastValue;
            mLastValue = value;
            return delta;
        }
    }
    [Rtti.MetaClass]
    [Editor.Editor_MacrossClassAttribute(ECSType.Common, Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    [Editor.Editor_MacrossClassIconAttribute("icon/CameraShake_x64.txpic", RName.enRNameType.Editor)]
    public class GCameraShake : GCameraModifier
    {
        [Rtti.MetaData]
        [Category("Common")]
        public float BlendInTime { get; set; } = 0.1f;
        [Rtti.MetaData]
        [Category("Common")]
        public float BlendOutTime { get; set; } = 0.2f;
        [Rtti.MetaData]
        [Category("LocOscillation")]
        public Oscillation XOscillation { get; set; } = new Oscillation();
        [Rtti.MetaData]
        [Category("LocOscillation")]
        public Oscillation YOscillation { get; set; } = new Oscillation();
        [Rtti.MetaData]
        [Category("LocOscillation")]
        public Oscillation ZOscillation { get; set; } = new Oscillation();
        public Vector3 GetLocationDeltaValue(float time)
        {
            Vector3 value = Vector3.Zero;
            value.X = XOscillation.GetDeltaValue(time);
            value.Y = YOscillation.GetDeltaValue(time);
            value.Z = ZOscillation.GetDeltaValue(time);
            return value;
        }
        public Vector3 GetLocationValue(float time)
        {
            Vector3 value = Vector3.Zero;
            value.X = XOscillation.GetValue(time);
            value.Y = YOscillation.GetValue(time);
            value.Z = ZOscillation.GetValue(time);
            return value;
        }
        [Rtti.MetaData]
        [Category("RotOscillation")]
        public Oscillation PitchOscillation { get; set; } = new Oscillation();
        [Rtti.MetaData]
        [Category("RotOscillation")]
        public Oscillation YawOscillation { get; set; } = new Oscillation();
        [Rtti.MetaData]
        [Category("RotOscillation")]
        public Oscillation RollOscillation { get; set; } = new Oscillation();
        public Vector3 GetRotationDeltaValue(float time)
        {
            Vector3 value = Vector3.Zero;
            value.X = PitchOscillation.GetDeltaValue(time);
            value.Y = YawOscillation.GetDeltaValue(time);
            value.Z = RollOscillation.GetDeltaValue(time);
            return value;
        }
        public Vector3 GetRotationValue(float time)
        {
            Vector3 value = Vector3.Zero;
            value.X = PitchOscillation.GetValue(time);
            value.Y = YawOscillation.GetValue(time);
            value.Z = RollOscillation.GetValue(time);
            return value;
        }
        [Rtti.MetaData]
        [Category("Common")]
        public Oscillation FovOscillation { get; set; } = new Oscillation();
        public override void OnStartExecution(GCamera camera)
        {
            base.OnStartExecution(camera);

        }
        public override void StopExecution(GCamera camera)
        {
            base.StopExecution(camera);

        }
        public override void OnPerform(GCamera camera)
        {
            base.OnPerform(camera);
            var rotOscillationValue = GetRotationValue(mElapseTime);
            if (rotOscillationValue != Vector3.Zero)
            {
                var euler = camera.Rotation.ToEuler() + rotOscillationValue;
                camera.Rotation = Quaternion.FromEuler(euler);
            }
            var posOscillationValue = GetLocationValue(mElapseTime);
            if (posOscillationValue != Vector3.Zero)
            {
                var euler = camera.Rotation.ToEuler() * MathHelper.Rad2Deg;
                var quat = Quaternion.GetQuaternion(-Vector3.UnitZ, camera.Direction).ToEuler() * MathHelper.Rad2Deg;
                var value = Vector3.TransformCoordinate(posOscillationValue, Matrix.RotationQuaternion(camera.Rotation));
                //var CTWmat = Matrix.Transformation(Vector3.UnitXYZ, camera.Rotation, camera.Position);
                camera.Position += value;
            }
            var fovOscillationValue = FovOscillation.GetDeltaValue(mElapseTime);
            if (fovOscillationValue != 0)
            {
                camera.Fov += fovOscillationValue;
            }
            //Console.WriteLine($"value : {value} , delta :{delta.X} ,OldRotationX{oldEuler.X}, RotationX : {euler.X}");
        }

    }
}
