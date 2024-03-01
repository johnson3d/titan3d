using EngineNS.Bricks.CodeBuilder.MacrossNode;
using EngineNS.Rtti;
//using MathNet.Numerics.Distributions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using NPOI.OpenXmlFormats.Dml;
using NPOI.SS.Formula.Eval;
using NPOI.SS.Formula.Functions;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using static EngineNS.UI.Bind.TtBindingOperations;

namespace EngineNS.UI.Bind
{
    public enum EBindingMode
    {
        TwoWay,
        OneWay,
        OneTime,
        OneWayToSource,
        Default,
    }
    public enum EUpdateSourceTrigger
    {
        Default,
        PropertyChanged,
        LostFocus,
        Explicit
    }
    [Flags]
    internal enum EPrivateFlags : uint
    {
        SourceToTarget = 1 << 0,
        TargetToSource = 1 << 1,
        PropDefault = 1 << 2,
        NotifyOnTargetUpdated = 1 << 3,
        DefaultValueConverter = 1 << 4,
        InTransfer = 1 << 5,
        InUpdate = 1 << 6,
        TransferPending = 1 << 7,
        NeedDataTransfer = 1 << 8,
        TransferDeferred = 1 << 9,
        UpdateOnLostFocus = 1 << 10,
        UpdateExplicitly = 1 << 11,
        UpdateDefault = UpdateExplicitly | UpdateOnLostFocus,
        NeedsUpdate = 1 << 12,
        PathGeneratedInternally = 1 << 13,
        UsingMentor = 1 << 14,
        ResolveNamesInTemplate = 1 << 15,
        Detaching = 1 << 16,
        NeedsCollectionView = 1 << 17,
        InPriorityBindingExpression = 1 << 18,
        InMultiBindingExpression = 1 << 19,
        UsingFallbackValue = 1 << 20,
        NotifyOnValidationError = 1 << 21,
        Attaching = 1 << 22,
        NotifyOnSourceUpdated = 1 << 23,
        ValidatesOnExceptions = 1 << 24,
        ValidatesOnDataErrors = 1 << 25,
        IllegalInput = 1 << 26,
        NeedsValidation = 1 << 27,
        TargetWantsXTNotification = 1 << 28,
        ValidatesOnNotifyDataErrors = 1 << 29,
        DataErrorsChangedPending = 1 << 30,
        DeferUpdateForComposition = 0x80000000,

        PropagationMask = SourceToTarget | TargetToSource | PropDefault,
        UpdateMask = UpdateOnLostFocus | UpdateExplicitly,
        AdoptionMask = SourceToTarget | TargetToSource | NeedsUpdate | NeedsValidation,
    }

    [TypeConverter(typeof(PropertyPathConverter))]
    public sealed class TtPropertyPath
    {
        string mPath;
        public string Path
        {
            get => mPath;
            set => mPath = value;
        }
        public TtBindingExpressionBase Expr;

        public TtPropertyPath(string path)
        {
            mPath = path;
        }
    }
    public abstract class TtBindingBase
    {
        public TtBindTypeConvertBase Convert;
        internal TtBindingExpressionBase SourceExp;
        internal TtBindingExpressionBase TargetExp;

        [Flags]
        internal enum EBindingFlags : uint
        {
            /// <summary> Data flows from source to target (only) </summary>
            OneWay = TtBindingExpressionBase.EBindingFlags.OneWay,
            /// <summary> Data flows in both directions - source to target and vice-versa </summary>
            TwoWay = TtBindingExpressionBase.EBindingFlags.TwoWay,
            /// <summary> Data flows from target to source (only) </summary>
            OneWayToSource = TtBindingExpressionBase.EBindingFlags.OneWayToSource,
            /// <summary> Target is initialized from the source (only) </summary>
            OneTime = TtBindingExpressionBase.EBindingFlags.OneTime,
            /// <summary> Data flow obtained from target property default </summary>
            PropDefault = TtBindingExpressionBase.EBindingFlags.PropDefault,

            /// <summary> Raise TargetUpdated event whenever a value flows from source to target </summary>
            NotifyOnTargetUpdated = TtBindingExpressionBase.EBindingFlags.NotifyOnTargetUpdated,
            /// <summary> Raise SourceUpdated event whenever a value flows from target to source </summary>
            NotifyOnSourceUpdated = TtBindingExpressionBase.EBindingFlags.NotifyOnSourceUpdated,
            /// <summary> Raise ValidationError event whenever there is a ValidationError on Update</summary>
            NotifyOnValidationError = TtBindingExpressionBase.EBindingFlags.NotifyOnValidationError,

            /// <summary> Obtain trigger from target property default </summary>
            UpdateDefault = TtBindingExpressionBase.EBindingFlags.UpdateDefault,
            /// <summary> Update the source value whenever the target value changes </summary>
            UpdateOnPropertyChanged = TtBindingExpressionBase.EBindingFlags.UpdateOnPropertyChanged,
            /// <summary> Update the source value whenever the target element loses focus </summary>
            UpdateOnLostFocus = TtBindingExpressionBase.EBindingFlags.UpdateOnLostFocus,
            /// <summary> Update the source value only when explicitly told to do so </summary>
            UpdateExplicitly = TtBindingExpressionBase.EBindingFlags.UpdateExplicitly,

            /// <summary>
            /// Used to determine whether the Path was internally Generated (such as the implicit
            /// /InnerText from an XPath).  If it is, then it doesn't need to be serialized.
            /// </summary>
            PathGeneratedInternally = TtBindingExpressionBase.EBindingFlags.PathGeneratedInternally,

            ValidatesOnExceptions = TtBindingExpressionBase.EBindingFlags.ValidatesOnExceptions,
            ValidatesOnDataErrors = TtBindingExpressionBase.EBindingFlags.ValidatesOnDataErrors,
            ValidatesOnNotifyDataErrors = TtBindingExpressionBase.EBindingFlags.ValidatesOnNotifyDataErrors,

            /// <summary> Flags describing data transfer </summary>
            PropagationMask = OneWay | TwoWay | OneWayToSource | OneTime | PropDefault,

            /// <summary> Flags describing update trigger </summary>
            UpdateMask = UpdateDefault | UpdateOnPropertyChanged | UpdateOnLostFocus | UpdateExplicitly,

            /// <summary> Default value</summary>
            Default = TtBindingExpressionBase.EBindingFlags.Default | ValidatesOnNotifyDataErrors,

            /// <summary> Error value, returned by FlagsFrom to indicate faulty input</summary>
            IllegalInput = TtBindingExpressionBase.EBindingFlags.IllegalInput,
        }
        EBindingFlags mFlags;

        internal bool TestFlag(EBindingFlags flag)
        {
            return (mFlags & flag) != 0;
        }
        internal void SetFlag(EBindingFlags flag)
        {
            mFlags |= flag;
        }
        internal void ClearFlag(EBindingFlags flag)
        {
            mFlags &= ~flag;
        }
        internal void ChangeFlag(EBindingFlags flag, bool value)
        {
            if (value)
                mFlags |= flag;
            else
                mFlags &= ~flag;
        }
        internal EBindingFlags GetFlagsWithinMask(EBindingFlags mask)
        {
            return (mFlags & mask);
        }
        internal void ChangeFlagsWithinMask(EBindingFlags mask, EBindingFlags flags)
        {
            mFlags = (mFlags & ~mask) | (flags & mask);
        }
        internal static EBindingFlags FlagsFrom(EBindingMode bindingMode)
        {
            switch(bindingMode)
            {
                case EBindingMode.Default: return EBindingFlags.PropDefault;
                case EBindingMode.OneWay: return EBindingFlags.OneWay;
                case EBindingMode.TwoWay: return EBindingFlags.TwoWay;
                case EBindingMode.OneWayToSource: return EBindingFlags.OneWayToSource;
                case EBindingMode.OneTime: return EBindingFlags.OneTime;
            }
            return EBindingFlags.IllegalInput;
        }
        internal static EBindingFlags FlagsFrom(EUpdateSourceTrigger updateSourceTriger)
        {
            switch(updateSourceTriger)
            {
                case EUpdateSourceTrigger.Default: return EBindingFlags.Default;
                case EUpdateSourceTrigger.PropertyChanged: return EBindingFlags.UpdateOnPropertyChanged;
                case EUpdateSourceTrigger.LostFocus: return EBindingFlags.UpdateOnLostFocus;
                case EUpdateSourceTrigger.Explicit: return EBindingFlags.UpdateExplicitly;
            }
            return EBindingFlags.IllegalInput;
        }

        [DefaultValue(EBindingMode.Default)]
        public EBindingMode Mode
        {
            get
            {
                switch (GetFlagsWithinMask(EBindingFlags.PropagationMask))
                {
                    case EBindingFlags.OneWay: return EBindingMode.OneWay;
                    case EBindingFlags.TwoWay: return EBindingMode.TwoWay;
                    case EBindingFlags.OneWayToSource: return EBindingMode.OneWayToSource;
                    case EBindingFlags.OneTime: return EBindingMode.OneTime;
                    case EBindingFlags.PropDefault: return EBindingMode.Default;
                }
                return EBindingMode.Default;
            }
            set
            {
                var flags = FlagsFrom(value);
                if (flags == EBindingFlags.IllegalInput)
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(EBindingMode));
                ChangeFlagsWithinMask(EBindingFlags.PropagationMask, flags);
            }
        }
        [DefaultValue(EUpdateSourceTrigger.Default)]
        public EUpdateSourceTrigger UpdateSourceTriger
        {
            get
            {
                switch (GetFlagsWithinMask(EBindingFlags.UpdateMask))
                {
                    case EBindingFlags.UpdateOnPropertyChanged: return EUpdateSourceTrigger.PropertyChanged;
                    case EBindingFlags.UpdateOnLostFocus: return EUpdateSourceTrigger.LostFocus;
                    case EBindingFlags.UpdateExplicitly: return EUpdateSourceTrigger.Explicit;
                    case EBindingFlags.UpdateDefault: return EUpdateSourceTrigger.Default;
                }
                return EUpdateSourceTrigger.Default;
            }
            set
            {
                var flags = FlagsFrom(value);
                if (flags == EBindingFlags.IllegalInput)
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(EUpdateSourceTrigger));
                ChangeFlagsWithinMask(EBindingFlags.UpdateMask, flags);
            }
        }
    }
    public abstract class TtBindingExpressionBase
    {
        protected TtBindingExpressionBase mParentExp = null;
        protected TtBindingBase mBinding;
        public TtBindingBase Binding => mBinding;

        TtPropertyPath mPath;
        public TtPropertyPath Path
        {
            get => mPath;
            set
            {
                mPath = value;
                ClearFlag(EBindingFlags.PathGeneratedInternally);
            }
        }
        WeakReference<IBindableObject> mSourceRef;
        public IBindableObject Source
        {
            get
            {
                if (mSourceRef == null)
                    return null;
                return mSourceRef.TryGetTarget(out IBindableObject retVal) ? retVal : null;
            }
            set
            {
                mSourceRef = new WeakReference<IBindableObject>(value);
            }
        }

        [Flags]
        internal enum EBindingFlags : uint
        {
            OneWay = EPrivateFlags.SourceToTarget,
            TwoWay = EPrivateFlags.SourceToTarget | EPrivateFlags.TargetToSource,
            OneWayToSource = EPrivateFlags.TargetToSource,
            OneTime = 0,
            PropDefault = EPrivateFlags.PropDefault,
            NotifyOnTargetUpdated = EPrivateFlags.NotifyOnTargetUpdated,
            NotifyOnSourceUpdated = EPrivateFlags.NotifyOnSourceUpdated,
            NotifyOnValidationError = EPrivateFlags.NotifyOnValidationError,
            UpdateOnPropertyChanged = 0,
            UpdateOnLostFocus = EPrivateFlags.UpdateOnLostFocus,
            UpdateExplicitly = EPrivateFlags.UpdateExplicitly,
            UpdateDefault = EPrivateFlags.UpdateDefault,
            PathGeneratedInternally = EPrivateFlags.PathGeneratedInternally,
            ValidatesOnExceptions = EPrivateFlags.ValidatesOnExceptions,
            ValidatesOnDataErrors = EPrivateFlags.ValidatesOnDataErrors,
            ValidatesOnNotifyDataErrors = EPrivateFlags.ValidatesOnNotifyDataErrors,

            Default = PropDefault | UpdateDefault,

            /// <summary> Error value, returned by FlagsFrom to indicate faulty input</summary>
            IllegalInput = EPrivateFlags.IllegalInput,

            PropagationMask = OneWay | TwoWay | OneWayToSource | OneTime | PropDefault,
            UpdateMask = UpdateOnPropertyChanged | UpdateOnLostFocus | UpdateExplicitly | UpdateDefault,
        }
        EBindingFlags mFlags;
        internal bool TestFlag(EBindingFlags flag)
        {
            return (mFlags & flag) != 0;
        }
        internal void SetFlag(EBindingFlags flag)
        {
            mFlags |= flag;
        }
        internal void ClearFlag(EBindingFlags flag)
        {
            mFlags &= ~flag;
        }
        internal void ChangeFlag(EBindingFlags flag, bool value)
        {
            if (value)
                mFlags |= flag;
            else
                mFlags &= ~flag;
        }
        internal EBindingFlags GetFlagsWithinMask(EBindingFlags mask)
        {
            return (mFlags & mask);
        }
        internal void ChangeFlagsWithinMask(EBindingFlags mask, EBindingFlags flags)
        {
            mFlags = (mFlags & ~mask) | (flags & mask);
        }
        internal static EBindingFlags FlagsFrom(EBindingMode bindingMode)
        {
            switch (bindingMode)
            {
                case EBindingMode.Default: return EBindingFlags.PropDefault;
                case EBindingMode.OneWay: return EBindingFlags.OneWay;
                case EBindingMode.TwoWay: return EBindingFlags.TwoWay;
                case EBindingMode.OneWayToSource: return EBindingFlags.OneWayToSource;
                case EBindingMode.OneTime: return EBindingFlags.OneTime;
            }
            return EBindingFlags.IllegalInput;
        }
        internal static EBindingFlags FlagsFrom(EUpdateSourceTrigger updateSourceTriger)
        {
            switch (updateSourceTriger)
            {
                case EUpdateSourceTrigger.Default: return EBindingFlags.Default;
                case EUpdateSourceTrigger.PropertyChanged: return EBindingFlags.UpdateOnPropertyChanged;
                case EUpdateSourceTrigger.LostFocus: return EBindingFlags.UpdateOnLostFocus;
                case EUpdateSourceTrigger.Explicit: return EBindingFlags.UpdateExplicitly;
            }
            return EBindingFlags.IllegalInput;
        }

        [DefaultValue(EBindingMode.Default)]
        public EBindingMode Mode
        {
            get
            {
                switch (GetFlagsWithinMask(EBindingFlags.PropagationMask))
                {
                    case EBindingFlags.OneWay: return EBindingMode.OneWay;
                    case EBindingFlags.TwoWay: return EBindingMode.TwoWay;
                    case EBindingFlags.OneWayToSource: return EBindingMode.OneWayToSource;
                    case EBindingFlags.OneTime: return EBindingMode.OneTime;
                    case EBindingFlags.PropDefault: return EBindingMode.Default;
                }
                return EBindingMode.Default;
            }
            set
            {
                var flags = FlagsFrom(value);
                if (flags == EBindingFlags.IllegalInput)
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(EBindingMode));
                ChangeFlagsWithinMask(EBindingFlags.PropagationMask, flags);
            }
        }
        [DefaultValue(EUpdateSourceTrigger.Default)]
        public EUpdateSourceTrigger UpdateSourceTriger
        {
            get
            {
                switch (GetFlagsWithinMask(EBindingFlags.UpdateMask))
                {
                    case EBindingFlags.UpdateOnPropertyChanged: return EUpdateSourceTrigger.PropertyChanged;
                    case EBindingFlags.UpdateOnLostFocus: return EUpdateSourceTrigger.LostFocus;
                    case EBindingFlags.UpdateExplicitly: return EUpdateSourceTrigger.Explicit;
                    case EBindingFlags.UpdateDefault: return EUpdateSourceTrigger.Default;
                }
                return EUpdateSourceTrigger.Default;
            }
            set
            {
                var flags = FlagsFrom(value);
                if (flags == EBindingFlags.IllegalInput)
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(EUpdateSourceTrigger));
                ChangeFlagsWithinMask(EBindingFlags.UpdateMask, flags);
            }
        }
        public TtBindingExpressionBase(TtBindingBase binding, TtBindingExpressionBase parent)
        {
            mParentExp = parent;
            mBinding = binding;
        }
        public abstract void SetValue<T>(TtBindableProperty bp, T value);
        public abstract T GetValue<T>(TtBindableProperty bp);
        public abstract void SetValueStore<T>(T value);
        public abstract void UpdateSource();
    }
    public class TtBinding : TtBindingBase
    {
    }
    public abstract class ValueStoreBase 
    {
        public void SetValue<T>(in T value)
        {
            var vs = this as ValueStore<T>;
            if(vs != null)
                vs.Value = value;
        }
        public T GetValue<T>()
        {
            var vs = this as ValueStore<T>;
            if (vs != null)
                return vs.Value;
            return default(T);
        }
        public abstract void CopyFrom(ValueStoreBase source);
        public bool IsValueEqual<T>(in T source)
        {
            var vs = this as ValueStore<T>;
            if (vs == null)
                return false;
            return EqualityComparer<T>.Default.Equals(vs.Value, source);
        }
        public abstract bool IsValueEqual(in ValueStoreBase source);
    }
    public class ValueStore<T> : ValueStoreBase
    {
        public T Value;
        public ValueStore() { }
        public ValueStore(in T val)
        {
            Value = val;
        }

        public override void CopyFrom(ValueStoreBase source)
        {
            var vs = source as ValueStore<T>;
            if (vs == null)
                return;
            Value = vs.Value;
        }

        public override bool IsValueEqual(in ValueStoreBase source)
        {
            var vs = source as ValueStore<T>;
            if (vs == null)
                return false;
            return EqualityComparer<T>.Default.Equals(Value, vs.Value);
        }
    }
    public class TtDefaultValueExpression : TtBindingExpressionBase
    {
        ValueStoreBase mFinalValue = null;
        public TtDefaultValueExpression(in ValueStoreBase val)
            : base(null, null)
        {
            mFinalValue = val;
        }
        public TtDefaultValueExpression(TtBindingBase binding, TtBindingExpressionBase parent)
            : base(binding, parent)
        {

        }
        public override T GetValue<T>(TtBindableProperty bp)
        {
            return mFinalValue.GetValue<T>();
        }

        public override void SetValue<T>(TtBindableProperty bp, T value)
        {
            mFinalValue.SetValue(value);
        }

        public override void SetValueStore<T>(T value)
        {
            mFinalValue.SetValue(value);
        }

        public override void UpdateSource()
        {
        }
    }
    public class TtBindingExpression<TProp> : TtBindingExpressionBase
    {
        ValueStoreBase mValueStore;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ValueStoreBase GetValueStore<T>()
        {
            if (mValueStore == null)
                mValueStore = new ValueStore<T>();
            return mValueStore;
        }
        ValueStoreBase mFinalValue;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected ValueStoreBase GetFinalValue<T>()
        {
            if(mFinalValue == null)
                mFinalValue = new ValueStore<T>();
            return mFinalValue;
        }
        public IBindableObject TargetObject;
        public TtBindableProperty<TProp> TargetProperty;
        protected UInt32 mSetValueTime = 0;

        public TtBindingExpression(TtBindingBase binding, TtBindingExpressionBase parent)
            : base(binding, parent)
        {

        }
        public TtBindingExpression(TtBindingBase binding, TProp val, TtBindingExpressionBase parent)
            : base(binding, parent)
        {
            GetValueStore<TProp>().SetValue(val);
        }
        public override void SetValueStore<T>(T value)
        {
            GetValueStore<T>().SetValue(value);
        }
        public unsafe override void SetValue<T>(TtBindableProperty bp, T value)
        {
            try
            {
                var binding = mBinding;// as TtBinding<TProp, TClass>;
                if (binding == null)
                    return;

                if(mParentExp != null)
                {
                    mParentExp.SetValue(bp, value);
                }
                else
                {
                    if (binding.Convert != null)
                    {
                        if (binding.TargetExp == this)
                        {
                            if (!binding.Convert.CanConvertTo<TProp, T>())
                                return;
                            var tempValue = binding.Convert.ConvertTo<TProp, T>(this, value);
                            if (GetValueStore<TProp>().IsValueEqual(tempValue))
                                return;
                            GetValueStore<TProp>().SetValue(tempValue);
                        }
                        else if (binding.SourceExp == this)
                        {
                            if (!binding.Convert.CanConvertFrom<T, TProp>())
                                return;
                            var tempValue = binding.Convert.ConvertFrom<T, TProp>(this, value);
                            if (GetValueStore<TProp>().IsValueEqual(tempValue))
                                return;
                            GetValueStore<TProp>().SetValue(tempValue);
                        }
                        else
                            return;
                    }
                    else
                    {
                        if (GetValueStore<T>().IsValueEqual(value)) 
                            return;
                        GetValueStore<T>().SetValue(value);
                    }

                    if(binding.TargetExp == this)
                    {
                        binding.SourceExp?.SetValueStore(value);
                    }
                    else if(binding.SourceExp == this)
                    {
                        binding.TargetExp?.SetValueStore(value);
                    }

                    switch (UpdateSourceTriger)
                    {
                        case EUpdateSourceTrigger.PropertyChanged:
                            UpdateSource();
                            break;
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        public override T GetValue<T>(TtBindableProperty bp)
        {
            if (typeof(T) == typeof(object))
                return (T)GetObjectValue(bp);
            return GetFinalValue<T>().GetValue<T>();
        }
        protected virtual object GetObjectValue(TtBindableProperty bp) 
        {
            return null;
        }
        public override void UpdateSource()
        {
            if ((Mode == EBindingMode.OneTime) && (mSetValueTime > 0))
                return;
            mSetValueTime++;
            GetFinalValue<TProp>().CopyFrom(mValueStore);
            if(mParentExp != null)
            {
                mParentExp.UpdateSource();
            }
            else
            {
                TargetProperty.OnValueChanged?.Invoke(TargetObject, TargetProperty, GetFinalValue<TProp>().GetValue<TProp>());
                if (TargetProperty.IsCallSetProperty)
                {
                    var proInfo = TargetObject.GetType().GetProperty(TargetProperty.Name);
                    if(proInfo != null)
                    {
                        proInfo.SetValue(TargetObject, GetValueStore<TProp>().GetValue<TProp>());
                    }
                }
            }
        }

    }
    public abstract class TtBindingMethodStoreBase
    {
        public void SetValue<T>(IBindableObject obj, TtBindableProperty bp, in T value)
        {
            var ms = this as TtBindingMethodStore<T>;
            if (ms != null)
                ms.Setter?.Invoke(obj, bp, value);
        }
        public T GetValue<T>(IBindableObject obj, TtBindableProperty bp)
        {
            var ms = this as TtBindingMethodStore<T>;
            if (ms != null && ms.Getter != null)
                return ms.Getter.Invoke(obj, bp);
            return default;
        }
    }
    public class TtBindingMethodStore<T> : TtBindingMethodStoreBase
    {
        public Delegate_BindingMethodGet<T> Getter;
        public Delegate_BindingMethodSet<T> Setter;
    }
    public class TtBindingMethodExpression<TProp> : TtBindingExpressionBase
    {
        public TtBindingMethodStoreBase MethodStore;

        public TtBindingMethodExpression(TtBindingBase binding, TtBindingExpressionBase parent) : base(binding, parent)
        {
        }

        public override T GetValue<T>(TtBindableProperty bp)
        {
            if(MethodStore != null)
                return MethodStore.GetValue<T>(Source, bp);
            return default(T);
        }

        public override void SetValue<T>(TtBindableProperty bp, T value)
        {
            if (MethodStore != null)
                MethodStore.SetValue(Source, bp, value);
        }

        public override void SetValueStore<T>(T value)
        {
        }

        public override void UpdateSource()
        {
        }
    }

    public abstract class TtBindablePropertyValueBase
    {
        public enum EType : byte
        {
            AttachedValue,
            ExpressionValue,
            TemplateSimple,
            TemplateBinding,
            PropertyTrigger,
        }
        public EType Type
        {
            get;
            protected set;
        } = EType.ExpressionValue;
        public abstract void SetValue<T>(IBindableObject obj, TtBindableProperty bp, in T value);
        public abstract T GetValue<T>(TtBindableProperty bp);
    }
    public class TtAttachedValue<TClass, TVal> : TtBindablePropertyValueBase
         where TClass : IBindableObject
    {
        public ValueStoreBase Value;
        public dynamic PropertyHostObject;
        public TtAttachedValue(TClass hostObj)
        {
            PropertyHostObject = hostObj;
            Type = EType.AttachedValue;
        }
        public override void SetValue<T>(IBindableObject obj, TtBindableProperty bp, in T value)
        {
            if (Value == null)
                Value = new ValueStore<T>(value);
            else
            {
                var vs = Value as ValueStore<T>;
                if (vs != null)
                {
                    vs.Value = value;
                    bp.CallOnValueChanged(obj, bp, value);
                }
                else
                    PropertyHostObject.SetAttachedPropertyValue(obj, bp, Value, value);
            }
        }
        public override T GetValue<T>(TtBindableProperty bp)
        {
            var vs = Value as ValueStore<T>;
            if (vs != null)
                return vs.Value;
            return PropertyHostObject.GetAttachedPropertyValue(bp, Value);
            //return default(T);
        }
    }
    public class TtExpressionValues : TtBindablePropertyValueBase
    {
        public List<TtBindingExpressionBase> Expressions = new List<TtBindingExpressionBase>();
        public TtExpressionValues()
        {
            Type = EType.ExpressionValue;
        }

        public override void SetValue<T>(IBindableObject obj, TtBindableProperty bp, in T value)
        {
            for (int i = 0; i < Expressions.Count; i++)
            {
                Expressions[i].SetValue(bp, value);
            }
        }
        public override T GetValue<T>(TtBindableProperty bp)
        {
            if (Expressions.Count > 0)
                return Expressions[0].GetValue<T>(bp);
            return default(T);
        }
    }

    public static class TtBindingOperations
    {
        public delegate T Delegate_BindingMethodGet<T>(IBindableObject obj, TtBindableProperty prop);
        public delegate void Delegate_BindingMethodSet<T>(IBindableObject obj, TtBindableProperty prop, in T value);
        public static TtBindingExpressionBase SetMethodBinding<TProp>(in IBindableObject target, string targetPath, Delegate_BindingMethodGet<TProp> getMethod, Delegate_BindingMethodSet<TProp> setMethod, EBindingMode mode = EBindingMode.Default)
        {
            var binding = new TtBinding()
            {
                Mode = mode,
            };
            return SetMethodBinding<TProp>(target, targetPath, getMethod, setMethod, binding);
        }
        public static TtBindingExpressionBase SetMethodBinding<TProp>(in IBindableObject target, string targetPath, Delegate_BindingMethodGet<TProp> getMethod, Delegate_BindingMethodSet<TProp> setMethod, TtBindingBase binding)
        {
            if (target == null)
                throw new ArgumentNullException("source");
            var sourceProp = target.FindBindableProperty(targetPath);
            if (sourceProp == null)
                return null;
            var finalMode = binding.Mode;
            if(finalMode == EBindingMode.Default)
            {
                finalMode = sourceProp.BindingMode;
            }
            var updateSourceTrigger = binding.UpdateSourceTriger;
            if (updateSourceTrigger == EUpdateSourceTrigger.Default)
                updateSourceTrigger = sourceProp.UpdateSourceTrigger;

            var exp = new TtBindingMethodExpression<TProp>(binding, null);
            exp.Mode = finalMode;
            exp.UpdateSourceTriger = updateSourceTrigger;
            exp.Source = target;
            exp.Path = new TtPropertyPath(targetPath);
            var methodStore = new TtBindingMethodStore<TProp>();
            methodStore.Getter = getMethod;
            methodStore.Setter = setMethod;
            exp.MethodStore = methodStore;
            target.SetBindExpression(sourceProp, exp);
            binding.TargetExp = exp;

            return exp;
        }
        public static TtBindingExpressionBase SetBinding<TTagProp, TSrcProp>(in IBindableObject target, string targetPath, in IBindableObject source, string sourcePath, EBindingMode mode = EBindingMode.Default)
        {
            var binding = new TtBinding()
            {
                Mode = mode,
            };
            return SetBinding<TTagProp, TSrcProp>(target, targetPath, source, sourcePath, binding);
        }
        public static TtBindingExpressionBase SetBinding<TTagProp, TSrcProp>(in IBindableObject target, string targetPath, in IBindableObject source, string sourcePath, TtBindingBase binding)
        {
            if (target == null)
                throw new ArgumentNullException("target");
            if (source == null)
                throw new ArgumentNullException("source");

            var targetProp = target.FindBindableProperty(targetPath);
            if (targetProp == null)
                return null;
            var sourceProp = source.FindBindableProperty(sourcePath);
            if (sourceProp == null)
                return null;
            TtBindingExpressionBase tagExpr = null;
            var finalMode = binding.Mode;
            if(finalMode == EBindingMode.Default)
            {
                finalMode = targetProp.BindingMode;
            }
            var updateSourceTrigger = binding.UpdateSourceTriger;
            if (updateSourceTrigger == EUpdateSourceTrigger.Default)
                updateSourceTrigger = targetProp.UpdateSourceTrigger;

            switch(finalMode)
            {
                case EBindingMode.Default:
                case EBindingMode.OneWay:
                case EBindingMode.OneTime:
                    {
                        tagExpr = target.CreateBindingExpression<TTagProp>(targetProp.Name, binding, null);
                        tagExpr.Mode = finalMode;
                        tagExpr.UpdateSourceTriger = updateSourceTrigger;
                        tagExpr.Source = source;
                        tagExpr.Path = new TtPropertyPath(sourcePath);
                        source.SetBindExpression(sourceProp, tagExpr);
                        binding.TargetExp = tagExpr;
                    }
                    break;
                case EBindingMode.TwoWay:
                    {
                        tagExpr = target.CreateBindingExpression<TTagProp>(targetProp.Name, binding, null);
                        tagExpr.Mode = finalMode;
                        tagExpr.UpdateSourceTriger = updateSourceTrigger;
                        tagExpr.Source = source;
                        tagExpr.Path = new TtPropertyPath(sourcePath);
                        source.SetBindExpression(sourceProp, tagExpr);
                        binding.TargetExp = tagExpr;

                        var sourceExp = source.CreateBindingExpression<TSrcProp>(sourceProp.Name, binding, null);
                        sourceExp.Mode = finalMode;
                        sourceExp.UpdateSourceTriger = updateSourceTrigger;
                        sourceExp.Source = target;
                        sourceExp.Path = new TtPropertyPath(targetPath);
                        target.SetBindExpression(targetProp, sourceExp);
                        binding.SourceExp = sourceExp;
                    }
                    break;
                case EBindingMode.OneWayToSource:
                    { 
                        var sourceExp = source.CreateBindingExpression<TSrcProp>(sourceProp.Name, binding, null);
                        sourceExp.Mode = finalMode;
                        sourceExp.UpdateSourceTriger = updateSourceTrigger;
                        sourceExp.Source = target;
                        sourceExp.Path = new TtPropertyPath(targetPath);
                        target.SetBindExpression(targetProp, sourceExp);
                        binding.SourceExp = sourceExp;
                        tagExpr = sourceExp;
                    }
                    break;
            }

            return tagExpr;
        }
        public static bool HasBinding(IBindableObject target, TtBindableProperty bp)
        {
            return false;
        }
        public static void ClearBinding(IBindableObject target, TtBindableProperty bp)
        {
            target.ClearBindExpression(bp);
        }

        public static void DefaultGetBindProperties(IBindableObject obj, Dictionary<TtBindableProperty, TtBindablePropertyValueBase> bindExprDic, ref EGui.Controls.PropertyGrid.CustomPropertyDescriptorCollection collection)
        {
            // attached properties
            foreach (var bindData in bindExprDic)
            {
                if (bindData.Value.Type == EngineNS.UI.Bind.TtBindablePropertyValueBase.EType.AttachedValue)
                {
                    var proDesc = EngineNS.EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                    proDesc.Name = bindData.Key.Name;
                    if (bindData.Key.DisplayNameAtt != null)
                        proDesc.DisplayName = bindData.Key.DisplayNameAtt.GetDisplayName(obj);
                    proDesc.PropertyType = bindData.Key.PropertyType;
                    proDesc.Category = bindData.Key.Category;
                    proDesc.CustomValueEditor = bindData.Key.CustomValueEditor;
                    collection.Add(proDesc);
                }
            }
        }
        public static object DefaultGetBindPropertyValue(Dictionary<TtBindableProperty, TtBindablePropertyValueBase> bindExprDic, string propertyName)
        {
            foreach (var bindData in bindExprDic)
            {
                if (bindData.Key.Name == propertyName)
                {
                    return bindData.Value.GetValue<object>(bindData.Key);
                }
            }
            return null;
        }
        public static void DefaultSetBindPropertyValue(IBindableObject obj, Dictionary<TtBindableProperty, TtBindablePropertyValueBase> bindExprDic, string propertyName, object value)
        {
            foreach (var bindData in bindExprDic)
            {
                if (bindData.Key.Name == propertyName)
                {
                    bindData.Value.SetValue<object>(obj, bindData.Key, in value);
                    break;
                }
            }
        }
    }
}
