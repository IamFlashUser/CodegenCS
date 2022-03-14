﻿using CodegenCS.POCO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SampleSourceGenerator
{
    [Generator]
    public class MyGenerator : ISourceGenerator
    {
        #region Errors/Warnings
        private static readonly DiagnosticDescriptor InvalidJsonSchemaWarning =
            new DiagnosticDescriptor(id: "MYSOURCEGEN001",
                                    title: "Couldn't parse JSON schema file",
                                    messageFormat: "Couldn't parse JSON schema file '{0}'",
                                    category: "MyXmlGenerator",
                                    DiagnosticSeverity.Warning,
                                    isEnabledByDefault: true);
        #endregion

        public void Initialize(GeneratorInitializationContext context)
        {
            // Registers a syntax receiver that will "visit" all nodes in the compilation,
            // and can "decide" which classes/methods/etc will be used (augmented) with code generation
            context.RegisterForSyntaxNotifications(() => new MySyntaxReceiver());
            System.Diagnostics.Debug.WriteLine("MyGenerator: Initialize");
        }

        public void Execute(GeneratorExecutionContext context)
        {
            System.Diagnostics.Debug.WriteLine("MyGenerator: Execute");
            MySyntaxReceiver syntaxReceiver = context.SyntaxReceiver as MySyntaxReceiver;
            if (syntaxReceiver == null)
                return;

            // This is how you would add a hardcoded source to the output
            //string source = "public class etc.. ";
            //context.AddSource("Product.cs", source);

            GeneratePOCOs(context);

            //GeneratePartialMethods(context, syntaxReceiver); // nothing useful here, just some tests with partial methods...
        }

        #region GeneratePOCOs
        internal void GeneratePOCOs(GeneratorExecutionContext context)
        {
            IEnumerable<AdditionalText> schemaFiles = context.AdditionalFiles.Where(at => at.Path.EndsWith("Schema.json", StringComparison.OrdinalIgnoreCase));
            foreach (AdditionalText schemaFile in schemaFiles)
            {
                //context.AddSource($"{schemaFile.Path}.cs", $"{schemaFile.Path}");
                //string text = schemaFile.GetText(context.CancellationToken).ToString();

                var generator = new SimplePOCOGenerator();
                generator.InputJsonSchema = schemaFile.Path;
                //generator.TargetFolder = argsParser["targetFolder"];
                generator.Namespace = "CodegenCS.AdventureWorksPOCOSample";
                generator.SingleFile = true;
                try
                {
                    generator.Generate();
                }
                catch (Newtonsoft.Json.JsonSerializationException)
                {
                    context.ReportDiagnostic(Diagnostic.Create(InvalidJsonSchemaWarning, Location.None, schemaFile.Path));
                    continue;
                }
                foreach (var file in generator._generatorContext.OutputFiles)
                    context.AddSource($"{Guid.NewGuid().ToString()}.cs", SourceText.From(file.GetContents(), Encoding.UTF8));
            }
        }
        #endregion

        #region GeneratePartialMethods
        internal void GeneratePartialMethods(GeneratorExecutionContext context, MySyntaxReceiver syntaxReceiver)
        {
            foreach (var c in syntaxReceiver.ClassesToProcess)
            {
                var ns = c.GetTypeName(TypeDeclarationSyntaxExtensions.TypeNameFormat.ONLY_NAMESPACE);
                var typeName = c.GetTypeName(TypeDeclarationSyntaxExtensions.TypeNameFormat.ONLY_TYPE); // Identifier not enough since class might be nested inside outer class

                //typeName += context.SyntaxContextReceiver.GetType().FullName;
                string x = context.SyntaxReceiver?.GetType().FullName.Replace(".", "_") ?? "SyntaxContextReceiver null";

                context.AddSource($"PartialMethods{c.Identifier}.cs", SourceText.From($@"

using System;

namespace {ns}
{{
    partial class {typeName}
    {{
        [System.ComponentModel.Description(""{x}"")]
        static partial void SampleGeneratedMethod() {{ Console.WriteLine(""This is a partial method generated by Source Generator""); }}
    }}
}}", Encoding.UTF8));
            }
        }
        #endregion



    }
}