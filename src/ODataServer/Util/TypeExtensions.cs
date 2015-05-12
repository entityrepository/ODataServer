// // -----------------------------------------------------------------------
// <copyright file="TypeExtensions.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------


using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http.OData.Formatter;

// ReSharper disable once CheckNamespace
namespace System
{

	/// <summary>
	/// Extension methods for <see cref="Type"/>.
	/// </summary>
	internal static class TypeExtensions
	{

		public static bool IsNullable(this Type type)
		{
			Contract.Requires<ArgumentNullException>(type != null);

			if (type.IsValueType)
			{
				// value types are only nullable if they are Nullable<T>
				return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
			}
			else
			{
				// reference types are always nullable
				return true;
			}
		}

		public static Type ToNullable(this Type t)
		{
			Contract.Requires<ArgumentNullException>(t != null);

			if (t.IsNullable())
			{
				return t;
			}
			else
			{
				return typeof(Nullable<>).MakeGenericType(t);
			}
		}

		public static bool IsCollection(this Type type)
		{
			Type elementType;
			return type.IsCollection(out elementType);
		}

		public static bool IsCollection(this Type type, out Type elementType)
		{
			Contract.Requires<ArgumentNullException>(type != null);

			elementType = type;

			// see if this type should be ignored.
			if (type == typeof(string))
			{
				return false;
			}

			Type collectionInterface
				= type.GetInterfaces()
					.Union(new[] { type })
					.FirstOrDefault(
						t => t.IsGenericType
							 && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

			if (collectionInterface != null)
			{
				elementType = collectionInterface.GetGenericArguments().Single();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Returns the innermost element type for a given type, dealing with
		/// nullables, arrays, etc.
		/// </summary>
		/// <param name="type">The type from which to get the innermost type.</param>
		/// <returns>The innermost element type</returns>
		internal static Type GetInnerMostElementType(Type type)
		{
			Contract.Requires<ArgumentNullException>(type != null);

			while (true)
			{
				Type nullableUnderlyingType = Nullable.GetUnderlyingType(type);
				if (nullableUnderlyingType != null)
				{
					type = nullableUnderlyingType;
				}
				else if (type.HasElementType)
				{
					type = type.GetElementType();
				}
				else
				{
					return type;
				}
			}
		}

		/// <summary>
		/// Returns type of T if the type implements IEnumerable of T, otherwise, return null.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		internal static Type GetImplementedIEnumerableType(Type type)
		{
			// get inner type from Task<T>
			if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
			{
				type = type.GetGenericArguments().First();
			}

			if (type.IsGenericType && type.IsInterface &&
				(type.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
				 type.GetGenericTypeDefinition() == typeof(IQueryable<>)))
			{
				// special case the IEnumerable<T>
				return GetInnerGenericType(type);
			}
			else
			{
				// for the rest of interfaces and strongly Type collections
				Type[] interfaces = type.GetInterfaces();
				foreach (Type interfaceType in interfaces)
				{
					if (interfaceType.IsGenericType &&
						(interfaceType.GetGenericTypeDefinition() == typeof(IEnumerable<>) ||
						 interfaceType.GetGenericTypeDefinition() == typeof(IQueryable<>)))
					{
						// special case the IEnumerable<T>
						return GetInnerGenericType(interfaceType);
					}
				}
			}

			return null;
		}

		private static Type GetInnerGenericType(Type interfaceType)
		{
			// Getting the type T definition if the returning type implements IEnumerable<T>
			Type[] parameterTypes = interfaceType.GetGenericArguments();

			if (parameterTypes.Length == 1)
			{
				return parameterTypes[0];
			}

			return null;
		}

	}

}