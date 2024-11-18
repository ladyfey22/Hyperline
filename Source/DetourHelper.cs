namespace Celeste.Mod.Hyperline;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;

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

    public static List<AssemblyDetourInfo> GetDetourList(Type type, string methodName, Type[] args)
    {
        MethodBase func = type.GetMethod(
            methodName, // Name of the method,
            args,
            null
        );

        if (func == null)
        {
            return null;
        }

        MethodDetourInfo detours = DetourManager.GetDetourInfo(func);
        return detours.Detours.Select(detour => new AssemblyDetourInfo(detour)).ToList();
    }

    public static string FindDetourIdFromAssembly(string assemblyName, Type type, string methodName, Type[] args)
    {
        List<AssemblyDetourInfo> detours = GetDetourList(type, methodName, args);
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

    public static string DumpDetours(Type type, string methodName, Type[] args)
    {
        List<AssemblyDetourInfo> detours = GetDetourList(type, methodName, args);
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

    public static string[] GenerateDetourList(List<string> assemblyNames, Type type, string methodName, Type[] args)
    {
        List<AssemblyDetourInfo> detours = GetDetourList(type, methodName, args);
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
