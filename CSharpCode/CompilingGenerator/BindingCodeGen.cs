using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CompilingGenerator
{
    [Generator]
    public sealed partial class BindingCodeGenerator : ISourceGenerator
    {
        static readonly string mBindPropAttrName = "EngineNS.UI.Bind.BindPropertyAttribute";
        static readonly string mAttachedPropAttrName = "EngineNS.UI.Bind.AttachedPropertyAttribute";
        static readonly string mBindObjectAttrName = "EngineNS.UI.Bind.BindableObjectAttribute";
        static readonly string mMetaAttrName = "EngineNS.Rtti.Meta";
        sealed class BindingSyntaxReceiver : ISyntaxContextReceiver
        {
            public List<FieldDeclarationSyntax> CandidateFields = new List<FieldDeclarationSyntax>();
            public List<PropertyDeclarationSyntax> CandidateProperties = new List<PropertyDeclarationSyntax>();
            public List<MethodDeclarationSyntax> CandidateMethods = new List<MethodDeclarationSyntax>();
            public List<ClassDeclarationSyntax> CandidateClasses = new List<ClassDeclarationSyntax>();
            public List<PropertyDeclarationSyntax> CandidateIBindableObjectProperties = new List<PropertyDeclarationSyntax>();
            public List<FieldDeclarationSyntax> CandidateIBindableObjectFields = new List<FieldDeclarationSyntax>();
            //static bool IsValidFieldDecSyntax(FieldDeclarationSyntax fieldDecSyntax, GeneratorSyntaxContext context)
            //{
            //    foreach(var attributeListSyntax in fieldDecSyntax.AttributeLists)
            //    {
            //        foreach(var attributeSyntax in attributeListSyntax.Attributes)
            //        {
            //            var symbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;
            //            if(symbol != null && symbol.ContainingType.ToDisplayString() == BindingCodeGenerator.mBindPropAttrName)
            //                return true;
            //        }
            //    }
            //    return false;
            //}
            static bool IsValidPropertyDecSyntax(PropertyDeclarationSyntax propertyDecSyntax, GeneratorSyntaxContext context)
            {
                foreach (var attributeListSyntax in propertyDecSyntax.AttributeLists)
                {
                    foreach (var attributeSyntax in attributeListSyntax.Attributes)
                    {
                        var symbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;
                        if (symbol != null && symbol.ContainingType.ToDisplayString() == BindingCodeGenerator.mBindPropAttrName)
                            return true;
                    }
                }
                return false;
            }
            //static bool IsValidIBindableObjectTypePropertyDecSyntax(PropertyDeclarationSyntax propertyDecSyntax, GeneratorSyntaxContext context)
            //{
            //    var symbol = context.SemanticModel.GetSymbolInfo(propertyDecSyntax).Symbol;
            //    if(symbol != null && symbol.ContainingType.ToDisplayString() == "EngineNS.UI.Bind.IBindableObject")
            //        return true;
            //    return false;
            //}
            static bool IsValidMethodDecSyntax(MethodDeclarationSyntax methodDecSyntax, GeneratorSyntaxContext context)
            {
                foreach (var attributeListSyntax in methodDecSyntax.AttributeLists)
                {
                    foreach (var attributeSyntax in attributeListSyntax.Attributes)
                    {
                        var symbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;
                        if (symbol != null)
                        {
                            if (symbol.ContainingType.ToDisplayString() == BindingCodeGenerator.mAttachedPropAttrName)
                                return true;
                        }
                    }
                }
                return false;
            }
            static bool IsValidClassDecSyntax(ClassDeclarationSyntax clsDecSyntax, GeneratorSyntaxContext context)
            {
                var compilation = context.SemanticModel.Compilation;
                var model = compilation.GetSemanticModel(clsDecSyntax.SyntaxTree);
                var clsSymbol = model.GetDeclaredSymbol(clsDecSyntax) as INamedTypeSymbol;
                if (clsSymbol != null)
                {
                    var baseType = clsSymbol;
                    while (baseType != null)
                    {
                        var typeStr = baseType.ToDisplayString();
                        if (typeStr == "object")
                            break;
                        if (typeStr == "EngineNS.UI.Controls.TtUIElement")
                            return true;
                        baseType = baseType.BaseType;
                    }
                }
                foreach (var attributeListSyntax in clsDecSyntax.AttributeLists)
                {
                    foreach (var attributeSyntax in attributeListSyntax.Attributes)
                    {
                        var symbol = context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol;
                        if (symbol != null && symbol.ContainingType.ToDisplayString() == BindingCodeGenerator.mBindObjectAttrName)
                            return true;
                    }
                }
                return false;
            }
            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                var syntaxNode = context.Node;
                if (syntaxNode is PropertyDeclarationSyntax propertyDecSyntax)
                {
                    if(propertyDecSyntax.AttributeLists.Count > 0)
                    {
                        if (IsValidPropertyDecSyntax(propertyDecSyntax, context))
                            CandidateProperties.Add(propertyDecSyntax);
                    }
                    //if(IsValidIBindableObjectTypePropertyDecSyntax(propertyDecSyntax, context))
                    //    CandidateIBindableObjectProperties.Add(propertyDecSyntax);
                }
                //else if (syntaxNode is FieldDeclarationSyntax fieldDecSyntax && fieldDecSyntax.AttributeLists.Count > 0)
                //{
                //    if(IsValidFieldDecSyntax(fieldDecSyntax, context))
                //        CandidateFields.Add(fieldDecSyntax);
                //}
                else if (syntaxNode is MethodDeclarationSyntax methodDecSyntax && methodDecSyntax.AttributeLists.Count > 0)
                {
                    if (IsValidMethodDecSyntax(methodDecSyntax, context))
                        CandidateMethods.Add(methodDecSyntax);
                }
                else if (syntaxNode is ClassDeclarationSyntax classDecSyntax)// && classDecSyntax.AttributeLists.Count > 0)
                {
                    if (IsValidClassDecSyntax(classDecSyntax, context))
                        CandidateClasses.Add(classDecSyntax);
                }
            }
        }

        string ProcessClass(INamedTypeSymbol? classSymbol, List<ISymbol> symbols, List<ISymbol> bindObjectMemberSymbols, ISymbol? bindPropertySymbol, ISymbol? attachedPropertySymbol, ISymbol? bindObjectSymbol, GeneratorExecutionContext context)
        {
            if (classSymbol == null || bindPropertySymbol == null || attachedPropertySymbol == null)
                return "";
            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            var className = classSymbol.Name;
            bool baseHasBindObjectInterface = false;
            var baseType = classSymbol.BaseType;
            bool baseFromUIElement = false;
            while ((baseType != null))
            {
                var baseTypeDisplayString = baseType.ToDisplayString();
                if (baseTypeDisplayString == "EngineNS.UI.Controls.TtUIElement")
                    baseFromUIElement = true;
                if (baseTypeDisplayString == "object")
                    break;
                if (baseTypeDisplayString == "EngineNS.UI.Bind.TtBindableObject")
                {
                    baseHasBindObjectInterface = true;
                    break;
                }
                foreach (var ifac in baseType.AllInterfaces)
                {
                    if (ifac.ToDisplayString() == "EngineNS.UI.Bind.IBindableObject")
                    {
                        baseHasBindObjectInterface = true;
                        break;
                    }
                }
                var baseTypeAttributes = baseType.GetAttributes();
                if (baseTypeAttributes.Length > 0)
                {
                    var attData = baseTypeAttributes.SingleOrDefault(ad => (ad.AttributeClass == null) ? false : ad.AttributeClass.Equals(bindObjectSymbol, SymbolEqualityComparer.Default));
                    if (attData != null)
                        baseHasBindObjectInterface = true;
                }
                if (baseHasBindObjectInterface)
                    break;
                baseType = baseType.BaseType;
            }
            string bindImpSource = "";
            string setValueWithPropertyName = $@"
#nullable enable
        public{(baseHasBindObjectInterface ? " override" : " virtual")} void SetValue<T>(in T value, [CallerMemberName] string? propertyName = null)
#nullable disable
        {{";
            bool hasSetValueWithPropertyNameSwitch = false;
            string setValueWithPropertyNameSwitch = $@"
            if(string.IsNullOrEmpty(propertyName))
                return;
            var propertyNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(propertyName);
            switch(propertyNameHash)
            {{";

            string getValueWithPropertyName = $@"
#nullable enable
        public{(baseHasBindObjectInterface ? " override" : " virtual")} T GetValue<T>([CallerMemberName] string? propertyName = null)
#nullable disable
        {{";
            bool hasGetValueWithPropertyNameSwitch = false;
            string getValueWithPropertyNameSwitch = $@"
            if(string.IsNullOrEmpty(propertyName))
                return default(T);
            var propertyNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(propertyName);
            switch(propertyNameHash)
            {{";

            string createBindingExpressionMethod = $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} EngineNS.UI.Bind.TtBindingExpressionBase CreateBindingExpression<TProperty>(string propertyName, EngineNS.UI.Bind.TtBindingBase binding, EngineNS.UI.Bind.TtBindingExpressionBase parent)
        {{";
            bool hasCreateBindingExpressionMethodSwitch = false;
            string createBindingExpressionMethodSwitch = $@"
            if(string.IsNullOrEmpty(propertyName))
                return null;
            var propertyNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(propertyName);
            switch(propertyNameHash)
            {{";

            string getPropertyValueSwitch = "";
            string setPropertyValueSwitch = "";
            string tourBindableProperties = "";
            string hasBindablePropertiesStr = "";

            //bool hasCreateBindingMethodExpressionMethodSwitch = false;
            //string createBindingMethodExpressionMethodSwitch = $@"
            //if(string.IsNullOrEmpty(propertyName))
            //    return null;
            //var propertyNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(propertyName);
            //switch(propertyNameHash)
            //{{";

            var source = $@"
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace {namespaceName}
{{";
            //        var classAttrs = classSymbol.GetAttributes();
            //        if (classAttrs.Length > 0)
            //        {
            //            var metaSymbol = context.Compilation.GetTypeByMetadataName(mMetaAttrName);
            //            var attData = classAttrs.SingleOrDefault(ad => (ad.AttributeClass == null) ? false : ad.AttributeClass.Equals(metaSymbol, SymbolEqualityComparer.Default));
            //            if (attData == null)
            //            {
            //                source += $@"
            //[{mMetaAttrName}]";
            //            }
            //        }
            bool HasIBindableObject = false;
            bool HasIPropertyCustomization = false;
            bool HasISerializer = false;
            foreach (var iface in classSymbol.AllInterfaces)
            {
                var ifaceStr = iface.ToDisplayString();
                switch(ifaceStr)
                {
                    case "EngineNS.UI.Bind.IBindableObject":
                        HasIBindableObject = true;
                        break;
                    case "EngineNS.EGui.Controls.PropertyGrid.IPropertyCustomization":
                        HasIPropertyCustomization = true;
                        break;
                    case "EngineNS.IO.ISerializer":
                        HasISerializer = true;
                        break;
                }
            }
            string attributes = "";
            if (!HasIBindableObject)
                attributes += "EngineNS.UI.Bind.IBindableObject,";
            if (!HasIPropertyCustomization)
                attributes += "EngineNS.EGui.Controls.PropertyGrid.IPropertyCustomization,";
            if (!HasISerializer)
                attributes += "EngineNS.IO.ISerializer,";
            if(!string.IsNullOrEmpty(attributes))
            {
                attributes = " : " + attributes.TrimEnd(',');
            }

            source += $@"
    public partial class {className}{attributes}
    {{";
            var bindExprDicName = $"mBindExprDic";
            var triggerDicName = "mPropertyTriggers_";
            var setAttachedPropertiesStr = "";
            var getAttachedPropertyValueStr = "";
            var setAttachedPropertyValueStr = "";
            if (!baseHasBindObjectInterface)
            {
                source += @$"
        protected Dictionary<EngineNS.UI.Bind.TtBindableProperty, EngineNS.UI.Bind.TtBindablePropertyValueBase> {bindExprDicName} = new Dictionary<EngineNS.UI.Bind.TtBindableProperty, EngineNS.UI.Bind.TtBindablePropertyValueBase>();
        protected EngineNS.UI.Trigger.TtTriggerCollection {triggerDicName} = new EngineNS.UI.Trigger.TtTriggerCollection();
        [System.ComponentModel.Browsable(false)]        
        public EngineNS.UI.Trigger.TtTriggerCollection Triggers => {triggerDicName};";

            }
            //Dictionary<ITypeSymbol, List<ISymbol>> symbolTypeDic = new Dictionary<ITypeSymbol, List<ISymbol>>();
            foreach (var symbol in symbols)
            {
                if (symbol is IFieldSymbol)
                {

                }
                else if (symbol is IPropertySymbol)
                {
                    var propSymbol = symbol as IPropertySymbol;
                    if (propSymbol != null)
                    {
                        var propName = propSymbol.Name;
                        var propTypeDisplayName = propSymbol.Type.ToDisplayString();
                        //if(!symbolTypeDic.TryGetValue(propSymbol.Type,out var symbolList))
                        //{
                        //    symbolList = new List<ISymbol>();
                        //    symbolTypeDic[propSymbol.Type] = symbolList;
                        //}
                        //symbolList.Add(propSymbol);

                        var attData = propSymbol.GetAttributes().Single(ad => (ad.AttributeClass == null) ? false : ad.AttributeClass.Equals(bindPropertySymbol, SymbolEqualityComparer.Default));
                        var defaultValOpt = attData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "DefaultValue").Value;
                        var defValStr = $"default({propTypeDisplayName})";
                        if (!defaultValOpt.IsNull)
                        {
                            defValStr = (defaultValOpt.Value == null) ? "null" : ((defaultValOpt.Value is bool) ? defaultValOpt.Value.ToString().ToLower() : defaultValOpt.Value.ToString());
                        }
                        var valEditorAttOpt = propSymbol.GetAttributes().SingleOrDefault(ad =>
                        {
                            var cls = ad.AttributeClass;
                            while (cls != null)
                            {
                                if (cls.ToDisplayString() == "EngineNS.EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute")
                                    return true;
                                cls = cls.BaseType;
                            }
                            return false;
                        });
                        string valEditorStr = "null";
                        if (valEditorAttOpt != null && valEditorAttOpt.AttributeClass != null)
                        {
                            var editorName = valEditorAttOpt.AttributeClass.ToDisplayString();
                            valEditorStr = @$"typeof({namespaceName}.{className}).GetProperty(""{propName}"").GetCustomAttributes(typeof({editorName}), false)[0] as {editorName}";
                        }
                        var displayNameAttOpt = propSymbol.GetAttributes().SingleOrDefault(ad =>
                        {
                            var cls = ad.AttributeClass;
                            while (cls != null)
                            {
                                if (cls.ToDisplayString() == "EngineNS.UI.Bind.BindPropertyDisplayNameAttribute")
                                    return true;
                                cls = cls.BaseType;
                            }
                            return false;
                        });
                        string displayNameStr = "null";
                        if (displayNameAttOpt != null && displayNameAttOpt.AttributeClass != null)
                        {
                            var attName = displayNameAttOpt.AttributeClass.ToDisplayString();
                            displayNameStr = @$"typeof({namespaceName}.{className}).GetProperty(""{propName}"").GetCustomAttributes(typeof({attName}), false)[0] as {attName}";
                        }
                        var valCategoryAttData = propSymbol.GetAttributes().SingleOrDefault(ad => (ad.AttributeClass == null) ? false : (ad.AttributeClass.ToDisplayString() == "System.ComponentModel.CategoryAttribute"));
                        string categoryValue = "";
                        if (valCategoryAttData != null && valCategoryAttData.ConstructorArguments.Length > 0)
                            categoryValue = System.Convert.ToString(valCategoryAttData.ConstructorArguments[0].Value);
                        var bindPropName = $"{propName}Property";
                        if (!classSymbol.MemberNames.Any(name => bindPropName == name))
                        {
                            source += @$"
        public static EngineNS.UI.Bind.TtBindableProperty {bindPropName} = EngineNS.UEngine.Instance.UIBindManager.Register<{propTypeDisplayName}, {className}>(""{propName}"",{(string.IsNullOrEmpty(categoryValue) ? "" : @$" ""{categoryValue}"",")} {defValStr}, null, {valEditorStr}, {displayNameStr});";
                        }

                        var bindingImpName = $"{className}_BindingImp_{propName}";
                        var bindingExprImpName = $"{className}_BindingExprImp_{propName}";
                        //var bindingMethodExprImpName = $"{className}_BindingMethodExprImp_{propName}";

                        hasCreateBindingExpressionMethodSwitch = true;
                        createBindingExpressionMethodSwitch += $@"
                case {Standart.Hash.xxHash.xxHash64.ComputeHash(propName)}: //{propName}
                    {{
                        var beImp = new {bindingExprImpName}(binding, {propName}, parent);
                        beImp.TargetObject = this;
                        beImp.TargetProperty = (EngineNS.UI.Bind.TtBindableProperty<{propTypeDisplayName}>){bindPropName};
                        return beImp;
                    }}";
                //        hasCreateBindingMethodExpressionMethodSwitch = true;
                //        createBindingMethodExpressionMethodSwitch += $@"
                //case {Standart.Hash.xxHash.xxHash64.ComputeHash(propName)}: //{propName}
                //    {{
                //        var beImp = new {bindingMethodExprImpName}(binding, parent);
                //        return beImp;
                //    }}";
                        hasSetValueWithPropertyNameSwitch = true;
                        setValueWithPropertyNameSwitch += $@"
                case {Standart.Hash.xxHash.xxHash64.ComputeHash(propName)}: //{propName}
                    SetValue<T>(value, {bindPropName});
                    return;";
                        hasGetValueWithPropertyNameSwitch = true;
                        getValueWithPropertyNameSwitch += $@"
                case {Standart.Hash.xxHash.xxHash64.ComputeHash(propName)}: //{propName}
                    return GetValue<T>({bindPropName});";

                        getPropertyValueSwitch += $@"
                case {Standart.Hash.xxHash.xxHash64.ComputeHash(propName)}: //{propName}
                    value = {propName};//GetValue<{propTypeDisplayName}>({bindPropName});
                    return true;";

                        if(!propSymbol.IsReadOnly)
                        {
                            setPropertyValueSwitch += $@"
                case {Standart.Hash.xxHash.xxHash64.ComputeHash(propName)}: //{propName}
                    //SetValue<{propTypeDisplayName}>(({propTypeDisplayName})value, {bindPropName});
                    {propName} = ({propTypeDisplayName})value;
                    return true;";
                        }
                        else
                        {
                            setPropertyValueSwitch += $@"
                case {Standart.Hash.xxHash.xxHash64.ComputeHash(propName)}: //{propName}
                        return true;";
                        }

                        tourBindableProperties += $@"
            tourAction.Invoke(""{propName}"", {bindPropName}, ref data);";

                        hasBindablePropertiesStr += $@"
            if(matchCase)
            {{
                if(""{propName}"".Contains(containString))
                    return true;
            }}
            else
            {{
                if(""{propName}"".ToLower().Contains(containString))
                    return true;
            }}";

                        bindImpSource += $@"
    public class {bindingExprImpName} : EngineNS.UI.Bind.TtBindingExpression<{propTypeDisplayName}>
    {{
        public {bindingExprImpName}(EngineNS.UI.Bind.TtBindingBase binding, EngineNS.UI.Bind.TtBindingExpressionBase parent)
            : base(binding, parent)
        {{
        }}
        public {bindingExprImpName}(EngineNS.UI.Bind.TtBindingBase binding, {propTypeDisplayName} val, EngineNS.UI.Bind.TtBindingExpressionBase parent)
            : base(binding, parent)
        {{
            GetValueStore<{propTypeDisplayName}>().SetValue(val);
        }}
        public override void UpdateSource()
        {{
            if ((Mode == EngineNS.UI.Bind.EBindingMode.OneTime) && (mSetValueTime > 0))
                return;
            mSetValueTime++;
            GetFinalValue<{propTypeDisplayName}>().CopyFrom(GetValueStore<{propTypeDisplayName}>());
            if(mParentExp != null)
            {{
                mParentExp.UpdateSource();
            }}";
                        if(propSymbol.IsReadOnly)
                        {
                            bindImpSource += $@"
        }}";
                        }
                        else
                        {
                            bindImpSource += $@"
            else
            {{
                TargetProperty?.OnValueChanged?.Invoke(TargetObject, TargetProperty, GetFinalValue<{propTypeDisplayName}>().GetValue<{propTypeDisplayName}>());
                (({namespaceName}.{className})TargetObject).{propName} = GetValueStore<{propTypeDisplayName}>().GetValue<{propTypeDisplayName}>();
            }}
        }}";
                        }
                        bindImpSource += $@"
        protected override object GetObjectValue(EngineNS.UI.Bind.TtBindableProperty bp)
        {{
            return GetFinalValue<{propTypeDisplayName}>().GetValue<{propTypeDisplayName}>();
        }}
    }}";

    //                    bindImpSource += $@"
    //public class {bindingMethodExprImpName} : EngineNS.UI.Bind.TtBindingMethodExpression<{propTypeDisplayName}>
    //{{
    //    public {bindingMethodExprImpName}(EngineNS.UI.Bind.TtBindingBase binding, EngineNS.UI.Bind.TtBindingExpressionBase parent)
    //        : base(binding, parent)
    //    {{
    //    }}
    //}}";

                    }
                }
                else if (symbol is IMethodSymbol)
                {
                    var methodSymbol = symbol as IMethodSymbol;
                    if (methodSymbol != null)
                    {
                        if (!methodSymbol.IsStatic)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "BindingGeneric",
                                    "Method is not static",
                                    "Attached property method {0} must be static",
                                    "Error",
                                    DiagnosticSeverity.Error,
                                    true), symbol.Locations.FirstOrDefault(),
                                    symbol.Name));

                        }
                        if (methodSymbol.Parameters.Length != 3)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "BindingGeneric",
                                    "Method need three parameters",
                                    $"Method parameters must be {className}, EngineNS.UI.Bind.TtBindableProperty, [property type]",
                                    "Error",
                                    DiagnosticSeverity.Error,
                                    true), symbol.Locations.FirstOrDefault(),
                                    symbol.Name));
                        }
                        if (methodSymbol.Parameters[0].Type.ToDisplayString() != $"EngineNS.UI.Bind.IBindableObject")
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "BindingGeneric",
                                    "Method first parameter type error",
                                    $"Method first parameter type must be EngineNS.UI.Bind.IBindableObject",
                                    "Error",
                                    DiagnosticSeverity.Error,
                                    true), symbol.Locations.FirstOrDefault(),
                                    symbol.Name));
                        }
                        if (methodSymbol.Parameters[1].Type.ToDisplayString() != "EngineNS.UI.Bind.TtBindableProperty")
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "BindingGeneric",
                                    "Method second parameter type error",
                                    $"Method second parameter type must be EngineNS.UI.Bind.TtBindableProperty",
                                    "Error",
                                    DiagnosticSeverity.Error,
                                    true), symbol.Locations.FirstOrDefault(),
                                    symbol.Name));
                        }
                        var propTypeDisplayName = methodSymbol.Parameters[2].Type.ToDisplayString();
                        var methodName = methodSymbol.Name;
                        var attData = methodSymbol.GetAttributes().Single(ad => (ad.AttributeClass == null) ? false : ad.AttributeClass.Equals(attachedPropertySymbol, SymbolEqualityComparer.Default));
                        var nameOpt = attData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "Name").Value;
                        string nameVal = "";
                        if (nameOpt.Value == null)
                        {
                            context.ReportDiagnostic(Diagnostic.Create(
                                new DiagnosticDescriptor(
                                    "BindingGeneric",
                                    "Attached property name not set",
                                    "Must set AttachedPropertyAttribute Name field, like Name = \"your property name\"",
                                    "Error",
                                    DiagnosticSeverity.Error,
                                    true), symbol.Locations.FirstOrDefault(),
                                    symbol.Name));
                        }
                        else
                            nameVal = nameOpt.Value.ToString();
                        var bindPropName = $"{nameVal}Property";
                        var defValOpt = attData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "DefaultValue").Value;
                        var defValStr = $"default({propTypeDisplayName})";
                        if (!defValOpt.IsNull)
                        {
                            defValStr = (defValOpt.Value == null) ? "null" : defValOpt.Value.ToString();
                        }
                        var valCategoryOpt = attData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "Category").Value;
                        string categoryVal = "";
                        if (valCategoryOpt.Value != null)
                            categoryVal = valCategoryOpt.Value.ToString();
                        setAttachedPropertiesStr += $@"
            target.AddAttachedProperty<{propTypeDisplayName}>({bindPropName}, this, {defValStr});";
                        getAttachedPropertyValueStr += $@"
                case {Standart.Hash.xxHash.xxHash64.ComputeHash(nameVal)}: //{nameVal}
                    return value.GetValue<{propTypeDisplayName}>();";
                        setAttachedPropertyValueStr += $@"
                case {Standart.Hash.xxHash.xxHash64.ComputeHash(nameVal)}: //{nameVal}
                    {{
                        var tagVal = ({propTypeDisplayName})value;
                        valueStore.SetValue<{propTypeDisplayName}>(tagVal);
                        bp.CallOnValueChanged(obj, bp, tagVal);
                    }}
                    break;";
                        var valEditorAttOpt = methodSymbol.GetAttributes().SingleOrDefault(ad =>
                        {
                            var cls = ad.AttributeClass;
                            while (cls != null)
                            {
                                if (cls.ToDisplayString() == "EngineNS.EGui.Controls.PropertyGrid.PGCustomValueEditorAttribute")
                                    return true;
                                cls = cls.BaseType;
                            }
                            return false;
                        });
                        string valEditorStr = "null";
                        if (valEditorAttOpt != null && valEditorAttOpt.AttributeClass != null)
                        {
                            var editorName = valEditorAttOpt.AttributeClass.ToDisplayString();
                            valEditorStr = @$"typeof({namespaceName}.{className}).GetMethod(""{methodName}"", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, new Type[] {{ typeof({methodSymbol.Parameters[0].Type.ToDisplayString()}), typeof({methodSymbol.Parameters[1].Type.ToDisplayString()}), typeof({methodSymbol.Parameters[2].Type.ToDisplayString()}) }}, null).GetCustomAttributes(typeof({editorName}), false)[0] as {editorName}";
                        }
                        var displayNameAttOpt = methodSymbol.GetAttributes().SingleOrDefault(ad =>
                        {
                            var cls = ad.AttributeClass;
                            while (cls != null)
                            {
                                if (cls.ToDisplayString() == "EngineNS.UI.Bind.BindPropertyDisplayNameAttribute")
                                    return true;
                                cls = cls.BaseType;
                            }
                            return false;
                        });
                        string displayNameStr = "null";
                        if (displayNameAttOpt != null && displayNameAttOpt.AttributeClass != null)
                        {
                            var attName = displayNameAttOpt.AttributeClass.ToDisplayString();
                            displayNameStr = @$"typeof({namespaceName}.{className}).GetMethod(""{methodName}"", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic, null, new Type[] {{ typeof({methodSymbol.Parameters[0].Type.ToDisplayString()}), typeof({methodSymbol.Parameters[1].Type.ToDisplayString()}), typeof({methodSymbol.Parameters[2].Type.ToDisplayString()}) }}, null).GetCustomAttributes(typeof({attName}), false)[0] as {attName}";
                        }
                        if (!classSymbol.MemberNames.Any(name => bindPropName == name))
                        {
                            source += $@"
        public static EngineNS.UI.Bind.TtBindableProperty {bindPropName} = EngineNS.UEngine.Instance.UIBindManager.RegisterAttached<{propTypeDisplayName}, {className}>(""{nameVal}"",{(string.IsNullOrEmpty(categoryVal) ? "" : @$" ""{categoryVal}"",")} {defValStr}, {methodName}, {valEditorStr}, {displayNameStr});";
                        }

                        if (!classSymbol.MemberNames.Any(name => $"Get{nameVal}" == name))
                        {
                            source += $@"
        public static {propTypeDisplayName} Get{nameVal}(EngineNS.UI.Bind.IBindableObject target)
        {{
            if(target == null)
                return {defValStr};
            return target.GetValue<{propTypeDisplayName}>({bindPropName});
        }}";
                        }
                        if (!classSymbol.MemberNames.Any(name => $"Set{nameVal}" == name))
                        {
                            source += $@"
        public static void Set{nameVal}(EngineNS.UI.Bind.IBindableObject target, {propTypeDisplayName} value)
        {{
            target?.SetValue<{propTypeDisplayName}>(value, {bindPropName});
        }}";
                        }
                    }
                }
            }

            //            createBindingMethod += $@"
            //            }}
            //            return null;
            //        }}
            //";
            //            source += createBindingMethod;

            createBindingExpressionMethodSwitch += $@"
            }}";
            if (hasCreateBindingExpressionMethodSwitch)
                createBindingExpressionMethod += createBindingExpressionMethodSwitch;
            if (baseHasBindObjectInterface)
            {
                createBindingExpressionMethod += $@"
            return base.CreateBindingExpression<TProperty>(propertyName, binding, parent);
        }}
";
            }
            else
            {
                createBindingExpressionMethod += $@"
            return null;
        }}
";
            }
            source += createBindingExpressionMethod;

        //    source += $@"
        //public{(baseHasBindObjectInterface ? " override" : " virtual")} EngineNS.UI.Bind.TtBindingExpressionBase CreateBindingMethodExpression(string propertyName, EngineNS.UI.Bind.TtBindingBase binding, EngineNS.UI.Bind.TtBindingExpressionBase parent)
        //{{";
        //    createBindingMethodExpressionMethodSwitch += $@"
        //    }}";
        //    if (hasCreateBindingMethodExpressionMethodSwitch)
        //        source += createBindingMethodExpressionMethodSwitch;
        //    if (baseHasBindObjectInterface)
        //        source += $@"
        //    return base.CreateBindingMethodExpression(propertyName, binding, parent);";
        //    else
        //        source += $@"
        //    return null;";
        //    source += $@"
        //}}";

            setValueWithPropertyNameSwitch += $@"
            }}";
            if (hasSetValueWithPropertyNameSwitch)
                setValueWithPropertyName += setValueWithPropertyNameSwitch;
            if (baseHasBindObjectInterface)
            {
                setValueWithPropertyName += $@"
            base.SetValue<T>(value, propertyName);
        }}
";
            }
            else
            {
                setValueWithPropertyName += $@"
        }}
";
            }
            source += setValueWithPropertyName;
            source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} void SetValue<T>(in T value, EngineNS.UI.Bind.TtBindableProperty bp)
        {{
            EngineNS.UI.Bind.TtBindablePropertyValueBase bpVal = null;
            lock ({bindExprDicName})
            {{
                if (!{bindExprDicName}.TryGetValue(bp, out bpVal))
                {{
                    if (bp.IsAttachedProperty)
                    {{
                        //bpVal = new EngineNS.UI.Bind.TtAttachedValue<EngineNS.UI.Bind.IBindableObject, T>(bpHost);
                        //{bindExprDicName}[bp] = bpVal;
                    }}
                    else
                    {{
                        var expVals = new EngineNS.UI.Bind.TtExpressionValues();
                        expVals.Expressions.Add(new EngineNS.UI.Bind.TtDefaultValueExpression(new EngineNS.UI.Bind.ValueStore<T>(((EngineNS.UI.Bind.TtBindableProperty<T>)bp).DefaultValue)));
                        bpVal = expVals;
                        {bindExprDicName}[bp] = bpVal;
                    }}
                }}
            }}
            if (bpVal == null)
                return;
            if({triggerDicName}.HasTrigger(bp))
            {{
                var oldVal = bpVal.GetValue<T>(bp);
                bpVal.SetValue<T>(this, bp, in value);
                {triggerDicName}.InvokeTriggers(this, bp, oldVal, value);
            }}
            else
                bpVal.SetValue<T>(this, bp, in value);
        }}
";
            getValueWithPropertyNameSwitch += $@"
            }}";
            if (hasGetValueWithPropertyNameSwitch)
                getValueWithPropertyName += getValueWithPropertyNameSwitch;
            if (baseHasBindObjectInterface)
            {
                getValueWithPropertyName += $@"
            return base.GetValue<T>(propertyName);
        }}
";
            }
            else
            {
                getValueWithPropertyName += $@"
            return default(T);
        }}
";
            }
            source += getValueWithPropertyName;
            source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} T GetValue<T>(EngineNS.UI.Bind.TtBindableProperty bp)
        {{
            if (bp == null)
                return default(T);
            EngineNS.UI.Bind.TtBindablePropertyValueBase bpVal = null;
            lock ({bindExprDicName})
            {{
                if (!{bindExprDicName}.TryGetValue(bp, out bpVal))
                    return default(T);
            }}
            return bpVal.GetValue<T>(bp);
        }}
";
            if (baseFromUIElement && !classSymbol.Constructors.Any(symbol => ((symbol.Parameters.Length == 0) && (!symbol.IsImplicitlyDeclared))))
            {
                source += $@"
        public {className}()
            : base()
        {{
        }}";
            }
            if (baseFromUIElement && !classSymbol.Constructors.Any((symbol) =>
            {
                if ((symbol.Parameters.Length == 1) && (symbol.Parameters[0].Type.ToDisplayString() == "EngineNS.UI.Controls.Containers.TtContainer"))
                    return true;
                return false;
            }))
            {
                source += $@"
        public {className}(EngineNS.UI.Controls.Containers.TtContainer parent)
            : base(parent)
        {{
        }}";
            }

            if (!classSymbol.MemberNames.Any(name => "SetBindExpression" == name))
            {
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} void SetBindExpression(EngineNS.UI.Bind.TtBindableProperty bp, EngineNS.UI.Bind.TtBindingExpressionBase expr)
        {{
            EngineNS.UI.Bind.TtBindablePropertyValueBase bpVal = null;
            lock({bindExprDicName})
            {{
                if(!{bindExprDicName}.TryGetValue(bp, out bpVal))
                {{
                    bpVal = new EngineNS.UI.Bind.TtExpressionValues();
                    {bindExprDicName}[bp] = bpVal;
                }}
            }}
            ((EngineNS.UI.Bind.TtExpressionValues)bpVal).Expressions.Add(expr);
        }}";
            }

            if (!classSymbol.MemberNames.Any(name => "FindBindableProperty" == name) && !baseHasBindObjectInterface)
            {
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} EngineNS.UI.Bind.TtBindableProperty FindBindableProperty(string propertyName)
        {{
            var nameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(propertyName);
            return GetBindableProperty(nameHash, propertyName);
        }}";
            }

            {
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} EngineNS.UI.Bind.TtBindableProperty GetBindableProperty(UInt64 propertyNameHash, string propertyName)
        {{";
                var switchCode = $@"
            switch(propertyNameHash)
            {{";
                bool hasSwitchCode = false;
                foreach (var symbol in symbols)
                {
                    if (symbol is IFieldSymbol)
                    {

                    }
                    else if (symbol is IPropertySymbol)
                    {
                        hasSwitchCode = true;
                        switchCode += $@"
                case {Standart.Hash.xxHash.xxHash64.ComputeHash(symbol.Name)}: // {symbol.Name}
                    return {symbol.Name}Property;";
                    }
                }
                switchCode += $@"
            }}";
                if (hasSwitchCode)
                    source += switchCode;
                if (baseHasBindObjectInterface)
                {
                    source += $@"
            return base.GetBindableProperty(propertyNameHash, propertyName);
        }}";
                }
                else
                {
                    source += $@"
            lock({bindExprDicName})
            {{
                foreach(var key in {bindExprDicName}.Keys)
                {{
                    if (key.Name == propertyName)
                        return key;
                }}
            }}
            return null;
        }}";
                }
            }

            if (!classSymbol.MemberNames.Any(name => "OnValueChange" == name))
            {
                source += $@"
#nullable enable
        public{(baseHasBindObjectInterface ? " override" : " virtual")} void OnValueChange<T>(in T value, in T oldValue, [CallerMemberName] string? propertyName = null)
#nullable disable
        {{
            if(string.IsNullOrEmpty(propertyName))
                return;
            var propertyNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(propertyName);
            var bp = GetBindableProperty(propertyNameHash, propertyName);
            if (bp == null)
                return;
            EngineNS.UI.Bind.TtBindablePropertyValueBase bpVal = null;
            lock ({bindExprDicName})
            {{
                {bindExprDicName}.TryGetValue(bp, out bpVal);
            }}
            if (bpVal == null)
            {{
                {triggerDicName}.InvokeTriggers(this, bp, oldValue, value);
            }}
            else
            {{
                if({triggerDicName}.HasTrigger(bp))
                {{
                    var oldVal = bpVal.GetValue<T>(bp);
                    bpVal.SetValue<T>(this, bp, in value);
                    {triggerDicName}.InvokeTriggers(this, bp, oldVal, value);
                }}
                else
                    bpVal.SetValue<T>(this, bp, value);
            }}
        }}";
            }
            if(!classSymbol.MemberNames.Any(name => "HasBinded" == name))
            {
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} bool HasBinded(EngineNS.UI.Bind.TtBindableProperty bp)
        {{
            lock({bindExprDicName})
            {{
                return {bindExprDicName}.ContainsKey(bp);
            }}
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "ClearBindExpression" == name))
            {
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} void ClearBindExpression(EngineNS.UI.Bind.TtBindableProperty bp)
        {{
            lock({bindExprDicName})
            {{
                {bindExprDicName}.Remove(bp);
            }}

            if(bp == null)
            {{
                foreach(var data in BindingDatas)
                {{
                    var bdMethod = data.Value as EngineNS.UI.Controls.TtUIElement.BindingData_Method;
                    if (bdMethod == null)
                        continue;

                    EngineNS.UI.Controls.TtUIElement.MacrossMethodData md;
                    if(MacrossMethods.TryGetValue(data.Key, out md))
                    {{
                        var pbData = md as EngineNS.UI.Controls.TtUIElement.MacrossPropertyBindMethodData;
                        if(pbData != null)
                        {{
                            if (pbData.GetDesc != null)
                                MethodDisplayNames.Remove(pbData.GetDesc);
                            if (pbData.SetDesc != null)
                                MethodDisplayNames.Remove(pbData.SetDesc);
                        }}
                    }}
                }}

                BindingDatas.Clear();
            }}
            else
            {{

                BindingDatas.Remove(bp.Name);

                EngineNS.UI.Controls.TtUIElement.MacrossMethodData data;
                if(MacrossMethods.TryGetValue(bp.Name, out data))
                {{
                    var pbData = data as EngineNS.UI.Controls.TtUIElement.MacrossPropertyBindMethodData;
                    if(pbData != null)
                    {{
                        if(pbData.GetDesc != null)
                            MethodDisplayNames.Remove(pbData.GetDesc);
                        if(pbData.SetDesc != null)
                            MethodDisplayNames.Remove(pbData.SetDesc);
                    }}
                    MacrossMethods.Remove(bp.Name);
                }}
            }}
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "RemoveAttachedProperties" == name))
            {
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} void RemoveAttachedProperties(System.Type propertiesHostType)
        {{
            var removePros = new System.Collections.Generic.HashSet<EngineNS.UI.Bind.TtBindableProperty>();
            foreach (var data in {bindExprDicName})
            {{
                if(data.Key.HostType.IsEqual(propertiesHostType))
                {{
                    removePros.Add(data.Key);
                }}
            }}
            foreach(var key in removePros)
            {{
                RemoveAttachedProperty(key);
            }}
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "RemoveAttachedProperty" == name))
            {
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} void RemoveAttachedProperty(EngineNS.UI.Bind.TtBindableProperty property)
        {{
            {bindExprDicName}.Remove(property);
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "SetAttachedProperties" == name))
            {
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} void SetAttachedProperties(EngineNS.UI.Bind.IBindableObject target)
        {{
            {(baseHasBindObjectInterface ? "base.SetAttachedProperties(target);" : "")}";
                source += setAttachedPropertiesStr;
                source += $@"
        }}";
            }
            if(!classSymbol.MemberNames.Any(name => "AddAttachedProperty" == name))
            {
                source += $@"
        public{(baseHasBindObjectInterface? " override" : " virtual")} void AddAttachedProperty<T>(EngineNS.UI.Bind.TtBindableProperty property, EngineNS.UI.Bind.IBindableObject bpHost, in T defaultValue)
        {{
            var bpVal = new EngineNS.UI.Bind.TtAttachedValue<EngineNS.UI.Bind.IBindableObject, T>(bpHost);
            mBindExprDic[property] = bpVal;
            if(mPropertyTriggers_.HasTrigger(property))
            {{
                var oldVal = bpVal.GetValue<T>(property);
                bpVal.SetValue<T>(this, property, in defaultValue);
                mPropertyTriggers_.InvokeTriggers(this, property, oldVal, defaultValue);
            }}
            else
                bpVal.SetValue<T>(this, property, in defaultValue);
        }}";
            }

            if(!classSymbol.MemberNames.Any(name => "GetAttachedPropertyValue" == name))
            {
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} object GetAttachedPropertyValue(EngineNS.UI.Bind.TtBindableProperty bp, EngineNS.UI.Bind.ValueStoreBase value)
        {{";
                if(!string.IsNullOrEmpty(getAttachedPropertyValueStr))
                {
                    source += $@"    
            var propertyNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(bp.Name);
            switch(propertyNameHash)
            {{
            {getAttachedPropertyValueStr}
            }}";
                }
                source += $@"
            {(baseHasBindObjectInterface?"return base.GetAttachedPropertyValue(bp, value);":"return null; ")}            
        }}";
            }
            if(!classSymbol.MemberNames.Any(name => "SetAttachedPropertyValue" == name))
            {
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} void SetAttachedPropertyValue(EngineNS.UI.Bind.IBindableObject obj, EngineNS.UI.Bind.TtBindableProperty bp, EngineNS.UI.Bind.ValueStoreBase valueStore, object value)
        {{";
                if(!string.IsNullOrEmpty(setAttachedPropertyValueStr))
                {
                    source += $@"    
            var propertyNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(bp.Name);
            switch(propertyNameHash)
            {{
            {setAttachedPropertyValueStr}
            }}";
                }
                source += $@"
            {(baseHasBindObjectInterface? "base.SetAttachedPropertyValue(obj, bp, valueStore, value);" : "")}            
        }}";
            }

            if (!classSymbol.MemberNames.Any(name => "IsPropertyVisibleDirty" == name))
            {
                source += $@"
        [System.ComponentModel.Browsable(false)]
        public{(baseHasBindObjectInterface ? " override" : " virtual")} bool IsPropertyVisibleDirty
        {{
            get;
            set;
        }} = false;";
            }
            if (!classSymbol.MemberNames.Any(name => "GetProperties" == name))
            {
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} void GetProperties(ref EngineNS.EGui.Controls.PropertyGrid.CustomPropertyDescriptorCollection collection, bool parentIsValueType)
        {{
            var type = EngineNS.Rtti.UTypeDesc.TypeOf(this.GetType());
            var pros = System.ComponentModel.TypeDescriptor.GetProperties(this);
            collection.InitValue(this, type, pros, parentIsValueType);

            // attached properties
            foreach(var bindData in {bindExprDicName})
            {{
                if(bindData.Value.Type == EngineNS.UI.Bind.TtBindablePropertyValueBase.EType.AttachedValue)
                {{
                    var proDesc = EngineNS.EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                    proDesc.Name = bindData.Key.Name;
                    if(bindData.Key.DisplayNameAtt != null)
                        proDesc.DisplayName = bindData.Key.DisplayNameAtt.GetDisplayName(this);
                    proDesc.PropertyType = bindData.Key.PropertyType;
                    proDesc.Category = bindData.Key.Category;
                    proDesc.CustomValueEditor = bindData.Key.CustomValueEditor;
                    collection.Add(proDesc);
                }}
            }}

            // events
            var tempCollection = collection;
            EngineNS.UI.Event.TtEventManager.QueryEvents(type, 
                ((curType, name, e)=>
                {{
                    var proDesc = EngineNS.EGui.Controls.PropertyGrid.PropertyCollection.PropertyDescPool.QueryObjectSync();
                    proDesc.Name = name;
                    proDesc.CanCreateNew = false;
                    proDesc.PropertyType = EngineNS.Rtti.UTypeDesc.TypeOf(typeof(EngineNS.UI.Event.TtRoutedEventHandler));
                    proDesc.Category = ""Events"";
                    proDesc.CustomValueEditor = new EngineNS.UI.Event.PGRoutedEventHandlerEditorAttribute();
                    tempCollection.Add(proDesc);
                }}), true);
        }}";
            }
            source += $@"
        protected{(baseHasBindObjectInterface ? " override" : " virtual")} bool _GetPropertyValueWithPropertyHash(ulong propertyNameHash, ref object value)
        {{";
            if (!string.IsNullOrEmpty(getPropertyValueSwitch))
            {
                source += $@"
            switch(propertyNameHash)
            {{
    {getPropertyValueSwitch}
            }}";
            }
            if(baseHasBindObjectInterface)
            {
                source += $@"
            return base._GetPropertyValueWithPropertyHash(propertyNameHash, ref value);";
            }
            else
            {
                source += $@"
            value = null;
            return false;";
            }
            source += $@"
        }}";
            if (!classSymbol.MemberNames.Any(name => "TourBindProperties" == name))
            {
                if(!baseHasBindObjectInterface)
                {
                    source += $@"
        public delegate void TourBindProperyAction<T>(string propertyName, EngineNS.UI.Bind.TtBindableProperty bindProperty, ref T data);";
                }
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} void TourBindProperties<T>(ref T data, TourBindProperyAction<T> tourAction)
        {{
            if(tourAction == null)
                return;
            {tourBindableProperties}";
                if(baseHasBindObjectInterface)
                {
                    source += $@"
            base.TourBindProperties(ref data, tourAction);";
                }
                source += $@"
        }}";
            }
            if(!classSymbol.MemberNames.Any(name => "HasBindProperties" == name))
            {
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} bool HasBindProperties(string containString, bool matchCase = false)
        {{
            if(!matchCase)
                containString = containString.ToLower();";
                if(string.IsNullOrEmpty(hasBindablePropertiesStr))
                {
                    if(baseHasBindObjectInterface)
                    {
                        source += $@"
            return base.HasBindProperties(containString, matchCase);";
                    }
                    else
                    {
                        source += $@"
            return false;";
                    }
                }
                else
                {
                    source += $@"
            {hasBindablePropertiesStr}";
                    if(baseHasBindObjectInterface)
                    {
                        source += $@"
            return base.HasBindProperties(containString, matchCase);";
                    }
                    else
                    {
                        source += $@"
            return false;";
                    }
                }
                source += $@"
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "GetPropertyValue" == name))
            {
                source += $@"
        public{(baseHasBindObjectInterface ? " override" : " virtual")} object GetPropertyValue(string propertyName)
        {{
            if(string.IsNullOrEmpty(propertyName))
                return null;
            object retValue = null;
            var propertyNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(propertyName);
            if(_GetPropertyValueWithPropertyHash(propertyNameHash, ref retValue))
                return retValue;
            var bp = GetBindableProperty(propertyNameHash, propertyName);
            if(bp != null)
            {{
                EngineNS.UI.Bind.TtBindablePropertyValueBase bpVal = null;
                lock (mBindExprDic)
                if (mBindExprDic.TryGetValue(bp, out bpVal))
                {{
                    return bpVal.GetValue<object>(bp);
                }}
            }}
            else
            {{
                var pro = this.GetType().GetProperty(propertyName);
                if (pro != null)
                    return pro.GetValue(this);
            }}

            //foreach(var bindData in {bindExprDicName})
            //{{
            //    if(bindData.Key.Name == propertyName)
            //    {{
            //        return bindData.Value.GetValue<object>(bindData.Key);
            //    }}
            //}}

            return null;
        }}";
            }
            source += $@"
        protected{(baseHasBindObjectInterface ? " override" : " virtual")} bool _SetPropertyValueWithPropertyHash(ulong propertyNameHash, object value)
        {{";
            if(!string.IsNullOrEmpty(setPropertyValueSwitch))
            {
                source += $@"
            switch(propertyNameHash)
            {{
    {setPropertyValueSwitch}
            }}";
            }
            if(baseHasBindObjectInterface)
            {
                source += $@"
            return base._SetPropertyValueWithPropertyHash(propertyNameHash, value);";
            }
            else
            {
                source += $@"
            return false;";
            }
            source += $@"
        }}";
            if (!classSymbol.MemberNames.Any(name => "SetPropertyValue" == name))
            {
                // todo: 这里可以生成代码来设置属性，不需要通过反射，另外可以生成泛型的 SetPropertyValue(string propertyName, T value) 来对应设置不同类型的属性，减少GC
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} void SetPropertyValue(string propertyName, object value)
        {{
            if(string.IsNullOrEmpty(propertyName))
                return;
            var propertyNameHash = Standart.Hash.xxHash.xxHash64.ComputeHash(propertyName);
            if(_SetPropertyValueWithPropertyHash(propertyNameHash, value))
                return;
            var bp = GetBindableProperty(propertyNameHash, propertyName);
            if(bp != null)
            {{
                EngineNS.UI.Bind.TtBindablePropertyValueBase bpVal = null;
                lock (mBindExprDic)
                if (mBindExprDic.TryGetValue(bp, out bpVal))
                {{
                    bpVal.SetValue<object>(this, bp, value);
                }}
            }}
            else
            {{
                var pro = this.GetType().GetProperty(propertyName);
                if (pro != null)
                    pro.SetValue(this, value);
            }}
            //var pro = this.GetType().GetProperty(propertyName);
            //if (pro != null)
            //    pro.SetValue(this, value);
            //else
            //{{
            //    foreach(var bindData in {bindExprDicName})
            //    {{
            //        if(bindData.Key.Name == propertyName)
            //        {{
            //            bindData.Value.SetValue<object>(this, bindData.Key, in value);
            //            break;
            //        }}
            //    }}
            //}}
        }}
";
            }

            if (!classSymbol.MemberNames.Any(name => "SetTemplateValue" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} void SetTemplateValue(EngineNS.UI.Template.TtTemplateSimpleValue simpleValue)
        {{
            var objType = this.GetType();
            if(simpleValue.Property.IsAttachedProperty)
            {{
                this.SetValue(simpleValue.Value, simpleValue.Property);
            }}
            else if(simpleValue.Property.HostType.IsEqual(objType) || simpleValue.Property.HostType.IsParentClass(objType))
            {{";
                string tempValSwitchCode = "";
                foreach (var valSymbol in symbols)
                {
                    if (valSymbol is IPropertySymbol)
                    {
                        var propertySymbol = valSymbol as IPropertySymbol;
                        if (propertySymbol != null && !propertySymbol.IsReadOnly)
                        {
                            tempValSwitchCode += $@"
                case {Standart.Hash.xxHash.xxHash64.ComputeHash(valSymbol.Name)}: // {valSymbol.Name}
                    this.{valSymbol.Name} = ({propertySymbol.Type.ToDisplayString()})(simpleValue.Value);
                    return;";
                        }
                    }
                }
                if (!string.IsNullOrEmpty(tempValSwitchCode))
                {
                    source += $@"
                switch(simpleValue.PropertyNameHash)
                {{{tempValSwitchCode}
                }}";
                }
                if (baseType != null)
                {
                    var baseTypeDS = baseType.ToDisplayString();
                    if (baseTypeDS != "object")
                    {
                        source += $@"
                base.SetTemplateValue(simpleValue);";
                    }
                }
                source += $@"
            }}
            else
            {{
                this.SetValue(simpleValue.Value, simpleValue.Property);
            }}";
                source += $@"
        }}";
            }

            if (!classSymbol.MemberNames.Any(name => "IsMatchTriggerCondition" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} bool IsMatchTriggerCondition<T>(EngineNS.UI.Trigger.TtTriggerConditionLogical<T> triggerCondition)
        {{
            if(triggerCondition.Property.HostType.IsParentClass(this.GetType()))
            {{";
                string tempSwitchCode = "";

                foreach (var valSymbol in symbols)
                {
                    if (valSymbol is IPropertySymbol)
                    {
                        var propertySymbol = valSymbol as IPropertySymbol;
                        if (propertySymbol != null)
                        {
                            if (propertySymbol.Type.IsReferenceType)
                            {
                                tempSwitchCode += $@"
                    case {Standart.Hash.xxHash.xxHash64.ComputeHash(valSymbol.Name)}: // {valSymbol.Name}
                        switch(triggerCondition.Op)
                        {{
                            case EngineNS.UI.Trigger.TtTriggerConditionLogical<T>.ELogicalOperation.Equal:
                                return this.{valSymbol.Name} == triggerCondition.Value.GetValue<{propertySymbol.Type.ToDisplayString()}>();
                            case EngineNS.UI.Trigger.TtTriggerConditionLogical<T>.ELogicalOperation.NotEqual:
                                return this.{valSymbol.Name} != triggerCondition.Value.GetValue<{propertySymbol.Type.ToDisplayString()}>();
                        }}
                        break;";
                            }
                            else
                            {
                                tempSwitchCode += $@"
                    case {Standart.Hash.xxHash.xxHash64.ComputeHash(valSymbol.Name)}: // {valSymbol.Name}
                        switch(triggerCondition.Op)
                        {{
                            case EngineNS.UI.Trigger.TtTriggerConditionLogical<T>.ELogicalOperation.Equal:
                                return this.{valSymbol.Name}.Equals(triggerCondition.Value.GetValue<{propertySymbol.Type.ToDisplayString()}>());
                            case EngineNS.UI.Trigger.TtTriggerConditionLogical<T>.ELogicalOperation.NotEqual:
                                return !this.{valSymbol.Name}.Equals(triggerCondition.Value.GetValue<{propertySymbol.Type.ToDisplayString()}>());
                        }}
                        break;";
                            }
                        }
                    }
                }

                if (!string.IsNullOrEmpty(tempSwitchCode))
                {
                    source += $@"
                switch(triggerCondition.PropertyNameHash)
                {{{tempSwitchCode}
                }}";
                }
                if (baseType != null)
                {
                    var baseTypeDS = baseType.ToDisplayString();
                    if (baseTypeDS != "object")
                    {
                        source += $@"
                return base.IsMatchTriggerCondition(triggerCondition);";
                    }
                    else
                    {
                        source += $@"
                return false;";
                    }
                }

                source += $@"
            }}
            else
            {{
                switch(triggerCondition.Op)
                {{
                    case EngineNS.UI.Trigger.TtTriggerConditionLogical<T>.ELogicalOperation.Equal:
                        return triggerCondition.Value.IsValueEqual<T>(this.GetValue<T>(triggerCondition.Property));
                    case EngineNS.UI.Trigger.TtTriggerConditionLogical<T>.ELogicalOperation.NotEqual:
                        return !triggerCondition.Value.IsValueEqual<T>(this.GetValue<T>(triggerCondition.Property));
                }}
            }}
            return false;
        }}";
            }

            if (!classSymbol.MemberNames.Any(name => "SetFromTriggerSimpleValue" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} void SetFromTriggerSimpleValue<T>(EngineNS.UI.Trigger.TtTriggerSimpleValue<T> triggerSimpleValue)
        {{
            var objType = this.GetType();
            if(triggerSimpleValue.Property.IsAttachedProperty)
            {{
                triggerSimpleValue.OldValueStore.SetValue(this.GetValue<T>(triggerSimpleValue.Property));
                this.SetValue(triggerSimpleValue.ValueStore.GetValue<T>(), triggerSimpleValue.Property);
            }}
            else if(triggerSimpleValue.Property.HostType.IsEqual(objType) || triggerSimpleValue.Property.HostType.IsParentClass(objType))
            {{";
            string triggerValSwitchCode = "";
            foreach(var valSymbol in symbols)
            {
                if(valSymbol is IPropertySymbol)
                {
                    var propertySymbol = valSymbol as IPropertySymbol;
                    if(propertySymbol != null && !propertySymbol.IsReadOnly)
                    {
                        triggerValSwitchCode += $@"
                case {Standart.Hash.xxHash.xxHash64.ComputeHash(valSymbol.Name)}: // {valSymbol.Name}
                    triggerSimpleValue.OldValueStore.SetValue(this.{valSymbol.Name});
                    this.{valSymbol.Name} = triggerSimpleValue.ValueStore.GetValue<{propertySymbol.Type.ToDisplayString()}>();
                    return;";
                    }
                }
            }
            if(!string.IsNullOrEmpty(triggerValSwitchCode))
            {
                source += $@"
                switch(triggerSimpleValue.PropertyNameHash)
                {{{triggerValSwitchCode}
                }}";
            }
            if(baseType != null)
            {
                var baseTypeDS = baseType.ToDisplayString();
                if(baseTypeDS != "object")
                {
                    source += $@"
                base.SetFromTriggerSimpleValue(triggerSimpleValue);";
                }
            }
            source += $@"
            }}
            else
            {{
                triggerSimpleValue.OldValueStore.SetValue(this.GetValue<T>(triggerSimpleValue.Property));
                this.SetValue(triggerSimpleValue.ValueStore.GetValue<T>(), triggerSimpleValue.Property);
            }}
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "RestoreFromTriggerSimpleValue" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} void RestoreFromTriggerSimpleValue<T>(EngineNS.UI.Trigger.TtTriggerSimpleValue<T> triggerSimpleValue)
        {{
            var objType = this.GetType();
            if(triggerSimpleValue.Property.IsAttachedProperty)
            {{
                this.SetValue(triggerSimpleValue.OldValueStore.GetValue<T>(), triggerSimpleValue.Property);
            }}
            else if(triggerSimpleValue.Property.HostType.IsEqual(objType) || triggerSimpleValue.Property.HostType.IsParentClass(objType))
            {{";
            string triggerRestoreValSwitchCode = "";
            foreach(var valSymbol in symbols)
            {
                if(valSymbol is IPropertySymbol)
                {
                    var propertySymbol = valSymbol as IPropertySymbol;
                    if(propertySymbol != null && !propertySymbol.IsReadOnly)
                    {
                        triggerRestoreValSwitchCode += $@"
                case {Standart.Hash.xxHash.xxHash64.ComputeHash(valSymbol.Name)}: // {{valSymbol.Name}}
                    this.{valSymbol.Name} = triggerSimpleValue.OldValueStore.GetValue<{propertySymbol.Type.ToDisplayString()}>();
                    return;";
                    }
                }
            }
            if(!string.IsNullOrEmpty(triggerRestoreValSwitchCode))
            {
                source += $@"
                switch(triggerSimpleValue.PropertyNameHash)
                {{{triggerRestoreValSwitchCode}
                }}";
            }
            if(baseType != null)
            {
                var baseTypeDS = baseType.ToDisplayString();
                if(baseTypeDS != "object")
                {
                    source += $@"
                base.RestoreFromTriggerSimpleValue(triggerSimpleValue);";
                }
            }
                source += $@"
            }}
            else
                this.SetValue(triggerSimpleValue.OldValueStore.GetValue<T>(), triggerSimpleValue.Property);
        }}";
            }

            // macross used bind values
            if (!baseHasBindObjectInterface && !classSymbol.MemberNames.Any(name => "BindingDatas" == name))
            {
                source += $@"
        [System.ComponentModel.Browsable(false)]
        [{mMetaAttrName}]
        public Dictionary<string, EngineNS.UI.Controls.TtUIElement.BindingDataBase> BindingDatas
        {{
            get;
            set;
        }} = new Dictionary<string, EngineNS.UI.Controls.TtUIElement.BindingDataBase>();";
            }
            if (!baseHasBindObjectInterface && !classSymbol.MemberNames.Any(name => "Id" == name))
            {
                source += $@"
        UInt64 mPrivateId = 0;
        [{mMetaAttrName}]
        public UInt64 Id
        {{
            get
            {{
                if (mPrivateId == 0)
                    mPrivateId = Standart.Hash.xxHash.xxHash64.ComputeHash(Guid.NewGuid().ToByteArray());
                return mPrivateId;
            }}
            set => mPrivateId = value;
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "GetPropertyBindMethodName" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} string GetPropertyBindMethodName(string propertyName, bool isSet)
        {{
            if (isSet)
                return ""Set_"" + propertyName + ""_"" + Id;
            else
                return ""Get_"" + propertyName + ""_"" + Id;
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "GetEventMethodName" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} string GetEventMethodName(string eventName)
        {{
            return ""On_"" + eventName + ""_"" + Id;
        }}";
            }
            if (!baseHasBindObjectInterface && !classSymbol.MemberNames.Any(name => "MethodDisplayNames" == name))
            {
                source += $@"
        [System.ComponentModel.Browsable(false)]
        public Dictionary<EngineNS.Bricks.CodeBuilder.UMethodDeclaration, EngineNS.UI.Controls.TtUIElement.MacrossMethodData> MethodDisplayNames = new Dictionary<EngineNS.Bricks.CodeBuilder.UMethodDeclaration, EngineNS.UI.Controls.TtUIElement.MacrossMethodData>();";
            }
            if (!classSymbol.MemberNames.Any(name => "GetMethodDisplayName" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} string GetMethodDisplayName(EngineNS.Bricks.CodeBuilder.UMethodDeclaration desc)
        {{
            EngineNS.UI.Controls.TtUIElement.MacrossMethodData val = null;
            if(MethodDisplayNames.TryGetValue(desc, out val))
            {{
                return val.GetDisplayName(desc);
            }}
            return desc.MethodName;
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "SetMethodDisplayNamesDirty" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} void SetMethodDisplayNamesDirty()
        {{
            foreach(var name in MethodDisplayNames)
            {{
                name.Value.DisplayNameDirty = true;
            }}
        }}";
            }
            if (!baseHasBindObjectInterface && !classSymbol.MemberNames.Any(name => "MacrossMethods" == name))
            {
                source += $@"
        Dictionary<string, EngineNS.UI.Controls.TtUIElement.MacrossMethodData> mMacrossMethods = new Dictionary<string, EngineNS.UI.Controls.TtUIElement.MacrossMethodData>();
        [EngineNS.Rtti.Meta]
        [System.ComponentModel.Browsable(false)]
        public Dictionary<string, EngineNS.UI.Controls.TtUIElement.MacrossMethodData> MacrossMethods
        {{
            get => mMacrossMethods;
            set
            {{
                mMacrossMethods = value;
            }}
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "SetEventBindMethod" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} void SetEventBindMethod(string eventName, EngineNS.Bricks.CodeBuilder.UMethodDeclaration desc)
        {{
            var data = new EngineNS.UI.Controls.TtUIElement.MacrossEventMethodData()
            {{
                EventName = eventName,
                Desc = desc,
                HostObject = this,
            }};
            MacrossMethods[eventName] = data;
            MethodDisplayNames[desc] = data;
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "HasMethod" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} bool HasMethod(string keyName, out string methodDisplayName)
        {{
            EngineNS.UI.Controls.TtUIElement.MacrossMethodData data;
            if (MacrossMethods.TryGetValue(keyName, out data))
            {{
                methodDisplayName = data.GetDisplayName(null);
                return true;
            }}
            methodDisplayName = """";
            return false;
        }}";
            }
        //    if (!classSymbol.MemberNames.Any(name => "SetPropertyBindMethod" == name))
        //    {
        //        source += $@"
        //public {(baseHasBindObjectInterface ? "override" : "virtual")} void SetPropertyBindMethod(string propertyName, EngineNS.Bricks.CodeBuilder.UMethodDeclaration desc, bool isSet)
        //{{
        //    EngineNS.UI.Controls.TtUIElement.MacrossMethodData data;
        //    if(MacrossMethods.TryGetValue(propertyName, out data))
        //    {{
        //        var pbData = data as EngineNS.UI.Controls.TtUIElement.MacrossPropertyBindMethodData;
        //        if (isSet)
        //            pbData.SetDesc = desc;
        //        else
        //            pbData.GetDesc = desc;
        //    }}
        //    else
        //    {{
        //        var pbData = new EngineNS.UI.Controls.TtUIElement.MacrossPropertyBindMethodData()
        //        {{
        //            PropertyName = propertyName,
        //            HostObject = this,
        //        }};
        //        if (isSet)
        //            pbData.SetDesc = desc;
        //        else
        //            pbData.GetDesc = desc;
        //        MacrossMethods[propertyName] = pbData;
        //        data = pbData;
        //    }}
        //    MethodDisplayNames[desc] = data;
        //}}";
        //    }
            if (!classSymbol.MemberNames.Any(name => "OnRemoveMacrossMethod" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} void OnRemoveMacrossMethod(ref EngineNS.UI.Editor.TtUIEditor.MacrossEditorRemoveMethodQueryData data)
        {{";
                if(baseHasBindObjectInterface)
                {
                    source += $@"
            base.OnRemoveMacrossMethod(ref data);";
                }
                else
                {
                    source += $@"
            List<string> needDeletes = new List<string>();
            foreach(var method in MacrossMethods)
            {{
                if(method.Value is EngineNS.UI.Controls.TtUIElement.MacrossEventMethodData)
                {{
                    // remove event bind method
                    var evd = method.Value as EngineNS.UI.Controls.TtUIElement.MacrossEventMethodData;
                    if (evd.Desc.Equals(data.Desc))
                    {{
                        needDeletes.Add(method.Key);
                        MethodDisplayNames.Remove(data.Desc);
                    }}
                }}
                else if(method.Value is EngineNS.UI.Controls.TtUIElement.MacrossPropertyBindMethodData)
                {{
                    // remove property bind method
                    var pvd = method.Value as EngineNS.UI.Controls.TtUIElement.MacrossPropertyBindMethodData;
                    if(pvd.SetDesc.Equals(data.Desc) ||
                       pvd.GetDesc.Equals(data.Desc))
                    {{
                        needDeletes.Add(method.Key);
                        MethodDisplayNames.Remove(data.Desc);
                        BindingDatas.Remove(method.Key);
                    }}
                }}
            }}
            for(int i=0; i<needDeletes.Count; i++)
            {{
                MacrossMethods.Remove(needDeletes[i]);
            }}";
                }
                foreach(var bindObj in bindObjectMemberSymbols)
                {
                    source += $@"
            if({bindObj.Name} != null)
                {bindObj.Name}.OnRemoveMacrossMethod(ref data);";
                }
            source += $@"
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "BindMacross" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} void BindMacross(ref int temp, EngineNS.Bricks.CodeBuilder.UClassDeclaration defClass)
        {{";
                if(baseHasBindObjectInterface)
                {
                    source += $@"
            base.BindMacross(ref temp, defClass);";
                }
                else
                {
                    source += $@"
            List<string> needDeletes = new List<string>();
            foreach(var evt in MacrossMethods)
            {{
                if(evt.Value is EngineNS.UI.Controls.TtUIElement.MacrossEventMethodData)
                {{
                    var data = evt.Value as EngineNS.UI.Controls.TtUIElement.MacrossEventMethodData;
                    var eventName = data.EventName;
                    var methodDesc = defClass.FindMethod(GetEventMethodName(eventName));
                    if(methodDesc == null)
                    {{
                        // method已删除或不存在
                        needDeletes.Add(evt.Key);
                    }}
                    else
                    {{
                        data.Desc = methodDesc;
                        methodDesc.GetDisplayNameFunc = GetMethodDisplayName;
                        MethodDisplayNames[methodDesc] = data;
                    }}
                }}
                else if(evt.Value is EngineNS.UI.Controls.TtUIElement.MacrossPropertyBindMethodData)
                {{
                    var data = evt.Value as EngineNS.UI.Controls.TtUIElement.MacrossPropertyBindMethodData;
                    var propertyName = data.PropertyName;
                    var setMethodDesc = defClass.FindMethod(GetPropertyBindMethodName(propertyName, true));
                    var getMethodDesc = defClass.FindMethod(GetPropertyBindMethodName(propertyName, false));
                    if(setMethodDesc == null && getMethodDesc == null)
                    {{
                        // method已删除或不存在
                        needDeletes.Add(evt.Key);
                    }}
                    else
                    {{
                        if(getMethodDesc != null)
                        {{
                            data.GetDesc = getMethodDesc;
                            getMethodDesc.GetDisplayNameFunc = GetMethodDisplayName;
                            MethodDisplayNames[getMethodDesc] = data;
                        }}
                        if(setMethodDesc != null)
                        {{
                            data.SetDesc = setMethodDesc;
                            setMethodDesc.GetDisplayNameFunc = GetMethodDisplayName;
                            MethodDisplayNames[setMethodDesc] = data;
                        }}
                    }}
                }}
            }}

            for(int i = 0; i<needDeletes.Count; i++)
            {{
                MacrossMethods.Remove(needDeletes[i]);
            }}";
                }
                foreach (var bindObj in bindObjectMemberSymbols)
                {
                    source += $@"
            if({bindObj.Name} != null)
                {bindObj.Name}.BindMacross(ref temp, defClass);";
                }
                source += $@"
        }}";
            }
            if (!baseHasBindObjectInterface && !classSymbol.MemberNames.Any(name => "Name" == name))
            {
                //bool find = false;
                //var tempBaseType = classSymbol.BaseType;
                //while ((baseType != null))
                //{
                //    if(baseType.MemberNames.Any(name => "Name" == name))
                //    {
                //        find = true;
                //        break;
                //    }
                //    baseType = baseType.BaseType;
                //}
                //if(!find)
                {
                    source += $@"
        [System.ComponentModel.Browsable(false)]
        public string Name {{ get; set; }}";
                }
            }
            if (!classSymbol.MemberNames.Any(name => "UpdatePropertiesName" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} void UpdatePropertiesName()
        {{";
                if(baseHasBindObjectInterface)
                {
                    source += $@"
            base.UpdatePropertiesName();";
                }
                foreach (var bindObj in bindObjectMemberSymbols)
                {
                    source += $@"
            if({bindObj.Name} != null)
            {{
                {bindObj.Name}.Name = Name + "".{bindObj.Name}"";
                {bindObj.Name}.UpdatePropertiesName();
            }}";
                }

                source += $@"
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "FindBindObject" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} EngineNS.UI.Bind.IBindableObject FindBindObject(UInt64 id)
        {{
            if (Id == id)
                return this;";
                foreach (var bindObj in bindObjectMemberSymbols)
                {
                    source += $@"
            if({bindObj.Name} != null)
            {{
                var retVal = {bindObj.Name}.FindBindObject(id);
                if(retVal != null)
                    return retVal;
            }}";
                }
                if (baseHasBindObjectInterface)
                {
                    source += $@"
            return base.FindBindObject(id);";
                }
                else
                {
                    source += $@"
            return null;";
                }
                
                source += $@"
        }}";
            }
            if (!classSymbol.MemberNames.Any(name => "GenerateBindingDataStatement" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} void GenerateBindingDataStatement(EngineNS.UI.Editor.TtUIEditor editor, List<EngineNS.Bricks.CodeBuilder.UStatementBase> sequence)
        {{";
                foreach (var bindObj in bindObjectMemberSymbols)
                {
                    source += $@"
            if({bindObj.Name} != null)
                {bindObj.Name}.GenerateBindingDataStatement(editor, sequence);";
                }
                if(baseHasBindObjectInterface)
                {
                    source += $@"
            base.GenerateBindingDataStatement(editor, sequence);";
                }
                else
                {
                    source += $@"
            foreach(var data in BindingDatas)
            {{
                data.Value.GenerateStatement(editor, sequence);
            }}";
                }
                source += $@"
        }}";
            }
            if (!HasISerializer)
            {
                if (!classSymbol.MemberNames.Any(name => "OnPreRead" == name))
                {
                    source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} void OnPreRead(object tagObject, object hostObject, bool fromXml) {{}}";
                }
                if (!classSymbol.MemberNames.Any(name => "OnPropertyRead" == name))
                {
                    source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} void OnPropertyRead(object tagObject, System.Reflection.PropertyInfo prop, bool fromXml) {{}}";
                }
            }
            if (!classSymbol.MemberNames.Any(name => "InitialMethodDeclaration" == name))
            {
                source += $@"
        public {(baseHasBindObjectInterface ? "override" : "virtual")} void InitialMethodDeclaration(string propertyName, EngineNS.Bricks.CodeBuilder.UMethodDeclaration desc, bool isSet)
        {{
            desc.MethodName = GetPropertyBindMethodName(propertyName, isSet);
            desc.GetDisplayNameFunc = GetMethodDisplayName;

            EngineNS.UI.Controls.TtUIElement.MacrossMethodData data;
            if(MacrossMethods.TryGetValue(propertyName, out data))
            {{
                var pbData = data as EngineNS.UI.Controls.TtUIElement.MacrossPropertyBindMethodData;
                if (isSet)
                    pbData.SetDesc = desc;
                else
                    pbData.GetDesc = desc;
            }}
            else
            {{
                var pbData = new EngineNS.UI.Controls.TtUIElement.MacrossPropertyBindMethodData()
                {{
                    PropertyName = propertyName,
                    HostObject = this,
                }};
                if (isSet)
                    pbData.SetDesc = desc;
                else
                    pbData.GetDesc = desc;
                MacrossMethods[propertyName] = pbData;
                data = pbData;
            }}
            MethodDisplayNames[desc] = data;
        }}";
            }

            source += "\r\n    }\r\n";
            source += bindImpSource;
            source += "}\r\n";
            return source;
        }
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not BindingSyntaxReceiver receiver)
                return;

            var compilation = context.Compilation;
            var bindPropertySymbol = compilation.GetTypeByMetadataName(mBindPropAttrName);
            var attachedPropertySymbol = compilation.GetTypeByMetadataName(mAttachedPropAttrName);
            var bindObjectSymbol = compilation.GetTypeByMetadataName(mBindObjectAttrName);
            List<ISymbol> symbols = new List<ISymbol>();
            List<ISymbol> bindObjectMemberSymbols = new List<ISymbol>();
            foreach(var prop in receiver.CandidateProperties)
            {
                var model = compilation.GetSemanticModel(prop.SyntaxTree);
                var propertySymbol = model.GetDeclaredSymbol(prop) as IPropertySymbol;
                if (propertySymbol == null)
                    continue;

                var attData = propertySymbol.GetAttributes().Single(ad => (ad.AttributeClass == null) ? false : ad.AttributeClass.Equals(bindPropertySymbol, SymbolEqualityComparer.Default));
                var isAutoGenOpt = attData.NamedArguments.SingleOrDefault(kvp => kvp.Key == "IsAutoGen").Value;
                bool isAutoGen = true;
                if (!isAutoGenOpt.IsNull)
                {
                    isAutoGen = System.Convert.ToBoolean(isAutoGenOpt.Value);
                }
                if(isAutoGen)
                {
                    symbols.Add(propertySymbol);
                }
            }
            //foreach(var prop in receiver.CandidateIBindableObjectProperties)
            //{
            //    var model = compilation.GetSemanticModel(prop.SyntaxTree);
            //    var propertySymbol = model.GetDeclaredSymbol(prop) as IPropertySymbol;
            //    if (propertySymbol == null)
            //        continue;
            //    bindObjectMemberSymbols.Add(propertySymbol);
            //}
            foreach (var field in receiver.CandidateFields)
            {
                var model = compilation.GetSemanticModel(field.SyntaxTree);
                foreach(var variable in field.Declaration.Variables)
                {
                    var fieldSymbol = model.GetDeclaredSymbol(variable) as IFieldSymbol;
                    if (fieldSymbol == null) continue;
                    symbols.Add(fieldSymbol);
                }
            }
            foreach(var method in receiver.CandidateMethods)
            {
                var model = compilation.GetSemanticModel(method.SyntaxTree);
                var methodSymbol = model.GetDeclaredSymbol(method) as IMethodSymbol;
                if(methodSymbol == null) 
                    continue;
                symbols.Add(methodSymbol);
            }
            foreach(var cls in receiver.CandidateClasses)
            {
                var model = compilation.GetSemanticModel(cls.SyntaxTree);
                var clsSymbol = model.GetDeclaredSymbol(cls) as INamedTypeSymbol;
                if (clsSymbol == null) 
                    continue;
                symbols.Add(clsSymbol);
            }
            var groups = symbols.GroupBy(f => (f.Kind == SymbolKind.NamedType)? f : f.ContainingType, SymbolEqualityComparer.Default);
            foreach(var group in groups)
            {
                var key = group.Key;
                if(key == null) continue;

                List<ISymbol> groupSymbols = group.ToList();
                bindObjectMemberSymbols.Clear();
                foreach (var symbol in groupSymbols)
                {
                    if(symbol is IPropertySymbol propertySymbol)
                    {
                        foreach(var tempGroup in groups)
                        {
                            if(tempGroup.Key.Equals(propertySymbol.Type, SymbolEqualityComparer.Default))
                            {
                                bindObjectMemberSymbols.Add(symbol);
                            }
                        }
                    }
                }

                var classSource = ProcessClass(key as INamedTypeSymbol, groupSymbols, bindObjectMemberSymbols, bindPropertySymbol, attachedPropertySymbol, bindObjectSymbol, context);
                var fileName = $"{key.ToDisplayString()}_bind.g.cs";
                context.AddSource(fileName, SourceText.From(classSource, Encoding.UTF8));
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new BindingSyntaxReceiver());
        }
    }
}
