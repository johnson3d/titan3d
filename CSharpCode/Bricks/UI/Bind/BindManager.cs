using EngineNS.Rtti;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static EngineNS.UI.Bind.TtBindableProperty;

namespace EngineNS.UI.Bind
{
    public class TtBindManager : UModule<TtEngine>
    {
        public override async Task<bool> Initialize(TtEngine host)
        {
            foreach(var service in UTypeDescManager.Instance.Services.Values)
            {
                foreach(var typeDesc in service.Types.Values)
                {
                    if(typeDesc.HasInterface(typeof(IBindableObject).Name))
                        typeDesc.RunClassConstructor();
                }
            }

            ///////////////////////////////////////////
            //Bind.BindTestClass.BindTest();
            ///////////////////////////////////////////

            return await base.Initialize(host);
        }

        private static int mBindablePropertyGlobalIndexCount = 0;
        internal static int GetBindablePropertyUniqueGlobalIndex()
        {
            if(mBindablePropertyGlobalIndexCount >= (int)TtBindableProperty.EFlags.GlobalIndexMask)
            {
                throw new InvalidOperationException("Too many bindable properties");
            }
            return Interlocked.Increment(ref mBindablePropertyGlobalIndexCount);
        }
        private static Dictionary<TtBindableProperty.NameKey, TtBindableProperty> mBindableProperties = new Dictionary<TtBindableProperty.NameKey, TtBindableProperty>();
        public TtBindableProperty FindBindableProperty(string name, Rtti.UTypeDesc hostType)
        {
            TtBindableProperty bp = null;
            while ((bp == null) && (hostType != null))
            {
                var key = new TtBindableProperty.NameKey(name, hostType);
                lock (mBindableProperties)
                {
                    mBindableProperties.TryGetValue(key, out bp);
                }
                hostType = hostType.BaseType;
            }
            return bp;
        }
        public TtBindableProperty Register<TProperty, TClass>(
            string name, TProperty defaultValue, 
            Action<IBindableObject, TtBindableProperty, TProperty> valueChangedCallback = null, 
            EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute valueEditor = null,
            BindPropertyDisplayNameAttribute displayNameAtt = null)
            where TClass : class, IBindableObject
        {
            return Register<TProperty, TClass>(name, "", defaultValue, valueChangedCallback, valueEditor, displayNameAtt);
        }
        public TtBindableProperty Register<TProperty, TClass>(
            string name, 
            string category, 
            TProperty defaultValue, 
            Action<IBindableObject, TtBindableProperty, TProperty> valueChangedCallback = null, 
            EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute valueEditor = null,
            BindPropertyDisplayNameAttribute displayNameAtt = null)
            where TClass : class, IBindableObject
        {
            var classType = typeof(TClass);
            var key = new NameKey(name, Rtti.UTypeDesc.TypeOf(classType));
            lock(mBindableProperties)
            {
                TtBindableProperty outPro;
                if (mBindableProperties.TryGetValue(key, out outPro))
                {
                    var pro = outPro as TtBindableProperty<TProperty>;
                    if(pro != null)
                    {
                        pro.DefaultValue = defaultValue;
                        pro.OnValueChanged = valueChangedCallback;
                        pro.CustomValueEditor = valueEditor;
                        pro.DisplayNameAtt = displayNameAtt;
                        return outPro;
                    }
                }
            }
            var bp = new TtBindableProperty<TProperty>()
            {
                Name = name,
                Category = category,
                PropertyType = UTypeDesc.TypeOf(typeof(TProperty)),
                HostType = UTypeDesc.TypeOf(classType),
                DefaultValue = defaultValue,
                OnValueChanged = valueChangedCallback,
                CustomValueEditor = valueEditor,
                DisplayNameAtt = displayNameAtt
            };
            var prop = classType.GetProperty(name);
            if (prop != null)
            {
                if (!prop.CanWrite)
                    bp.IsReadonly = true;
                var atts = prop.GetCustomAttributes(typeof(BindPropertyAttribute), false);
                if (atts.Length > 0)
                {
                    var bpAtt = atts[0] as BindPropertyAttribute;
                    bp.BindingMode = bpAtt.DefaultMode;
                    bp.UpdateSourceTrigger = bpAtt.UpdateSourceTrigger;
                    bp.IsAutoGen = bpAtt.IsAutoGen;
                    bp.IsCallSetProperty = bpAtt.IsCallSetProperty;
                }
            }

            lock (mBindableProperties)
            {
                mBindableProperties[key] = bp;
            }
            return bp;
        }
        public TtBindableProperty RegisterAttached<TProperty, TClass>(
            string name, 
            TProperty defaultValue, 
            Action<IBindableObject, TtBindableProperty, TProperty> valueChangedCallback = null, 
            EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute valueEditor = null,
            BindPropertyDisplayNameAttribute displayNameAtt = null)
            where TClass : class, IBindableObject
        {
            return RegisterAttached<TProperty, TClass>(name, "", defaultValue, valueChangedCallback, valueEditor, displayNameAtt);
        }
        public TtBindableProperty RegisterAttached<TProperty, TClass>(
            string name, 
            string category, 
            TProperty defaultValue, 
            Action<IBindableObject, TtBindableProperty, TProperty> valueChangedCallback = null, 
            EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute valueEditor = null,
            BindPropertyDisplayNameAttribute displayNameAtt = null)
            where TClass : class, IBindableObject
        {
            //var classType = typeof(TClass);
            //var key = new NameKey(name, Rtti.UTypeDesc.TypeOf(classType));
            var pro = Register<TProperty, TClass>(name, category, defaultValue, valueChangedCallback, valueEditor, displayNameAtt);
            pro.IsAttachedProperty = true;
            return pro;
        }
    }
}

namespace EngineNS
{
    public partial class TtEngine
    {
        public UI.Bind.TtBindManager UIBindManager { get; } = new UI.Bind.TtBindManager();
    }
}