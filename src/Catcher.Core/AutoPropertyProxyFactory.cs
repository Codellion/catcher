using System;
using System.Reflection.Emit;
using System.Reflection;
using System.Linq;

namespace Catcher.Core
{
    internal static class AutoPropertyProxyFactory
    {
        static Type baseProxyType = typeof(BaseProxy);
        static Type voidType = typeof(void);
        static Type serviceProvType = typeof(IServiceProvider);
        static Type stringType = typeof(string);
        static Type typeType = typeof(Type);

        internal static Type CreateProxy(Type interfaceType, Type originalImplType)
        {
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException($"{interfaceType.Name} must be an Interface.");
            }

            if (!interfaceType.IsAssignableFrom(originalImplType))
            {
                throw new ArgumentException($"{originalImplType.Name} must implement {interfaceType.Name}.");
            }

            // Dynamic creation of the proxy type
            var an = new AssemblyName(Guid.NewGuid().ToString());
            AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            ModuleBuilder mdBuilder = asmBuilder.DefineDynamicModule("ProxyModule");
            TypeBuilder tb = mdBuilder.DefineType($"AutoPropertyProxy{originalImplType.Name}", originalImplType.Attributes);

            // Interface implementation is added
            tb.AddInterfaceImplementation(interfaceType);
            tb.SetParent(baseProxyType);

            // Private field with the real implementation
            var innerFld = tb.DefineField("inner", interfaceType, FieldAttributes.Private | FieldAttributes.InitOnly);

            var innerInjectionTypes = originalImplType.GetProperties()
                .Where(prop => !prop.PropertyType.IsValueType)
                .ToList();

            // Thanks to Scrutor's decorators the real implementation is added by constructor
            var ctor = tb.DefineConstructor(
                MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard,
                new[] { interfaceType, serviceProvType });

            var ctorGen = ctor.GetILGenerator();
            // this
            ctorGen.Emit(OpCodes.Ldarg_0);
            // First argument - InterfaceImpl
            ctorGen.Emit(OpCodes.Ldarg_1);
            // Write the last evalutaion in a field - inner = InterfaceImpl
            ctorGen.Emit(OpCodes.Stfld, innerFld);

            // For each property get the implemented service from ServiceProvider
            innerInjectionTypes.ForEach(prop =>
            {
                ctorGen.Emit(OpCodes.Ldarg_1);
                ctorGen.Emit(OpCodes.Callvirt, originalImplType.GetMethod($"get_{prop.Name}"));

                Label noBlock = ctorGen.DefineLabel();
                ctorGen.Emit(OpCodes.Brtrue, noBlock);

                ctorGen.Emit(OpCodes.Ldarg_1);

                ctorGen.Emit(OpCodes.Ldarg_2);

                ctorGen.Emit(OpCodes.Ldstr, prop.PropertyType.AssemblyQualifiedName);
                ctorGen.Emit(OpCodes.Call, typeType.GetMethod(nameof(Type.GetType), new[] { stringType }));

                ctorGen.Emit(OpCodes.Callvirt, serviceProvType.GetMethod(nameof(IServiceProvider.GetService)));

                ctorGen.Emit(OpCodes.Castclass, prop.PropertyType);

                ctorGen.Emit(OpCodes.Call, originalImplType.GetMethod($"set_{prop.Name}"));
                ctorGen.MarkLabel(noBlock);
            });
           
            // Return
            ctorGen.Emit(OpCodes.Ret);

            CreateMethods(tb, interfaceType, originalImplType, innerFld);

            return tb.CreateTypeInfo().AsType();
        }

        private static void CreateMethods(TypeBuilder tb, Type interfaceType, Type originalImplType, FieldBuilder innerFld)
        {
            // Search the base interfaces
            interfaceType.GetInterfaces()
                    .ToList()
                    .ForEach(n =>
                        CreateMethods(tb, n, originalImplType, innerFld));

            // Proxy methods creation
            foreach (var method in interfaceType.GetMethods())
            {
                // A new mirror method is created
                var oriMethod = originalImplType.GetMethod(method.Name);
                var typeParams = oriMethod.GetParameters().Select(n => n.ParameterType).ToArray();

                var ilGen = tb.DefineMethod(method.Name, oriMethod.Attributes,
                    oriMethod.ReturnType, typeParams)
                    .GetILGenerator();

                var isVoidReturned = (oriMethod.ReturnType.Equals(voidType));

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, innerFld);

                for (int i = 0; i < typeParams.Length; i++)
                {
                    ilGen.Emit(OpCodes.Ldarg, i + 1);
                }

                // Call the real method
                ilGen.Emit(OpCodes.Callvirt, method);


                // If a modified return value is returned it's loaded into the evaluation stack for the return
                if (!isVoidReturned && oriMethod.ReturnType.IsValueType)
                {
                    ilGen.Emit(OpCodes.Unbox_Any, oriMethod.ReturnType);
                }

                //return
                ilGen.Emit(OpCodes.Ret);
            }
        }
    }
}
