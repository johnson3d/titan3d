using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace EngineNS.Bricks.Particle
{
    public abstract class ParticleBase
    {
        //[EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        [Editor.DisplayParamName("初始化")]
        public virtual void Init(ParticleData data)
        {
        }

        //[EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        [Editor.DisplayParamName("更新")]
        public virtual void Update(float dt, ParticleData data)
        {
        }

        //[EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        [Editor.DisplayParamName("回收逻辑")]
        public virtual void Clean(ParticleData data)
        {
        }
    }

    public class DataGradient
    {
        public DataGradient()
        {
            StartTick = EngineNS.CEngine.Instance.EngineTimeSecond;
        }

        public virtual void GetTickData(float tick)
        {
        }
        public virtual void InitArray()
        {
        }

        public virtual void SetMax(float max)
        {
        }

        public virtual void SetMin(float min)
        {
        }

        public virtual void SetMax(ref Vector2 max)
        {
        }

        public virtual void SetMin(ref Vector2 min)
        {
        }

        public virtual void SetMax(ref Vector3 max)
        {
        }

        public virtual void SetMin(ref Vector3 min)
        {
        }

        public virtual void SetMax(ref Vector4 max)
        {
        }

        public virtual void SetMin(ref Vector4 min)
        {
        }

        public virtual void SetMax(ref Quaternion max)
        {
        }

        public virtual void SetMin(ref Quaternion min)
        {
        }

        public virtual void GetStartValue()
        {
        }

        public bool IsUseDefaultTick
        {
            get;
            set;
        } = true;

        public bool Loop
        {
            get;
            set;
        }

        public float Duration
        {
            get;
            set;
        } = 0;

        [Browsable(false)]
        public float StartTick
        {
            get;
            set;
        } = 0;

        public float GetProportion(float tick)
        {
            if (Loop)
            {
                return (tick % Duration) / Duration;
            }
            else
            {
                return MathHelper.FClamp(tick / Duration, 0f, 1f);
            }
        }
    }

    public class DataGradient4 : DataGradient
    {
        public class Data
        {
            public Vector4 value = Vector4.Zero;

            public float offset = 0f;
            public Data(float v1, Vector4 v2)
            {
                offset = v1;
                value = v2;
            }
        }

        public DataGradient4()
        {
            InitArray();
        }

        public List<Data> DataArray = new List<Data>();
        public Vector4 _ResultValue = Vector4.Zero;

        public override void GetTickData(float  tick)
        {
            if (DataArray.Count == 0)
                return;

            if ((DataArray.Count == 1))
            {
                _ResultValue = DataArray[0].value;
                return;
            }

            for (int i = 1; i<DataArray.Count; i ++)
            {
                if (i == (DataArray.Count - 1) && tick > DataArray[i].offset)
                {
                    _ResultValue = DataArray[i].value;
                }
                else if (i == 1 && tick <= DataArray[0].offset)
                {
                    _ResultValue = DataArray[0].value;
                }
                else if (((tick<DataArray[i].offset)
                            && (tick > DataArray[i - 1].offset)))
                {
                    _ResultValue = EngineNS.Vector4.Lerp(DataArray[i - 1].value, DataArray[i].value, ((tick - DataArray[i - 1].offset)
                                    / (DataArray[i].offset - DataArray[i - 1].offset)));
                }
            }
        }

    }

    public class DataGradient3 : DataGradient
    {
        public class Data
        {
            public Vector3 value = Vector3.Zero;

            public float offset = 0f;
            public Data(float v1, Vector3 v2)
            {
                offset = v1;
                value = v2;
            }
        }

        public DataGradient3()
        {
            InitArray();
        }

        public List<Data> DataArray = new List<Data>();
        public Vector3 _ResultValue = Vector3.Zero;

        public override void GetTickData(float tick)
        {
            if (DataArray.Count == 0)
                return;

            if ((DataArray.Count == 1))
            {
                if (tick >= DataArray[0].offset)
                {
                    _ResultValue = DataArray[0].value;
                }
                else
                {
                    _ResultValue = Vector3.Zero;
                }
               
                return;
            }
            
            for (int i = 1; i < DataArray.Count; i++)
            {
                if (i == (DataArray.Count - 1) && tick > DataArray[i].offset)
                {
                    _ResultValue = DataArray[i].value;
                }
                else if (i == 1 && tick <= DataArray[0].offset)
                {
                    _ResultValue = DataArray[0].value;
                }
                else if (((tick <= DataArray[i].offset)
                            && (tick >= DataArray[i - 1].offset)))
                {
                    _ResultValue = EngineNS.Vector3.Lerp(DataArray[i - 1].value, DataArray[i].value, ((tick - DataArray[i - 1].offset)
                                    / (DataArray[i].offset - DataArray[i - 1].offset)));
                }
            }
        }

    }

    public class DataGradientRotation : DataGradient
    {
        public class Data
        {
            public float offset;
            public Quaternion value;
            public Data(float v1, Quaternion v2)
            {
                offset = v1;
                value = v2;
            }

        }

        public List<Data> DataArray = new List<Data>();

        public DataGradientRotation()
        {
            InitArray();
        }
    }

    public class DataGradient2 : DataGradient
    {
        public class Data
        {
            public Vector2 value = Vector2.Zero;

            public float offset = 0f;
            public Data(float v1, Vector2 v2)
            {
                offset = v1;
                value = v2;
            }
        }

        public DataGradient2()
        {
            InitArray();
        }

        public List<Data> DataArray = new List<Data>();
        public Vector2 _ResultValue = Vector2.Zero;

        public override void GetTickData(float tick)
        {
            if (DataArray.Count == 0)
                return;

            if ((DataArray.Count == 1))
            {
                if (tick >= DataArray[0].offset)
                {
                    _ResultValue = DataArray[0].value;
                }
                else
                {
                    _ResultValue = Vector2.Zero;
                }
                return;
            }

            for (int i = 1; i < DataArray.Count; i++)
            {
                if (i == (DataArray.Count - 1) && tick > DataArray[i].offset)
                {
                    _ResultValue = DataArray[i].value;
                }
                else if (i == 1 && tick <= DataArray[0].offset)
                {
                    _ResultValue = DataArray[0].value;
                }
                else if (((tick <= DataArray[i].offset)
                            && (tick >= DataArray[i - 1].offset)))
                {
                    _ResultValue = EngineNS.Vector2.Lerp(DataArray[i - 1].value, DataArray[i].value, ((tick - DataArray[i - 1].offset)
                                    / (DataArray[i].offset - DataArray[i - 1].offset)));
                }
            }
        }

    }
    public class DataGradient1 : DataGradient
    {
        public class Data
        {
            public float value = 0;

            public float offset = 0f;
            public Data(float v1, float v2)
            {
                offset = v1;
                value = v2;
            }
        }

        public DataGradient1()
        {
            InitArray();
        }

        public List<Data> DataArray = new List<Data>();
        public float _ResultValue = 0;

        public override void GetTickData(float tick)
        {
            if (DataArray.Count == 0)
                return;

            if ((DataArray.Count == 1))
            {
                if (tick >= DataArray[0].offset)
                {
                    _ResultValue = DataArray[0].value;
                }
                else
                {
                    _ResultValue = 0;
                }
                
                return;
            }

            for (int i = 1; i < DataArray.Count; i++)
            {
                if (i == (DataArray.Count - 1) && tick > DataArray[i].offset)
                {
                    _ResultValue = DataArray[i].value;
                }
                else if (i == 1 && tick <= DataArray[0].offset)
                {
                    _ResultValue = DataArray[0].value;
                }
                else if (((tick < DataArray[i].offset)
                            && (tick > DataArray[i - 1].offset)))
                {
                    _ResultValue = MathHelper.FloatLerp(DataArray[i - 1].value, DataArray[i].value, ((tick - DataArray[i - 1].offset)
                                    / (DataArray[i].offset - DataArray[i - 1].offset)));
                }
            }
        }
    }


    public class ColorBaseNode
    {
        public class Data
        {
            public Color4 value;
            public float offset;
            public Data(float v1, Color4 v2)
            {
                offset = v1;
                value = v2;
            }
        }

        public bool IsUseDefaultTick
        {
            get;
            set;
        } = true;

        public bool Loop
        {
            get;
            set;
        }

        public float Duration
        {
            get;
            set;
        } = 0;

        [Browsable(false)]
        public float StartTick
        {
            get;
            set;
        } = 0;

        public float GetProportion(float  tick)
        {
            if (Loop)
            {
                return (tick % Duration) / Duration;
            }
            else
            {
                return MathHelper.FClamp(tick / Duration, 0f, 1f);
            }
        }

        public List<Data> DataArray = new List<Data>();

        public ColorBaseNode()
        {
            InitArray();
        }
        public virtual void InitArray()
        {
        }

        public enum BlendType
        {
            Disenable,
            Add,
            Subtract,
            Modulate,
        }

        public BlendType _BlendType;
        public int Colume = 0;
        public Color4 ColorValue;
        public Color4 StartValue;
        public Color4 Color1;
        public Color4 Color2;
        public Color4 Result;

        public virtual void SetColor1(ref Color4 color)
        {
            Color1 = color;
        }

        public virtual void SetColor2(ref Color4 color)
        {
            Color2 = color;
        }

        public Color4 ParticleStartColor;
        public void SetParticleColor(float life, float tick, ref CGfxParticlePose pose, ref CGfxParticlePose startpos)
        {
            var factor = tick / life;
            if (IsUseDefaultTick == false)
            {
                factor = GetProportion(tick);
            }
            SetParticleColor(factor);

            if (_BlendType == BlendType.Disenable)
            {
                pose.SetUserParams_Color4(Colume, ColorValue);
                return;
            }

            {
                UInt8_4 startcolor = startpos.GetUserParams(0);
                ParticleStartColor.Red = (float)startcolor.x / 255.0f;
                ParticleStartColor.Green = (float)startcolor.y / 255.0f;
                ParticleStartColor.Blue = (float)startcolor.z / 255.0f;
                ParticleStartColor.Alpha = (float)startcolor.w / 255.0f;
            }

            if (_BlendType == BlendType.Add)
            {
                EngineNS.Color4.Add(ref ParticleStartColor, ref ColorValue, out Result);
            }
            else if (_BlendType == BlendType.Subtract)
            {
                EngineNS.Color4.Subtract(ref ParticleStartColor, ref ColorValue, out Result);
            }
            else if (_BlendType == BlendType.Modulate)
            {
                EngineNS.Color4.Modulate(ref ParticleStartColor, ref ColorValue, out Result);
            }

            pose.SetUserParams_Color4(Colume, Result);
        }

        public virtual void SetParticleColor(float tick)
        {
            if (DataArray.Count == 0)
                return;

            if ((DataArray.Count == 1))
            {
                ColorValue = DataArray[0].value;
                return;
            }

            for (int i = 1; i < DataArray.Count; i++)
            {
                if (i == (DataArray.Count - 1) && tick > DataArray[i].offset)
                {
                    ColorValue = DataArray[i].value;
                }
                else if (i == 1 && tick <= DataArray[0].offset)
                {
                    ColorValue = DataArray[0].value;
                }
                else if (((tick < DataArray[i].offset)
                            && (tick > DataArray[i - 1].offset)))
                {
                    ColorValue = Color4.Lerp(DataArray[i - 1].value, DataArray[i].value, ((tick - DataArray[i - 1].offset)
                                    / (DataArray[i].offset - DataArray[i - 1].offset)));
                }
            }
        }
        
        public virtual void GetStartValue()
        {
        }
    }

    public class TrailNode
    {

        public Graphics.Mesh.CGfxMeshPrimitives GeomMesh;

        public TrailDataControl TrailDataControlData
        {
            get;
            set;
        }

        List<TrailDataControl> TempTrailDataControlDatas = new List<TrailDataControl>();

        public TrailNode()
        {
            CallSetDealTrailDataColorAndWidth(this);

            GeomMesh = new Graphics.Mesh.CGfxMeshPrimitives();
            GeomMesh.Init(CEngine.Instance.RenderContext, null, 1);
            CDrawPrimitiveDesc dpDesc = new CDrawPrimitiveDesc();
            dpDesc.SetDefault();
            dpDesc.StartIndex = 0xFFFFFFFF;
            GeomMesh.PushAtomLOD(0, ref dpDesc);
        }

        public void CreateTrail(ref CGfxParticle particle)
        {
            if (particle.Tag == null)
            {
                var obj = new ParticleTag();
                var data = new TrailDataControl();
                data.Width = TrailDataControlData.Width;
                data.Life = TrailDataControlData.Life;
                data.MinVertextDistance = TrailDataControlData.MinVertextDistance;
                obj.TrailDataControl = data;
                particle.Tag = obj;
                TempTrailDataControlDatas.Add(data);

                return;
            }

            var tag = particle.Tag as ParticleTag;
            if (tag.TrailDataControl == null)
            {
                var data = new TrailDataControl();
                data.Width = TrailDataControlData.Width;
                data.Life = TrailDataControlData.Life;
                data.MinVertextDistance = TrailDataControlData.MinVertextDistance;
                tag.TrailDataControl = data;
                TempTrailDataControlDatas.Add(data);
            }
            else
            {
                tag.TrailDataControl.Clear();
            }

        }
        
        EngineNS.Bricks.Particle.TrailDataControl.DealTrailDataColorAndWidthDelegate DealTrailDataColorAndWidth;
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        [Editor.DisplayParamName("设置飘带更新中宽度和颜色的更改的回调")]
        public virtual void CallSetDealTrailDataColorAndWidth(TrailNode trailNode)
        {
        }
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        [Editor.DisplayParamName("设置飘带更新中宽度和颜色的更改")]
        public void SetDealTrailDataColorAndWidth(EngineNS.Bricks.Particle.TrailDataControl.DealTrailDataColorAndWidthDelegate func)
        {
            DealTrailDataColorAndWidth = func;
        }


        public void CreateAndUpdateTrailData(CGfxParticleSystem particlesys, ref CGfxParticle pfx, float e)
        {
            if ( pfx.Tag == null)
                return;
            var tag = pfx.Tag as ParticleTag;

            var trail = tag.TrailDataControl;
            if (trail == null)
                return;

            unsafe
            {
                Matrix worldMatrix = new Matrix();
                if (particlesys.HostActor != null)
                {
                    worldMatrix = particlesys.HostActor.Placement.WorldMatrix;
                }
                else if (particlesys.HostActorMesh != null)
                {
                    worldMatrix = particlesys.HostActorMesh.Placement.WorldMatrix;
                }
                trail.CreateTrailData(ref pfx.FinalPose.mPosition, ref pfx.FinalPose.mRotation, ref pfx.FinalPose.mScale, ref worldMatrix, particlesys.IsBind);
                trail.Update(e, DealTrailDataColorAndWidth);
                trail.CalculateVertexindexes(TrailDataControlData.GetPositionCount());
                TrailDataControlData.Compose(trail);
            }
        }

        public void DealDeathParticle(ref CGfxParticle pfx)
        {
            var tag = pfx.Tag as ParticleTag;
            if (tag != null)
            {
                TempTrailDataControlDatas.Remove(tag.TrailDataControl);
                tag.TrailDataControl = null;
            }
            
        }

        public void Clear()
        {
            if (TrailDataControlData != null)
            {
                TrailDataControlData.Clear();
            }
        }
    }

    //粒子状态结点
    public class ParticleStateNode
    {
        public class ParticleStateData
        {
           [Browsable(false)]
            public bool IsAcceleration
            {
                set;
                get;
            } = false;

            [Browsable(false)]
            public float Acceleration
            {
                set;
                get;
            } = 0.10f;
        }

        public ParticleStateData Data = new ParticleStateData();
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        [Editor.DisplayParamName("初始化")]
        public virtual void Init(ParticleData data, ref CGfxParticle f, ref CGfxParticleState ps)
        {
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        [Editor.DisplayParamName("更新")]
        public virtual void Update(float dt, ParticleData data, ref CGfxParticle f, ref CGfxParticleState ps)
        {
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        [Editor.DisplayParamName("回收逻辑")]
        public virtual void Clean(ParticleData data, ref CGfxParticle f, ref CGfxParticleState ps)
        {
        }
    }

    [Editor.MacrossPanelPath("粒子系统/粒子缩放结点(Particle Scale Node)")]
    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Inheritable | Editor.Editor_MacrossClassAttribute.enMacrossType.Useable | Editor.Editor_MacrossClassAttribute.enMacrossType.Createable)]
    //粒子缩放结点
    public class ParticleScaleNode : DataGradient3
    {
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Scale
        {
            get;
            set;
        } = Vector3.Zero;

        public Vector3 StartValue = Vector3.UnitXYZ;
        public Vector3 _Min = Vector3.UnitXYZ;
        public Vector3 _Max = Vector3.UnitXYZ;
        public override void SetMax(ref Vector3 max)
        {
            base.SetMax(ref max);
            _Max = max;
        }

        public override void SetMin(ref Vector3 min)
        {
            base.SetMin(ref min);
            _Min = min;
        }

        public virtual void SetParticleScale(float life, float tick, ref CGfxParticlePose pose)
        {
            var factor = Math.Min(1f, tick / life);
            if (IsUseDefaultTick == false)
            {
                factor = GetProportion(tick);
            }
            GetTickData(factor);
            Scale = _ResultValue;
        }

        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        [EngineNS.Editor.DisplayParamName("粒子缩放的回调 修改pose里面的scale")]
        public virtual void SetParticleScale2(float particlelife, float particletick, ref CGfxParticlePose pose)
        {
        }
    }

    public class ParticleVelocityNode : DataGradient3
    {
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public Vector3 Velocity
        {
            get;
            set;
        } = Vector3.Zero;

        public Vector3 StartValue = Vector3.UnitXYZ;
        public Vector3 _Min = Vector3.Zero;
        public Vector3 _Max = Vector3.Zero;
        public List<Vector3> StartValues = new List<Vector3>();
        public Dictionary<IntPtr, Vector3> StartValues2 = new Dictionary<IntPtr, Vector3>();

        public void AddStartValue(int index, ref Vector3 value)
        {
            if (StartValues.Count <= index)
            {
                StartValues.Insert(0, value);
                //StartValues.Add(value);
            }
            else
            {
                StartValues[index] = value;
            }
        }

        public unsafe void AddStartValue(CGfxParticleState* pt, ref Vector3 value)
        {
            IntPtr ptr = (IntPtr)pt;
            if (StartValues2.ContainsKey(ptr) == false)
            {
                //StartValues.Insert(StartValues.Count, value);
                StartValues2.Add(ptr, value);
            }
            else
            {
                StartValues2[ptr] = value;
            }
        }

        public unsafe void RemoveValue(CGfxParticleState* pt)
        {
            IntPtr ptr = (IntPtr)pt;
            StartValues2.Remove(ptr);
        }
        public override void SetMax(ref Vector3 max)
        {
            base.SetMax(ref max);
            _Max = max;
        }

        public override void SetMin(ref Vector3 min)
        {
            base.SetMax(ref min);
            _Min = min;
        }

        public virtual void SetParticleVelocity(float life, float tick, ref CGfxParticlePose pose)
        {
            var factor = Math.Min(1f, tick / life);
            if (IsUseDefaultTick == false)
            {
                factor = GetProportion(tick);
            }
            GetTickData(factor);
            Velocity = _ResultValue;
        }

        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        [EngineNS.Editor.DisplayParamName("粒子速递")]
        public virtual void SetParticleVelocity2(float particlelife, float particletick, ref CGfxParticlePose pose)
        {
        }
    }

    //粒子向心速度结点
    public class ParticleVelocityByCenterNode : DataGradient1
    {
        public Vector3 Direction = Vector3.Zero;
        public float Power = 1;


        public void GetValue(ref CGfxParticleState state, CGfxParticleSystem sys, CGfxParticleSubState substate, float e)
        {
            state.GetDirectionToCenter(sys, substate, out Direction);
            if (Power != 0)
            {
                Direction *= Power * e * state.mPose.mAcceleration.X;
            }
            else
            {
                Direction *= e * state.mPose.mAcceleration.X;
            }
        }

        public virtual void SetPower(float life, float tick)
        {
            var factor = Math.Min(1f, tick / life);
            if (IsUseDefaultTick == false)
            {
                factor = GetProportion(tick);
            }
            GetTickData(factor);
            Power = _ResultValue;
        }

        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void SetPower2(float particlelife, float particletick)
        {
        }

        public void OnlyUseThisNode(float elaspe, ref CGfxParticleState state, float speed = 1)
        {
            if (Power != 0)
            {
                state.AccelerationEffectCenter(elaspe, ref Direction, speed * Power);
            }
           
        }
    }

    //粒子切线速度结点
    public class ParticleVelocityByTangentNode : DataGradient3
    {
        public class TangentPower : DataGradient1
        {
            public float _Max = 1.0f;
            public float _Min = 0f;

            public float StartValue = 0f;

            public float Power = 1f;

            public override void SetMax(float max)
            {
                base.SetMax(max);

                _Max = max;
            }

            public override void SetMin(float min)
            {
                base.SetMin(min);

                _Min = min;
            }

            public override void GetStartValue()
            {
                if (_Min >= _Max)
                {
                    StartValue = _Max;
                    return;
                }
                StartValue = McParticleEffector.SRandomF2(_Min, _Max);
            }
        }

        public TangentPower Power = new TangentPower();

        public Vector3 Aix = Vector3.Zero;
        public Vector3 Offset = Vector3.Zero;

        public Vector3 _Max = Vector3.Zero;
        public Vector3 _Min = Vector3.Zero;

        public Vector3 StartValue = Vector3.Zero;

        public override void SetMax(ref Vector3 max)
        {
            base.SetMax(ref max);

            _Max = max;
        }

        public override void SetMin(ref Vector3 min)
        {
            base.SetMin(ref min);

            _Min = min;
        }

        public Quaternion Quaternion = Quaternion.Identity;

        public virtual void SetAix(float life, float tick, ref Vector3 startvalue, float startpower)
        {
            var factor = Math.Min(1f, tick / life);
            if (IsUseDefaultTick == false)
            {
                factor = GetProportion(tick);
            }
            GetTickData(factor);
            Aix = _ResultValue + startvalue;

            //
            factor = Math.Min(1f, tick / life);
            if (Power.IsUseDefaultTick == false)
            {
                factor = Power.GetProportion(tick);
            }
            Power.GetTickData(factor);
            Power.Power = Power._ResultValue;
        }

        [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
        public virtual void SetAix2(float particlelife, float particletick, ref Vector3 startvalue)
        {
        }
        
        public void GetValue(ref CGfxParticleState state, CGfxParticleSystem sys, CGfxParticleSubState substate, float e)
        {
            var speed = state.mPose.mAcceleration.X * Power.Power * state.mStartPose.mAcceleration.Y;
            if (MathHelper.Epsilon > Math.Abs(speed))
            {
                Offset = Vector3.Zero;
            }
            else
            {
                Quaternion.RotationAxis(ref Aix, e * speed, out Quaternion);
                state.GetRotationByCenter(sys, substate, ref Quaternion, out Offset);
            }
        }

        public void OnlyUseThisNode(float elaspe, ref CGfxParticleState state, float speed = 1)
        {
            if (Offset.Equals(Vector3.Zero) == false)
            {
                state.AccelerationEffectTangent(elaspe, ref Offset, speed);
            }
        }
    }
    //粒子组合结点
    public class ParticleComposeNode
    {
        public int LeftSubStateIndex = -1;
        public int RightSubStateIndex = -1;

        public ParticleComposeNode ComposeNodeLeft;
        public ParticleComposeNode ComposeNodeRight;

        public CGfxParticlePose ResultPose = new CGfxParticlePose();
        public CGfxParticlePose TempPose = new CGfxParticlePose();

        public enum ControlType
        {
            Add,
            Sub,
            Lerp,
            Custom,
        }

        public bool CanUsed
        {
            get
            {
                return (RightSubStateIndex != -1 || ComposeNodeRight != null) && (LeftSubStateIndex != -1 || ComposeNodeLeft != null);
            }
        }

        public class Data
        {
            [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [EngineNS.Editor.DisplayParamName("粒子组合方式")]
            public ControlType ComposeFunction
            {
                get;
                set;
            } = ControlType.Add;
        }

        public Vector4 ResultV4;
        public Data _Data;

        //Matrix
        public Vector3 Translation;
        public Vector3 Scale;
        public Quaternion Rotation;

        public Matrix Matrix;

        public void SetTRS(TransformNode node)
        {
            Translation = node.Translation;
            Rotation = node.Rotation;
            Scale = node.Scale;
        }
        public ParticleComposeNode()
        {
            _Data = new Data();
        }

        public float Factor = 0.5f;

        public virtual void GetFactor(ref CGfxParticle p, float elaspe)
        {
        }
        public void ComposeParticle(CGfxParticleSystem sys, ref CGfxParticle p, int substatenum, float elaspe, bool usecompute)
        {
            if (_Data.ComposeFunction == ControlType.Add)
            {
                AddFunc(sys, ref p, substatenum, elaspe, usecompute);
            }
            else if (_Data.ComposeFunction == ControlType.Sub)
            {
                SubFunc(sys, ref p, substatenum, elaspe, usecompute);
            }
            else if (_Data.ComposeFunction == ControlType.Lerp)
            {
                LerpFunc(sys, ref p, substatenum, elaspe, usecompute);
            }
        }

        public void ComputeMatrix(CGfxParticleSubState substate, TransformNode node, ref CGfxParticlePose pose, bool usecompute)
        {
            if (usecompute)
            {
                Quaternion.RotationYawPitchRoll(node.YawPitchRoll.X, node.YawPitchRoll.Y, node.YawPitchRoll.Z, out node.Rotation);
                Matrix.Transformation(node.Scale, node.Rotation, node.Translation, out Matrix);
            }
            Vector3.Transform(ref pose.mPosition, ref Matrix, out ResultV4);
            pose.mPosition.X = ResultV4.X;
            pose.mPosition.Y = ResultV4.Y;
            pose.mPosition.Z = ResultV4.Z;
            if (node.IgnoreRotation == false)
            {
                if (substate.RotationNodes == null || substate.RotationNodes.Count == 0)
                {
                    pose.mRotation = node.Rotation;
                }
                else
                {
                    pose.mRotation *= node.Rotation;
                }
            }

            if (node.IgnoreScale == false)
            {
                if (substate.ParticleScaleNode != null)
                {
                    pose.mScale *= node.Scale;
                }
                else
                {
                    pose.mScale = node.Scale;
                }

            }
        }

        unsafe public void AddFunc(CGfxParticleSystem sys, ref CGfxParticle p, int substatenum, float elaspe, bool usecompute)
        {
            if (CanUsed == false)
                return;
            
            if (ComposeNodeLeft != null)
            {
                ResultPose = ComposeNodeLeft.ResultPose;
            }
            else
            {
                var substate = sys.SubStates[LeftSubStateIndex];
                var ps = p.GetState(LeftSubStateIndex);

                TempPose = ps->mPose;
                if (substate.TransformNode != null)
                {
                    ComputeMatrix(substate, substate.TransformNode, ref TempPose, usecompute);
                }

                ResultPose = TempPose;
            }

            if (ComposeNodeRight != null)
            {
                CGfxParticlePose.AddFunc(ref ResultPose, ref ResultPose, ref ComposeNodeRight.ResultPose);
            }
            else
            {
                var ps = p.GetState(RightSubStateIndex);
                var substate = sys.SubStates[RightSubStateIndex];
                TempPose = ps->mPose;
                if (substate.TransformNode != null)
                {
                    ComputeMatrix(substate, substate.TransformNode, ref TempPose, usecompute);
                }
                CGfxParticlePose.AddFunc(ref ResultPose, ref ResultPose, ref TempPose);
            }
            
        }

        unsafe public void SubFunc(CGfxParticleSystem sys, ref CGfxParticle p, int substatenum, float elaspe, bool usecompute)
        {
            if (CanUsed == false)
                return;

            if (ComposeNodeLeft != null)
            {
                ResultPose = ComposeNodeLeft.ResultPose;
            }
            else
            {
                var substate = sys.SubStates[LeftSubStateIndex];
                var ps = p.GetState(LeftSubStateIndex);

                TempPose = ps->mPose;
                if (substate.TransformNode != null)
                {
                    ComputeMatrix(substate, substate.TransformNode, ref TempPose, usecompute);
                }

                ResultPose = TempPose;
            }

            if (ComposeNodeRight != null)
            {
                CGfxParticlePose.SubFunc(ref ResultPose, ref ResultPose, ref ComposeNodeRight.ResultPose);
            }
            else
            {
                var ps = p.GetState(RightSubStateIndex);
                var substate = sys.SubStates[RightSubStateIndex];
                TempPose = ps->mPose;
                if (substate.TransformNode != null)
                {
                    ComputeMatrix(substate, substate.TransformNode, ref TempPose, usecompute);
                }
                CGfxParticlePose.SubFunc(ref ResultPose, ref ResultPose, ref ps->mPose);
            }
        }
        
        unsafe public void LerpFunc(CGfxParticleSystem sys, ref CGfxParticle p, int substatenum, float elaspe, bool usecompute)
        {
            if (CanUsed == false)
                return;

            GetFactor(ref p, elaspe);

            if (ComposeNodeLeft != null)
            {
                ResultPose = ComposeNodeLeft.ResultPose;
            }
            else
            {
                var substate = sys.SubStates[LeftSubStateIndex];
                var ps = p.GetState(LeftSubStateIndex);

                TempPose = ps->mPose;
                if (substate.TransformNode != null)
                {
                    ComputeMatrix(substate, substate.TransformNode, ref TempPose, usecompute);
                }

                ResultPose = TempPose;
            }

            if (ComposeNodeRight != null)
            {
                CGfxParticlePose.Lerp(ref ResultPose, ref ResultPose, ref ComposeNodeRight.ResultPose, Factor);
            }
            else
            {
                var ps = p.GetState(RightSubStateIndex);
                var substate = sys.SubStates[RightSubStateIndex];
                TempPose = ps->mPose;
                if (substate.TransformNode != null)
                {
                    ComputeMatrix(substate, substate.TransformNode, ref TempPose, usecompute);
                }

                CGfxParticlePose.Lerp(ref ResultPose, ref ResultPose, ref TempPose, Factor);
            }
         }
      
    }

    //粒子组合结点
    public class ParticleData
    {
        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.MacrossPanelPathAttribute("粒子系统/ParticleData/ParticleSystem")]
        [Editor.DisplayParamName("当前的粒子系统对象")]
        public CGfxParticleSystem ParticleSystem
        {
            get;
            set;
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.MacrossPanelPathAttribute("粒子系统/ParticleData/ParticleSubState")]
        [Editor.DisplayParamName("当前的粒子发射器状态")]
        public CGfxParticleSubState ParticleSubState
        {
            get;
            set;
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.MacrossPanelPathAttribute("粒子系统/ParticleData/ParticleEmitterShape")]
        [Editor.DisplayParamName("当前的粒子发射器形状")]
        public EmitShape.CGfxParticleEmitterShape ParticleEmitterShape
        {
            get;
            set;
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.MacrossPanelPathAttribute("粒子系统/ParticleData/Particle")]
        [Editor.DisplayParamName("当前粒子")]
        public unsafe CGfxParticle* Particle
        {
            get;
            set;
        }

        [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [Editor.MacrossPanelPathAttribute("粒子系统/ParticleData/ParticleState")]
        [Editor.DisplayParamName("当前粒子的状态")]
        public unsafe CGfxParticleState* ParticleState
        {
            get;
            set;
        }
    }

    public class TextureCutNode
    {
        public class Data
        {
            [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [EngineNS.Editor.DisplayParamName("序列数量")]
            public int Number
            {
                get;
                set;
            } = 1;

            [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [EngineNS.Editor.DisplayParamName("是否循环")]
            public bool Loop
            {
                get;
                set;
            } = false;

            [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [EngineNS.Editor.DisplayParamName("是否使用粒子生命周期")]
            public bool IsUseParticleLife
            {
                get;
                set;
            } = true;

            [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [EngineNS.Editor.DisplayParamName("循环一次的时间")]
            public float Time
            {
                get;
                set;
            } = 0.1f;

            [Category("初始值")]
            [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [EngineNS.Editor.DisplayParamName("初始最小序列")]
            public int Min
            {
                get;
                set;
            } = 0;

            [Category("初始值")]
            [Editor.MacrossMemberAttribute(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            [EngineNS.Editor.DisplayParamName("初始最大序列")]
            public int Max
            {
                get;
                set;
            } = 1;
        }

        public int CurrentIndex = 1;
        public int StartValue = 0;
        public Data _Data;
        public TextureCutNode()
        {
            _Data = new Data();
        }

        public virtual void GetStartValue(ref CGfxParticle p)
        {
            unsafe
            {
                CGfxParticleState* ps = p.GetState(0);
                ps->mPose.SetUserParamsZ(1, (byte)McParticleEffector.SRandomI2(_Data.Min, _Data.Max));
            }
        }

        public void Update(float e, ref CGfxParticle p)
        {
            var tick = MathHelper.FClamp(p.mLifeTick, 0f, p.mLife);
            int startindex = 0;
            unsafe
            {
                CGfxParticleState* ps = p.GetState(0);
                startindex = ps->mPose.GetUserParamsZ(1);
            }
            
            if (_Data.IsUseParticleLife)
            {
                CurrentIndex = MathHelper.Clamp<int>((int)Math.Floor((tick / p.mLife) * (float)(_Data.Number- startindex)) + startindex, 1, _Data.Number);
            }
            else
            {
                if (_Data.Loop == false)
                {
                    float factor = MathHelper.FClamp(tick / _Data.Time, 0f, 1f);
                    CurrentIndex = MathHelper.Clamp<int>((int)Math.Floor((tick / _Data.Time) * (float)(_Data.Number - startindex)) + startindex, 1, _Data.Number);
                }
                else
                {
                    float factor = (tick / _Data.Time) % 1f;
                    
                    CurrentIndex = MathHelper.Clamp<int>((int)Math.Floor(factor * (float)_Data.Number), 1, _Data.Number);
                    CurrentIndex = (CurrentIndex + startindex) % _Data.Number;
                    if (CurrentIndex == 0)
                    {
                        CurrentIndex = _Data.Number;
                    }

                }
            }
        }

        public void SetValue()
        {
        }
    }

    //var actor = actorObj as EngineNS.GamePlay.Actor.GActor;

    //float yaw, pitch, roll;
    //                if (mRotationX is EditorCommon.PropertyMultiValue)
    //                    pitch = (((float)(((EditorCommon.PropertyMultiValue) mRotationX).Values[index]))) * delta;
    //                else
    //                    pitch = (System.Convert.ToSingle(mRotationX)) * delta;
    //                if (mRotationY is EditorCommon.PropertyMultiValue)
    //                    yaw = (((float)(((EditorCommon.PropertyMultiValue) mRotationY).Values[index]))) * delta;
    //                else
    //                    yaw = (System.Convert.ToSingle(mRotationY)) * delta;
    //                if (mRotationZ is EditorCommon.PropertyMultiValue)
    //                    roll = (((float)(((EditorCommon.PropertyMultiValue) mRotationZ).Values[index]))) * delta;
    //                else
    //                    roll = (System.Convert.ToSingle(mRotationZ)) * delta;

    //actor.Placement.Rotation = EngineNS.Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
    public class RotationNode : DataGradient3
    {
        public override void SetMax(ref Vector3 max)
        {
            _Max = max;
        }

        public override void SetMin(ref Vector3 min)
        {
            _Min = min;
        }

        public Quaternion Quaternion = Quaternion.Identity;
        public Quaternion StartValue = Quaternion.Identity;

        public Vector3 YawPitchRoll = Vector3.Zero;
        public Vector3 StartValue2 = Vector3.Zero;
        public Vector3 _Min = Vector3.Zero;
        public Vector3 _Max = Vector3.Zero;


        public virtual void SetYawPitchRoll(float life, float tick)
        {
            var factor = Math.Min(1f, tick / life);
            if (IsUseDefaultTick == false)
            {
                factor = GetProportion(tick);
            }
            GetTickData(factor);
            YawPitchRoll = _ResultValue;
        }

        public virtual void GetStartValue2()
        {
            GetStartValue();
            StartValue2 *= MathHelper.Deg2Rad;
            Quaternion.RotationYawPitchRoll(StartValue2.X, StartValue2.Y, StartValue2.Z, out StartValue);

        }

        public void Update(float elaspe, ref CGfxParticlePose pose, ref CGfxParticlePose startpose)
        {
            YawPitchRoll *= MathHelper.Deg2Rad;
            Quaternion.RotationYawPitchRoll(YawPitchRoll.X, YawPitchRoll.Y , YawPitchRoll.Z, out Quaternion);
            pose.mRotation = startpose.mRotation * Quaternion;
        }
    }

    public class TransformNode
    {
        public class Data
        {
            public Vector3 translation = Vector3.Zero;
            public Quaternion rotation = Quaternion.Identity;
            public Vector3 yawpitchroll = Vector3.Zero;
            public Vector3 scale = Vector3.UnitXYZ;
            public float offset = 0f;
            public Data(float v, Vector3 v1, Quaternion v2, Vector3 v3)
            {
                offset = v;
                translation = v1;
                rotation = v2;
                scale = v3;
            }

            public Data(float v, Vector3 v1, Vector3 v2, Vector3 v3)
            {
                offset = v;
                translation = v1;
                yawpitchroll = v2;
                scale = v3;
            }
        }
        
        public Vector3 Translation;
        public Vector3 Scale;
        public Quaternion Rotation;
        public Vector3 YawPitchRoll;

        public float StartTick = 0f;
        public bool Loop
        {
            get;
            set;
        } = false;

        public float Duration
        {
            get;
            set;
        } = 0f;

        public bool IgnoreRotation
        {
            get;
            set;
        } = true;

        public bool IgnoreScale
        {
            get;
            set;
        } = true;

        public TransformNode()
        {
            InitArray();
        }

        public virtual void SetMatrix(float tick)
        {
            if (DataArray.Count == 0)
                return;

            if ((DataArray.Count == 1))
            {
                Translation = DataArray[0].translation;
                YawPitchRoll = DataArray[0].yawpitchroll;
                Scale = DataArray[0].scale;
                return;
            }

            for (int i = 1; i < DataArray.Count; i ++)
            {
                if (i == (DataArray.Count - 1) && tick > DataArray[i].offset)
                {
                    Translation = DataArray[i].translation;
                    Scale = DataArray[i].scale;
                    YawPitchRoll = DataArray[i].yawpitchroll * MathHelper.Deg2Rad;
                }
                else if (i == 1 && tick <= DataArray[0].offset)
                {
                    Translation = DataArray[0].translation;
                    Scale = DataArray[0].scale;
                    YawPitchRoll = DataArray[0].yawpitchroll * MathHelper.Deg2Rad;
                }
                else if (((tick < DataArray[i].offset)
                            && (tick > DataArray[i - 1].offset)))
                {
                    Translation = EngineNS.Vector3.Lerp(DataArray[i - 1].translation, DataArray[i].translation, ((tick - DataArray[i - 1].offset)
                                    / (DataArray[i].offset - DataArray[i - 1].offset)));
                    //Rotation = EngineNS.Quaternion.Slerp(DataArray[i - 1].rotation, DataArray[i].rotation, ((tick - DataArray[i - 1].offset)
                    //                / (DataArray[i].offset - DataArray[i - 1].offset)));
                    YawPitchRoll = EngineNS.Vector3.Lerp(DataArray[i - 1].yawpitchroll, DataArray[i].yawpitchroll, ((tick - DataArray[i - 1].offset)
                                   / (DataArray[i].offset - DataArray[i - 1].offset))) * MathHelper.Deg2Rad;
                    Scale = EngineNS.Vector3.Lerp(DataArray[i - 1].scale, DataArray[i - 1].scale, ((tick - DataArray[i - 1].offset)
                                    / (DataArray[i].offset - DataArray[i - 1].offset)));
                }
            }
        }

        public List<Data> DataArray = new List<Data>();
        public virtual void InitArray()
        {
        }

    }

    [EngineNS.Editor.Editor_MacrossClass(ECSType.Client, Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class ParticleSystemNode : ParticleBase
    {

    }

    //随机方向
    public class RandomDirectionNode
    {
        public class Data
        {
            public EmitShape.CGfxParticleEmitterShape EmitterShape;
            bool mRandomDirAvailableX = false;
            [Category("随机方向")]
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public bool RandomDirAvailableX
            {
                get
                {
                    return mRandomDirAvailableX;
                }
                set
                {
                    mRandomDirAvailableX = value;
                    if (EmitterShape != null)
                    {
                        EmitterShape.RandomDirAvailableX = value;
                    }
                }
            }

            bool mRandomDirAvailableY = false;
            [Category("随机方向")]
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public bool RandomDirAvailableY
            {
                get
                {
                    return mRandomDirAvailableY;
                }
                set
                {
                    mRandomDirAvailableY = value;
                    if (EmitterShape != null)
                    {
                        EmitterShape.RandomDirAvailableY = value;
                    }
                }
            }

            bool mRandomDirAvailableZ = false;
            [Category("随机方向")]
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public bool RandomDirAvailableZ
            {
                get
                {
                    return mRandomDirAvailableZ;
                }
                set
                {
                    mRandomDirAvailableZ = value;
                    if (EmitterShape != null)
                    {
                        EmitterShape.RandomDirAvailableZ = value;
                    }
                }
            }

            bool mRandomDirAvailableInvX = false;
            [Category("随机方向")]
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public bool RandomDirAvailableInvX
            {
                get
                {
                    return mRandomDirAvailableInvX;
                }
                set
                {
                    mRandomDirAvailableInvX = value;
                    if (EmitterShape != null)
                    {
                        EmitterShape.RandomDirAvailableInvX = value;
                    }
                }
            }

            bool mRandomDirAvailableInvY = false;
            [Category("随机方向")]
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public bool RandomDirAvailableInvY
            {
                get
                {
                    return mRandomDirAvailableInvY;
                }
                set
                {
                    mRandomDirAvailableInvY = value;
                    if (EmitterShape != null)
                    {
                        EmitterShape.RandomDirAvailableInvY = value;
                    }
                }
            }

            bool mRandomDirAvailableInvZ = false;
            [Category("随机方向")]
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public bool RandomDirAvailableInvZ
            {
                get
                {
                    return mRandomDirAvailableInvZ;
                }
                set
                {
                    mRandomDirAvailableInvZ = value;
                    if (EmitterShape != null)
                    {
                        EmitterShape.RandomDirAvailableInvZ = value;
                    }
                }
            }
        }

        public Data _Data;
        public void GetValue(EmitShape.CGfxParticleEmitterShape shape)
        {
            if (_Data == null)
                return;

            shape.RandomDirAvailableX = _Data.RandomDirAvailableX;
            shape.RandomDirAvailableY = _Data.RandomDirAvailableY;
            shape.RandomDirAvailableZ = _Data.RandomDirAvailableZ;
            shape.RandomDirAvailableInvX = _Data.RandomDirAvailableInvX;
            shape.RandomDirAvailableInvY = _Data.RandomDirAvailableInvY;
            shape.RandomDirAvailableInvZ = _Data.RandomDirAvailableInvZ;

            shape.IsRandomDirection = true;
        }
    }

    //加速度结点
    public class AcceleratedNode : DataGradient1
    {
        public override void SetMax(float max)
        {
            base.SetMax(max);
            _Max = max;
        }

        public override void SetMin(float min)
        {
            base.SetMin(min);
            _Min = min;
        }

        public float Accelerated = 1f;
        public float StartValue = 1f;
        public float _Min = 1f;
        public float _Max = 1f;


        public virtual void SetAccelerated(float life, float tick)
        {
            var factor = Math.Min(1f, tick / life);
            if (IsUseDefaultTick == false)
            {
                factor = GetProportion(tick);
            }
            GetTickData(factor);
            Accelerated = _ResultValue;
        }
      
    }

    public class MaterialInstanceNode
    {
        public class Data
        {

        }

        public Data _Data;

        public virtual void SetMaterialInstanceValue(EngineNS.Graphics.Mesh.CGfxMesh mesh)
        {
        }
    }

    public class ParticleEmitShapeNode : ParticleBase
    {
    }

    public class TriggerNode
    {
        public enum TriggerType
        {
            Death = 1,
            Collision = 2,
            Born = 3,
            Update = 4,
        }

        public List<CGfxParticleSystem> ParticleSystems = new List<CGfxParticleSystem>();

        public class Data
        {

            //[EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable | Editor.MacrossMemberAttribute.enMacrossType.Overrideable)]
            //public List<string> TriggerNames
            //{
            //    get;
            //    set;
            //} = new List<string>();
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public string TriggerReceiver
            {
                get;
                set;
            }


            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public bool InheritVelocity
            {
                get;
                set;
            } = false;
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public float Acceleration
            {
                get;
                set;
            } = 0f;
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public bool InheritPosition
            {
                get;
                set;
            } = true;
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public bool InheritRotation
            {
                get;
                set;
            } = false;
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public bool InheritScale
            {
                get;
                set;
            } = false;
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public TriggerType Type
            {
                get;
                set;
            } = TriggerType.Death;
        }

        public Data _Data = new Data();

        public void TriggerDeathEvent(ref CGfxParticlePose pose)
        {
            if (ParticleSystems.Count == 0)
                return;

            if (_Data.Type == TriggerType.Death)
            {
                for (int i = 0; i < ParticleSystems.Count; i++)
                {
                    TriggerEvent te = new TriggerEvent();
                    te.TriggerData = _Data;
                    te.Pose = pose;
                    ParticleSystems[i].TriggerEvents.Add(te);
                }
            }
        }

        public void TriggerUpdateEvent(ref CGfxParticlePose pose, ref Vector3 prepos)
        {
            if (ParticleSystems.Count == 0)
                return;

            if (_Data.Type == TriggerType.Update)
            {
                for (int i = 0; i < ParticleSystems.Count; i++)
                {
                    TriggerEvent te = new TriggerEvent();
                    te.TriggerData = _Data;
                    te.Pose = pose;
                    te.Pose.mVelocity = te.Pose.mPosition - prepos; //TODO.
                    ParticleSystems[i].TriggerEvents.Add(te);
                }
            }
        }

        bool IsFirst = true;
        public void TriggerBornEvent(ref CGfxParticlePose pose)
        {
            if (IsFirst == false)
                return;

            if (ParticleSystems.Count == 0)
                return;

            if (_Data.Type == TriggerType.Born)
            {
                for (int i = 0; i < ParticleSystems.Count; i++)
                {
                    TriggerEvent te = new TriggerEvent();
                    te.TriggerData = _Data;
                    te.Pose = pose;
                    ParticleSystems[i].TriggerEvents.Add(te);
                }

                IsFirst = false;
            }
        }

        public void RefreshUpdateEvent(ref CGfxParticlePose pose, ref Vector3 prepos)
        {
            if (ParticleSystems.Count == 0)
                return;

            if (_Data.Type == TriggerType.Update)
            {
                for (int i = 0; i < ParticleSystems.Count; i++)
                {
                    for (int j = 0; j < ParticleSystems[i].TriggerEvents.Count; j++)
                    {
                        var te = ParticleSystems[i].TriggerEvents[j];
                        te.Pose.mVelocity = pose.mPosition;
                    }
                }
            }
        }
    }


    public class PointGravityNode
    {
        public class Data
        {
            [Category("重心")]
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public Vector3 Point
            {
                get;
                set;
            } = Vector3.Zero;

            [Category("半径")]
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]
            public float Radius
            {
                get;
                set;
            } = 1024f;

            [Category("重力加速度")]
            [EngineNS.Editor.MacrossMember(Editor.MacrossMemberAttribute.enMacrossType.Callable)]

            public float Accelerated
            {
                get;
                set;
            } = 1f;
        }

        public Data _Data = new Data();
    }

    public class BeamNode
    {
        //public class BeamModifier
        //{
        //    public enum ModifierType
        //    {
        //        PEB2MT_Source
        //    }
        //}

        public class Noise
        {
            #region LowFreq
            public bool LowFreq_Enabled
            {
                get;
                set;
            } = true;

            public float Frequency
            {
                get;
                set;
            } = 10.0f;

            //[-250, 250]
            public float Frequency_LowRange
            {
                get;
                set;
            } = 10.0f;

            #endregion
        }


    }
}
