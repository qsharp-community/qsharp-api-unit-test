using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using QsharpCommunity.QsCompiler.Transformations;
using Microsoft.CodeAnalysis;
using Microsoft.Quantum.QsCompiler;
using Microsoft.Quantum.QsCompiler.CompilationBuilder;
using Microsoft.Quantum.QsCompiler.DataTypes;
using Microsoft.Quantum.QsCompiler.SyntaxTree;
using Microsoft.Quantum.QsCompiler.Transformations;
using Microsoft.Quantum.QsCompiler.Transformations.BasicTransformations;
using VS = Microsoft.VisualStudio.LanguageServer.Protocol;
using Constants = Microsoft.Quantum.QsCompiler.ReservedKeywords.AssemblyConstants;


namespace QsharpCommunity.QsCompiler.Extensions.DocsToTests
{
    public class DocsToTests : IRewriteStep
    {
        private const string DefaultNamespaceName = "QsharpCommunity.CompilerExtensions.DocsToTests";
        private const string CodeSource = "__GeneratedSourceForDocsToTests__.g.qs";
        private const string ReferenceSource = "__GeneratedReferencesForDocsToTests__.g.dll";

        private static readonly IEnumerable<string> OpenedForTesting = new[] {
            BuiltIn.CoreNamespace.Value,
            BuiltIn.IntrinsicNamespace.Value,
            BuiltIn.DiagnosticsNamespace.Value,
            BuiltIn.CanonNamespace.Value,
            BuiltIn.StandardArrayNamespace.Value
        }.ToImmutableArray();

        private readonly List<IRewriteStep.Diagnostic> Diagnostics =
            new List<IRewriteStep.Diagnostic>();

        private static readonly FilterBySourceFile FilterSourceFiles = 
            new FilterBySourceFile(source => source.Value.EndsWith(".qs"));

        private static bool ContainsNamespace(QsCompilation compilation, string nsName) =>
            compilation.Namespaces.Any(ns => ns.Name.Value == nsName);

        private FileContentManager InitializeFileManager(IEnumerable<string> examples, QsCompilation compilation, string nsName = null)
        {
            var (pre, post) = ($"namespace {nsName ?? DefaultNamespaceName}{{ {Environment.NewLine}", $"{Environment.NewLine}}}");
            var openDirs = String.Join(Environment.NewLine, 
                OpenedForTesting
                .Where(nsName => ContainsNamespace(compilation, nsName))
                .Select(nsName => $"open {nsName};"));

            string WrapInNamespace(string example) => pre + openDirs + example + post;
            examples = examples.Where(ex => !String.IsNullOrWhiteSpace(ex));
            var sourceCode = String.Join(Environment.NewLine, examples.Select(WrapInNamespace));

            var sourceName = NonNullable<string>.New(Path.GetFullPath($"{nsName}{CodeSource}"));
            return CompilationUnitManager.TryGetUri(sourceName, out var sourceUri)
                ? CompilationUnitManager.InitializeFileManager(sourceUri, sourceCode)
                : null;
        }

        // interface properties

        public string Name => "DocsToTests";
        public int Priority => 0; // doesn't matter

        public IDictionary<string, string> AssemblyConstants => null;
        public IEnumerable<IRewriteStep.Diagnostic> GeneratedDiagnostics =>
            this.Diagnostics;

        public bool ImplementsPreconditionVerification => false;
        public bool ImplementsTransformation => true;
        public bool ImplementsPostconditionVerification => false;

        // interface methods

        public bool Transformation(QsCompilation compilation, out QsCompilation transformed)
        {
            transformed = FilterSourceFiles.Apply(compilation);
            var manager = new CompilationUnitManager();

            // get source code from examples

            var fileManagers = ExamplesInDocs.Extract(transformed)
                .Select(g => InitializeFileManager(g, compilation, g.Key))
                .Where(m => m != null).ToImmutableHashSet();
            manager.AddOrUpdateSourceFilesAsync(fileManagers, suppressVerification: true);
            var sourceFiles = fileManagers.Select(m => m.FileName).ToImmutableHashSet();
            bool IsGeneratedSourceFile(NonNullable<string> source) => sourceFiles.Contains(source);

            // get everything contained in the compilation as references

            var refName = NonNullable<string>.New(Path.GetFullPath(ReferenceSource));
            var refHeaders = new References.Headers(refName, DllToQs.Rename(compilation).Namespaces);
            var refDict = new Dictionary<NonNullable<string>, References.Headers>{{ refName, refHeaders }};
            var references = new References(refDict.ToImmutableDictionary());
            manager.UpdateReferencesAsync(references);

            // compile the examples in the doc comments and add any diagnostics to the list of generated diagnostics

            var built = manager.Build();
            var diagnostics = built.Diagnostics();
            this.Diagnostics.AddRange(diagnostics.Select(d => IRewriteStep.Diagnostic.Create(d, IRewriteStep.Stage.Transformation)));
            if (diagnostics.Any(d => d.Severity == VS.DiagnosticSeverity.Error)) return false;

            // add the extracted namespace elements from doc comments to the transformed compilation

            var toBeAdded = built.BuiltCompilation.Namespaces.ToImmutableDictionary(
                ns => ns.Name,
                ns => FilterBySourceFile.Apply(ns, IsGeneratedSourceFile));
            var namespaces = compilation.Namespaces.Select(ns =>
                toBeAdded.TryGetValue(ns.Name, out var add)
                ? new QsNamespace(ns.Name, ns.Elements.AddRange(add.Elements), ns.Documentation)
                : ns);
            var addedNamespaces = toBeAdded.Values.Where(add => !compilation.Namespaces.Any(ns => ns.Name.Value == add.Name.Value));
            transformed = new QsCompilation(namespaces.Concat(addedNamespaces).ToImmutableArray(), compilation.EntryPoints);

            // mark all newly created callables that take unit as argument as unit tests to run on the QuantumSimulator and ResourcesEstimator

            bool IsSuitableForUnitTest(QsCallable c) => IsGeneratedSourceFile(c.SourceFile) && c.Signature.ArgumentType.Resolution.IsUnitType;
            var qsimAtt = AttributeUtils.BuildAttribute(BuiltIn.Test.FullName, AttributeUtils.StringArgument(Constants.QuantumSimulator));
            var restAtt = AttributeUtils.BuildAttribute(BuiltIn.Test.FullName, AttributeUtils.StringArgument(Constants.ResourcesEstimator));
            transformed = AttributeUtils.AddToCallables(transformed, (qsimAtt, IsSuitableForUnitTest), (restAtt, IsSuitableForUnitTest));
            return true;
        }

        public bool PostconditionVerification(QsCompilation compilation) =>
            throw new NotImplementedException();

        public bool PreconditionVerification(QsCompilation compilation) =>
            throw new NotImplementedException();

    }
}
