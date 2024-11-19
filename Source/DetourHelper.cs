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

        public override string ToString() => $"Detour ID {Id}, Assembly {AssemblyName}, Method {Method.Name}";

        public string AssemblyName { get; set; }
        public MethodBase Method { get; set; }
        public string Id { get; set; }
    }

    /// <summary>
    /// Get the MethodInfo for a method in a class.
    /// </summary>
    /// <param name="name">The name of the method to be obtained.</param>
    /// <typeparam name="T">The class to get the method from.</typeparam>
    /// <returns>The found method info, or null if not found.</returns>
    /// <example> <code> MethodInfo method = DetourHelper.GetMethodInfo&lt;PlayerHair&gt;("GetHairColor"); </code> </example>
    public static MethodInfo GetMethodInfo<T>(string name) => GetMethodInfo(typeof(T), name);

    /// <summary>
    /// Get the method info by providing a delegate.
    /// </summary>
    /// <param name="classDelegate">The delegate that calls the method to get MethodInfo from.</param>
    /// <typeparam name="T">The type that is inherited from Delegate.</typeparam>
    /// <returns>The found method info, or null if not found.</returns>
    /// <example> <code> MethodInfo method = DetourHelper.GetMethodInfo&lt;Action&gt;(myDelegate); </code> </example>
    public static MethodInfo GetMethodInfo<T>(T classDelegate) where T : Delegate => classDelegate?.Method;

    /// <summary>
    /// Get the MethodInfo for a method in a class.
    /// </summary>
    /// <param name="classType">The class to get the method from.</param>
    /// <param name="name">The name of the method to be obtained.</param>
    /// <returns>The found method info, or null if not found.</returns>
    /// <example> <code> MethodInfo method = DetourHelper.GetMethodInfo(typeof(PlayerHair), "GetHairColor"); </code> </example>
    public static MethodInfo GetMethodInfo(Type classType, string name)
    {
        MethodInfo returnV = classType.GetMethod(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
        //if it's null, provide some debug info
        if (returnV == null)
        {
            Logger.Log(LogLevel.Warn, "Hyperline", $"DetourHelper.GetMethodInfo<${classType.Name}> Method {name} not found in {classType.Name}");
        }
        return returnV;
    }

    /// <summary>
    /// Create a DetourContext to allow adjusting the order of detours, even when there is no id assigned, using the name of assemblies instead.
    /// </summary>
    /// <param name="method">The method to generate the DetourContext for.</param>
    /// <param name="before">The list of assembly names to run further from the base method (less priority).</param>
    /// <param name="after">The list of assembly names to run closer to the base method (more priority).</param>
    /// <param name="priority"> The priority of the detour. Only ordered on priority after before/after are taken into account.</param>
    /// <example>
    /// <code>
    /// using(DetourHelper.GenerateDetourContext(typeof(PlayerHair).GetMethod("GetHairColor"), new List&lt;string&gt;{"Assembly1", "Assembly2"}, new List&lt;string&gt;{"Assembly3", "Assembly4"}, 100))
    /// {
    ///     //hook code here
    /// }
    /// </code>
    /// </example>
    public static DataScope GenerateDetourContext(MethodInfo method, List<string> before = null,
        List<string> after = null, int? priority = null)
    {
        List<string> beforeIds = before != null ? GenerateDetourList(before, method)?.ToList() : null;
        List<string> afterIds = before != null ? GenerateDetourList(after, method)?.ToList() : null;
        DetourConfigContext returnV = new(new("Hyperline", before: beforeIds, after: afterIds, priority: priority));
        return returnV.Use();
    }

    /// <summary>
    /// Create a DetourContext to allow adjusting the order of detours, even when there is no id assigned, using the name of assemblies instead.
    /// </summary>
    /// <param name="classType">The class to get the method from.</param>
    /// <param name="name">The name of the method to generate the DetourContext for.</param>
    /// <param name="before">The list of assembly names to run further from the base method (less priority).</param>
    /// <param name="after">The list of assembly names to run closer to the base method (more priority).</param>
    /// <param name="priority"> The priority of the detour. Only ordered on priority after before/after are taken into account.</param>
    /// <example>
    /// <code>
    /// using(DetourHelper.GenerateDetourContext(typeof(PlayerHair), "GetHairColor", new List&lt;string&gt;{"Assembly1", "Assembly2"}, new List&lt;string&gt;{"Assembly3", "Assembly4"}, 100))
    /// {
    ///     // hook code here
    /// }
    /// </code>
    /// </example>

    public static DataScope GenerateDetourContext(Type classType, string name, List<string> before = null,
        List<string> after = null, int? priority = null)
    {
        MethodInfo method = GetMethodInfo(classType, name);
        return GenerateDetourContext(method, before, after, priority);
    }

    /// <summary>
    /// Create a DetourContext to allow adjusting the order of detours, even when there is no id assigned, using the name of assemblies instead.
    /// </summary>
    /// <param name="m">The delegate to get the method from.</param>
    /// <param name="before">The list of assembly names to run further from the base method (less priority).</param>
    /// <param name="after">The list of assembly names to run closer to the base method (more priority).</param>
    /// <param name="priority"> The priority of the detour. Only ordered on priority after before/after are taken into account.</param>
    /// <typeparam name="T">The type that is inherited from Delegate.</typeparam>
    /// <example>
    /// <code>
    /// using(DetourHelper.GenerateDetourContext((Action)MyMethod, new List&lt;string&gt;{"Assembly1", "Assembly2"}, new List&lt;string&gt;{"Assembly3", "Assembly4"}, 100))
    /// {
    ///     // hook code here
    /// }
    /// </code>
    /// </example>
    public static DataScope GenerateDetourContext<T>(T m, List<string> before = null,
        List<string> after = null, int? priority = null) where T : Delegate
    {
        MethodInfo method = GetMethodInfo(m);
        return GenerateDetourContext(method, before, after, priority);
    }

    /// <summary>
    /// Create a DetourContext to allow adjusting the order of detours, even when there is no id assigned, using the name of assemblies instead.
    /// </summary>
    /// <param name="methodName">The name of the method to generate the DetourContext for.</param>
    /// <param name="before">The list of assembly names to run further from the base method (less priority).</param>
    /// <param name="after">The list of assembly names to run closer to the base method (more priority).</param>
    /// <param name="priority"> The priority of the detour. Only ordered on priority after before/after are taken into account.</param>
    /// <typeparam name="T">The class to get the method from.</typeparam>
    /// <example>
    /// <code>
    /// using(DetourHelper.GenerateDetourContext&lt;PlayerHair&gt;("GetHairColor", new List&lt;string&gt;{"Assembly1", "Assembly2"}, new List&lt;string&gt;{"Assembly3", "Assembly4"}, 100))
    /// {
    ///     // hook code here
    /// }
    /// </code>
    /// </example>
    public static DataScope GenerateDetourContext<T>(string methodName, List<string> before = null,
        List<string> after = null, int? priority = null)
    {
        MethodInfo method = GetMethodInfo<T>(methodName);
        return GenerateDetourContext(method, before, after, priority);
    }

    /// <summary>
    /// Get the list of detours for a method.
    /// </summary>
    /// <param name="methodInfo">The method to get the detours for.</param>
    /// <returns>The list of detours for the method, or null if the method is null.</returns>
    /// <example>
    /// <code>
    /// List&lt;AssemblyDetourInfo&gt; detours = DetourHelper.GetDetourList(typeof(PlayerHair).GetMethod("GetHairColor"));
    /// </code>
    /// </example>
    public static List<AssemblyDetourInfo> GetDetourList(MethodInfo methodInfo)
    {
        if (methodInfo == null)
        {
            return null;
        }

        MethodDetourInfo detours = DetourManager.GetDetourInfo(methodInfo);
        return detours.Detours.Select(detour => new AssemblyDetourInfo(detour)).ToList();
    }

    /// <summary>
    /// Find the detour id from an assembly name.
    /// </summary>
    /// <param name="assemblyName">The name of the assembly to find the detour id for.</param>
    /// <param name="input">The method to find the detour id for.</param>
    /// <returns>The detour id for the assembly, or null if not found.</returns>
    /// <example>
    /// <code>
    /// string id = DetourHelper.FindDetourIdFromAssembly("Assembly1", typeof(PlayerHair).GetMethod("GetHairColor"));
    /// </code>
    /// </example>
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

    /// <summary>
    /// Dump all detours for a method as a readable string.
    /// </summary>
    /// <param name="methodName">The method to dump the detours for.</param>
    /// <typeparam name="T">The class to get the method from.</typeparam>
    /// <returns>The detours for the method as a readable string, or null if the method is not found.</returns>
    /// <example>
    /// <code>
    /// string detours = DetourHelper.DumpDetours&lt;PlayerHair&gt;("GetHairColor");
    /// </code>
    /// </example>
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

    /// <summary>
    /// Generate a list of detour ids from a list of assembly names.
    /// </summary>
    /// <param name="assemblyNames">The list of assembly names to generate the detour ids for.</param>
    /// <param name="input">The method to generate the detour ids for.</param>
    /// <returns>The list of detour ids for the assembly names, or null if the assembly names are null.</returns>
    /// <example>
    /// <code>
    /// string[] detours = DetourHelper.GenerateDetourList(new List&lt;string&gt;{"Assembly1", "Assembly2"}, typeof(PlayerHair).GetMethod("GetHairColor"));
    /// </code>
    /// </example>
    public static string[] GenerateDetourList(List<string> assemblyNames, MethodInfo input)
    {
        if (assemblyNames == null)
        {
            return null;
        }

        // we want to not include any detours that weren't found
        List<string> returnV = [];
        foreach (string assemblyName in assemblyNames)
        {
            string id = FindDetourIdFromAssembly(assemblyName, input);
            if (id != null)
            {
                returnV.Add(id);
            }
        }
        return returnV.ToArray();
    }
}
