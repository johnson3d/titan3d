using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace EngineNS.CodeCompiler
{
    public class CSharpCompiler
    {
        public static void CompilerCSharpCodes(string[] cshaprFiles, string[] refAssemblyFiles, string[] preprocessorSymbols, string outputFile, string pdbFile, CSharpCompilationOptions option)
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

            var metaRefs = new PortableExecutableReference[refAssemblyFiles.Length];
            for (int i = 0; i < refAssemblyFiles.Length; i++)
            {
                metaRefs[i] = MetadataReference.CreateFromFile(refAssemblyFiles[i]);
            }

            var name = IO.FileManager.GetPureName(outputFile);
            var compilation = CSharpCompilation.Create(name, syntaxTrees, metaRefs, option);
            using (var outStream = new MemoryStream())
            using (var pdbStream = new MemoryStream())
            {
                var emitOptions = new EmitOptions(false);
                if (option.OptimizationLevel == OptimizationLevel.Debug)
                {
                    if (string.IsNullOrEmpty(pdbFile))
                        pdbFile = System.IO.Path.ChangeExtension(outputFile, "pdb");
                    emitOptions = emitOptions.WithDebugInformationFormat(DebugInformationFormat.PortablePdb).WithPdbFilePath(pdbFile);
                }

                var emitResult = compilation.Emit(outStream, pdbStream, null, null, null, emitOptions);
                if (emitResult.Success)
                {
                    using (var fs = new FileStream(outputFile, FileMode.Create))
                    {
                        fs.Write(outStream.ToArray());
                    }
                    if(option.OptimizationLevel == OptimizationLevel.Debug)
                    {
                        using (var fs = new FileStream(pdbFile, FileMode.Create))
                        {
                            fs.Write(pdbStream.ToArray());
                        }
                    }
                }
                else
                {

                }
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

        public static void CompilerCSharpWithArguments(string[] args)
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

            CompilerCSharpCodes(csFiles.ToArray(), refAssemblyFiles.ToArray(), preprocessorSymbols.ToArray(), outputFile, pdbFile, option);
        }
    }
}