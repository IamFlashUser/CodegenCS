﻿using CodegenCS.Utils;
using System;
using System.IO;

namespace CodegenCS.DbSchema.Templates.EFCoreGenerator
{
    public class Program
    {
        private string _commandLine { get; set; }
        public Program(string commandLine)
        {
            _commandLine = commandLine;
        }

        static int Main(string[] args)
        {
            return new Program(System.AppDomain.CurrentDomain.FriendlyName).Run(args);
        }
        public int Run(string[] args)
        { 
            #region Command-Line Arguments
            var argsParser = new CommandLineArgsParser(args);
            if (argsParser["?"] != null || argsParser["help"] != null)
            {
                ShowUsage();
                return 0;
            }

            //string outputJsonSchema = Path.GetFullPath(Path.Combine(GetScriptFolder(), @".\AdventureWorksSchema.json"));
            var options = new EFCoreGeneratorOptions(inputJsonSchema: argsParser["input"]); // this is required, but if not provided EFCoreGeneratorConsoleHelper.GetOptions will ask for it

            if (argsParser.ContainsKey("targetFolder"))
                options.TargetFolder = argsParser["targetFolder"];
            if (argsParser.ContainsKey("namespace"))
                options.EntitiesNamespace = argsParser["namespace"];
            if (argsParser.ContainsKey("dbcontextname"))
                options.ContextName = argsParser["dbcontextname"];

            EFCoreGeneratorConsoleHelper.GetOptions(options); // if mandatory args were not provided, ask in Console
            #endregion

            var generator = new EFCoreGenerator(options);

            #region Adding EFCoreGenerator.csx
            var mainProgram = new CodegenTextWriter();
            mainProgram.WriteLine($@"
                class Program
                {{
                    static void Main()
                    {{
                        //var options = new CodegenCS.DbSchema.Templates.EFCoreGenerator.EFCoreGeneratorOptions(inputJsonSchema: @""{options.InputJsonSchema}"");
                        var options = Newtonsoft.Json.JsonConvert.DeserializeObject<CodegenCS.DbSchema.Templates.EFCoreGenerator.EFCoreGeneratorOptions>(@""
                            {Newtonsoft.Json.JsonConvert.SerializeObject(options, Newtonsoft.Json.Formatting.Indented).Replace("\"", "\"\"")}
                        "");
                        var generator = new CodegenCS.DbSchema.Templates.EFCoreGenerator.EFCoreGenerator(options);
                        generator.Generate();
                        generator.Save();
                    }}
                }}
            ");
            // Export CS template (for customization)
            // Save with CSX extension so that it doesn't interfere with other existing CSPROJs (which by default include *.cs)
            generator.GeneratorContext["EFCoreGenerator.csx"].WriteLine(
                "//This file is supposed to be launched using: codegencs run EFCoreGenerator.csx" + Environment.NewLine
                + new StreamReader(typeof(EFCoreGenerator).Assembly.GetManifestResourceStream(typeof(EFCoreGenerator).FullName + ".cs")).ReadToEnd() + Environment.NewLine
                + mainProgram.ToString()
            );
            #endregion

            generator.Generate();
            generator.Save();
            var previousColor = Console.ForegroundColor; Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("To regenerate the outputs use \"codegencs run EFCoreGenerator.csx\". Modify the csx file to customize the template output.");
            Console.ForegroundColor = previousColor;
            return 0;
        }

        void ShowUsage()
        {
            Console.WriteLine(string.Format("Usage: {0} [/input=jsonschema] [/targetfolder=folder] [/namespace=namespace] [/dbcontextname=contextname]", _commandLine));
        }

    }
}