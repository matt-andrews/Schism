using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace Schism.Lib.Core.Internal;
public interface IInterfaceEmitter
{
    /// <summary>
    /// Create a new type that inherits from <typeparamref name="TInterface"/> where each method
    /// implements the <paramref name="func"/> parameter
    /// </summary>
    /// <typeparam name="TInterface"></typeparam>
    /// <param name="func"></param>
    /// <returns></returns>
    TInterface CreateType<TInterface>(Func<CallerInfo, Task<object?>> func);
}

internal class InterfaceEmitter : IInterfaceEmitter
{
    private readonly ConcurrentDictionary<string, object> _cache = [];
    public TInterface CreateType<TInterface>(Func<CallerInfo, Task<object?>> func)
    {
        return _cache.TryGetValue(typeof(TInterface).Name, out object? cachedVal) ? (TInterface)cachedVal : CreateTypeImpl<TInterface>(func);
    }
    private TInterface CreateTypeImpl<TInterface>(Func<CallerInfo, Task<object?>> func)
    {
        if (!typeof(TInterface).IsInterface)
        {
            throw new ArgumentException($"{nameof(TInterface)} must be an interface");
        }
        AssemblyBuilder assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("InterfaceEmitterDynamicAssembly"), AssemblyBuilderAccess.Run);
        ModuleBuilder module = assembly.DefineDynamicModule("InterfaceEmitterDynamicModule");
        TypeBuilder type = module.DefineType("InterfaceEmitter_" + typeof(TInterface).Name);
        type.AddInterfaceImplementation(typeof(TInterface));
        Dictionary<string, Func<CallerInfo, object?>> fieldDelegates = [];
        foreach (MethodInfo method in typeof(TInterface).GetMethods())
        {
            ParameterInfo[] parameters = method.GetParameters();
            FieldBuilder field = type.DefineField("_invoker_" + method.Name, func.GetType(), FieldAttributes.Public | FieldAttributes.Static);
            fieldDelegates.Add("_invoker_" + method.Name, func);

            MethodBuilder stub =
              type.DefineMethod(
                  method.Name,
                  MethodAttributes.Public | MethodAttributes.Virtual,
                  method.ReturnType,
                  parameters.Select(s => s.ParameterType).ToArray());

            ILGenerator il = stub.GetILGenerator();

            //Declare local variable of type CallerInfo
            LocalBuilder callerInfoLocalBuilder = il.DeclareLocal(typeof(CallerInfo));
            il.Emit(OpCodes.Newobj, typeof(CallerInfo).GetConstructor([])!);
            il.Emit(OpCodes.Stloc_0);

            //define CallerInfo.MethodName
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldstr, method.Name);
            il.Emit(OpCodes.Callvirt, typeof(CallerInfo).GetMethod("set_MethodName")!);

            //define CallerInfo.ReturnType
            il.Emit(OpCodes.Ldloc_0);
            il.Emit(OpCodes.Ldtoken, method.ReturnType);
            il.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!);
            il.Emit(OpCodes.Callvirt, typeof(CallerInfo).GetMethod("set_ReturnType")!);

            //loop over args and add them to the CallerInfo list
            //and define CallerInfo.Parameters
            for (int i = 0; i < parameters.Length; i++)
            {
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Ldarg, i + 1);
                il.Emit(OpCodes.Callvirt, typeof(CallerInfo).GetMethod("Add")!);
                il.Emit(OpCodes.Ldloc_0);
                il.Emit(OpCodes.Callvirt, typeof(CallerInfo).GetMethod("get_Parameters")!);
                il.Emit(OpCodes.Ldtoken, parameters[i].ParameterType);
                il.Emit(OpCodes.Call, typeof(Type).GetMethod(nameof(Type.GetTypeFromHandle))!);
                il.Emit(OpCodes.Callvirt, typeof(List<Type>).GetMethod("Add")!);
            }

            //select target
            il.Emit(OpCodes.Ldarg_0);
            //select field on target
            il.Emit(OpCodes.Ldfld, field);
            //select local CallerInfo var
            il.Emit(OpCodes.Ldloc_0);
            //call func with var
            il.Emit(OpCodes.Callvirt, func.GetType().GetMethod("Invoke")!);
            //return
            il.Emit(OpCodes.Ret);

            type.DefineMethodOverride(stub, method);
        }
        Type endType = type.CreateType();
        TInterface returnObject = (TInterface)Activator.CreateInstance(endType)!;
        _cache.TryAdd(typeof(TInterface).Name, returnObject);
        foreach (KeyValuePair<string, Func<CallerInfo, object?>> field in fieldDelegates)
        {
            FieldInfo? instance = endType.GetField(field.Key);
            instance!.SetValue(returnObject, field.Value);
        }
        return returnObject;
    }
}

public class CallerInfo : IEnumerable<object>
{
    public string MethodName { get; set; } = "";
    public Type? ReturnType { get; set; }
    public List<Type> Parameters { get; set; } = [];
    private readonly List<object> _args = [];
    private int _index = 0;
    public object? MoveNext()
    {
        return _index >= _args.Count ? null : _args[_index++];
    }
    public void Add(object arg)
    {
        _args.Add(arg);
    }
    public void Reset()
    {
        _index = 0;
    }

    public IEnumerator<object> GetEnumerator()
    {
        return _args.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}