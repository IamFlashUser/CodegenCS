﻿using CodegenCS.Runtime;
using CodegenCS.VisualStudio.Shared.RunTemplate;
using CodegenCS.VisualStudio.Shared.Utils;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CodegenCS.VisualStudio.Shared.CustomToolGenerator
{
    /// <summary>
    /// This CustomTool is invoked not only by the custom tool (auto executed or manually executed) but also by the custom Menu/Command
    /// So the File Extension doesn't matter (CSX, CGCS, or CS which can be set to Compile Remove to avoid breaking the owner csproj)
    /// The custom tool can also be lenient if the template did NOT implement the expected interfaces - it can search for different entrypoints/signatures and inject whatever is needed.
    /// </summary>
    [Guid("CA74B7A2-1AFE-4503-B4D7-67207DE213FD")]
    public sealed class CodegenCSCodeGenerator : BaseCodeGeneratorWithSite
    {
        static CodegenCSCodeGenerator()
        {
            AssemblyLoaderInitialization.Initialize();
        }

        public const string CustomToolName = "CodegenCS"; // this name is registered by [ProvideCodeGenerator] (so Custom Tool in Properties should be set to "CodegenCS")

        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            ProjectItem templateItem = GetProjectItem();
            string templateItemPath = templateItem.FileNames[0];
            //Solution solution = templateItem.DTE.Solution;
            var solution = (IVsSolution)Package.GetGlobalService(typeof(IVsSolution));
            var dte = (DTE2)templateItem.DTE;
            var package = (AsyncPackage)Package.GetGlobalService(typeof(AsyncPackage));
            Project project = templateItem.ContainingProject;
            string templateDir = new FileInfo(templateItemPath).Directory.FullName; // Path.GetDirectoryName(project.FullName) + "\\";
            //inputFileName is same as projectItem.Properties.Item("FullPath").Value.ToString() or projectItem.FileNames[1]
            var projectUniqueName = project.FileName;
            IVsHierarchy hierarchyItem; // error tasks need to be associated to a project 
            solution.GetProjectOfUniqueName(projectUniqueName, out hierarchyItem);

            RunTemplateWrapper.Init();
            RunTemplateWrapper._customPane.Clear();
            //RunTemplateWrapper._customPane.Activate();

            string solutionPath; solution.GetSolutionInfo(out _, out solutionPath, out _);
            string projectPath = project.FullName;
            var executionContext = new VSExecutionContext(templateItemPath, projectPath, solutionPath);
            var runTemplateWrapper = new RunTemplateWrapper(dte, ThreadHelper.JoinableTaskFactory, base.SiteServiceProvider, templateItem, templateItemPath, templateDir, templateDir, hierarchyItem, executionContext);
            ThreadHelper.JoinableTaskFactory.Run(() => runTemplateWrapper.RunAsync());

            // if GenerateCode returns null then int IVsSingleFileGenerator.Generate returns VSConstants.E_FAIL and this isn't even called.
            // However, Error List would show a failure. So let's just generate something

            // IVsSingleFileGenerator expects to generate a single file (and all we can change is the extension).
            // Since we're generating multiple files we just have to generate something, just to avoid an error shown in Error List
            // if we returned null here then IVsSingleFileGenerator.Generate would return VSConstants.E_FAIL and GetDefaultExtension wouldn't even be called.

            string log =
                $"Generated by CodegenCS Custom Tool at {{DateTime.Now.ToString()}}." + Environment.NewLine +
                $"For more info please check out https://github.com/CodegenCS/CodegenCS";

            return Encoding.UTF8.GetBytes(log);
        }
        protected override string GetDefaultExtension() => ".log";

    }
}
