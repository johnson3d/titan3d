if "%1" == "Release" (
xcopy /y x64\Release\*.dll
) else (
xcopy /y x64\Debug\*.dll
)

