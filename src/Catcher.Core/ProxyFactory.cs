using System;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

namespace Catcher.Core
{
    internal static class ProxyFactory
    {
        static Type iInterceptorType = typeof(IInterceptor);
        static Type objectType = typeof(object);
        static Type objectArrType = typeof(object[]);
        static Type methodBaseType = typeof(MethodBase);
        static Type catcherContextType = typeof(CatcherContext);
        static Type baseProxyType = typeof(BaseProxy);
        static Type voidType = typeof(void);

        internal static Type CreateProxy(Type interceptorType, Type interfaceType, Type originalImplType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException($"{interfaceType.Name} must be an Interface.");
            }

            if (!interfaceType.IsAssignableFrom(originalImplType))
            {
                throw new ArgumentException($"{originalImplType.Name} must implement {interfaceType.Name}.");
            }

            if (!iInterceptorType.IsAssignableFrom(interceptorType))
            {
                throw new ArgumentException($"{interceptorType.Name} must implement IInterceptor interface.");
            }

            // Dynamic creation of the proxy type
            var an = new AssemblyName(Guid.NewGuid().ToString());
            AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder mdBuilder = asmBuilder.DefineDynamicModule("ProxyModule");
            TypeBuilder tb = mdBuilder.DefineType($"Proxy{originalImplType.Name}", originalImplType.Attributes);

            // Interface implementation is added
            tb.AddInterfaceImplementation(interfaceType);
            tb.SetParent(baseProxyType);

            // Private field with the real implementation
            var innerFld = tb.DefineField("inner", interfaceType, FieldAttributes.Private | FieldAttributes.InitOnly);
            // Private field with the interceptor
            var interFld = tb.DefineField("intercept", interfaceType, FieldAttributes.Private | FieldAttributes.InitOnly);

            // Thanks to Scrutor's decorators the real implementation is added by constructor
            var ctor = tb.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard,
                new[] { interfaceType, interceptorType });

            var ctorGen = ctor.GetILGenerator();
            // this
            ctorGen.Emit(OpCodes.Ldarg_0);
            // First argument - InterfaceImpl
            ctorGen.Emit(OpCodes.Ldarg_1);
            // Write the last evalutaion in a field - inner = InterfaceImpl
            ctorGen.Emit(OpCodes.Stfld, innerFld);
            // this
            ctorGen.Emit(OpCodes.Ldarg_0);
            // Second argument - Interceptor
            ctorGen.Emit(OpCodes.Ldarg_2);
            // Write the last evalutaion in a field - intercept = Interceptor
            ctorGen.Emit(OpCodes.Stfld, interFld);
            // Return
            ctorGen.Emit(OpCodes.Ret);

            // Proxy methods creation
            foreach (var method in interfaceType.GetMethods())
            {
                // A new mirror method is created
                var oriMethod = originalImplType.GetMethod(method.Name);
                var typeParams = oriMethod.GetParameters().Select(n => n.ParameterType).ToArray();

                var ilGen = tb.DefineMethod(method.Name, oriMethod.Attributes,
                    oriMethod.ReturnType, typeParams)
                    .GetILGenerator();

                // The idea is call the interceptor
                // Then call the real method with the new args
                // Finally return the interceptor return value

                LocalBuilder retValue = null;
                LocalBuilder argsVar = ilGen.DeclareLocal(objectArrType);

                // Interceptor field is loaded for the first call
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, interFld);

                var isVoidReturned = (oriMethod.ReturnType.Equals(voidType));
                if (!isVoidReturned)
                {
                    retValue = ilGen.DeclareLocal(oriMethod.ReturnType);
                }

                var ctx = ilGen.DeclareLocal(catcherContextType);

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

                // Create context with args and method
                ilGen.Emit(OpCodes.Newobj, catcherContextType
                    .GetConstructor(new Type[] { objectArrType, methodBaseType }));

                // Save into the local var
                ilGen.Emit(OpCodes.Stloc, ctx);

                // Call the pre method of interceptor with context
                ilGen.Emit(OpCodes.Ldloc, ctx);
                ilGen.Emit(OpCodes.Call, interceptorType.GetMethod(nameof(IInterceptor.PreIntercept)));

                // Get the Cancel property for to check if it'is cancelled the execution
                ilGen.Emit(OpCodes.Ldloc, ctx);
                ilGen.Emit(OpCodes.Callvirt, catcherContextType.GetMethod($"get_{nameof(CatcherContext.Cancel)}"));

                // If no cancel execution
                Label noBlock = ilGen.DefineLabel();
                ilGen.Emit(OpCodes.Brtrue, noBlock);

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, innerFld);
                
                // Load the modified context args into the method call

                ilGen.Emit(OpCodes.Ldloc, ctx);
                ilGen.Emit(OpCodes.Callvirt, catcherContextType.GetMethod($"get_{nameof(CatcherContext.Args)}"));

                ilGen.Emit(OpCodes.Stloc, argsVar);

                for (int i = 0; i < typeParams.Length; i++)
                {
                    ilGen.Emit(OpCodes.Ldloc, argsVar);
                    ilGen.Emit(OpCodes.Ldc_I4, i);
                    ilGen.Emit(OpCodes.Ldelem_Ref);

                    if (typeParams[i].IsValueType)
                    {
                        ilGen.Emit(OpCodes.Unbox_Any, typeParams[i]);
                    }
                }

                // Call the real method
                ilGen.Emit(OpCodes.Callvirt, method);

                // If a value is returned it's saved into the context
                if (!isVoidReturned)
                {
                    if (oriMethod.ReturnType.IsValueType)
                    {
                        ilGen.Emit(OpCodes.Box, oriMethod.ReturnType);
                    }
                    ilGen.Emit(OpCodes.Stloc, retValue);
                    
                    ilGen.Emit(OpCodes.Ldloc, ctx);
                    ilGen.Emit(OpCodes.Ldloc, retValue);
                    ilGen.Emit(OpCodes.Callvirt, catcherContextType.GetMethod($"set_{nameof(CatcherContext.ReturnValue)}"));
                }
                
                ilGen.MarkLabel(noBlock);

                // Call the pre method of interceptor with context
                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, interFld);
                ilGen.Emit(OpCodes.Ldloc, ctx);
                ilGen.Emit(OpCodes.Callvirt, interceptorType.GetMethod(nameof(IInterceptor.PostIntercept)));

                // If a modified return value is returned it's loaded into the evaluation stack for the return
                if (!isVoidReturned)
                {
                    ilGen.Emit(OpCodes.Ldloc, ctx);
                    ilGen.Emit(OpCodes.Callvirt, catcherContextType.GetMethod($"get_{nameof(CatcherContext.ReturnValue)}"));
                }

                //return
                ilGen.Emit(OpCodes.Ret);
            }

            return tb.CreateTypeInfo().AsType();
        }
    }
}
