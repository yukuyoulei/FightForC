﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public partial class ConfigGen
{
    private const string autoConfigEditorPath = "Codes/ConfigAuto/Configs/";

    [InitializeOnLoadMethod]
    public static void Init()
    {
        if (!Directory.Exists(autoConfigEditorPath))
            return;

        var types = typeof(ConfigGen).Assembly.GetTypes()
            .Where(t => t.Name != "Config_" && t.Name.StartsWith("Config_"));
        foreach (var type in types)
        {
            var path = autoConfigEditorPath + $"{type.Name}.cs";

            //if (type.Name != "Config_Test") continue;

            var str = DoGen(path, type);
            File.WriteAllText(path, str, new System.Text.UTF8Encoding(false));
        }
    }

    private static string DoGen(string path, Type type)
    {
        var obj = Activator.CreateInstance(type);
        var className = type.Name.Split('_', 2)[1];
        classFuncs.Clear();
        classCustomFuncs.Clear();
        classIgnoreFuncs.Clear();
        classEnums.Clear();
        CheckNestedTypes(obj);
        var detailStr = DescObj(obj, type.Name);
        var funcsStr = ClassFuncs();
        var enumStr = ClassEnums();
        var alltext = @"//本文件为自动生成，请不要手动修改
using System;
using System.Collections.Generic;
namespace ConfigAuto
{
";
        alltext += $"{GetTabs(1)}public partial class {type.Name}{Environment.NewLine}{GetTabs(1)}{{";
        alltext += $"{Environment.NewLine}{GetTabs(2)}private static {type.Name} _{className};";
        alltext += $"{Environment.NewLine}{GetTabs(2)}public static {type.Name} {className}";
        alltext += $"{Environment.NewLine}{GetTabs(2)}{{";
        alltext += $"{Environment.NewLine}{GetTabs(3)}get";
        alltext += $"{Environment.NewLine}{GetTabs(3)}{{";
        alltext += $"{Environment.NewLine}{GetTabs(4)}if (_{className} == null)";
        alltext += $"{Environment.NewLine}{GetTabs(5)}_{className} = new()";
        alltext += $"{Environment.NewLine}{GetTabs(5)}{{";
        alltext += detailStr;
        alltext += $"{Environment.NewLine}{GetTabs(5)}}};";
        alltext += $"{Environment.NewLine}{GetTabs(4)}return _{className};";
        alltext += $"{Environment.NewLine}{GetTabs(3)}}}";
        alltext += $"{Environment.NewLine}{GetTabs(2)}}}";
        alltext += Environment.NewLine;
        alltext += funcsStr;
        if (!string.IsNullOrEmpty(enumStr))
        {
            alltext += Environment.NewLine;
            alltext += enumStr;
        }
        alltext += @"
    }
}";
        //Debug.Log($"====== DoGen ======" +
        //    $"{Environment.NewLine}" +
        //    $"{alltext}" +
        //    $"{Environment.NewLine}" +
        //    $"====== End ======");
        return alltext;
    }

    private static List<string> ignoreClass = new() { "items", "baoxiang", "selectedblindbox" };
    private static string ClassFuncs()
    {
        var str = "";
        foreach (var kv in classFuncs)
        {
            if (ignoreClass.Contains(kv.Key))
                continue;
            var isroot = kv.Key.StartsWith("Config_");
            var classname = kv.Key;
            if (!isroot)
            {
                if (classname == "data")
                    classname = $"Root{classname}";
                str += $"{GetTabs(2)}public partial class {classname}{Environment.NewLine}";
                str += $"{GetTabs(2)}{{{Environment.NewLine}";
            }
            foreach (var v in kv.Value)
            {
                str += $"{GetTabs(isroot ? 2 : 3)}{v}{Environment.NewLine}";
            }
            if (classCustomFuncs.TryGetValue(classname, out var customFuncs))
            {
                foreach (var v in customFuncs)
                {
                    str += $"{GetTabs(isroot ? 2 : 3)}{v}{Environment.NewLine}";
                }
            }
            if (!isroot)
                str += $"{GetTabs(2)}}}{Environment.NewLine}";
        }
        return str;
    }

    private static string ClassEnums()
    {
        var str = "";
        foreach (var kv in classEnums)
        {
            var classname = kv.Key;
            if (classname == "data")
                classname = $"Root{classname}";
            str += $"{GetTabs(2)}public enum {classname}{Environment.NewLine}";
            str += $"{GetTabs(2)}{{{Environment.NewLine}";

            foreach (var v in kv.Value)
            {
                str += $"{GetTabs(3)}{v}{Environment.NewLine}";
            }
            str += $"{GetTabs(2)}}}{Environment.NewLine}";
        }
        return str;
    }

    private static string GetTabs(int count)
    {
        if (count == 0)
            return string.Empty;
        var res = "";
        for (var i = 0; i < count; i++)
            res += "\t";
        return res;
    }
    enum ETreeType
    {
        ENormal,
        EFromArray,
        EFromObj,
        EValueTuple,
        EFromDic,
    }
    private static Dictionary<string, List<string>> classFuncs = new();
    private static Dictionary<string, List<string>> classCustomFuncs = new();
    private static Dictionary<string, List<string>> classIgnoreFuncs = new();
    private static Dictionary<string, List<string>> classEnums = new();

    private static void CheckNestedTypes(object obj)
    {
        var type = obj.GetType();
        var nestedTypes = type.GetNestedTypes();
        foreach (var nestedType in nestedTypes)
        {
            var typeName = nestedType.Name;
            if (nestedType.IsEnum)
            {
                if (!classEnums.TryGetValue(typeName, out var enumList))
                {
                    enumList = new();
                    classEnums[typeName] = enumList;
                }
                var names = Enum.GetNames(nestedType);
                var values = (int[])Enum.GetValues(nestedType);
                for (int i = 0; i < names.Length; i++)
                {
                    enumList.Add($"{names[i]} = {values[i]},");
                }
            }
            else if (nestedType.IsClass)
            {
                if (!classCustomFuncs.TryGetValue(typeName, out var funcList))
                {
                    funcList = new List<string>();
                    classCustomFuncs[typeName] = funcList;
                }
                if (!classIgnoreFuncs.TryGetValue(typeName, out var funcIgnoreList))
                {
                    funcIgnoreList = new List<string>();
                    classIgnoreFuncs[typeName] = funcIgnoreList;
                }

                var temp = Activator.CreateInstance(nestedType);
                var properties = nestedType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var property in properties)
                {
                    var vnames = property.Name.Split(new char[] { '<', '>' });
                    var vname = vnames.Length > 1 ? vnames[1] : vnames[0];
                    var funcs = $"public {GetRealType(property.PropertyType)} {vname} {{get;set;}}";
                    if (property.PropertyType.IsValueType)
                    {
                        var defaultValue = property.GetValue(temp);
                        if (defaultValue != Activator.CreateInstance(property.PropertyType))
                        {
                            funcs += $" = {GetRealValue(defaultValue)};";
                        }
                    }
                    funcList.Add(funcs);
                    funcIgnoreList.Add(vname);
                }
            }
        }
    }

    private static string DescObj(object obj, string className, int classDeep = 6, ETreeType treeType = ETreeType.ENormal)
    {
        var str = "";
        var type = obj.GetType();
        if (!classFuncs.TryGetValue(className, out var list))
        {
            list = new List<string>();
            classFuncs[className] = list;
        }
        classIgnoreFuncs.TryGetValue(className == "data" ? $"Root{className}" : className, out var ignoreList);

        var fs = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (var f in fs)
        {
            var vnames = f.Name.Split(new char[] { '<', '>' });
            var vname = vnames.Length > 1 ? vnames[1] : vnames[0];
            var fvalue = f.GetValue(obj);
            var vtype = fvalue.GetType();
            var realtype = GetRealType(fvalue, vname);
            if (realtype == "data")
                realtype = $"Root{realtype}";
            if (string.IsNullOrEmpty(realtype))
                continue;
            if (ignoreList == null || !ignoreList.Contains(vname))
            {
                var funcs = $"public {realtype} {vname} {{get;set;}}";
                var fallback = fallbackFloatType(realtype, vname, list);
                if (!string.IsNullOrEmpty(fallback))
                    funcs = $"public {fallback} {vname} {{get;set;}}";
                else
                {
                    fallback = advanceInt32Type(realtype, vname, list);
                    if (!string.IsNullOrEmpty(fallback))
                        funcs = $"public {fallback} {vname} {{get;set;}}";
                }

                if (!list.Contains(funcs))
                    list.Add(funcs);
            }
            str += GetObjectStr(fvalue, vname, classDeep, ETreeType.EFromObj);
        }

        if (classDeep > 6 || treeType == ETreeType.EFromObj)
        {
            classDeep--;
            str += Environment.NewLine + GetTabs(classDeep) + "}";
        }
        return str;
    }
    private static string advanceInt32Type(string realtype, string vname, List<string> list)
    {
        if (realtype == "Int64")
        {
            var lremove = new List<string>();
            foreach (var v in list)
            {
                if (v.Contains(" Int32 " + vname))
                    lremove.Add(v);
            }
            foreach (var v in lremove)
                list.Remove(v);
        }
        else if (realtype == "Int32")
        {
            foreach (var v in list)
            {
                if (v.Contains(" Int64 " + vname))
                    return "Int64";
            }
        }
        return "";
    }
    private static string fallbackFloatType(string realtype, string vname, List<string> list)
    {
        if (realtype == "float")
        {
            var lremove = new List<string>();
            foreach (var v in list)
            {
                if (v.Contains(" Int32 " + vname))
                    lremove.Add(v);
            }
            foreach (var v in lremove)
                list.Remove(v);
        }
        else if (realtype == "Int32")
        {
            foreach (var v in list)
            {
                if (v.Contains(" float " + vname))
                    return "float";
            }
        }
        return "";
    }

    private static string GetObjectStr(object fvalue, string vname, int classDeep, ETreeType treeType = ETreeType.ENormal)
    {
        var str = "";
        var vtype = fvalue.GetType();
        var fname = vtype.Name;
        if (fname.Contains("AnonymousType"))
        {
            if (treeType != ETreeType.ENormal)
                str += Environment.NewLine + GetTabs(classDeep);
            if (treeType != ETreeType.EFromArray)
                str += vname + " = ";
            str += "new()"
                + Environment.NewLine + GetTabs(classDeep) + "{";
            str += DescObj(fvalue, vname, classDeep + 1, ETreeType.EFromObj);
            if (treeType != ETreeType.ENormal)
            {
                return str + ",";
            }
        }
        else if (fname.Contains("Dictionary"))
        {
            var dic = GetReflexDictionaryKeyValueType(fvalue);
            var keytype = "";
            var valuetype = "";
            foreach (var kv in dic)
            {
                keytype = GetRealType(kv.Key, vname);
                valuetype = GetRealType(kv.Value, vname);
                break;
            }
            str += $"{Environment.NewLine}{GetTabs(classDeep)}{vname} = new Dictionary<{keytype}, {valuetype}>{Environment.NewLine}{GetTabs(classDeep)}{{" + Environment.NewLine;
            foreach (var kv in dic)
            {
                str += $"{GetTabs(classDeep + 1)}{{{GetRealValue(kv.Key)}, ";
                var kvvtype = kv.Value.GetType();
                if (kvvtype.Name.Contains("AnonymousType"))
                {
                    str += $"new {valuetype} {{";
                    str += DescObj(kv.Value, vname, classDeep + 2, ETreeType.EFromObj);
                    str += "}," + Environment.NewLine;
                }
                else if (kvvtype.IsArray)
                    str += GetObjectStr(kv.Value, vname, classDeep + 2, ETreeType.EFromDic) + Environment.NewLine + GetTabs(classDeep + 1) + "}," + Environment.NewLine;
                else
                    str += $"{GetRealValue(kv.Value)}}},{Environment.NewLine}";
            }
            str += GetTabs(classDeep) + "},";
        }
        else if (vtype.IsArray || fvalue is objectarr)
        {
            var isobjectarr = fvalue is objectarr;

            Array avalues;
            if (isobjectarr)
                avalues = (fvalue as objectarr).objects;
            else
                avalues = fvalue as Array;

            str += $"{Environment.NewLine}{GetTabs(classDeep)}";
            if (treeType != ETreeType.EFromArray && treeType != ETreeType.EFromDic)
                str += $"{vname} = ";

            if (isobjectarr)
                str += $"new object[]";
            else
                str += $"new()";

            str += $"{Environment.NewLine}{GetTabs(classDeep)}{{";
            str += Environment.NewLine;
            foreach (var v in avalues)
            {
                var atype = v.GetType();
                if (!atype.IsArray && !atype.Name.Contains("AnonymousType") && !atype.Name.StartsWith("ValueTuple"))
                    str += GetTabs(classDeep + 1);
                break;
            }
            foreach (var v in avalues)
            {
                str += $"{GetObjectStr(v, vname, classDeep + 1, ETreeType.EFromArray)}";
            }
            str += $"{Environment.NewLine}{GetTabs(classDeep)}}}";
            if (treeType != ETreeType.EFromDic)
                str += ",";
        }
        else
        {
            if (vtype.Name.StartsWith("ValueTuple"))
            {
                str += $"{Environment.NewLine}{GetTabs(classDeep)}new()";
                str += $"{Environment.NewLine}{GetTabs(classDeep)}{{";
                str += DescObj(fvalue, vname, classDeep + 1, ETreeType.EValueTuple);
                str += ",";
            }
            else if (vtype.IsEnum)
                str += Environment.NewLine + GetTabs(classDeep) + vname + " = " + GetRealValue(fvalue) + ",";
            else if (classDeep == 1)
                str += Environment.NewLine + GetTabs(classDeep) + "public " + fname + " " + vname + " = " + GetRealValue(fvalue) + ";";
            else if (treeType == ETreeType.EFromArray)
                str += GetRealValue(fvalue) + ",";
            else if (treeType == ETreeType.EFromObj)
                str += Environment.NewLine + GetTabs(classDeep) + vname + " = " + GetRealValue(fvalue) + ",";
            else if (treeType == ETreeType.EValueTuple)
                str += Environment.NewLine + GetTabs(classDeep) + vname + " = " + GetRealValue(fvalue) + ",";
            else
                str += Environment.NewLine + GetTabs(classDeep) + vname + " = " + GetRealValue(fvalue) + ";";
        }
        return str;
    }
    static Dictionary<object, object> GetReflexDictionaryKeyValueType(object dictionaryVal)
    {
        Dictionary<object, object> result = new Dictionary<object, object>();
        var count = 0;
        Type type = dictionaryVal.GetType();
        bool b = type.IsGenericType;
        Type IDictionaryType = type.GetInterface("IDictionary", false);
        if (b && IDictionaryType != null)
        {
            //获取集合属性的值的属性
            PropertyInfo countPro = type.GetProperty("Count");
            PropertyInfo keyPro = type.GetProperty("Keys");
            PropertyInfo ValuesPro = type.GetProperty("Values");

            //获取对应的值
            count = (int)countPro.GetValue(dictionaryVal);
            object objKey = keyPro.GetValue(dictionaryVal);
            object objValue = ValuesPro.GetValue(dictionaryVal);

            //获取索引Get方法
            MethodInfo keyGet = objKey.GetType().GetMethod("GetEnumerator", BindingFlags.Instance | BindingFlags.Public);
            MethodInfo valuesGet = objValue.GetType().GetMethod("GetEnumerator", BindingFlags.Instance | BindingFlags.Public);
            var keys = keyGet.Invoke(objKey, null) as IEnumerator;
            var values = valuesGet.Invoke(objValue, null) as IEnumerator;
            keys.MoveNext();
            values.MoveNext();
            for (int i = 0; i < count; i++)
            {
                result.Add(keys.Current, values.Current);
                keys.MoveNext();
                values.MoveNext();
            }
        }

        return result;
    }
    private static string GetRealValue(object fvalue)
    {
        var vtype = fvalue.GetType();
        if (vtype == typeof(string))
            return $@"@""{fvalue}""";
        if (vtype == typeof(bool))
            return fvalue.ToString().ToLower();
        if (vtype == typeof(Double) || vtype == typeof(Single))
            return fvalue + "f";
        if (vtype.IsEnum)
            return $"{vtype.Name}.{fvalue}";
        return fvalue.ToString();
    }

    private static string GetRealType(Type type)
    {
        if (type.IsGenericType)
        {
            var str = type.Name.Split('`')[0];
            var genericTypes = type.GetGenericArguments();
            return $"{str}<{string.Join(", ", genericTypes.Select((t) => GetRealType(t)))}>";
        }
        return type.Name;
    }

    private static string GetRealType(object obj, string className)
    {
        if (obj is Array avalues)
            foreach (var v in avalues)
            {
                var vtype = v.GetType();
                var fname = vtype.Name;
                if (fname.Contains("AnonymousType"))
                    return $"List<{className}>";
                else if (vtype.IsArray)
                    return $"List<{GetRealType(v, className)}>";
                else if (fname.StartsWith("ValueTuple"))
                    return $"List<{className}>";
                else if (fname.StartsWith("Dictionary"))
                {
                    var keytype = "";
                    var valuetype = "";
                    var dic = GetReflexDictionaryKeyValueType(obj);
                    foreach (var kv in dic)
                    {
                        keytype = GetRealType(kv.Key, className);
                        valuetype = GetRealType(kv.Value, className);
                        break;
                    }
                    return $"Dictionary<{keytype}, {valuetype}>";
                }
                else if (fname.Equals("double", StringComparison.CurrentCultureIgnoreCase))
                    return $"List<float>";
                else
                    return $"List<{vtype.Name}>";
            }
        else
        {
            var vtype = obj.GetType();
            var fname = vtype.Name;
            if (fname.Contains("AnonymousType"))
                return className;
            else if (obj is objectarr)
                return "object[]";
            else if (fname.StartsWith("ValueTuple"))
                return className;
            else if (fname.StartsWith("Dictionary"))
            {
                var keytype = "";
                var valuetype = "";
                var dic = GetReflexDictionaryKeyValueType(obj);
                foreach (var kv in dic)
                {
                    keytype = GetRealType(kv.Key, className);
                    valuetype = GetRealType(kv.Value, className);
                    break;
                }
                return $"Dictionary<{keytype}, {valuetype}>";
            }
            else if (fname.Equals("double", StringComparison.CurrentCultureIgnoreCase))
                return "float";
            else if (vtype.IsEnum)
                return vtype.Name;
            else
                return vtype.Name;
        }
        return null;
    }
}

public class objectarr
{
    public object[] objects;

    public objectarr(params object[] objects)
    {
        this.objects = objects;
    }
}
