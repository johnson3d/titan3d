rem 以后还要先做从下载依赖资源和第三方库

winrar x -ad Native\3rd\ShaderConductor\Bin\Debug\dxcompiler.rar
winrar x -ad Native\3rd\ShaderConductor\Bin\Release\dxcompiler.rar

xcopy /y Native\3rd\ShaderConductor\Bin\Debug\*.* binaries\x64\Debug\*.*
xcopy /y Native\3rd\oiio\Bin\*.* binaries\x64\Debug\*.*
xcopy /y Native\3rd\PhysX-3.4\PhysX_3.4\bin\vc15win64\*.* binaries\x64\Debug\*.*
xcopy /y Native\3rd\PhysX-3.4\PxShared\bin\vc15win64\*.* binaries\x64\Debug\*.*
xcopy /y Native\3rd\pthread\dll\x64\*.* binaries\x64\Debug\*.*

xcopy /y Native\3rd\ShaderConductor\Bin\Release\*.* binaries\x64\Release\*.*
xcopy /y Native\3rd\oiio\Bin\*.* binaries\x64\Release\*.*
xcopy /y Native\3rd\PhysX-3.4\PhysX_3.4\bin\vc15win64\*.* binaries\x64\Release\*.*
xcopy /y Native\3rd\PhysX-3.4\PxShared\bin\vc15win64\*.* binaries\x64\Release\*.*
xcopy /y Native\3rd\pthread\dll\x64\*.* binaries\x64\Release\*.*

