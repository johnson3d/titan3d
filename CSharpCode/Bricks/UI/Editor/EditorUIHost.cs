using EngineNS.Bricks.CodeBuilder;
using EngineNS.Rtti;
using EngineNS.UI;
using EngineNS.UI.Bind;
using EngineNS.UI.Canvas;
using EngineNS.UI.Controls;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EngineNS.UI.Editor
{
    public interface IUIBindingDataBase : IO.ISerializer
    {
        void GenerateStatement(List<UStatementBase> statements);
        UExpressionBase GetVariableExpression();
        UTypeDesc GetVariableType();
        string GetBindPath();
        bool IsSameTarget<T>(T target);
        void Draw(EditorUIHost host, in ImDrawList drawList);
    }
    public class UIBindingData_Element : IUIBindingDataBase
    {
        [Rtti.Meta]
        public string PropertyName { get; set; }
        [Rtti.Meta]
        public Rtti.UTypeDesc PropertyType { get; set; }
        [Rtti.Meta]
        public UInt64 Id { get; set; }

        public void GenerateStatement(List<UStatementBase> statements)
        {
            var findElementInvokeStatement = new UMethodInvokeStatement(
                "FindElement",
                new UVariableDeclaration()
                {
                    VariableName = "UIElement" + Id,
                    VariableType = new UTypeReference(typeof(TtUIElement)),
                },
                new UVariableReferenceExpression("HostElement"),
                new UMethodInvokeArgumentExpression(new UPrimitiveExpression(Id)))
            {
                DeclarationReturnValue = true,
            };
            for(int i=0; i<statements.Count; i++)
            {
                if (findElementInvokeStatement.Equals(statements[i]))
                    return;
            }
            statements.Add(findElementInvokeStatement);
        }
        public UExpressionBase GetVariableExpression()
        {
            return new UVariableReferenceExpression("UIElement_" + Id);
        }
        public UTypeDesc GetVariableType()
        {
            return PropertyType;
        }
        public string GetBindPath()
        {
            return PropertyName;
        }
        public bool IsSameTarget<T>(T target)
        {
            var element = target as TtUIElement;
            if (element != null)
                return element.Id == Id;
            return false;
        }
        public void Draw(EditorUIHost host, in ImDrawList drawList)
        {
            var element = host.FindElement(Id);
            ImGuiAPI.Text(TtUIEditor.GetElementShowName(element));
            ImGuiAPI.SameLine(0, -1);
            ImGuiAPI.Text(PropertyName);
        }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
        }
    }
    public class UIBindingData_Method : IUIBindingDataBase
    {

        public void Draw(EditorUIHost host, in ImDrawList drawList)
        {

        }

        public void GenerateStatement(List<UStatementBase> statements)
        {

        }

        public string GetBindPath()
        {
            throw new NotImplementedException();
        }

        public UExpressionBase GetVariableExpression()
        {
            return null;
        }

        public UTypeDesc GetVariableType()
        {
            return null;
        }

        public bool IsSameTarget<T>(T target)
        {
            return false;
        }

        public void OnPreRead(object tagObject, object hostObject, bool fromXml)
        {
        }

        public void OnPropertyRead(object tagObject, PropertyInfo prop, bool fromXml)
        {
        }
    }
    public class EditorOnlyData : IO.BaseSerializer
    {
        public class BindingData : IO.BaseSerializer
        {
            [Rtti.Meta]
            public IUIBindingDataBase Source { get; set; }
            [Rtti.Meta]
            public IUIBindingDataBase Target { get; set; }
            [Rtti.Meta]
            public EBindingMode Mode { get; set; } = EBindingMode.Default;

            public virtual void DrawBindInfo(EditorUIHost host, in ImDrawList drawList)
            {
                Source.Draw(host, drawList);
                ImGuiAPI.SameLine(0, -1);
                ImGuiAPI.Text(" (" + Mode.ToString() + ")");
            }
            public virtual void GenerateStatement(List<UStatementBase> sequence)
            {
                Source.GenerateStatement(sequence);
                Target.GenerateStatement(sequence);
                var bindCall = new UMethodInvokeStatement()
                {
                    MethodName = "SetBinding",
                    Host = new UClassReferenceExpression(UTypeDesc.TypeOf(typeof(Bind.TtBindingOperations))),
                };
                bindCall.GenericTypes.Add(Target.GetVariableType());
                bindCall.GenericTypes.Add(Source.GetVariableType());
                bindCall.Arguments.Add(new UMethodInvokeArgumentExpression(Target.GetVariableExpression()));
                bindCall.Arguments.Add(new UMethodInvokeArgumentExpression(new UPrimitiveExpression(Target.GetBindPath())));
                bindCall.Arguments.Add(new UMethodInvokeArgumentExpression(Source.GetVariableExpression()));
                bindCall.Arguments.Add(new UMethodInvokeArgumentExpression(new UPrimitiveExpression(Source.GetBindPath())));
                bindCall.Arguments.Add(new UMethodInvokeArgumentExpression(new UPrimitiveExpression(Mode)));
                sequence.Add(bindCall);
            }
        }

        public class BindingData_Method : BindingData
        {
            public override void DrawBindInfo(EditorUIHost host, in ImDrawList drawList)
            {
                var tg = Target as UIBindingData_Element;
                var element = host.FindElement(tg.Id);
                if (element == null)
                {
                    return;
                }
                var name = TtUIEditor.GetElementShowName(element);
                ImGuiAPI.Text($"Method: {name}");
                ImGuiAPI.SameLine(0, -1);
                ImGuiAPI.Text(" (" + Mode.ToString() + ")");                
            }
            public override void GenerateStatement(List<UStatementBase> sequence)
            {
                Target.GenerateStatement(sequence);
                var bindCall = new UMethodInvokeStatement()
                {
                    MethodName = "SetMethodBinding",
                    Host = new UClassReferenceExpression(UTypeDesc.TypeOf(typeof(Bind.TtBindingOperations))),
                };
            }
        }

        [Rtti.Meta]
        public List<BindingData> BindingDatas
        {
            get;
            set;
        } = new List<BindingData>();

        public void ClearTargetBindData(IBindableObject target, string path)
        {
            for(int i=BindingDatas.Count - 1; i>=0; i--)
            {
                if(BindingDatas[i].Target.IsSameTarget(target))
                {
                    if(string.IsNullOrEmpty(path))
                        BindingDatas.RemoveAt(i);
                    else if(path == BindingDatas[i].Target.GetBindPath())
                        BindingDatas.RemoveAt(i);
                }
            }
        }
    }

    public partial class EditorUIHost : TtUIHost
    {
        public float PathWidth = 10;
        TtPath mEdgePath;
        TtPathStyle mEdgePathStyle = new TtPathStyle();
        TtCanvasBrush mDrawBrush = new TtCanvasBrush()
        {
            Name = "@MatInst:ui/uimat_inst_default.uminst:Engine",
            Color = Color.White,
        };
        public TtCanvasBrush DrawBrush => mDrawBrush;
        TtUIEditor mHostEditor = null;
        public TtUIEditor HostEditor => mHostEditor;

        EditorOnlyData mEditorOnlyData;
        public EditorOnlyData EditorOnlyData
        {
            get
            {
                if (mEditorOnlyData == null)
                    mEditorOnlyData = new EditorOnlyData();
                return mEditorOnlyData;
            }
        }

        public EditorUIHost(TtUIEditor editor)
        {
            mHostEditor = editor;
        }

        public void SaveEditorOnlyData(RName asset)
        {
            var typeStr = Rtti.UTypeDescManager.Instance.GetTypeStringFromType(EditorOnlyData.GetType());
            using (var xnd = new IO.TtXndHolder(typeStr, 0, 0))
            {
                using (var attr = xnd.NewAttribute("EditorOnlyData", 0, 0))
                {
                    using (var ar = attr.GetWriter(512))
                    {
                        ar.Write(EditorOnlyData);
                    }
                    xnd.RootNode.AddAttribute(attr);
                }
                var fileName = asset.Address + "/EditorOnlyData.dat";
                xnd.SaveXnd(fileName);
                UEngine.Instance.SourceControlModule.AddFile(fileName);
            }
        }
        public void LoadEditorOnlyData(RName asset)
        {
            using (var xnd = IO.TtXndHolder.LoadXnd(asset.Address + "/EditorOnlyData.dat"))
            {
                if (xnd == null)
                    return;

                var attr = xnd.RootNode.TryGetAttribute("EditorOnlyData");
                if (attr.NativePointer == IntPtr.Zero)
                    return;

                using (var ar = attr.GetReader(null))
                {
                    try
                    {
                        ar.ReadObject(out mEditorOnlyData);
                    }
                    catch (Exception ex)
                    {
                        Profiler.Log.WriteException(ex);
                    }
                }
            }
        }

        protected override void CustomBuildMesh(Canvas.TtCanvasDrawBatch batch)
        {
            var canvas = mCanvas.Background; //batch.Middleground;

            if (mEdgePath == null)
            {
                mEdgePath = new TtPath();
            }
            mEdgePathStyle.PathWidth = PathWidth;
            mEdgePathStyle.FillArea = false;
            mEdgePathStyle.StrokeMode = EngineNS.Canvas.EPathStrokeMode.Stroke_Dash;
            canvas.PushPathStyle(mEdgePathStyle);
            canvas.PushTransformIndex(mTransformIndex);
            canvas.PushBrush(mDrawBrush);
            mEdgePath.BeginPath();
            var start = new Vector2(mDesignRect.Left, mDesignRect.Top);
            mEdgePath.MoveTo(in start);
            var tr = new Vector2(mDesignRect.Right, mDesignRect.Top);
            mEdgePath.LineTo(in tr);
            var br = new Vector2(mDesignRect.Right, mDesignRect.Bottom);
            mEdgePath.LineTo(in br);
            var bl = new Vector2(mDesignRect.Left, mDesignRect.Bottom);
            mEdgePath.LineTo(in bl);
            mEdgePath.LineTo(in start);

            //mEdgePath.S_CCW_ArcTo(new Vector2(150.0f, 300.0f), 500.0f);
            //mEdgePath.L_CCW_ArcTo(new Vector2(150.0f, 150.0f), 1.0f);

            mEdgePath.EndPath(canvas);
            canvas.PopBrush();
            canvas.PopTransformIndex();
            canvas.PopPathStyle();

            mDrawBrush.IsDirty = true;
        }

        public override bool CanAddChild(Rtti.UTypeDesc childType)
        {
            if (Children.Count > 0)
                return false;
            return true;
        }
    }

    public partial class SelectedDecorator : TtUIHost
    {
        TtPath mEdgePath;
        TtPathStyle mEdgePathStyle = new TtPathStyle();
        TtCanvasBrush mDrawBrush = new TtCanvasBrush()
        {
            Name = "@MatInst:ui/uimat_inst_default.uminst:Engine",
            Color = Color.White,
        };
        public TtCanvasBrush DrawBrush => mDrawBrush;

        public List<TtUIElement> SelectedElements;

        protected override void CustomBuildMesh(Canvas.TtCanvasDrawBatch batch)
        {
            var canvas = mCanvas.Background;

            if(mEdgePath == null)
            {
                mEdgePath = new TtPath();
            }
            mEdgePathStyle.PathWidth = 2;
            mEdgePathStyle.FillArea = false;
            mEdgePathStyle.StrokeMode = EngineNS.Canvas.EPathStrokeMode.Stroke_Dash;
            canvas.PushPathStyle(mEdgePathStyle);
            canvas.PushTransformIndex(mTransformIndex);
            canvas.PushBrush(mDrawBrush);

            for(int i=0; i<SelectedElements.Count; i++)
            {
                var element = SelectedElements[i];
                canvas.PushMatrix(in element.RootUIHost.TransformedElements[element.TransformIndex].Matrix);
                mEdgePath.BeginPath();

                var rect = element.PreviousArrangeRect;
                var start = new Vector2(rect.Left, rect.Top);
                mEdgePath.MoveTo(in start);
                var tr = new Vector2(rect.Right, rect.Top);
                mEdgePath.LineTo(in tr);
                var br = new Vector2(rect.Right, rect.Bottom);
                mEdgePath.LineTo(in br);
                var bl = new Vector2(rect.Left, rect.Bottom);
                mEdgePath.LineTo(in bl);
                mEdgePath.LineTo(in start);

                mEdgePath.EndPath(canvas);
                canvas.PopMatrix();
            }

            canvas.PopBrush();
            canvas.PopTransformIndex();
            canvas.PopPathStyle();

            mDrawBrush.IsDirty = true;
        }
    }
}
