using System;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace Catcher.Core
{
    internal static class ProxyFactory
    {
        static Type objectType = typeof(object);
        static Type objectArrType = typeof(object[]);
        static Type methodBaseType = typeof(MethodBase);
        static Type catcherContextType = typeof(CatcherContext);
        static Type baseProxyType = typeof(BaseProxy);
        static Type voidType = typeof(void);
        static Type serviceProviderType = typeof(IServiceProvider);
        static Type stringType = typeof(string);
        static Type typeType = typeof(Type);

        internal static Type CreateProxy(Type interfaceType, Type originalImplType = null)
        {
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException($"{interfaceType.Name} must be an Interface.");
            }

            // Dynamic creation of the proxy type
            var an = new AssemblyName(Guid.NewGuid().ToString());
            AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder mdBuilder = asmBuilder.DefineDynamicModule("ProxyModule");
            TypeBuilder tb = mdBuilder.DefineType($"{interfaceType.Name}Proxy", TypeAttributes.Public);

            // Interface implementation is added
            tb.AddInterfaceImplementation(interfaceType);
            tb.SetParent(baseProxyType);

            var baseConstructor = baseProxyType.GetConstructor(BindingFlags.Public | BindingFlags.FlattenHierarchy 
                | BindingFlags.Instance, null, new Type[] { stringType, objectType, serviceProviderType } , null);

            // Private field with the real implementation
            var innerFld = tb.DefineField("inner", interfaceType, FieldAttributes.Private | FieldAttributes.InitOnly);

            var innerInjectionTypes = originalImplType?.GetProperties()
                    .Where(prop => !prop.PropertyType.IsValueType)
                    .ToList();

            // Thanks to Scrutor's decorators the real implementation is added by constructor
            var ctor = tb.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard,
                new[] { interfaceType, serviceProviderType });

            var ctorGen = ctor.GetILGenerator();

            ctorGen.Emit(OpCodes.Ldarg_0);   
            ctorGen.Emit(OpCodes.Ldstr, interfaceType.FullName);
            ctorGen.Emit(OpCodes.Ldarg_1);
            ctorGen.Emit(OpCodes.Ldarg_2);
            ctorGen.Emit(OpCodes.Call, baseConstructor);

            // this
            ctorGen.Emit(OpCodes.Ldarg_0);
            // First argument - InterfaceImpl
            ctorGen.Emit(OpCodes.Ldarg_1);
            // Write the last evalutaion in a field - inner = InterfaceImpl
            ctorGen.Emit(OpCodes.Stfld, innerFld);

            // For each property get the implemented service from ServiceProvider for property injection
            innerInjectionTypes?.ForEach(prop =>
            {
                ctorGen.Emit(OpCodes.Ldarg_1);
                ctorGen.Emit(OpCodes.Callvirt, originalImplType.GetMethod($"get_{prop.Name}"));

                Label noBlock = ctorGen.DefineLabel();
                ctorGen.Emit(OpCodes.Brtrue, noBlock);

                ctorGen.Emit(OpCodes.Ldarg_1);

                ctorGen.Emit(OpCodes.Ldarg_2);

                ctorGen.Emit(OpCodes.Ldstr, prop.PropertyType.AssemblyQualifiedName);
                ctorGen.Emit(OpCodes.Call, typeType.GetMethod(nameof(Type.GetType), new[] { stringType }));

                ctorGen.Emit(OpCodes.Callvirt, serviceProviderType.GetMethod(nameof(IServiceProvider.GetService)));

                ctorGen.Emit(OpCodes.Castclass, prop.PropertyType);

                ctorGen.Emit(OpCodes.Call, originalImplType.GetMethod($"set_{prop.Name}"));
                ctorGen.MarkLabel(noBlock);
            });

            // Return
            ctorGen.Emit(OpCodes.Ret);

            CreateMethods(tb, interfaceType, innerFld);

            return tb.CreateTypeInfo().AsType();
        }

        private static void CreateMethods(TypeBuilder tb, Type interfaceType, FieldBuilder innerFld)
        {
            // Search the base interfaces
            interfaceType.GetInterfaces()
                    .ToList()
                    .ForEach(n =>
                        CreateMethods(tb, n, innerFld));

            // Proxy methods creation
            foreach (var method in interfaceType.GetMethods())
            {
                // A new mirror method is created
                var typeParams = method.GetParameters().Select(n => n.ParameterType).ToArray();

                var ilGen = tb.DefineMethod(method.Name, MethodAttributes.Public | MethodAttributes.Final
                    | MethodAttributes.Virtual | MethodAttributes.HideBySig | MethodAttributes.NewSlot,
                    method.ReturnType, typeParams)
                    .GetILGenerator();

                // The idea is call the interceptor
                // Then call the real method with the new args
                // Finally return the interceptor return value

                var isVoidReturned = (method.ReturnType.Equals(voidType));
                var ctx = ilGen.DeclareLocal(catcherContextType);

                ilGen.Emit(OpCodes.Ldarg_0);

                // Array ctor
                ilGen.Emit(OpCodes.Ldc_I4, typeParams.Length);
                ilGen.Emit(OpCodes.Newarr, objectType);

                // Copy the args into array elements
                for (int i = 0; i < typeParams.Length; i++)
                {
                    ilGen.Emit(OpCodes.Dup);
                    ilGen.Emit(OpCodes.Ldc_I4, i);
                    ilGen.Emit(OpCodes.Ldarg, i + 1);
                    if (typeParams[i].IsValueType)
                    {
                        ilGen.Emit(OpCodes.Box, typeParams[i]);
                    }
                    ilGen.Emit(OpCodes.Stelem_Ref);
                }

                // Create the interception context
                // Get the call method info                
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Callvirt, baseProxyType.GetMethod(nameof(BaseProxy.GetMethod)));

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, innerFld);

                // Create context with args and method
                ilGen.Emit(OpCodes.Newobj, catcherContextType
                    .GetConstructor(new Type[] { objectArrType, methodBaseType, objectType }));

                // Save into the local var
                ilGen.Emit(OpCodes.Stloc, ctx);

                // Call the pre method of interceptor with context
                ilGen.Emit(OpCodes.Ldloc, ctx);
                ilGen.Emit(OpCodes.Call, baseProxyType.GetMethod(nameof(BaseProxy.Execute)));

                // If a modified return value is returned it's loaded into the evaluation stack for the return
                if (!isVoidReturned)
                {
                    ilGen.Emit(OpCodes.Ldloc, ctx);
                    ilGen.Emit(OpCodes.Callvirt, catcherContextType.GetMethod($"get_{nameof(CatcherContext.ReturnValue)}"));
                    if (method.ReturnType.IsValueType)
                    {
                        ilGen.Emit(OpCodes.Unbox_Any, method.ReturnType);
                    }
                }

                //return
                ilGen.Emit(OpCodes.Ret);
            }
        }
    }
}
