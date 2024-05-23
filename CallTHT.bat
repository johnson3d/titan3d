cd %~dp0
%~dp0binaries\Tools\net8.0\CppWeavingTools.exe vcxproj=%~dp0Core.Window\Core.Window.vcxproj CppOut=%~dp0codegen\NativeBinder\ CsOut=%~dp0codegen\NativeBinder\ ModuleNC=EngineNS.CoreSDK.CoreModule Pch=%~dp0Core.Window\pch.h TargetCppPOD=%~dp0codegen\NativeBinder\PODStructDefine.h
%~dp0binaries\Tools\net8.0\CSharpCodeTools.exe %~dp0Rpc_Engine.txt mode=Cs2Cpp