using CodeDomNode.Particle;
using CodeGenerateSystem.Base;
using EngineNS.IO;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace CodeDomNode.Particle
{
    public interface IParticleNode
    {
        System.Threading.Tasks.Task InitGraph();
        CreateObject GetCreateObject();

        StructLinkControl GetLinkControlUp();

        string GetClassName();
    }

    public interface IParticleGradient
    {
        void SetPGradient(EngineNS.Bricks.Particle.CGfxParticleSystem sys);
        Object GetShowGradient();

        void SyncValues(EngineNS.Bricks.Particle.CGfxParticleSystem sys);
    }

    [EngineNS.Editor.Editor_MacrossClass(EngineNS.ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.Useable)]
    public class VectorData : INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        public ParticleDataGradient DataGradient;

        public virtual bool IsVaild()
        {
            return false;
        }

        public virtual VectorData Duplicate(ParticleDataGradient datagradient)
        {
            return null;
        }
    }

    public class FloatData : VectorData
    {
        float mMax;
        [Category("初始值")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Max
        {
            get => mMax;
            set
            {
                mMax = value;
                if(DataGradient.PDataGradient != null)
                    DataGradient.PDataGradient.SetMax(value);
            }
        }

        float mMin;
        [Category("初始值")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Min
        {
            get => mMin;
            set
            {
                mMin = value;
                if (DataGradient.PDataGradient != null)
                    DataGradient.PDataGradient.SetMin(value);
            }
        }

        public override bool IsVaild()
        {
            if (Min == 0 && Max == 0)
                return false;

            return true;
        }

        public override VectorData Duplicate(ParticleDataGradient datagradient)
        {
            FloatData data = new FloatData();
            data.DataGradient = datagradient;
            data.Max = Max;
            data.Min = Min;
            return data;

        }
    }

    public class Vector2Data : VectorData
    {
        EngineNS.Vector2 mMax;
        [Category("初始值")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.Vector2 Max
        {
            get => mMax;
            set
            {
                mMax = value;
                if (DataGradient.PDataGradient != null)
                    DataGradient.PDataGradient.SetMax(ref value);
            }
        }

        EngineNS.Vector2 mMin;
        [Category("初始值")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.Vector2 Min
        {
            get => mMin;
            set
            {
                mMin = value;
                if (DataGradient.PDataGradient != null)
                    DataGradient.PDataGradient.SetMin(ref value);
            }
        }

        public override bool IsVaild()
        {
            if (Min.Equals(EngineNS.Vector2.Zero) && Max.Equals(EngineNS.Vector2.Zero))
                return false;

            return true;
        }

        public override VectorData Duplicate(ParticleDataGradient datagradient)
        {
            Vector2Data data = new Vector2Data();
            data.DataGradient = datagradient;
            data.Max = Max;
            data.Min = Min;
            return data;
        }
    }

    public class Vector3Data : VectorData
    {
        EngineNS.Vector3 mMax;
       [Category("初始值")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.Vector3 Max
        {
            get => mMax;
            set
            {
                mMax = value;
                if (DataGradient.PDataGradient != null)
                {
                    if (DataGradient.IsRotation)
                    {
                        var delta = (float)(System.Math.PI / 180);
                        float pitch = value.X * delta;
                        float yaw = value.Y * delta;
                        float roll = value.Z * delta;

                        EngineNS.Quaternion rot = EngineNS.Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
                        DataGradient.PDataGradient.SetMax(ref rot);
                    }
                    else
                    {
                        DataGradient.PDataGradient.SetMax(ref value);
                    }
                    
                }
            }
        }

        EngineNS.Vector3 mMin;
        [Category("初始值")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.Vector3 Min
        {
            get => mMin;
            set
            {
                mMin = value;
                if (DataGradient.PDataGradient != null)
                {
                    if (DataGradient.IsRotation)
                    {
                        var delta = (float)(System.Math.PI / 180);
                        float pitch = value.X * delta;
                        float yaw = value.Y * delta;
                        float roll = value.Z * delta;

                        EngineNS.Quaternion rot = EngineNS.Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
                        DataGradient.PDataGradient.SetMin(ref rot);
                    }
                    else
                    {
                        DataGradient.PDataGradient.SetMin(ref value);
                    }
                }
            }
        }

        public override bool IsVaild()
        {
            if (Min.Equals(EngineNS.Vector3.Zero) && Max.Equals(EngineNS.Vector3.Zero))
                return false;

            return true;
        }

        public override VectorData Duplicate(ParticleDataGradient datagradient)
        {
            Vector3Data data = new Vector3Data();
            data.DataGradient = datagradient;
            data.Max = Max;
            data.Min = Min;
            return data;
        }
    }

    public class Vector4Data : VectorData
    {
        EngineNS.Vector4 mMax;
        [Category("初始值")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.Vector4 Max
        {
            get => mMax;
            set
            {
                mMax = value;
                if (DataGradient.PDataGradient != null)
                    DataGradient.PDataGradient.SetMax(ref value);
            }
        }

        EngineNS.Vector4 mMin;
        [Category("初始值")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.Vector4 Min
        {
            get => mMin;
            set
            {
                mMin = value;
                if (DataGradient.PDataGradient != null)
                    DataGradient.PDataGradient.SetMin(ref value);
            }
        }

        public override bool IsVaild()
        {
            if (Min.Equals(EngineNS.Vector4.Zero) && Max.Equals(EngineNS.Vector4.Zero))
                return false;

            return true;
        }

        public override VectorData Duplicate(ParticleDataGradient datagradient)
        {
            Vector4Data data = new Vector4Data();
            data.DataGradient = datagradient;
            data.Max = Max;
            data.Min = Min;
            return data;
        }
    }

    [EngineNS.Editor.Editor_MacrossClass(EngineNS.ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class ParticleDataGradient: INotifyPropertyChanged
    {
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion

        [Category("数值")]
        [DisplayName("数值轴")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("DataGradientSetter")]
        public WPG.Themes.TypeEditors.TimerLine.DataCollect DataCollect
        {
            get;
            set;
        } = new WPG.Themes.TypeEditors.TimerLine.DataCollect();

        bool mIsUseDefaultTick = true;
        [Category("数值")]
        [DisplayName("使用默认时间")]
        public bool IsUseDefaultTick
        {
            get
            {
                if (PDataGradient != null)
                {
                    return PDataGradient.IsUseDefaultTick;
                }
                return mIsUseDefaultTick;
            }
            set
            {
                if (PDataGradient != null)
                {
                    PDataGradient.IsUseDefaultTick = value ;
                }
                mIsUseDefaultTick = value;
                OnPropertyChanged("IsUseDefaultTick");
            }
        }

        bool mLoop = true;
        [Category("数值")]
        [DisplayName("循环")]
        public bool Loop
        {
            get
            {
                if (PDataGradient != null)
                {
                    return PDataGradient.Loop;
                }
                return mLoop;
            }
            set
            {
                if (PDataGradient != null)
                {
                    PDataGradient.Loop = value;
                }
                mLoop = value;
                OnPropertyChanged("Loop");
            }
        }

        float mDuration = 0f;
        [Category("数值")]
        [DisplayName("时间长")]
        public float Duration
        {
            get
            {
                if (PDataGradient != null)
                {
                    return PDataGradient.Duration;
                }
                return mDuration;
            }
            set
            {
                if (PDataGradient != null)
                {
                    PDataGradient.Duration = value;
                }
                mDuration = value;
                OnPropertyChanged("Duration");
            }
        }
        
        public ParticleDataGradient Duplicate()
        {

            ParticleDataGradient datagradient = new ParticleDataGradient(DataCollect.TypeStr);

            datagradient.VectorData = VectorData.Duplicate(datagradient);
            datagradient.DataCollect = DataCollect.Duplicate();
            return datagradient;
        }

        public void OnChangeValue()
        {
            if (PDataGradient == null)
                return;

            SortDataCollect();
            if (DataCollect.TypeStr.Equals("Float"))
            {
                var datagradient = PDataGradient as EngineNS.Bricks.Particle.DataGradient1;
                datagradient.DataArray.Clear();
                for (int i = 0; i < DataCollect.Datas.Count; i++)
                {
                    datagradient.DataArray.Add(new EngineNS.Bricks.Particle.DataGradient1.Data(DataCollect.Datas[i].offset, DataCollect.Datas[i].value.X)); 
                }
            }
            else if (DataCollect.TypeStr.Equals("Float2"))
            {
                var datagradient = PDataGradient as EngineNS.Bricks.Particle.DataGradient2;
                datagradient.DataArray.Clear();
                for (int i = 0; i < DataCollect.Datas.Count; i++)
                {
                    datagradient.DataArray.Add(new EngineNS.Bricks.Particle.DataGradient2.Data(DataCollect.Datas[i].offset, new EngineNS.Vector2(DataCollect.Datas[i].value.X, DataCollect.Datas[i].value.Y)));
                }
            }
            else if (DataCollect.TypeStr.Equals("Float3"))
            {
                if (IsRotation)
                {
                    var datagradient = PDataGradient as EngineNS.Bricks.Particle.DataGradientRotation;
                    datagradient.DataArray.Clear();
                    for (int i = 0; i < DataCollect.Datas.Count; i++)
                    {
                        var data = DataCollect.Datas[i].value;
                        var delta = (float)(System.Math.PI / 180);
                        float pitch = data.X * delta;
                        float yaw = data.Y * delta;
                        float roll = data.Z * delta;

                        EngineNS.Quaternion rot = EngineNS.Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
                        datagradient.DataArray.Add(new EngineNS.Bricks.Particle.DataGradientRotation.Data(DataCollect.Datas[i].offset, rot));
                    }
                }
                else
                {
                    var datagradient = PDataGradient as EngineNS.Bricks.Particle.DataGradient3;
                    datagradient.DataArray.Clear();
                    for (int i = 0; i < DataCollect.Datas.Count; i++)
                    {
                        datagradient.DataArray.Add(new EngineNS.Bricks.Particle.DataGradient3.Data(DataCollect.Datas[i].offset, new EngineNS.Vector3(DataCollect.Datas[i].value.X, DataCollect.Datas[i].value.Y, DataCollect.Datas[i].value.Z)));
                    }
                }
                
            }
            else if (DataCollect.TypeStr.Equals("Float4"))
            {
                var datagradient = PDataGradient as EngineNS.Bricks.Particle.DataGradient4;
                datagradient.DataArray.Clear();
                for (int i = 0; i < DataCollect.Datas.Count; i++)
                {
                    datagradient.DataArray.Add(new EngineNS.Bricks.Particle.DataGradient4.Data(DataCollect.Datas[i].offset, new EngineNS.Vector4(DataCollect.Datas[i].value.X, DataCollect.Datas[i].value.Y, DataCollect.Datas[i].value.Z, DataCollect.Datas[i].value.W)));
                }
            }
        }

        
        [Category("初始值")]
        [EngineNS.Editor.Editor_ShowOnlyInnerProperties]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public VectorData VectorData
        {
            get;
            set;
        }

        public EngineNS.Bricks.Particle.DataGradient PDataGradient;
       
        [Browsable(false)]
        public DataGradientElement DataGradient;
        public ParticleDataGradient(string typestr)
        {
            DataGradientElementConstructParam dataparam = new DataGradientElementConstructParam();
            dataparam.TypeStr = typestr;
            DataGradient = new DataGradientElement(dataparam);

            DataCollect.TypeStr = typestr;
            if (typestr == "Float4")
            {
                VectorData = new Vector4Data();
            }
            else if (typestr == "Float3")
            {
                VectorData = new Vector3Data();
            }
            else if (typestr == "Float2")
            {
                VectorData = new Vector2Data();
            }
            else
            {
                VectorData = new FloatData();
            }

            VectorData.DataGradient = this;

            DataCollect.OnChangeValue -= OnChangeValue;
            DataCollect.OnChangeValue += OnChangeValue;

        }

        public void Save(XndNode xndNode, bool newGuid)
        {
            if (DataGradient.TimeLine != null)
            {
                DataGradient.TimeLine.SetDataCollect(DataCollect);
            }
            DataGradient.Save(xndNode, newGuid);

            var attr = xndNode.AddAttrib("ParticleDataGradient_value");
            attr.Version = 1;
            attr.BeginWrite();
            switch (DataGradient.TypeStr)
            {
                case "Float":
                    {
                        var data = VectorData as FloatData;
                        attr.Write(data.Max);
                        attr.Write(data.Min);
                    }
                    break;
                case "Float2":
                    {
                        var data = VectorData as Vector2Data;
                        attr.Write(data.Max);
                        attr.Write(data.Min);
                    }
                    break;
                case "Float3":
                    {
                        var data = VectorData as Vector3Data;
                        attr.Write(data.Max);
                        attr.Write(data.Min);
                    }
                    break;
                case "Float4":
                    {
                        var data = VectorData as Vector4Data;
                        attr.Write(data.Max);
                        attr.Write(data.Min);
                    }
                    break;
            }

            attr.Write(IsUseDefaultTick);
            attr.Write(Loop);
            attr.Write(Duration);
            attr.EndWrite();
        }

        public async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            await DataGradient.Load(xndNode);
            if (DataGradient.TimeLine != null)
            {
                DataGradient.TimeLine.GetDataCollect(DataCollect);
            }

            var attr = xndNode.FindAttrib("ParticleDataGradient_value");
            if (attr != null)
            {
                if (attr.Version >= 0)
                {
                    attr.BeginRead();
                    switch (DataGradient.TypeStr)
                    {
                        case "Float":
                            {
                                var data = VectorData as FloatData;
                                float value1;
                                float value2;
                                attr.Read(out value1);
                                attr.Read(out value2);

                                data.Max = value1;
                                data.Min = value2;
                            }
                            break;
                        case "Float2":
                            {
                                var data = VectorData as Vector2Data;

                                EngineNS.Vector2 value1;
                                EngineNS.Vector2 value2;
                                attr.Read(out value1);
                                attr.Read(out value2);

                                data.Max = value1;
                                data.Min = value2;
                            }
                            break;
                        case "Float3":
                            {
                                var data = VectorData as Vector3Data;

                                EngineNS.Vector3 value1;
                                EngineNS.Vector3 value2;
                                attr.Read(out value1);
                                attr.Read(out value2);

                                data.Max = value1;
                                data.Min = value2;
                            }
                            break;
                        case "Float4":
                            {
                                var data = VectorData as Vector4Data;

                                EngineNS.Vector4 value1;
                                EngineNS.Vector4 value2;
                                attr.Read(out value1);
                                attr.Read(out value2);

                                data.Max = value1;
                                data.Min = value2;
                            }
                            break;
                    }

                    if (attr.Version >= 1)
                    {
                        bool loop, isdefaulttick;
                        float duration;
                        attr.Read(out isdefaulttick);
                        attr.Read(out loop);
                        attr.Read(out duration);

                        Loop = loop;
                        IsUseDefaultTick = isdefaulttick;
                        Duration = duration;
                        OnPropertyChanged("Loop");
                        OnPropertyChanged("IsUseDefaultTick");
                        OnPropertyChanged("Duration");
                    }
                    attr.EndRead();

                    VectorData.OnPropertyChanged("Max");
                    VectorData.OnPropertyChanged("Min");
                }
            }
           
        }

        public void SetDataCollect()
        {
            if (DataGradient.TimeLine != null)
            {
                DataGradient.TimeLine.SetDataCollect(DataCollect);
            }
        }

        public void SortDataCollect()
        {
            DataCollect.Datas.Sort((a, b) =>
            {
                if (b.offset < a.offset)
                    return 1;
                if (b.offset == a.offset)
                    return 0;
                else
                    return -1;
            });
        }

        [Browsable(false)]
        public bool IsRotation
        {
            get;
            set;
        } = false;
        public CodeExpression GetCreateExpression(EngineNS.Vector4 data)
        {
            if (DataCollect.TypeStr.Equals("Float"))
            {
                return new CodePrimitiveExpression(data.X);
            }
            else if (DataCollect.TypeStr.Equals("Float2"))
            {
                return new CodeObjectCreateExpression(typeof(EngineNS.Vector2), new CodePrimitiveExpression(data.X),
                                                                 new CodePrimitiveExpression(data.Y));
            }
            else if (DataCollect.TypeStr.Equals("Float3"))
            {
                if (IsRotation)
                {
                    var delta = (float)(System.Math.PI / 180);
                    float pitch = data.X * delta;
                    float yaw = data.Y * delta;
                    float roll = data.Z * delta;

                    EngineNS.Quaternion rot = EngineNS.Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
                    return new CodeObjectCreateExpression(typeof(EngineNS.Quaternion), new CodePrimitiveExpression(rot.X), new CodePrimitiveExpression(rot.Y), new CodePrimitiveExpression(rot.Z), new CodePrimitiveExpression(rot.W));
                }
                else
                {
                    return new CodeObjectCreateExpression(typeof(EngineNS.Vector3), new CodePrimitiveExpression(data.X),
                                                 new CodePrimitiveExpression(data.Y),
                                                 new CodePrimitiveExpression(data.Z));
                }

            }
            else
            {
                return new CodeObjectCreateExpression(typeof(EngineNS.Vector4), new CodePrimitiveExpression(data.X),
                                                                 new CodePrimitiveExpression(data.Y),
                                                                 new CodePrimitiveExpression(data.Z),
                                                                  new CodePrimitiveExpression(data.W));
            }

        }

        public CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression LerpMethodInvokeScale(CodeExpression value1obj, CodeExpression value2obj, CodeBinaryOperatorExpression v3)
        {
            System.CodeDom.CodeTypeReferenceExpression typeref;
            if (DataCollect.TypeStr.Equals("Float2"))
            {
                typeref = new System.CodeDom.CodeTypeReferenceExpression(typeof(EngineNS.Vector2).FullName);
            }
            else if (DataCollect.TypeStr.Equals("Float3"))
            {
                if (IsRotation)
                {
                    typeref = new System.CodeDom.CodeTypeReferenceExpression(typeof(EngineNS.Quaternion).FullName);
                }
                else
                {
                    typeref = new System.CodeDom.CodeTypeReferenceExpression(typeof(EngineNS.Vector3).FullName);
                }
            }
            else if (DataCollect.TypeStr.Equals("Float4"))
            {
                typeref = new System.CodeDom.CodeTypeReferenceExpression(typeof(EngineNS.Vector4).FullName);
            }
            else
            {
                typeref = new System.CodeDom.CodeTypeReferenceExpression(typeof(EngineNS.MathHelper).FullName);
               
            }

            //string lerpname;
            //if (IsRotation)
            //{
            //}
            //else
            //{
            //    lerpname = DataCollect.TypeStr.Equals("Float") ? "FloatLerp" : "Lerp";
            //}
            CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression  methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
            typeref,
            // methodName indicates the method to invoke.
            DataCollect.TypeStr.Equals("Float") ? "FloatLerp" : IsRotation ? "Slerp" : "Lerp",
            // parameters array contains the parameters for the method.
            new CodeExpression[] { value1obj, value2obj, v3 });

            return methodInvoke;
        }

        public void GCode_CodeDom_GenerateCode_For(CodeTypeDeclaration codeClass, CodeStatementCollection initCodeStatementCollection, string arrayname)
        {
            SortDataCollect();
           
            //初始化
            {
                Type type;
                if (DataCollect.TypeStr.Equals("Float4"))
                {
                    type = typeof(EngineNS.Bricks.Particle.DataGradient4.Data);
                }
                else if (DataCollect.TypeStr.Equals("Float3"))
                {
                    type = typeof(EngineNS.Bricks.Particle.DataGradient3.Data);
                }
                else if (DataCollect.TypeStr.Equals("Float2"))
                {
                    if (IsRotation)
                    {
                        type = typeof(EngineNS.Bricks.Particle.DataGradientRotation.Data);
                    }
                    else
                    {
                        type = typeof(EngineNS.Bricks.Particle.DataGradient2.Data);
                    }
                }
                else
                {
                    type = typeof(EngineNS.Bricks.Particle.DataGradient1.Data);
                }

                for (int i = 0; i < DataCollect.Datas.Count; i++)
                {
                   var methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                      // targetObject that contains the method to invoke.
                      new CodeVariableReferenceExpression(arrayname),
                      // methodName indicates the method to invoke.
                      "Add",
                      // parameters array contains the parameters for the method.
                      new CodeExpression[] { new CodeObjectCreateExpression(type, new CodeExpression[] { new CodePrimitiveExpression(DataCollect.Datas[i].offset), GetCreateExpression(DataCollect.Datas[i].value) } ) });
                    initCodeStatementCollection.Add(methodInvoke);
                }
            }
        }

        public void GCode_CodeDom_GenerateCode_Rotation_For(CodeTypeDeclaration codeClass, CodeStatementCollection initCodeStatementCollection, CodeStatementCollection codeStatementCollection, CodeExpression express, string arrayname, string strValueName)
        {
            if (DataCollect.TypeStr.Equals("Float3") == false)
                throw new Exception("Rotation 类型不对");

            //初始化 Max Min
            {

                var delta = (float)(System.Math.PI / 180);
                if (VectorData.IsVaild())
                {
                    var GetStartValue = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
                    GetStartValue.Name = "GetStartValue";
                    GetStartValue.ReturnType = new CodeTypeReference(typeof(void));
                    GetStartValue.Attributes = MemberAttributes.Public | MemberAttributes.Override;
                    codeClass.Members.Add(GetStartValue);
                    
                    var data = VectorData as Vector3Data;

                    float pitch = data.Max.X * delta;
                    float yaw = data.Max.Y * delta;
                    float roll = data.Max.Z * delta;

                    EngineNS.Quaternion rot = EngineNS.Quaternion.RotationYawPitchRoll(yaw, pitch, roll);

                    var ClassType = codeClass as CodeGenerateSystem.CodeDom.CodeTypeDeclaration;

   

                    ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("_Max"), new CodeObjectCreateExpression(typeof(EngineNS.Quaternion), new CodePrimitiveExpression(rot.X), new CodePrimitiveExpression(rot.Y), new CodePrimitiveExpression(rot.Z), new CodePrimitiveExpression(rot.W))));

                    pitch = data.Min.X * delta;
                    yaw = data.Min.Y * delta;
                    roll = data.Min.Z * delta;
                    rot = EngineNS.Quaternion.RotationYawPitchRoll(yaw, pitch, roll);
                    ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("_Min"), new CodeObjectCreateExpression(typeof(EngineNS.Quaternion), new CodePrimitiveExpression(rot.X), new CodePrimitiveExpression(rot.Y), new CodePrimitiveExpression(rot.Z), new CodePrimitiveExpression(rot.W))));


                    var var1 = new CodeVariableReferenceExpression("ref _Min");
                    var var2 = new CodeVariableReferenceExpression("ref _Max");
                    var var3 = new CodeVariableReferenceExpression("out StartValue");

                    var typeref = new System.CodeDom.CodeTypeReferenceExpression(typeof(EngineNS.Bricks.Particle.McParticleEffector).FullName);
                    CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                    // targetObject that contains the method to invoke.
                    typeref,
                    // methodName indicates the method to invoke.
                    "SRandomRotation",
                    // parameters array contains the parameters for the method.
                    new CodeExpression[] { var1, var2, var3 });

                    GetStartValue.Statements.Add(methodInvoke);

                }

            }

            SortDataCollect();

            //初始化
            {
                for (int i = 0; i < DataCollect.Datas.Count; i++)
                {
    
                    var methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                       // targetObject that contains the method to invoke.
                       new CodeVariableReferenceExpression(arrayname),
                       // methodName indicates the method to invoke.
                       "Add",
                       // parameters array contains the parameters for the method.
                       new CodeExpression[] { new CodeObjectCreateExpression(typeof(EngineNS.Bricks.Particle.DataGradientRotation.Data), new CodeExpression[] { new CodePrimitiveExpression(DataCollect.Datas[i].offset), GetCreateExpression(DataCollect.Datas[i].value) }) });
                    initCodeStatementCollection.Add(methodInvoke);
                }
            }

            //只有一个数据
            //if (DataCollect.Datas.Count == 1)
            {
                var condition = new CodeBinaryOperatorExpression(
                    new CodeVariableReferenceExpression("DataArray.Count"),
                    CodeBinaryOperatorType.ValueEquality,
                    new CodePrimitiveExpression(1));
                CodeAssignStatement result = new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), new CodeVariableReferenceExpression("DataArray[0].value"));
                codeStatementCollection.Add(new CodeConditionStatement(
                   condition,
                   result));
                //return;
            }

            //For
            {

                List<CodeStatement> codestatements = new List<CodeStatement>();

                var value = express;
                var offset1 = arrayname + "[i-1].offset";
                var value1 = arrayname + "[i-1].value";
                var offset2 = arrayname + "[i].offset";
                var value2 = arrayname + "[i].value";

                var value1obj = new CodeVariableReferenceExpression(value1);
                var value2obj = new CodeVariableReferenceExpression(value2);


                //当 i == 1
                {
                    var last = new CodeBinaryOperatorExpression(
                  value,
                   CodeBinaryOperatorType.GreaterThanOrEqual,
                    new CodeVariableReferenceExpression(offset1));

                    var condition = new CodeBinaryOperatorExpression(
                         new CodeVariableReferenceExpression("i"),
                        CodeBinaryOperatorType.ValueEquality,
                         new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("DataArray.Count"), CodeBinaryOperatorType.Subtract, new CodePrimitiveExpression(1)));

                    var CodeConditionStatement = new CodeConditionStatement(
                     last,
                     new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), value2obj));

                    codestatements.Add(new CodeConditionStatement(
                     condition,
                     CodeConditionStatement));
                }

                {
                    var first = new CodeBinaryOperatorExpression(
                     value,
                     CodeBinaryOperatorType.LessThanOrEqual,
                      new CodeVariableReferenceExpression(offset2));
                    var condition = new CodeBinaryOperatorExpression(
                      new CodeVariableReferenceExpression("i"),
                     CodeBinaryOperatorType.ValueEquality,
                      new CodePrimitiveExpression(1));


                    var CodeConditionStatement = new CodeConditionStatement(
                        first,
                        new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), value1obj));

                    codestatements.Add(new CodeConditionStatement(
                    condition,
                    CodeConditionStatement));

                }


                //"t = (value - offset1) / (offset2 - offset1) "
                CodeBinaryOperatorExpression v1 = new CodeBinaryOperatorExpression(
                value,
                CodeBinaryOperatorType.Subtract,
                new CodeVariableReferenceExpression(offset1));

                CodeBinaryOperatorExpression v2 = new CodeBinaryOperatorExpression(
                new CodeVariableReferenceExpression(offset2),
                CodeBinaryOperatorType.Subtract,
                new CodeVariableReferenceExpression(offset1));

                CodeBinaryOperatorExpression v3 = new CodeBinaryOperatorExpression(
                v1,
                CodeBinaryOperatorType.Divide,
                v2);

                CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression methodInvoke = LerpMethodInvokeScale(value1obj, value2obj, v3);
              

                CodeAssignStatement result = new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), methodInvoke);
                //  CodeBinaryOperatorExpression result = new CodeBinaryOperatorExpression(
                //new CodeVariableReferenceExpression(strValueName),
                // CodeBinaryOperatorType.Assign,
                // methodInvoke);

                var greaterthan = new CodeBinaryOperatorExpression(
               value,
               CodeBinaryOperatorType.LessThan,
                new CodeVariableReferenceExpression(offset2));

                var lessthan = new CodeBinaryOperatorExpression(
              value,
              CodeBinaryOperatorType.GreaterThan,
               new CodeVariableReferenceExpression(offset1));

                codestatements.Add(new CodeConditionStatement(
                                new CodeBinaryOperatorExpression(greaterthan, CodeBinaryOperatorType.BooleanAnd, lessthan),
                                result));


                CodeVariableDeclarationStatement testInt = new CodeVariableDeclarationStatement(typeof(int), "i", new CodePrimitiveExpression(1));

                CodeIterationStatement forLoop = new CodeIterationStatement(new CodeAssignStatement(new CodeVariableReferenceExpression("i"), new CodePrimitiveExpression(1)), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"), CodeBinaryOperatorType.LessThan, new CodeVariableReferenceExpression("DataArray.Count")), new CodeAssignStatement(new CodeVariableReferenceExpression("i"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))), codestatements.ToArray());

                codeStatementCollection.Add(testInt);
                codeStatementCollection.Add(forLoop);
            }
        }

        public void GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, string IDName, CodeGenerateSystem.CodeDom.CodeMemberMethod GetStartValue, string obename = "", string startvalue = "StartValue", bool isscale = false)
        {
            if (string.IsNullOrEmpty(obename))
            {
                obename = "";
            }
            else
            {
                obename += ".";
            }
            var ClassType = codeClass as CodeGenerateSystem.CodeDom.CodeTypeDeclaration;

            ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(obename + "Loop"), new CodePrimitiveExpression(Loop)));
            ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(obename + "IsUseDefaultTick"), new CodePrimitiveExpression(IsUseDefaultTick)));
            ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(obename + "Duration"), new CodePrimitiveExpression(Duration)));

            switch (DataGradient.TypeStr)
            {
                case "Float":
                    {
                        var data = VectorData as FloatData;

                        ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(obename + "_Max"), new CodePrimitiveExpression(data.Max)));
                        ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(obename + "_Min"), new CodePrimitiveExpression(data.Max)));

                        var var1 = new CodeVariableReferenceExpression(obename + "_Min");
                        var var2 = new CodeVariableReferenceExpression(obename + "_Max");

                        var typeref = new System.CodeDom.CodeTypeReferenceExpression("EngineNS.Bricks.Particle.McParticleEffector");
                        CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                       // targetObject that contains the method to invoke.
                       typeref,
                       // methodName indicates the method to invoke.
                       "SRandomF2",
                       // parameters array contains the parameters for the method.
                       new CodeExpression[] { var1, var2 });

                        CodeAssignStatement result = new CodeAssignStatement(new CodeVariableReferenceExpression(startvalue), methodInvoke);
                        GetStartValue.Statements.Add(result);
                    }
                    break;

                case "Float2":
                    {
                        var data = VectorData as Vector2Data;

                        ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(obename + "_Max"), new CodeObjectCreateExpression(typeof(EngineNS.Vector2), new CodePrimitiveExpression(data.Max.X), new CodePrimitiveExpression(data.Max.Y))));
                        ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(obename + "_Min"), new CodeObjectCreateExpression(typeof(EngineNS.Vector2), new CodePrimitiveExpression(data.Min.X), new CodePrimitiveExpression(data.Min.Y))));

                        var var1 = new CodeVariableReferenceExpression("ref _Min");
                        var var2 = new CodeVariableReferenceExpression("ref _Max");
                        var var3 = new CodeVariableReferenceExpression("ref " + startvalue);

                        var typeref = new System.CodeDom.CodeTypeReferenceExpression("EngineNS.Bricks.Particle.McParticleEffector");
                        CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                       // targetObject that contains the method to invoke.
                       typeref,
                       // methodName indicates the method to invoke.
                       "SRandomV2",
                       // parameters array contains the parameters for the method.
                       new CodeExpression[] { var1, var2, var3 });

                        GetStartValue.Statements.Add(methodInvoke);
                    }
                    break;
                case "Float3":
                    {
                        var data = VectorData as Vector3Data;
                        ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(obename + "_Max"), new CodeObjectCreateExpression(typeof(EngineNS.Vector3), new CodePrimitiveExpression(data.Max.X), new CodePrimitiveExpression(data.Max.Y), new CodePrimitiveExpression(data.Max.Z))));
                        ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(obename + "_Min"), new CodeObjectCreateExpression(typeof(EngineNS.Vector3), new CodePrimitiveExpression(data.Min.X), new CodePrimitiveExpression(data.Min.Y), new CodePrimitiveExpression(data.Min.Z))));


                        var var1 = new CodeVariableReferenceExpression("ref _Min");
                        var var2 = new CodeVariableReferenceExpression("ref _Max");
                        var var3 = new CodeVariableReferenceExpression("ref " + startvalue);

                        var typeref = new System.CodeDom.CodeTypeReferenceExpression("EngineNS.Bricks.Particle.McParticleEffector");
                        CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                       // targetObject that contains the method to invoke.
                       typeref,
                       // methodName indicates the method to invoke.
                       isscale ? "SRandomScale" : "SRandomV3",
                       // parameters array contains the parameters for the method.
                       new CodeExpression[] { var1, var2, var3 });

                        GetStartValue.Statements.Add(methodInvoke);
                    }
                    break;
                case "Float4":
                    {
                        var data = VectorData as Vector4Data;

                        ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(obename + "_Max"), new CodeObjectCreateExpression(typeof(EngineNS.Vector3), new CodePrimitiveExpression(data.Max.X), new CodePrimitiveExpression(data.Max.Y), new CodePrimitiveExpression(data.Max.Z), new CodePrimitiveExpression(data.Max.W))));
                        ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(obename + "_Min"), new CodeObjectCreateExpression(typeof(EngineNS.Vector3), new CodePrimitiveExpression(data.Min.X), new CodePrimitiveExpression(data.Min.Y), new CodePrimitiveExpression(data.Min.Z), new CodePrimitiveExpression(data.Min.W))));

                        var var1 = new CodeVariableReferenceExpression("ref _Min");
                        var var2 = new CodeVariableReferenceExpression("ref _Max");
                        var var3 = new CodeVariableReferenceExpression("ref " + startvalue);

                        var typeref = new System.CodeDom.CodeTypeReferenceExpression("EngineNS.Bricks.Particle.McParticleEffector");
                        CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                       // targetObject that contains the method to invoke.
                       typeref,
                       // methodName indicates the method to invoke.
                       "SRandomV4",
                       // parameters array contains the parameters for the method.
                       new CodeExpression[] { var1, var2, var3 });

                        GetStartValue.Statements.Add(methodInvoke);
                    }
                    break;

            }

        }

        public void GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass, string IDName, string startvalue = "StartValue", bool isscale = false)
        {
            //if (VectorData.IsVaild() == false)
            //    return;

            var GetStartValue = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
            GetStartValue.Name = "GetStartValue";
            GetStartValue.ReturnType = new CodeTypeReference(typeof(void));
            GetStartValue.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            codeClass.Members.Add(GetStartValue);

            GCode_CodeDom_GenerateCode(codeClass, IDName, GetStartValue, "", startvalue, isscale);
        }
    }

    [EngineNS.Editor.Editor_MacrossClass(EngineNS.ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class ParticleDataGradient2
    {

        [Category("数据1")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public ParticleDataGradient DataGradient1
        {
            get;
            set;
        }

        [Category("数据2")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public ParticleDataGradient DataGradient2
        {
            get;
            set;
        }
        public ParticleDataGradient2(string typestr1, string typestr2)
        {
            DataGradient1 = new ParticleDataGradient(typestr1);

            DataGradient2 = new ParticleDataGradient(typestr2);
        }

        public void Save(XndNode xndNode, bool newGuid)
        {
            DataGradient1.Save(xndNode, newGuid);
            DataGradient2.Save(xndNode, newGuid);
        }

        public async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            await DataGradient1.Load(xndNode);
            await DataGradient2.Load(xndNode);

        }

        public void SetDataCollect()
        {
            DataGradient1.SetDataCollect();

            DataGradient2.SetDataCollect();
        }
    }

    [EngineNS.Editor.Editor_MacrossClass(EngineNS.ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class ParticleColorGradient : INotifyPropertyChanged
    {
    
        #region INotifyPropertyChangedMembers
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            EngineNS.Editor.PropertyChangedUtility.PropertyChangedProcess(this, propertyName, PropertyChanged);
        }
        #endregion
        

       [Category("数值")]
        [DisplayName("颜色轴")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("ColorGradientSetter")]
        public WPG.Themes.TypeEditors.ColorGradient.DataCollect DataCollect
        {
            get;
            set;
        } = new WPG.Themes.TypeEditors.ColorGradient.DataCollect();

        bool mIsUseDefaultTick = true;
        [Category("数值")]
        [DisplayName("使用默认时间")]
        public bool IsUseDefaultTick
        {
            get
            {
                if (ColorBaseNode != null)
                {
                    return ColorBaseNode.IsUseDefaultTick;
                }

                return mIsUseDefaultTick;
            }
            set
            {
                if (ColorBaseNode != null)
                {
                    ColorBaseNode.IsUseDefaultTick = value;
                }

                mIsUseDefaultTick = value;
                OnPropertyChanged("IsUseDefaultTick");
            }
        }

        bool mLoop = true;
        [Category("数值")]
        [DisplayName("循环")]
        public bool Loop
        {
            get
            {
                if (ColorBaseNode != null)
                {
                    return ColorBaseNode.Loop;
                }
                
                return mLoop;
            }
            set
            {
                if (ColorBaseNode != null)
                {
                    ColorBaseNode.Loop = value;
                }
                mLoop = value;
                OnPropertyChanged("Loop");
            }
        }

        float mDuration = 0f;
        [Category("数值")]
        [DisplayName("时间长")]
        public float Duration
        {
            get
            {
                if (ColorBaseNode != null)
                {
                    return ColorBaseNode.Duration;
                }
                return mDuration;
            }
            set
            {
                if (ColorBaseNode != null)
                {
                    ColorBaseNode.Duration = value;
                }
                mDuration = value;
                OnPropertyChanged("Duration");
            }
        }

        EngineNS.Color4 m_Color1;
        [Category("初始值")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_UseCustomEditorAttribute]
        public EngineNS.Color4 Color1
        {
            get => m_Color1;
            set
            {
                m_Color1 = value;

                if (ColorBaseNode != null)
                {
                    ColorBaseNode.SetColor1(ref value);
                }
                OnPropertyChanged("Color1");
            }
        }

        EngineNS.Color4 m_Color2;
        [Category("初始值")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_UseCustomEditorAttribute]
        public EngineNS.Color4 Color2
        {
            get => m_Color2;
            set
            {
                m_Color2 = value;
                if (ColorBaseNode != null)
                {
                    ColorBaseNode.SetColor2(ref value);
                }
                OnPropertyChanged("Color2");
            }
        }

        EngineNS.Bricks.Particle.ColorBaseNode.BlendType mBlendType;
        [Category("混合方式")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public EngineNS.Bricks.Particle.ColorBaseNode.BlendType _BlendType
        {
            get => mBlendType;
            set
            {
                mBlendType = value;
                if (ColorBaseNode != null)
                {
                    ColorBaseNode._BlendType = value;
                }
                OnPropertyChanged("_BlendType");
            }
        }

        public ParticleColorGradient Duplicate( )
        {
            ParticleColorGradient colorGradient = new ParticleColorGradient();
            colorGradient.Color1 = Color1;
            colorGradient.Color2 = Color2;
            colorGradient._BlendType = _BlendType;
            colorGradient.DataCollect = DataCollect.Duplicate();
            return colorGradient;
        }

        [Browsable(false)]
        public ColorGradientControl ColorGradient;

        public ParticleColorGradient()
        {
            ColorGradientControlConstructParam colorparam = new ColorGradientControlConstructParam();
            ColorGradient = new ColorGradientControl(colorparam);

            var UIColorGradient = ColorGradient.UIColorGradient;

            DataCollect.OnChangeValue -= OnChangeValue;
            DataCollect.OnChangeValue += OnChangeValue;
        }

        public EngineNS.Bricks.Particle.ColorBaseNode ColorBaseNode;
        public void OnChangeValue()
        {
            if (ColorBaseNode == null)
                return;

            SortDataCollect();
            ColorBaseNode.DataArray.Clear();
            for (int i = 0; i < DataCollect.Datas.Count; i++)
            {
                ColorBaseNode.DataArray.Add(new EngineNS.Bricks.Particle.ColorBaseNode.Data(DataCollect.Datas[i].offset, DataCollect.Datas[i].value));
            }
        }

        public void Save(XndNode xndNode, bool newGuid)
        {
            //ColorGradient.UIColorGradient.SetDataCollect(DataCollect);
            ColorGradient.Save(xndNode, newGuid);

            var attr = xndNode.AddAttrib("ParticleColor_InitValue");
            attr.Version = 1;
            attr.BeginWrite();
            attr.Write(Color1);
            attr.Write(Color2);
            attr.Write(_BlendType);

            attr.Write(IsUseDefaultTick);
            attr.Write(Loop);
            attr.Write(Duration);
            attr.EndWrite();

        }

        public async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            await ColorGradient.Load(xndNode);

            ColorGradient.UIColorGradient.GetDataCollect(DataCollect);

            var attr = xndNode.FindAttrib("ParticleColor_InitValue");
            if (attr != null)
            {
                if (attr.Version >= 0)
                {
                    attr.BeginRead();
                    EngineNS.Color4 color;
                    attr.Read(out color);
                    Color1 = color;

                    attr.Read(out color);
                    Color2 = color;

                    attr.Read(out mBlendType);
                    _BlendType = mBlendType;

                    if (attr.Version >= 1)
                    {
                        bool loop, isdefaulttick;
                        float duration;
                        attr.Read(out isdefaulttick);
                        attr.Read(out loop);
                        attr.Read(out duration);

                        Loop = loop;
                        IsUseDefaultTick = isdefaulttick;
                        Duration = duration;

                        OnPropertyChanged("Loop");
                        OnPropertyChanged("IsUseDefaultTick");
                        OnPropertyChanged("Duration");
                    }

                    attr.EndRead();
                }
            }
        }

        public void SetDataCollect()
        {
            ColorGradient.UIColorGradient.SetDataCollect(DataCollect);
        }

        public void SortDataCollect()
        {
            DataCollect.Datas.Sort((a, b) =>
            {
                if (b.offset < a.offset)
                    return 1;
                if (b.offset == a.offset)
                    return 0;
                else
                    return -1;
            });
        }

        public void GCode_CodeDom_GenerateCode_For(CodeTypeDeclaration codeClass, CodeStatementCollection initCodeStatementCollection, string arrayname)
        {
            SortDataCollect();

            //初始化
            {
                for (int i = 0; i < DataCollect.Datas.Count; i++)
                {
                    var value = DataCollect.Datas[i].value;
                    var methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                       // targetObject that contains the method to invoke.
                       new CodeVariableReferenceExpression(arrayname),
                       // methodName indicates the method to invoke.
                       "Add",
                       // parameters array contains the parameters for the method.
                       new CodeExpression[] { new CodeObjectCreateExpression(typeof(EngineNS.Bricks.Particle.ColorBaseNode.Data), new CodeExpression[] { new CodePrimitiveExpression(DataCollect.Datas[i].offset),  new CodeObjectCreateExpression(typeof(EngineNS.Color4), new CodePrimitiveExpression(value.Alpha), new CodePrimitiveExpression(value.Red), new CodePrimitiveExpression(value.Green),     new CodePrimitiveExpression(value.Blue) ) }) });
                    initCodeStatementCollection.Add(methodInvoke);
                }
            }

            ////只有一个数据
            ////if (DataCollect.Datas.Count == 1)
            //{
            //    var condition = new CodeBinaryOperatorExpression(
            //        new CodeVariableReferenceExpression("DataArray.Count"),
            //        CodeBinaryOperatorType.ValueEquality,
            //        new CodePrimitiveExpression(1));
            //    CodeAssignStatement result = new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), new CodeVariableReferenceExpression("DataArray[0].value"));
            //    codeStatementCollection.Add(new CodeConditionStatement(
            //       condition,
            //       result));
            //    //return;
            //}

            ////For
            //{

            //    List<CodeStatement> codestatements = new List<CodeStatement>();

            //    var value = express;
            //    var offset1 = arrayname + "[i-1].offset";
            //    var value1 = arrayname + "[i-1].value";
            //    var offset2 = arrayname + "[i].offset";
            //    var value2 = arrayname + "[i].value";

            //    var value1obj = new CodeVariableReferenceExpression(value1);
            //    var value2obj = new CodeVariableReferenceExpression(value2);


            //    //当 i == 1
            //    {
            //        var last = new CodeBinaryOperatorExpression(
            //      value,
            //       CodeBinaryOperatorType.GreaterThanOrEqual,
            //        new CodeVariableReferenceExpression(offset1));

            //        var condition = new CodeBinaryOperatorExpression(
            //             new CodeVariableReferenceExpression("i"),
            //            CodeBinaryOperatorType.ValueEquality,
            //             new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("DataArray.Count"), CodeBinaryOperatorType.Subtract, new CodePrimitiveExpression(1)));

            //        var CodeConditionStatement = new CodeConditionStatement(
            //         last,
            //         new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), value2obj));

            //        codestatements.Add(new CodeConditionStatement(
            //         condition,
            //         CodeConditionStatement));
            //    }

            //    {
            //        var first = new CodeBinaryOperatorExpression(
            //         value,
            //         CodeBinaryOperatorType.LessThanOrEqual,
            //          new CodeVariableReferenceExpression(offset2));
            //        var condition = new CodeBinaryOperatorExpression(
            //          new CodeVariableReferenceExpression("i"),
            //         CodeBinaryOperatorType.ValueEquality,
            //          new CodePrimitiveExpression(1));


            //        var CodeConditionStatement = new CodeConditionStatement(
            //            first,
            //            new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), value1obj));

            //        codestatements.Add(new CodeConditionStatement(
            //        condition,
            //        CodeConditionStatement));

            //    }


            //    //"t = (value - offset1) / (offset2 - offset1) "
            //    CodeBinaryOperatorExpression v1 = new CodeBinaryOperatorExpression(
            //    value,
            //    CodeBinaryOperatorType.Subtract,
            //    new CodeVariableReferenceExpression(offset1));

            //    CodeBinaryOperatorExpression v2 = new CodeBinaryOperatorExpression(
            //    new CodeVariableReferenceExpression(offset2),
            //    CodeBinaryOperatorType.Subtract,
            //    new CodeVariableReferenceExpression(offset1));

            //    CodeBinaryOperatorExpression v3 = new CodeBinaryOperatorExpression(
            //    v1,
            //    CodeBinaryOperatorType.Divide,
            //    v2);

            //    var typeref = new System.CodeDom.CodeTypeReferenceExpression("EngineNS.Color4");
            //    //Lerp( Color4 color1, Color4 color2, float amount )
            //    var methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
            //    // targetObject that contains the method to invoke.
            //    typeref,
            //    // methodName indicates the method to invoke.
            //    "Lerp",
            //    // parameters array contains the parameters for the method.
            //    new CodeExpression[] { value1obj, value2obj, v3 });
            //    CodeAssignStatement result = new CodeAssignStatement(new CodeVariableReferenceExpression(strValueName), methodInvoke);

            //    var greaterthan = new CodeBinaryOperatorExpression(
            //   value,
            //   CodeBinaryOperatorType.LessThan,
            //    new CodeVariableReferenceExpression(offset2));

            //    var lessthan = new CodeBinaryOperatorExpression(
            //  value,
            //  CodeBinaryOperatorType.GreaterThan,
            //   new CodeVariableReferenceExpression(offset1));

            //    codestatements.Add(new CodeConditionStatement(
            //                    new CodeBinaryOperatorExpression(greaterthan, CodeBinaryOperatorType.BooleanAnd, lessthan),
            //                    result));


            //    CodeVariableDeclarationStatement testInt = new CodeVariableDeclarationStatement(typeof(int), "i", new CodePrimitiveExpression(1));

            //    CodeIterationStatement forLoop = new CodeIterationStatement(new CodeAssignStatement(new CodeVariableReferenceExpression("i"), new CodePrimitiveExpression(1)), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"), CodeBinaryOperatorType.LessThan, new CodeVariableReferenceExpression("DataArray.Count")), new CodeAssignStatement(new CodeVariableReferenceExpression("i"), new CodeBinaryOperatorExpression(new CodeVariableReferenceExpression("i"), CodeBinaryOperatorType.Add, new CodePrimitiveExpression(1))), codestatements.ToArray());

            //    codeStatementCollection.Add(testInt);
            //    codeStatementCollection.Add(forLoop);



            //}
        }

        public void GCode_CodeDom_GenerateCode(CodeTypeDeclaration codeClass)
        {
            CodeGenerateSystem.CodeDom.CodeTypeDeclaration customcodeClass = codeClass as CodeGenerateSystem.CodeDom.CodeTypeDeclaration;
            if(customcodeClass == null)
                throw new InvalidOperationException("请使用 CodeGenerateSystem.CodeDom.CodeTypeDeclaration 替换 System.CodeDom.CodeTypeDeclaration");

            customcodeClass.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("_BlendType"), new CodeVariableReferenceExpression(EngineNS.Rtti.RttiHelper.GetAppTypeString(_BlendType.GetType()) + "." + _BlendType.ToString())));

            var GetStartValue = new CodeGenerateSystem.CodeDom.CodeMemberMethod();
            GetStartValue.Name = "GetStartValue";
            GetStartValue.ReturnType = new CodeTypeReference(typeof(void));
            GetStartValue.Attributes = MemberAttributes.Public | MemberAttributes.Override;
            codeClass.Members.Add(GetStartValue);


            var ClassType = codeClass as CodeGenerateSystem.CodeDom.CodeTypeDeclaration;

            ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("Loop"), new CodePrimitiveExpression(Loop)));
            ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("IsUseDefaultTick"), new CodePrimitiveExpression(IsUseDefaultTick)));
            ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("Duration"), new CodePrimitiveExpression(Duration)));

            ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("Color1"), new CodeObjectCreateExpression(typeof(EngineNS.Color4), new CodePrimitiveExpression(Color1.Alpha), new CodePrimitiveExpression(Color1.Red), new CodePrimitiveExpression(Color1.Green), new CodePrimitiveExpression(Color1.Blue))));
            ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression("Color2"), new CodeObjectCreateExpression(typeof(EngineNS.Color4), new CodePrimitiveExpression(Color2.Alpha), new CodePrimitiveExpression(Color2.Red), new CodePrimitiveExpression(Color2.Green), new CodePrimitiveExpression(Color2.Blue))));


            var var1 = new CodeVariableReferenceExpression("Color1");
            var var2 = new CodeVariableReferenceExpression("Color2");

            var lerpvar = new CodeVariableDeclarationStatement(typeof(float), "temp");
            var lerpvar1 = new CodeVariableDeclarationStatement(typeof(float), "temp1", new CodePrimitiveExpression(0));
            var lerpvar2 = new CodeVariableDeclarationStatement(typeof(float), "temp2", new CodePrimitiveExpression(1));
            var var3 = new CodeVariableReferenceExpression("ref StartValue");

            GetStartValue.Statements.Add(lerpvar);
            GetStartValue.Statements.Add(lerpvar1);
            GetStartValue.Statements.Add(lerpvar2);

            var typeref = new System.CodeDom.CodeTypeReferenceExpression("EngineNS.Bricks.Particle.McParticleEffector");
            CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
           // targetObject that contains the method to invoke.
           typeref,
           // methodName indicates the method to invoke.
           "SRandomF2",
           // parameters array contains the parameters for the method.
           new CodeExpression[] { new CodeVariableReferenceExpression("temp1"), new CodeVariableReferenceExpression("temp2") });
            CodeAssignStatement result = new CodeAssignStatement(new CodeVariableReferenceExpression("temp"), methodInvoke);

            GetStartValue.Statements.Add(result);

            typeref = new System.CodeDom.CodeTypeReferenceExpression("EngineNS.Color4");
            //Lerp( Color4 color1, Color4 color2, float amount )
            methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
            // targetObject that contains the method to invoke.
            typeref,
            // methodName indicates the method to invoke.
            "Lerp",
            // parameters array contains the parameters for the method.
            new CodeExpression[] { var1, var2, new CodeVariableReferenceExpression("temp") });
            result = new CodeAssignStatement(new CodeVariableReferenceExpression("StartValue"), methodInvoke);
            GetStartValue.Statements.Add(result);

        }
    }

    [EngineNS.Editor.Editor_MacrossClass(EngineNS.ECSType.Client, EngineNS.Editor.Editor_MacrossClassAttribute.enMacrossType.AllFeatures)]
    public class ParticleDataGradientTF
    {
        [Category("数值")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        [EngineNS.Editor.Editor_PropertyGridDataTemplateAttribute("TransformGradientSetter")]
        public WPG.Themes.TypeEditors.TransformGradient.DataCollect DataCollect
        {
            get;
            set;
        } = new WPG.Themes.TypeEditors.TransformGradient.DataCollect();

        public EngineNS.Bricks.Particle.TransformNode TransformNode;
        bool mLoop = false;
        [Category("属性")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool Loop
        {
            get
            {
                return mLoop;
            }
            set
            {
                mLoop = value;
                if (TransformNode != null)
                {
                    TransformNode.Loop = value;
                }
            }
        }

        bool mIgnoreRotation = true;
        [Category("属性")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IgnoreRotation
        {
            get
            {
                return mIgnoreRotation;
            }
            set
            {
                mIgnoreRotation = value;
                if (TransformNode != null)
                {
                    TransformNode.IgnoreRotation = value;
                }
            }
        }

        bool mIgnoreScale = true;
        [Category("属性")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public bool IgnoreScale
        {
            get
            {
                return mIgnoreScale;
            }
            set
            {
                mIgnoreScale = value;
                if (TransformNode != null)
                {
                    TransformNode.IgnoreScale = value;
                }
            }
        }

        float mDuration = 0f;
        [Category("属性")]
        [EngineNS.Editor.MacrossMember(EngineNS.Editor.MacrossMemberAttribute.enMacrossType.Callable)]
        public float Duration
        {
            get
            {
                return mDuration;
            }
            set
            {
                mDuration = value;
                if (TransformNode != null)
                {
                    TransformNode.Duration = value;
                }
            }
        }

        public ParticleDataGradientTF Duplicate()
        {
            ParticleDataGradientTF datagradient = new ParticleDataGradientTF();
            datagradient.Loop = Loop;
            datagradient.Duration = Duration;
            datagradient.IgnoreScale = IgnoreScale;

            datagradient.IgnoreRotation = IgnoreRotation;
            datagradient.DataCollect = DataCollect.Duplicate();
            return datagradient;
        }

        public ParticleDataGradientTF()
        {
            DataCollect.OnChangeValue -= OnChangeValue;
            DataCollect.OnChangeValue += OnChangeValue;
        }

        public void SortDataCollect()
        {
            DataCollect.Datas.Sort((a, b) =>
            {
                if (b.offset < a.offset)
                    return 1;
                if (b.offset == a.offset)
                    return 0;
                else
                    return -1;
            });
        }

        public void OnChangeValue()
        {
            if (TransformNode == null)
                return;

            SortDataCollect();

            EngineNS.Vector3 translation;
            EngineNS.Vector3 scale;
            EngineNS.Quaternion rotation;
            TransformNode.DataArray.Clear();
            for (int i = 0; i < DataCollect.Datas.Count; i++)
            {
                DataCollect.Datas[i].value.Placement.WorldMatrix.Decompose(out scale, out rotation, out translation);
                TransformNode.DataArray.Add(new EngineNS.Bricks.Particle.TransformNode.Data(DataCollect.Datas[i].offset, translation, DataCollect.Datas[i].value.YawPitchRoll, scale));
            }
        }
        
        public void Save(XndNode xndNode, bool newGuid)
        {
            DataCollect.Save(xndNode, newGuid);

            var attr = xndNode.AddAttrib("ParticleDataGradientTF_value");
            attr.Version = 0;
            attr.BeginWrite();
            attr.Write(Loop);
            attr.Write(Duration);
            attr.EndWrite();
        }

        public async System.Threading.Tasks.Task Load(XndNode xndNode)
        {
            await DataCollect.Load(xndNode);

            var attr = xndNode.FindAttrib("ParticleDataGradientTF_value");
            if (attr != null)
            {
                attr.BeginRead();
                if (attr.Version == 0)
                {
                    attr.Read(out mLoop);
                    attr.Read(out mDuration);
                }
                attr.EndRead();
            }
        }
        public void SetDataCollect()
        {
           
        }
        
        public void GCode_CodeDom_GenerateCode_For(CodeTypeDeclaration codeClass, CodeStatementCollection initCodeStatementCollection, string arrayname)
        {
            SortDataCollect();

            //构造
            {
                var ClassType = codeClass as CodeGenerateSystem.CodeDom.CodeTypeDeclaration;

                var properties = this.GetType().GetProperties();
                foreach (var p in properties)
                {
                    var attrs = p.GetCustomAttributes(typeof(EngineNS.Editor.MacrossMemberAttribute), true);
                    
                    if (attrs.Length > 0)
                    {
                        var type = p.GetValue(this).GetType();
                        if (type.IsPrimitive)
                        {
                            ClassType.ConstructStatements.Add(new CodeAssignStatement(new CodeVariableReferenceExpression(p.Name), new CodePrimitiveExpression(p.GetValue(this))));
                        }
                       
                    }
                }
            }

            //初始化
            {
                for (int i = 0; i < DataCollect.Datas.Count; i++)
                {
                    EngineNS.Vector3 pos;
                    EngineNS.Vector3 scale;
                    EngineNS.Quaternion rot;
                    var Placement = DataCollect.Datas[i].value.Placement;
                    Placement.WorldMatrix.Decompose(out scale, out rot, out pos);
                    if (float.IsNaN(rot.X) || float.IsNaN(rot.Y) || float.IsNaN(rot.Z) || float.IsNaN(rot.W))
                    {
                        rot = EngineNS.Quaternion.Identity;
                    }

                    string callstaticfunction = "EngineNS.Matrix.Transformation(new EngineNS.Vector3(" + scale.X + "f," + scale.Y + "f," + scale.Z + "f) ," +
                                       " new EngineNS.Quaternion(" + rot.X + "f," + rot.Y + "f," + rot.Z + "f," + rot.W + "f), new EngineNS.Vector3(" + pos.X + "f," + pos.Y + "f," + pos.Z + "f))";
                    CodeSnippetExpression expression = new CodeSnippetExpression(callstaticfunction);

                    var createtranslation = new CodeObjectCreateExpression(typeof(EngineNS.Vector3), new CodeExpression[] { new CodePrimitiveExpression(pos.X), new CodePrimitiveExpression(pos.Y), new CodePrimitiveExpression(pos.Z) });
                    var createscale = new CodeObjectCreateExpression(typeof(EngineNS.Vector3), new CodeExpression[] { new CodePrimitiveExpression(scale.X), new CodePrimitiveExpression(scale.Y), new CodePrimitiveExpression(scale.Z) });
                    var createrotation = new CodeObjectCreateExpression(typeof(EngineNS.Quaternion), new CodeExpression[] { new CodePrimitiveExpression(rot.X), new CodePrimitiveExpression(rot.Y), new CodePrimitiveExpression(rot.Z), new CodePrimitiveExpression(rot.W) });

                    var methodInvoke = new CodeGenerateSystem.CodeDom.CodeMethodInvokeExpression(
                       // targetObject that contains the method to invoke.
                       new CodeVariableReferenceExpression(arrayname),
                       // methodName indicates the method to invoke.
                       "Add",
                       // parameters array contains the parameters for the method.
                       new CodeExpression[] { new CodeObjectCreateExpression(typeof(EngineNS.Bricks.Particle.TransformNode.Data), new CodeExpression[] { new CodePrimitiveExpression(DataCollect.Datas[i].offset), createtranslation, createrotation, createscale }) });
                    initCodeStatementCollection.Add(methodInvoke);
                }
            }
            
        }
    }

    public class ParticleNodesControl
    {
        public static ParticleSystemControl FindParticleSystemNode(IParticleNode node)
        {
            var mCtrlValueLinkHandleUp = node.GetLinkControlUp();
            if (mCtrlValueLinkHandleUp.HasLink == false)
            {
                return null;
            }

            LinkInfo linkinfo = mCtrlValueLinkHandleUp.GetLinkInfo(0);
            if (linkinfo.m_linkFromObjectInfo == null || linkinfo.m_linkFromObjectInfo.HostNodeControl == null)
                return null;

            var basenodecontrol = linkinfo.m_linkFromObjectInfo.HostNodeControl;
            while (basenodecontrol != null)
            {
                if ((basenodecontrol as ParticleSystemControl) != null)
                {
                    return basenodecontrol as ParticleSystemControl;
                }

                IParticleNode particlenode = basenodecontrol as IParticleNode;
                if (particlenode == null)
                {
                    return null;
                }

                var linkcontrol = particlenode.GetLinkControlUp();
                if (linkcontrol == null)
                {
                    return null;
                }

                //if (linkcontrol.HasLink == false)
                //{
                //    return null;
                //}

                linkinfo = linkcontrol.GetLinkInfo(0);
                if (linkinfo == null || linkinfo.m_linkFromObjectInfo == null)
                    return null;


                basenodecontrol = linkinfo.m_linkFromObjectInfo.HostNodeControl;
            }
            return null;
        }
    }
}
