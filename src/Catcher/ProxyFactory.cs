using System;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

namespace Catcher
{
    internal static class ProxyFactory
    {
        static Type iInterceptorType = typeof(IInterceptor);
        static Type objectType = typeof(object);
        static Type objectArrType = typeof(object[]);
        static Type methodBaseType = typeof(MethodBase);
        static Type catcherContextType = typeof(CatcherContext);
        static Type baseProxyType = typeof(BaseProxy);

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

            var an = new AssemblyName(Guid.NewGuid().ToString());
            AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder mdBuilder = asmBuilder.DefineDynamicModule("ProxyModule");
            TypeBuilder tb = mdBuilder.DefineType($"Proxy{originalImplType.Name}", originalImplType.Attributes);
            tb.AddInterfaceImplementation(interfaceType);
            tb.SetParent(baseProxyType);

            var innerFld = tb.DefineField("inner", interfaceType, FieldAttributes.Private | FieldAttributes.InitOnly);
            var interFld = tb.DefineField("intercept", interfaceType, FieldAttributes.Private | FieldAttributes.InitOnly);

            var ctor = tb.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard,
                new[] { interfaceType, interceptorType });

            var ctorGen = ctor.GetILGenerator();

            ctorGen.Emit(OpCodes.Ldarg_0);
            ctorGen.Emit(OpCodes.Ldarg_1);
            ctorGen.Emit(OpCodes.Stfld, innerFld);
            ctorGen.Emit(OpCodes.Ldarg_0);
            ctorGen.Emit(OpCodes.Ldarg_2);
            ctorGen.Emit(OpCodes.Stfld, interFld);
            ctorGen.Emit(OpCodes.Ret);

            foreach (var method in interfaceType.GetMethods())
            {
                var oriMethod = originalImplType.GetMethod(method.Name);
                var typeParams = oriMethod.GetParameters().Select(n => n.ParameterType).ToArray();

                var ilGen = tb.DefineMethod(method.Name, oriMethod.Attributes,
                    oriMethod.ReturnType, typeParams)
                    .GetILGenerator();


                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, interFld);

                var ctx = ilGen.DeclareLocal(catcherContextType);

                ilGen.Emit(OpCodes.Ldc_I4, typeParams.Length);
                ilGen.Emit(OpCodes.Newarr, objectType);

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

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Callvirt, baseProxyType.GetMethod(nameof(BaseProxy.GetMethod)));
                ilGen.Emit(OpCodes.Newobj, catcherContextType
                    .GetConstructor(new Type[] { objectArrType, methodBaseType }));

                ilGen.Emit(OpCodes.Stloc, ctx);

                ilGen.Emit(OpCodes.Ldloc, ctx);
                ilGen.Emit(OpCodes.Call, interceptorType.GetMethod(nameof(IInterceptor.PreIntercept)));

                ilGen.Emit(OpCodes.Ldloc, ctx);
                ilGen.Emit(OpCodes.Callvirt, catcherContextType.GetMethod($"get_{nameof(CatcherContext.Cancel)}"));

                Label noBlock = ilGen.DefineLabel();
                ilGen.Emit(OpCodes.Brtrue, noBlock);

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, innerFld);
                for (int i = 1; i <= typeParams.Length; i++)
                {
                    ilGen.Emit(OpCodes.Ldarg, i);
                }
                ilGen.Emit(OpCodes.Callvirt, method);
                ilGen.MarkLabel(noBlock);

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, interFld);
                ilGen.Emit(OpCodes.Callvirt, interceptorType.GetMethod(nameof(IInterceptor.PostIntercept)));

                ilGen.Emit(OpCodes.Ret);
            }

            return tb.CreateTypeInfo().AsType();
        }
    }
}
