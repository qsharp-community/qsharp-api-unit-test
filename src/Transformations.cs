using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.Quantum.QsCompiler.DataTypes;
using Microsoft.Quantum.QsCompiler.Documentation;
using Microsoft.Quantum.QsCompiler.SyntaxTree;
using Core = Microsoft.Quantum.QsCompiler.Transformations.Core;


namespace QsharpCommunity.QsCompiler.Transformations
{
    using CodeSnippets = List<(string, string)>;

    public class ExamplesInDocs 
    : Core.SyntaxTreeTransformation<CodeSnippets>
    {
        /// <summary>
        /// Returns a look-up with all Q# examples extracted from doc comments for each namespace.
        /// The key may be null if the extracted example is specified outside a namespace.
        /// </summary>
        public static ILookup<string, string> Extract(QsCompilation compilation)
        {
            var instance = new ExamplesInDocs();
            instance.Apply(compilation);
            return instance.SharedState.ToLookup(entry => entry.Item1, entry => entry.Item2);
        }

        private ExamplesInDocs() 
        : base(new CodeSnippets(), Core.TransformationOptions.NoRebuild)
        {
            this.Namespaces = new NamespaceTransformation(this);
            this.Statements = new Core.StatementTransformation<CodeSnippets>(this, Core.TransformationOptions.Disabled);
            this.Expressions = new Core.ExpressionTransformation<CodeSnippets>(this, Core.TransformationOptions.Disabled);
            this.Types = new Core.TypeTransformation<CodeSnippets>(this, Core.TransformationOptions.Disabled);
        }

        private class NamespaceTransformation 
        : Core.NamespaceTransformation<CodeSnippets>
        {
            private string CurrentNamespace;

            private T ProcessDocumentation<T>(string nsName, ImmutableArray<string> docs)
            {
                this.CurrentNamespace = nsName;
                this.OnDocumentation(docs);
                this.CurrentNamespace = null;
                return default;
            }

            public NamespaceTransformation(ExamplesInDocs parent) 
            : base(parent, Core.TransformationOptions.NoRebuild)
            { }

            public override QsCustomType OnTypeDeclaration(QsCustomType t) =>
                ProcessDocumentation<QsCustomType>(t.FullName.Namespace.Value, t.Documentation);

            public override QsCallable OnCallableDeclaration(QsCallable c) =>
                ProcessDocumentation<QsCallable>(c.FullName.Namespace.Value, c.Documentation);

            public override QsSpecialization OnSpecializationDeclaration(QsSpecialization s) =>
                ProcessDocumentation<QsSpecialization>(s.Parent.Namespace.Value, s.Documentation);

            public override ImmutableArray<string> OnDocumentation(ImmutableArray<string> doc)
            {
                var example = new DocComment(doc).Example; 
                while (example.Any())
                {
                    example = String.Concat(example
                        .Substring(example.IndexOf("```") + 3)
                        .SkipWhile(c => !Char.IsWhiteSpace(c)));
                    var next = example.IndexOf("```");
                    this.SharedState.Add((CurrentNamespace, example.Substring(0, next)));
                    example = String.Concat(example
                        .Substring(next + 3)
                        .SkipWhile(c => !Char.IsWhiteSpace(c)));
                }
                return doc;
            }
        }
    }

    public class DllToQs 
    : Core.SyntaxTreeTransformation
    {
        private static readonly DllToQs Instance = new DllToQs();
        public static QsCompilation Rename(QsCompilation compilation) => 
            Instance.Apply(compilation);

        private DllToQs() 
        : base()
        {
            this.Namespaces = new NamespaceTransformation(this);
            this.Statements = new Core.StatementTransformation(this, Core.TransformationOptions.Disabled);
            this.Expressions = new Core.ExpressionTransformation(this, Core.TransformationOptions.Disabled);
            this.Types = new Core.TypeTransformation(this, Core.TransformationOptions.Disabled);
        }

        private class NamespaceTransformation : Core.NamespaceTransformation
        {
            public NamespaceTransformation(DllToQs parent) 
            : base(parent)
            { }

            public override NonNullable<string> OnSourceFile(NonNullable<string> f)
            {
                var dir = Path.GetDirectoryName(f.Value);
                var fileName = Path.GetFileNameWithoutExtension(f.Value);
                return NonNullable<string>.New(Path.Combine(dir, fileName + ".qs"));
            }
        }
    }
}