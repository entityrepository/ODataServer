// -----------------------------------------------------------------------
// <copyright file="ReflectionExtensions.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reflection;

namespace EntityRepository.ODataServer.Util
{
	/// <summary>
	/// Extension methods for reflection.
	/// </summary>
	public static class ReflectionExtensions
	{

		/// <summary>
		/// Returns true if <c>this</c> is a generic version of generic type definition <paramref name="genericTypeDefinition"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="genericTypeDefinition"></param>
		/// <returns></returns>
		public static bool IsGenericType(this Type type, Type genericTypeDefinition)
		{
			return type.IsGenericType
			       && ReferenceEquals(type.GetGenericTypeDefinition(), genericTypeDefinition);
		}

		/// <summary>
		/// Returns true if <c>this</c> is a generic version of, or inherits from, generic type definition <paramref name="genericTypeDefinition"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="genericTypeDefinition"></param>
		/// <returns></returns>
		public static bool IsDerivedFromGenericType(this Type type, Type genericTypeDefinition)
		{
			if (type == null)
			{
				return false;
			}
			if (genericTypeDefinition == null)
			{
				return false;
			}

			if (type.IsGenericType)
			{
				Type genericType = type.GetGenericTypeDefinition();
				if (ReferenceEquals(genericType, genericTypeDefinition))
				{
					return true;
				}
			}
			// Recurse
			return IsDerivedFromGenericType(type.BaseType, genericTypeDefinition);
		}

		internal static object GetPropertyValue(this object instance, string propertyName)
		{
			Contract.Requires<ArgumentNullException>(instance != null);
			Contract.Requires<ArgumentNullException>(propertyName != null);

			return GetPropertyValue(instance.GetType(), instance, propertyName);
		}

		internal static object GetPropertyValue(this Type type, object instance, string propertyName)
		{
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Requires<ArgumentNullException>(propertyName != null);
			Contract.Requires<ArgumentException>(!type.IsGenericTypeDefinition);

			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.FlattenHierarchy;
			if (instance == null)
			{
				bindingFlags |= BindingFlags.Static;
			}
			else
			{
				bindingFlags |= BindingFlags.Instance;
			}
			PropertyInfo propertyInfo = type.GetProperty(propertyName, bindingFlags);
			return propertyInfo.GetValue(instance, null);
		}

		internal static void SetPropertyValue(this object instance, string propertyName, object value)
		{
			Contract.Requires<ArgumentNullException>(instance != null);
			Contract.Requires<ArgumentNullException>(propertyName != null);

			SetPropertyValue(instance.GetType(), instance, propertyName, value);
		}

		internal static void SetPropertyValue(this Type type, object instance, string propertyName, object value)
		{
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Requires<ArgumentNullException>(propertyName != null);
			Contract.Requires<ArgumentException>(!type.IsGenericTypeDefinition);

			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.FlattenHierarchy;
			if (instance == null)
			{
				bindingFlags |= BindingFlags.Static;
			}
			else
			{
				bindingFlags |= BindingFlags.Instance;
			}
			PropertyInfo propertyInfo = type.GetProperty(propertyName, bindingFlags);
			propertyInfo.SetValue(instance, value);
		}

		/// <summary>
		/// Invokes a non-generic method using reflection and the specified parameters.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="methodName"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		internal static object InternalInvokeMethod(Type type, object instance, string methodName, params object[] arguments)
		{
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Requires<ArgumentNullException>(methodName != null);
			Contract.Requires<ArgumentException>(! type.IsGenericTypeDefinition);

			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.FlattenHierarchy;
			if (instance == null)
			{
				bindingFlags |= BindingFlags.Static;
			}
			else
			{
				bindingFlags |= BindingFlags.Instance;
			}
			MethodInfo methodInfo = type.GetMethod(methodName, bindingFlags);
			if (methodInfo == null)
			{
				throw new ArgumentException(string.Format("Public method named {0} not found on type {1}", methodName, type), "methodName");
			}
			return methodInfo.Invoke(instance, arguments);
		}

		/// <summary>
		/// Invokes a non-generic method using reflection and the specified parameters.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="methodName"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		internal static object InvokeMethod(this object instance, string methodName, params object[] arguments)
		{
			Contract.Requires<ArgumentNullException>(instance != null);
			Contract.Requires<ArgumentNullException>(methodName != null);

			return InternalInvokeMethod(instance.GetType(), instance, methodName, arguments);
		}

		/// <summary>
		/// Invokes a non-generic static method using reflection and the specified parameters.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="methodName"></param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		internal static object InvokeStaticMethod(this Type type, string methodName, params object[] arguments)
		{
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Requires<ArgumentNullException>(methodName != null);
			Contract.Requires<ArgumentException>(!type.IsGenericTypeDefinition);

			return InternalInvokeMethod(type, null, methodName, arguments);
		}

		/// <summary>
		/// Invokes a generic method using reflection and the specified parameters.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="instance"></param>
		/// <param name="methodName"></param>
		/// <param name="genericMethodParameters">The generic type arguments.</param>
		/// <param name="arguments"></param>
		/// <returns></returns>
		internal static object InvokeMethod(this Type type, object instance, string methodName, Type[] genericMethodParameters, params object[] arguments)
		{
			Contract.Requires<ArgumentNullException>(type != null);
			Contract.Requires<ArgumentNullException>(methodName != null);
			Contract.Requires<ArgumentNullException>(genericMethodParameters != null);
			Contract.Requires<ArgumentException>(!type.IsGenericTypeDefinition);

			BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.FlattenHierarchy;
			if (instance == null)
			{
				bindingFlags |= BindingFlags.Static;
			}
			else
			{
				bindingFlags |= BindingFlags.Instance;
			}

			try
			{
				MethodInfo methodInfo =
					type.GetMethods(bindingFlags)
					    .Where(mi => (mi.Name == methodName) && mi.IsGenericMethodDefinition && (mi.GetGenericArguments().Length == genericMethodParameters.Length)
					                                         && (mi.GetParameters().Length == arguments.Length))
					    .Select(mi => mi.MakeGenericMethod(genericMethodParameters))
					    .First();
				if (methodInfo == null)
				{
					throw new ArgumentException(string.Format("Public method named {0} not found on type {1}", methodName, type), "methodName");
				}
				return methodInfo.Invoke(instance, arguments);
			}
			catch (InvalidOperationException excp)
			{
				string msg = string.Format("No public method on Type {0} found named {1} with type parameters {2} and {3} arguments.",
				                           type,
				                           methodName,
				                           string.Join<Type>(", ", genericMethodParameters),
				                           arguments.Length);
				throw new InvalidOperationException(msg, excp);
			}
		}

		/// <summary>
		/// Copies the values of all primitive (non-navigation) properties on source to destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		internal static void CopyPublicPrimitivePropertyValues(object source, object destination)
		{
			Contract.Requires<ArgumentNullException>(source != null);
			Contract.Requires<ArgumentNullException>(destination != null);

			// Handle EF proxies here?
			// Maybe this is too leaky, should be handled elsewhere?
			var destinationType = destination.GetType();
			if (destinationType.BaseType != null && destinationType.Namespace == "System.Data.Entity.DynamicProxies")
			{
				destinationType = destinationType.BaseType;
			}

			Type sourceType = source.GetType();
			if (! Object.ReferenceEquals(sourceType, destinationType))
			{
				throw new ArgumentException(string.Format("Source type '{0}' and destination type '{1}' must match.", sourceType.FullName, destination.GetType().FullName));
			}

			foreach (PropertyInfo property in sourceType.GetProperties(BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Instance))
			{
				if (IsEdmPrimitiveType(property.PropertyType))
				{
					object propertyValue = property.GetValue(source);
					property.SetValue(destination, propertyValue);
				}
			}
		}

		/// <summary>
		/// Determines whether a type is an EDM primitive type.  Should return false for all Navigation properties.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <remarks>
		/// Edm primitive types are CLR primitive types, or  <see cref="string"/>, <see cref="DateTime"/>, <see cref="Decimal"/>,
		/// <see cref="Guid"/>, <see cref="DateTimeOffset"/> or <see cref="TimeSpan"/>, or <see cref="Nullable"/> primitive types.
		/// </remarks>
		internal static bool IsEdmPrimitiveType(Type type)
		{
			Contract.Requires<ArgumentNullException>(type != null);

			if (type.IsArray)
			{
				Type elementType = type.GetElementType();
				if (ReferenceEquals(elementType, typeof(byte))
				    || ReferenceEquals(elementType, typeof(char)))
				{
					return true;
				}
			}
			else if (type.IsClass)
			{
				if (ReferenceEquals(type, typeof(string)))
				{
					return true;
				}
			}
			else if (type.IsValueType)
			{
				// Handle Nullable
				if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					type = type.GetGenericArguments()[0];
				}

				if (type.IsPrimitive)
				{
					return true;
				}
				else if (ReferenceEquals(type, typeof(DateTime))
					|| ReferenceEquals(type, typeof(DateTimeOffset))
					|| ReferenceEquals(type, typeof(TimeSpan))
					|| ReferenceEquals(type, typeof(Decimal))
					|| ReferenceEquals(type, typeof(Guid)))
				{
					return true;
				}
			}

			return false;
		}
	}
}
