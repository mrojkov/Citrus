using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace Tangerine.UI.RemoteScripting
{
	public class CSharpCodeCompiler
	{
		private static readonly string netFrameworkPath = $"{Path.GetDirectoryName(typeof(string).Assembly.Location)}\\";
		public static readonly ImmutableArray<string> DefaultProjectReferences = ImmutableArray.Create(
			$"{netFrameworkPath}mscorlib.dll",
			$"{netFrameworkPath}System.dll",
			$"{netFrameworkPath}System.Core.dll"
		);
		public static readonly ImmutableArray<string> DefaultNamespaces = ImmutableArray.Create(
			"System",
			"System.IO",
			"System.Net",
			"System.Linq",
			"System.Text",
			"System.Text.RegularExpressions",
			"System.Collections.Generic",
			"System.Threading.Tasks"
		);

		public CSharpParseOptions ParseOptions { get; set; } = CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp7_3);
		public IEnumerable<string> ProjectReferences { get; set; } = DefaultProjectReferences;
		public IEnumerable<string> Namespaces { get; set; } = DefaultNamespaces;
		public OutputKind OutputKind { get; set; } = OutputKind.DynamicallyLinkedLibrary;
		public OptimizationLevel OptimizationLevel { get; set; } = OptimizationLevel.Debug;

		public async Task<Result> CompileAssemblyToRawBytesAsync(string assemblyName, IEnumerable<string> csFiles)
		{
			return await Task<Result>.Factory.StartNew(() => CompileAssemblyToRawBytes(assemblyName, csFiles));
		}

		public Result CompileAssemblyToRawBytes(string assemblyName, IEnumerable<string> csFiles)
		{
			using (var memoryStreamForPdb = new MemoryStream()) {
				using (var memoryStream = new MemoryStream()) {
					var emitResult = CompileAssembly(assemblyName, csFiles, memoryStream, memoryStreamForPdb);
					return new Result {
						Success = emitResult.Success,
						AssemblyRawBytes = emitResult.Success ? memoryStream.ToArray() : null,
						PdbRawBytes = emitResult.Success ? memoryStreamForPdb.ToArray() : null,
						Diagnostics = emitResult.Diagnostics
					};
				}
			}
		}

		public EmitResult CompileAssembly(string assemblyName, IEnumerable<string> csFiles, Stream stream, Stream streamForPdb)
		{
			var syntaxTrees = new List<SyntaxTree>();
			foreach (var csFile in csFiles) {
				var source = File.ReadAllText(csFile);
				var syntaxTree = SyntaxFactory.ParseSyntaxTree(source, ParseOptions, csFile, System.Text.Encoding.UTF8);
				syntaxTrees.Add(syntaxTree);
			}
			var references = ProjectReferences.Select(referencePath => MetadataReference.CreateFromFile(referencePath));
			var compilationOptions = new CSharpCompilationOptions(OutputKind)
				.WithOverflowChecks(true)
				.WithOptimizationLevel(OptimizationLevel)
				.WithUsings(Namespaces);
			var compilation = CSharpCompilation.Create(assemblyName, syntaxTrees, references, compilationOptions);
			return compilation.Emit(stream, streamForPdb);
		}

		public class Result
		{
			public bool Success;
			public byte[] AssemblyRawBytes;
			public byte[] PdbRawBytes;
			public ImmutableArray<Diagnostic> Diagnostics;
		}
	}
}
