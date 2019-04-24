namespace NanoDiCs
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// The Dependency Injection container for storing registered dependency types.
    /// </summary>
    public class DependencyContainer
    {
        private readonly Dictionary<Type, ResolvedTypeWithLifeTimeOptions> container = new Dictionary<Type, ResolvedTypeWithLifeTimeOptions>();

        /// <summary>
        /// Registers a new type to the <see cref="DependencyContainer"/>
        /// </summary>
        /// <typeparam name="TTypeToResolve">The type to resolve.</typeparam>
        /// <typeparam name="TResolvedType">The resolved type to return.</typeparam>
        public void Register<TTypeToResolve, TResolvedType>()
        {
            this.Register<TTypeToResolve, TResolvedType>(LifeTimeOptions.Transient);
        }

        /// <summary>
        /// Registers a new type to the <see cref="DependencyContainer"/>
        /// </summary>
        /// <typeparam name="TTypeToResolve">The type to resolve.</typeparam>
        /// <typeparam name="TResolvedType">The resolved type to return.</typeparam>
        /// <param name="options">The <see cref="LifeTimeOptions"/> for the type to register.</param>
        public void Register<TTypeToResolve, TResolvedType>(LifeTimeOptions options)
        {
            if (this.container.ContainsKey(typeof(TTypeToResolve)))
            {
                throw new Exception($"Type {typeof(TTypeToResolve).FullName} already registered.");
            }

            ResolvedTypeWithLifeTimeOptions targetType = new ResolvedTypeWithLifeTimeOptions(typeof(TResolvedType), options);

            this.container.Add(typeof(TTypeToResolve), targetType);
        }

        /// <summary>
        /// Unregisters the type provided from the DI container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void UnResgister<T>()
        {
            Type t = typeof(T);

            if (!this.container.ContainsKey(t))
            {
                throw new Exception($"Type {t} is not registered in this container.");
            }

            object resolvedObject = this.container[t];
            this.container.Remove(t);

            if (resolvedObject is IDisposable disposableType)
            {
                disposableType.Dispose();
            }
        }

        /// <summary>
        /// Determines if the provided type is registered in the container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public bool IsRegistered<T>()
        {
            Type t = typeof(T);

            return container.ContainsKey(t);
        }

        /// <summary>
        /// Resolves the requested type from the <see cref="DependencyContainer"/>.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> to resolve.</typeparam>
        /// <returns>The resolved <see cref="Type"/>.</returns>
        public T Resolve<T>(bool throwOnFailedParameterResolution = true)
        {
            return (T)this.Resolve(typeof(T), throwOnFailedParameterResolution);
        }

        /// <summary>
        /// Resolves the requested type from the <see cref="DependencyContainer"/>.
        /// </summary>
        /// <param name="typeToResolve">The <see cref="Type"/> to resolve.</param>
        /// <returns>The resolved <see cref="Type"/>.</returns>
        /// <exception cref="System.Exception">Type not registered.</exception>
        /// <exception cref="System.Exception">Unable to resolve a parameter for this <see cref="Type"/>.</exception>
        public object Resolve(Type typeToResolve, bool throwOnFailedParameterResolution = true)
        {
            if (!this.container.ContainsKey(typeToResolve))
            {
                throw new Exception($"Unable to resolve {typeToResolve.FullName}. Type is not registered.");
            }

            ResolvedTypeWithLifeTimeOptions resolvedType = this.container[typeToResolve];

            if (resolvedType.LifeTimeOption == LifeTimeOptions.ContainerControlled && resolvedType.InstanceValue != null)
            {
                return resolvedType.InstanceValue;
            }

            ConstructorInfo constructorInfo = resolvedType.ResolvedType.GetConstructors().First();

            ParameterInfo[] paramsInfo = constructorInfo.GetParameters();
            object[] resolvedParams = new object[paramsInfo.Length];

            for (int x = 0; x < paramsInfo.Length; x++)
            {
                ParameterInfo param = paramsInfo[x];
                Type t = param.ParameterType;
                try
                {
                    object res = this.Resolve(t);
                    resolvedParams[x] = res;
                }
                catch (Exception ex)
                {
                    if (throwOnFailedParameterResolution)
                    {
                        throw new Exception($"Unable to resolve paramter type {t} for {typeToResolve}", ex);
                    }

                    resolvedParams[x] = null;
                    continue;
                }
            }

            object retOjbect = constructorInfo.Invoke(resolvedParams);

            if (resolvedType.LifeTimeOption == LifeTimeOptions.ContainerControlled)
            {
                resolvedType.InstanceValue = retOjbect;
            }

            return retOjbect;
        }
    }
}