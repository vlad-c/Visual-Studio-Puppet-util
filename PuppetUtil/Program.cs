using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace PuppetUtil
{
    class Program
    {
        static DirectoryInfo rootDirInfo;
        static XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
        static XElement rootDoc = new XElement(ns +"Project",
            new XAttribute("DefaultTargets","Build")
        );

        static XElement foldersItemGroup = new XElement(ns + "ItemGroup");
        static XElement filesItemGroup = new XElement(ns +"ItemGroup");

        static string[] excludedExtensions = { ".sln", ".suo", ".ppm" };

        static void AddDirectoryToProject(DirectoryInfo dirInfo)
        {
            foreach (var subDir in dirInfo.GetDirectories())
            {
                if (subDir.Name[0] == '.')
                {

                }
                else
                {
                    foldersItemGroup.Add(new XElement(ns + "Folder", new XAttribute("Include", subDir.FullName.Substring(1 + rootDirInfo.FullName.Length))));
                    AddDirectoryToProject(subDir);
                }
            }
            foreach (var file in dirInfo.GetFiles())
            {
                if (excludedExtensions.Contains(file.Extension))
                {
                }
                else
                {
                    filesItemGroup.Add(new XElement(ns + "None", new XAttribute("Include", file.FullName.Substring(1 + rootDirInfo.FullName.Length))));
                }
            }
        }

        static void Main(string[] args)
        {
            string rootDir = (args.Length > 0) ? args[0] : Directory.GetCurrentDirectory();
            rootDirInfo = new DirectoryInfo(rootDir);

            string projectName = rootDirInfo.Name;
            Guid projectGuid = Guid.NewGuid();
            rootDoc.Add(new XElement(ns + "PropertyGroup",
                new XElement(ns + "Configuration", "Release"),
                new XElement(ns + "SchemaVersion", "2.0"),
                new XElement(ns + "ProjectGuid", projectGuid.ToString()),
                new XElement(ns + "OutputType", "Exe"),
                new XElement(ns + "RootNamespace", projectName),
                new XElement(ns + "AssemblyName", projectName),
                new XElement(ns + "EnableUnmanagedDebugging", "false"),
                new XElement(ns + "Name", projectName),
                new XElement(ns + "PuppetForgeUserName", Environment.UserName),
                new XElement(ns + "PuppetForgeModuleName", projectName),
                new XElement(ns + "PuppetForgeModuleVersion", "0.0.1"),
                new XElement(ns + "PuppetForgeModuleDependency", "'puppetlabs/stdlib', '&gt;=2.2.1'"),
                new XElement(ns + "PuppetForgeModuleSummary", "Summary for project " + projectName),
                new XElement(ns + "PuppetForgeModuleDescription", "Description for project " + projectName)
                )
            );

            rootDoc.Add(foldersItemGroup);
            rootDoc.Add(filesItemGroup);

            AddDirectoryToProject(rootDirInfo);
            Console.WriteLine(rootDoc.ToString());
            DateTime t = DateTime.Now;
            if (File.Exists(projectName + ".ppm"))
            {
                File.Move(projectName + ".ppm", projectName + ".ppm.bak-" + t.ToString("yyyyMMddhhmmss"));
            }
            rootDoc.Save(projectName + ".ppm", SaveOptions.OmitDuplicateNamespaces);
            Console.WriteLine("File " + projectName + ".ppm created");

            if (File.Exists(projectName + ".sln"))
            {
                File.Move(projectName + ".sln", projectName + ".sln.bak-" + t.ToString("yyyyMMddhhmmss"));
            }
            var slnWriter = File.CreateText(projectName + ".sln");
            slnWriter.WriteLine();
            slnWriter.WriteLine("Microsoft Visual Studio Solution File, Format Version 12.00");
            slnWriter.WriteLine("# Visual Studio 2013");
            slnWriter.WriteLine("VisualStudioVersion = 12.0.31101.0");
            slnWriter.WriteLine("MinimumVisualStudioVersion = 10.0.40219.1");
            slnWriter.WriteLine("Project(\"{FD63057A-0BA7-4B6E-9033-09AF317E5532}\") = \"" + projectName + "\", \"" + projectName + ".ppm\", \"" + projectGuid.ToString("B") + "\"");
            slnWriter.WriteLine("EndProject");
            slnWriter.WriteLine("Global");
            slnWriter.WriteLine("	GlobalSection(SolutionProperties) = preSolution");
            slnWriter.WriteLine("		HideSolutionNode = FALSE");
            slnWriter.WriteLine("	EndGlobalSection");
            slnWriter.WriteLine("EndGlobal");
            slnWriter.Close();
        }
    }
}
