namespace Celeste.Mod.Hyperline;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;

public class DetourHelper
{

    public struct AssemblyDetourInfo
    {
        public AssemblyDetourInfo(){}
        public AssemblyDetourInfo(DetourInfo info)
        {
            if (info.Config != null)
            {
                Id = info.Config.Id;
            }

            if (info.Entry.DeclaringType != null)
            {
                AssemblyName = info.Entry.DeclaringType.Assembly.GetName().Name;
            }

            Method = info.Entry;
        }

        public override string ToString() => $"{Id} : {AssemblyName} : {Method.Name}";

        public string AssemblyName { get; set; }
        public MethodBase Method { get; set; }
        public string Id { get; set; }
    }

    public static MethodInfo GetMethodInfo<T>(string name)
    {
        MethodInfo returnV =typeof(T).GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        //if it's null, provide some debug info
        if (returnV == null)
        {
            Logger.Log(LogLevel.Warn, "Hyperline", $"DetourHelper.GetMethodInfo<${typeof(T).Name}> Method {name} not found in {typeof(T).Name}");
        }
        return returnV;
    }

    public static DataScope GenerateDetourContext<T>(string methodName, List<string> before = null,
        List<string> after = null, int? priority = null)
    {
        MethodInfo method = GetMethodInfo<T>(methodName);

        List<string> beforeIds = before != null ? GenerateDetourList(before, method)?.ToList() : null;
        List<string> afterIds = before != null ? GenerateDetourList(after, method)?.ToList() : null;
        DetourConfigContext returnV = new(new("Hyperline", before: beforeIds, after: afterIds, priority: priority));
        return returnV.Use();
    }

    public static List<AssemblyDetourInfo> GetDetourList(MethodInfo methodInfo)
    {
        if (methodInfo == null)
        {
            return null;
        }

        MethodDetourInfo detours = DetourManager.GetDetourInfo(methodInfo);
        return detours.Detours.Select(detour => new AssemblyDetourInfo(detour)).ToList();
    }

    public static string FindDetourIdFromAssembly(string assemblyName, MethodInfo input)
    {
        List<AssemblyDetourInfo> detours = GetDetourList(input);
        if (detours == null)
        {
            return null;
        }

        foreach (AssemblyDetourInfo assemblyDetourInfo in detours)
        {
            if (assemblyDetourInfo.AssemblyName == assemblyName)
            {
                return assemblyDetourInfo.Id;
            }
        }
        return null;
    }

    public static string DumpDetours<T>(string methodName)
    {
        MethodInfo method = GetMethodInfo<T>(methodName);
        List<AssemblyDetourInfo> detours = GetDetourList(method);
        if (detours == null)
        {
            return null;
        }

        string returnValue = string.Empty;
        foreach (AssemblyDetourInfo assemblyDetourInfo in detours)
        {
            returnValue += assemblyDetourInfo + "\n";
        }

        return returnValue;
    }

    public static string[] GenerateDetourList(List<string> assemblyNames, MethodInfo input)
    {

        if (assemblyNames == null)
        {
            return null;
        }


        List<AssemblyDetourInfo> detours = GetDetourList(input);
        if (detours == null)
        {
            return null;
        }
        List<string> returnValue = [];

        foreach (string t in assemblyNames)
        {
            // find the detour that matches the assembly
            foreach (AssemblyDetourInfo assemblyDetourInfo in detours)
            {
                if (assemblyDetourInfo.AssemblyName == t)
                {
                    returnValue.Add(assemblyDetourInfo.Id);
                }
            }
        }

        return returnValue.ToArray();
    }
}
