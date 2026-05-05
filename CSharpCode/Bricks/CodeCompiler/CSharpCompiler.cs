using EngineNS.Profiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.MSBuild;
using Org.BouncyCastle.Operators;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.CodeCompiler
{
    public class CSharpCompiler
    {
        static string[] mBaseAssemblys =
        {
            "System.Core.dll",
            "System.Runtime.dll",
            "System.Private.CoreLib.dll",
            "System.Collections.dll",
            "System.ComponentModel.Primitives.dll",
            "System.ComponentModel.TypeConverter.dll",
        };
        static EmitResult mLastEmitResult;
        public static EmitResult LastEmitResult
        {
            get => mLastEmitResult;
            private set
            {
                mLastEmitResult = value;
            }
        }

        public class CustomAnalyzerConfigOptionsProvider : AnalyzerConfigOptionsProvider
        {
            private readonly AnalyzerConfigOptions mGlobalOptions;

            public CustomAnalyzerConfigOptionsProvider(Dictionary<string, string> options)
            {
                mGlobalOptions = new CustomAnalyzerConfigOptions(options);
            }

            public override AnalyzerConfigOptions GlobalOptions => mGlobalOptions;

            public override AnalyzerConfigOptions GetOptions(SyntaxTree tree)
                => mGlobalOptions;

            public override AnalyzerConfigOptions GetOptions(AdditionalText textFile)
                => mGlobalOptions;

            private class CustomAnalyzerConfigOptions : AnalyzerConfigOptions
            {
                private readonly Dictionary<string, string> _options;

                public CustomAnalyzerConfigOptions(Dictionary<string, string> options)
                {
                    _options = options;
                }

                public override bool TryGetValue(string key, out string value)
                    => _options.TryGetValue(key, out value);
            }
        }

        public static bool CompilerCSharpCodes(string[] cshaprFiles, string[] refAssemblyFiles, string[] preprocessorSymbols, string outputFile, string pdbFile, CSharpCompilationOptions option)
        {
            try
            {
                var syntaxTrees = new SyntaxTree[cshaprFiles.Length];
                for (int i = 0; i < cshaprFiles.Length; i++)
                {
                    var fileContent = System.IO.File.ReadAllText(cshaprFiles[i], Encoding.UTF8);
                    syntaxTrees[i] = CSharpSyntaxTree.ParseText(fileContent,
                        options: new CSharpParseOptions().WithPreprocessorSymbols(preprocessorSymbols),
                        path: cshaprFiles[i],
                        encoding: Encoding.UTF8);
                }

                // base reference
                var metaRefs = new PortableExecutableReference[refAssemblyFiles.Length + mBaseAssemblys.Length];
                var baseAssembDir = IO.TtFileManager.GetBaseDirectory(typeof(object).Assembly.Location);
                for (int i = 0; i < mBaseAssemblys.Length; i++)
                {
                    metaRefs[i] = MetadataReference.CreateFromFile(baseAssembDir + mBaseAssemblys[i]);
                }
                // reference assemblies
                for (int i = 0; i < refAssemblyFiles.Length; i++)
                {
                    metaRefs[i + mBaseAssemblys.Length] = MetadataReference.CreateFromFile(refAssemblyFiles[i]);
                }

                var genFilePath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "temp", "Generated");
                EngineNS.IO.TtFileManager.CreateDirectory(genFilePath);
                //var analyzerConfigOptions = new Dictionary<string, string>
                //{
                //    ["build_property.EmitCompilerGeneratedFiles"] = "true",
                //    ["build_property.CompilerGeneratedFilesOutputPath"] = genFilePath + "\\"
                //};
                //var optionsProvider = new CustomAnalyzerConfigOptionsProvider(analyzerConfigOptions);
                var name = IO.TtFileManager.GetPureName(outputFile);
                var compilation = CSharpCompilation.Create(name, syntaxTrees, metaRefs, option);
                var bindingGenerator = new CompilingGenerator.BindingCodeIncrementalGenerator();
                GeneratorDriver generatorDriver = CSharpGeneratorDriver.Create(new[] { bindingGenerator });
                //generatorDriver.WithUpdatedAnalyzerConfigOptions(optionsProvider)
                //               .RunGeneratorsAndUpdateCompilation(compilation, out var updateCompilation, out var diagnostics);
                generatorDriver = generatorDriver.RunGeneratorsAndUpdateCompilation(compilation, out var updateCompilation, out var diagnostics);
                var genResult = generatorDriver.GetRunResult();
                if(bindingGenerator.GeneratedCodes.Count > 0)
                {
                    foreach(var genCodeData in bindingGenerator.GeneratedCodes)
                    {
                        var tempGenFilePath = System.IO.Path.Combine(genFilePath, genCodeData.Key);
                        if (IO.TtFileManager.FileExists(tempGenFilePath))
                            IO.TtFileManager.DeleteFile(tempGenFilePath);
                        using (var fs = new StreamWriter(tempGenFilePath, false, Encoding.UTF8))
                        {
                            fs.Write(genCodeData.Value);
                        }
                    }
                }
                foreach (var genTree in genResult.GeneratedTrees)
                {
                    var genFile = genTree.ToString();
                    //var genFileName = System.IO.Path.GetFileName(genFile);
                    //var tempGenFilePath = System.IO.Path.Combine(genFilePath, genFileName);
                    //using (var fs = new FileStream(tempGenFilePath, FileMode.Create))
                    //{
                    //    fs.Write(genTree. .ToFullString());
                    //}
                }
                bool retValue = true;
                using (var outStream = new MemoryStream())
                using (var pdbStream = new MemoryStream())
                {
                    var emitOptions = new EmitOptions(false);
                    if (option.OptimizationLevel == OptimizationLevel.Debug)
                    {
                        if (string.IsNullOrEmpty(pdbFile))
                            pdbFile = System.IO.Path.ChangeExtension(outputFile, "tpdb");
                        emitOptions = emitOptions.WithDebugInformationFormat(DebugInformationFormat.PortablePdb).WithPdbFilePath(pdbFile);
                    }

                    LastEmitResult = updateCompilation.Emit(outStream, pdbStream, null, null, null, emitOptions);
                    if (LastEmitResult.Success)
                    {
                        retValue = true;
                        using (var fs = new FileStream(outputFile, FileMode.Create))
                        {
                            fs.Write(outStream.ToArray());
                        }
                        if (option.OptimizationLevel == OptimizationLevel.Debug)
                        {
                            try
                            {
                                if (IO.TtFileManager.FileExists(pdbFile))
                                    IO.TtFileManager.DeleteFile(pdbFile);
                                using (var fs = new FileStream(pdbFile, FileMode.Create))
                                {
                                    fs.Write(pdbStream.ToArray());
                                }
                            }
                            catch (System.Exception ex)
                            {
                                Log.WriteException(ex);
                            }
                        }
                        System.Diagnostics.Debug.WriteLine("Macross build success");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Macross build failed");
                        foreach (var i in LastEmitResult.Diagnostics)
                        {
                            System.Diagnostics.Debug.WriteLine(i.ToString());
                        }
                        foreach (var i in LastEmitResult.Diagnostics)
                        {
                            Console.WriteLine(i.ToString());
                        }
                        retValue = false;
                    }
                }
                return retValue;
            }
            catch (Exception ex)
            {
                LastEmitResult = null;
                Log.WriteException(ex);
                return false;
            }
        }

        public enum enCommandType
        {
            Unknow = 0,
            CSFile,
            RefAssemblyFile,
            OutputFile,
            PdbFile,
            Outputkind,
            ReportSuppressedDiagnostics,
            ModuleName,
            MainTypeName,
            ScriptClassName,
            Usings,
            OptimizationLevel,
            CheckOverflow,
            AllowUnsafe,
            CryptoKeyContainer,
            CryptoKeyFile,
            CryptoPublicKey,
            DelaySign,
            Platform,
            GeneralDiagnosticOption,
            WarningLevel,
            SpecificDiagnosticOptions,
            ConcurrentBuild,
            Deterministic,
            XmlReferenceResolver,
            SourceReferenceResolver,
            MetadataReferenceResolver,
            AssemblyIdentityComparer,
            StrongNameProvider,
            PublicSign,
            MetadataImportOptions,
            NullableContextOptions,
            PreprocessorSymbol,
            Count,
        }

        public static string GetCommandArguments(enCommandType type, string command)
        {
            return "-" + (int)type + ":" + command.Replace("\\", "/");
        }

        public static string GetCommandWithArguments(string[] args)
        {
            return string.Join(',', args);
        }

        static enCommandType GetCommand(string arg, out string command)
        {
            command = "";
            var idx = arg.IndexOf(':');
            if(idx < 0)
                return enCommandType.Unknow;

            var typeStr = arg.Substring(0, idx);
            var cmd = arg.Substring(idx + 1, arg.Length - idx - 1);

            for(int i=(int)enCommandType.Unknow+1; i<(int)enCommandType.Count; i++)
            {
                if(typeStr == ("-" + i))
                {
                    command = cmd;
                    return (enCommandType)i;
                }
            }

            return enCommandType.Unknow;
        }

        public static bool CompilerCSharpWithArguments(string[] args)
        {
            List<string> csFiles = new List<string>();
            List<string> refAssemblyFiles = new List<string>();
            string outputFile = null;
            string pdbFile = null;
            OutputKind outputKind = OutputKind.DynamicallyLinkedLibrary;
            bool reportSuppressedDiagnostics = false;
            string moduleName = default;
            string mainTypeName = default;
            string scriptClassName = default;
            List<string> usings = default;
            OptimizationLevel optimizationLevel = OptimizationLevel.Debug;
            bool checkOverflow = false;
            bool allowUnsafe = false;
            string cryptoKeyContainer = default;
            string cryptoKeyFile = default;
            ImmutableArray<byte> cryptoPublicKey = default;
            bool delaySign = default;
            Platform platform = Platform.AnyCpu;
            ReportDiagnostic generalDiagnosticOption = ReportDiagnostic.Default;
            int warningLevel = 4;
            Dictionary<string, ReportDiagnostic> specificDiagnosticOptions = default;
            bool concurrentBuild = true;
            bool deterministic = false;
            XmlReferenceResolver xmlReferenceResolver = default;
            SourceReferenceResolver sourceReferenceResolver = default;
            MetadataReferenceResolver metadataReferenceResolver = default;
            AssemblyIdentityComparer assemblyIdentityComparer = default;
            StrongNameProvider strongNameProvider = default;
            bool publicSign = false;
            MetadataImportOptions metadataImportOptions = MetadataImportOptions.Public;
            NullableContextOptions nullableContextOptions = NullableContextOptions.Disable;
            List<string> preprocessorSymbols = new List<string>();

            for (int argIdx = 0; argIdx < args.Length; argIdx++)
            {
                string command;
                var commandType = GetCommand(args[argIdx], out command);
                switch(commandType)
                {
                    case enCommandType.CSFile:
                        csFiles.Add(command);
                        break;
                    case enCommandType.RefAssemblyFile:
                        refAssemblyFiles.Add(command);
                        break;
                    case enCommandType.OutputFile:
                        outputFile = command;
                        break;
                    case enCommandType.PdbFile:
                        pdbFile = command;
                        break;
                    case enCommandType.Outputkind:
                        Enum.TryParse(command, out outputKind);
                        break;
                    case enCommandType.ReportSuppressedDiagnostics:
                        reportSuppressedDiagnostics = System.Convert.ToBoolean(command);
                        break;
                    case enCommandType.ModuleName:
                        moduleName = command;
                        break;
                    case enCommandType.MainTypeName:
                        mainTypeName = command;
                        break;
                    case enCommandType.ScriptClassName:
                        scriptClassName = command;
                        break;
                    case enCommandType.Usings:
                        {
                            if (usings == null)
                                usings = new List<string>();
                            usings.Add(command);
                        }
                        break;
                    case enCommandType.OptimizationLevel:
                        System.Enum.TryParse(command, out optimizationLevel);
                        break;
                    case enCommandType.CheckOverflow:
                        checkOverflow = System.Convert.ToBoolean(command);
                        break;
                    case enCommandType.AllowUnsafe:
                        allowUnsafe = System.Convert.ToBoolean(command);
                        break;
                    case enCommandType.CryptoKeyContainer:
                        cryptoKeyContainer = command;
                        break;
                    case enCommandType.CryptoKeyFile:
                        cryptoKeyFile = command;
                        break;
                    case enCommandType.CryptoPublicKey:
                        {
                            if (cryptoPublicKey == null)
                                cryptoPublicKey = new ImmutableArray<byte>();
                            cryptoPublicKey.Add(System.Convert.ToByte(command));
                        }
                        break;
                    case enCommandType.DelaySign:
                        delaySign = System.Convert.ToBoolean(command);
                        break;
                    case enCommandType.Platform:
                        System.Enum.TryParse(command, out platform);
                        break;
                    case enCommandType.GeneralDiagnosticOption:
                        System.Enum.TryParse(command, out generalDiagnosticOption);
                        break;
                    case enCommandType.WarningLevel:
                        warningLevel = System.Convert.ToInt32(command);
                        break;
                    case enCommandType.SpecificDiagnosticOptions:
                        {
                            if (specificDiagnosticOptions == null)
                                specificDiagnosticOptions = new Dictionary<string, ReportDiagnostic>();
                            var splits = command.Split('|');
                            ReportDiagnostic diag;
                            System.Enum.TryParse(splits[1], out diag);
                            specificDiagnosticOptions[splits[0]] = diag;
                        }
                        break;
                    case enCommandType.ConcurrentBuild:
                        concurrentBuild = System.Convert.ToBoolean(command);
                        break;
                    case enCommandType.Deterministic:
                        deterministic = System.Convert.ToBoolean(command);
                        break;
                    case enCommandType.XmlReferenceResolver:
                        break;
                    case enCommandType.SourceReferenceResolver:
                        break;
                    case enCommandType.MetadataReferenceResolver:
                        break;
                    case enCommandType.AssemblyIdentityComparer:
                        break;
                    case enCommandType.StrongNameProvider:
                        break;
                    case enCommandType.PublicSign:
                        publicSign = System.Convert.ToBoolean(command);
                        break;
                    case enCommandType.MetadataImportOptions:
                        System.Enum.TryParse(command, out metadataImportOptions);
                        break;
                    case enCommandType.NullableContextOptions:
                        System.Enum.TryParse(command, out nullableContextOptions);
                        break;
                    case enCommandType.PreprocessorSymbol:
                        preprocessorSymbols.Add(command);
                        break;
                }
            }

            var option = new CSharpCompilationOptions(
                outputKind, reportSuppressedDiagnostics, moduleName, mainTypeName, scriptClassName, usings, optimizationLevel,
                checkOverflow, allowUnsafe, cryptoKeyContainer, cryptoKeyFile, cryptoPublicKey, delaySign, platform, generalDiagnosticOption,
                warningLevel, specificDiagnosticOptions, concurrentBuild, deterministic, xmlReferenceResolver, sourceReferenceResolver, metadataReferenceResolver, 
                assemblyIdentityComparer, strongNameProvider, publicSign,
                metadataImportOptions, nullableContextOptions);

            return CompilerCSharpCodes(csFiles.ToArray(), refAssemblyFiles.ToArray(), preprocessorSymbols.ToArray(), outputFile, pdbFile, option);
        }
    }
}
