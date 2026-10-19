#nullable disable
using EngineNS.Bricks.CodeBuilder;

internal static class MacrossPatchStubProbe
{
    public static void Run()
    {
        // CodeBuilder 声明对象的默认值依赖 Rtti，探针进程无引擎启动流程，按 TtEngine 初始化顺序注册类型
        EngineNS.Rtti.TtTypeDescManager.Instance.InitAssembly(null, typeof(object).Assembly);
        EngineNS.Rtti.TtTypeDescManager.Instance.InitAssembly(null, typeof(TtTypeReference).Assembly);

        Func<TtTypeReference, string> typeName = t => t.TypeFullName;
        var cls = new TtClassDeclaration() { ClassName = "Foo" };
        var method = new TtMethodDeclaration() { MethodName = "Hit" };
        method.Arguments.Add(new TtMethodArgumentDeclaration() { VariableName = "a", VariableType = new TtTypeReference("System.Int32") });
        method.Arguments.Add(new TtMethodArgumentDeclaration() { VariableName = "s", VariableType = new TtTypeReference("System.String") });
        method.ReturnValue = new TtVariableDeclaration() { VariableName = "ret", VariableType = new TtTypeReference("System.Boolean") };

        var sig = TtMacrossPatchStubCodeGen.BuildSignature("NS.Foo", method, typeName);
        Expect(sig == "NS.Foo::Hit(System.Int32,System.String)", sig);
        var id = EngineNS.Macross.TtMacrossPatchId.Compute(sig);
        var stub = TtMacrossPatchStubCodeGen.BuildStub(id, method, typeName);
        Expect(stub[0] == $"if (EngineNS.Macross.TtMacrossPatch.Active && EngineNS.Macross.TtMacrossPatch.TryInvoke(0x{id:X16}UL, this, out System.Boolean __mcPatchRet, a, s))", stub[0]);
        Expect(stub[1] == "    return __mcPatchRet;", stub[1]);
        Expect(TtMacrossPatchStubCodeGen.CanPatch(cls, method), "plain method patchable");

        method.ReturnValue = null;
        stub = TtMacrossPatchStubCodeGen.BuildStub(id, method, typeName);
        Expect(stub[0].EndsWith($"TryInvoke(0x{id:X16}UL, this, a, s))") && stub[1] == "    return;", stub[0]);

        method.Arguments[0].OperationType = EMethodArgumentAttribute.Ref;
        Expect(!TtMacrossPatchStubCodeGen.CanPatch(cls, method), "ref excluded");
        method.Arguments[0].OperationType = EMethodArgumentAttribute.Default;
        method.AsyncType = TtMethodDeclaration.EAsyncType.SystemTask;
        Expect(!TtMacrossPatchStubCodeGen.CanPatch(cls, method), "async excluded");
        method.AsyncType = TtMethodDeclaration.EAsyncType.None;
        method.RawBodyCode = "void Hit() {}";
        Expect(!TtMacrossPatchStubCodeGen.CanPatch(cls, method), "RawBodyCode excluded");
        method.RawBodyCode = null;
        cls.IsStruct = true;
        Expect(!TtMacrossPatchStubCodeGen.CanPatch(cls, method), "struct excluded");
        cls.IsStruct = false;
        for (var i = 0; i < 7; ++i)
            method.Arguments.Add(new TtMethodArgumentDeclaration() { VariableName = "x" + i, VariableType = new TtTypeReference("System.Int32") });
        Expect(!TtMacrossPatchStubCodeGen.CanPatch(cls, method), "more than 8 args excluded");
        Console.WriteLine("Macross patch stub codegen probe passed.");
    }

    private static void Expect(bool condition, string what)
    {
        if (!condition)
            throw new InvalidOperationException("Macross patch stub probe failed: " + what);
    }
}
