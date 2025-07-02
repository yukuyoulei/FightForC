using System;
using UnityEditor;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Text;
using CloverExternal;

public class OnGenerateCSProjectProcessor : AssetPostprocessor
{
    public static string OnGeneratedCSProject(string path, string content)
    {
        if (path.EndsWith("Codes.csproj"))
        {
            content = content.Replace("<Compile Include=\"Assets\\HotfixScripts\\Empty.cs\" />", string.Empty);
            content = content.Replace("<None Include=\"Assets\\HotfixScripts\\Codes.asmdef\" />", string.Empty);
            return GenerateCustomProject(path, content, @"Codes\**\*.cs", true, true);
        }
        foreach (var dll in CodeLoader.ExtraFiles)
        {
            if (path.EndsWith($"{dll}.csproj"))
            {
                content = content.Replace($"<Compile Include=\"Assets\\BaseScripts\\{dll}\\Empty.cs\" />", string.Empty);
                content = content.Replace($"<None Include=\"Assets\\BaseScripts\\{dll}\\{dll}.asmdef\" />", string.Empty);
                return GenerateCustomProject(path, content, $@"{dll}\**\*.cs", false, false);
            }
        }

        return content;
    }

    private static string GenerateCustomProject(string path, string content, string codesPath
        , bool withEditorConfig
        , bool withAnalyzer)
    {
        Debug.Log($"GenerateCustomProject {path}");
        int startIdx = content.IndexOf("<OutputPath>", StringComparison.Ordinal);
        int endIdx = content.IndexOf("</OutputPath>", StringComparison.Ordinal);
        string betweenTags = content.Substring(startIdx, endIdx - startIdx);
        content = content.Replace(betweenTags, "<OutputPath>Temp\\Bin\\Debug\\");
        content = content.Replace("<DebugType>full<", "<DebugType>portable<");

        XmlDocument doc = new XmlDocument();
        doc.LoadXml(content);

        var newDoc = doc.Clone() as XmlDocument;

        var rootNode = newDoc.GetElementsByTagName("Project")[0];

        var elementNewPropertyGroup = newDoc.CreateElement("PropertyGroup", newDoc.DocumentElement.NamespaceURI);
        var elementPostBuildEvent = newDoc.CreateElement("PostBuildEvent", elementNewPropertyGroup.NamespaceURI);
        elementNewPropertyGroup.AppendChild(elementPostBuildEvent);
        elementPostBuildEvent.InnerText = @"echo f| xcopy /r /y $(TargetDir)$(ProjectName).dll $(TargetDir)..\..\..\Library\ScriptAssemblies\$(ProjectName).dll
echo f| xcopy /r /y $(TargetDir)$(ProjectName).pdb $(TargetDir)..\..\..\Library\ScriptAssemblies\$(ProjectName).pdb";
        rootNode.AppendChild(elementNewPropertyGroup);

        var itemGroup = newDoc.CreateElement("ItemGroup", newDoc.DocumentElement.NamespaceURI);
        var compile = newDoc.CreateElement("Compile", newDoc.DocumentElement.NamespaceURI);
        compile.SetAttribute("Include", codesPath);
        itemGroup.AppendChild(compile);
        rootNode.AppendChild(itemGroup);
        if (withEditorConfig)
        {
            itemGroup = newDoc.CreateElement("ItemGroup", newDoc.DocumentElement.NamespaceURI);
            compile = newDoc.CreateElement("None", newDoc.DocumentElement.NamespaceURI);
            compile.SetAttribute("Include", @"Codes\.editorconfig");
            itemGroup.AppendChild(compile);
        }

        if (withAnalyzer)
        {
            var projectReference = newDoc.CreateElement("ProjectReference", newDoc.DocumentElement.NamespaceURI);
            projectReference.SetAttribute("Include", @"Analyzers\Analyzer.csproj");
            projectReference.SetAttribute("OutputItemType", @"Analyzer");
            projectReference.SetAttribute("ReferenceOutputAssembly", @"false");
            var project = newDoc.CreateElement("Project", newDoc.DocumentElement.NamespaceURI);
            project.InnerText = @"{d1f2986b-b296-4a2d-8f12-be9f470014c3}";
            projectReference.AppendChild(project);
            var name = newDoc.CreateElement("Name", newDoc.DocumentElement.NamespaceURI);
            name.InnerText = "Analyzer";
            projectReference.AppendChild(project);
            itemGroup.AppendChild(projectReference);
        }

        rootNode.AppendChild(itemGroup);

        using (StringWriter sw = new StringWriter())
        {
            using (XmlTextWriter tx = new XmlTextWriter(sw))
            {
                tx.Formatting = Formatting.Indented;
                newDoc.WriteTo(tx);
                tx.Flush();
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}