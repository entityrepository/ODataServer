// -----------------------------------------------------------------------
// <copyright file="DbEnumManager.cs" company="EntityRepository Contributors" year="2013">
// This software is part of the EntityRepository library
// Copyright © 2012 EntityRepository Contributors
// http://entityrepository.codeplex.org/
// </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Scrum.Model.Base
{
	/// <summary>
	/// Provides static lookup methods for <see cref="NamedDbEnum{TId,T}"/> values.
	/// </summary>
	public static class DbEnumManager
	{

		/// <summary>
		/// Static lock object - only used during initialization to prevent conflicting writes.
		/// </summary>
		private static readonly object s_staticLock = new object();

		/// <summary>
		/// Holds a map of NamedDbEnum types (subclasses) to an Id lookup map for all values in the NamedDbEnum type;
		/// </summary>
		private static readonly IDictionary<Type, IDictionary<object, object>> s_mapIdLookupPerType = new Dictionary<Type, IDictionary<object, object>>();

		/// <summary>
		/// Holds a map of NamedDbEnum types (subclasses) to all defined values in the subtype;
		/// </summary>
		private static readonly IDictionary<Type, IList<object>> s_mapAllValuesPerType = new Dictionary<Type, IList<object>>();

		/// <summary>
		/// Register a <see cref="NamedDbEnum{TId, T}"/>-derived constant for subsequent lookup.
		/// </summary>
		/// <typeparam name="TId"></typeparam>
		/// <typeparam name="T"> </typeparam>
		/// <param name="dbEnumValue"></param>
		public static void RegisterDbEnumValue<TId, T>(NamedDbEnum<TId, T> dbEnumValue) where T : NamedDbEnum<TId, T>
		{
			lock (s_staticLock)
			{
				// Store this in the id lookup map associated with this NamedDbEnum type
				IDictionary<object, object> idLookupMap;
				if (! s_mapIdLookupPerType.TryGetValue(dbEnumValue.GetType(), out idLookupMap))
				{
					idLookupMap = new Dictionary<object, object>();
					s_mapIdLookupPerType[dbEnumValue.GetType()] = idLookupMap;
				}
				object previousWithSameId;
				lock (idLookupMap)
				{
					idLookupMap.TryGetValue(dbEnumValue.ID, out previousWithSameId);
					idLookupMap[dbEnumValue.ID] = dbEnumValue;
				}

				// Store this in the "all values" list associated with this NamedDbEnum type
				IList<object> allValuesList;
				if (!s_mapAllValuesPerType.TryGetValue(dbEnumValue.GetType(), out allValuesList))
				{
					allValuesList = new List<object>();
					s_mapAllValuesPerType[dbEnumValue.GetType()] = allValuesList;
				}
				lock (allValuesList)
				{
					if (previousWithSameId != null)
					{
						// Remove the conflicting item; keep only one with the same Id
						allValuesList.Remove(previousWithSameId);
					}
					allValuesList.Add(dbEnumValue);
				}
			}
		}

		/// <summary>
		/// Returns all instantiated values for the given <see cref="NamedDbEnum{TId, T}"/>-derived type.
		/// </summary>
		/// <typeparam name="TId">The <c>TId</c> type parameter for <c>TDbEnum</c>.</typeparam>
		/// <typeparam name="T">A class that subclasses <see cref="NamedDbEnum{TId, T}"/>.</typeparam>
		/// <returns>All instantiated values for the given <see cref="NamedDbEnum{TId, T}"/>-derived type.</returns>
		public static IEnumerable<T> GetValues<TId, T>() where T : NamedDbEnum<TId, T>
		{
			IList<object> allValuesList;
			Type dbEnumType = typeof(T);
			lock (s_staticLock)
			{
				s_mapAllValuesPerType.TryGetValue(dbEnumType, out allValuesList);
			}
			if (allValuesList == null)
			{
				// Use reflection to initialize the static fields
				ReflectAllStaticReadonlyValuesInType(dbEnumType);

				// Try again
				lock (s_staticLock)
				{
					s_mapAllValuesPerType.TryGetValue(dbEnumType, out allValuesList);
				}
				if (allValuesList == null)
				{
					throw new InvalidOperationException("No instances of NamedDbEnum subclass " + typeof(T) +
					                                    " are declared as static readonly fields;" +
					                                    "OR, " + typeof(T) + " does not call the correct base class constructor.");
				}
			}

			lock (allValuesList)
			{
				return allValuesList.Cast<T>();
			}
		}

		/// <summary>
		/// Lookup the instantiated <see cref="NamedDbEnum{TId}"/> value for the specified <c>id</c>.  Returns <c>null</c> if no
		/// such value is found.
		/// </summary>
		/// <typeparam name="TId">The <c>TId</c> type parameter for <c>TDbEnum</c>.</typeparam>
		/// <typeparam name="T">A class that subclasses <see cref="NamedDbEnum{TId}"/>.</typeparam>
		/// <param name="id">The Id to lookup.</param>
		/// <returns>A value of type <c>TDbEnum</c> with Id <c>== id</c>; or <c>null</c> if no such value is found.</returns>
		public static T LookupById<TId, T>(TId id) where T : NamedDbEnum<TId, T>
		{
			IDictionary<object, object> idLookupMap;
			Type dbEnumType = typeof(T);
			lock (s_staticLock)
			{
				s_mapIdLookupPerType.TryGetValue(dbEnumType, out idLookupMap);
			}
			if (idLookupMap == null)
			{
				// Use reflection to initialize the static fields
				ReflectAllStaticReadonlyValuesInType(dbEnumType);

				// Try again
				lock (s_staticLock)
				{
					s_mapIdLookupPerType.TryGetValue(dbEnumType, out idLookupMap);
				}
				if (idLookupMap == null)
				{
					throw new InvalidOperationException("No instances of NamedDbEnum subclass " + typeof(T) +
					                                    " are declared as static readonly fields.");
				}
			}

			lock (idLookupMap)
			{
				object returnVal;
				if (idLookupMap.TryGetValue(id, out returnVal))
				{
					return returnVal as T;
				}
				else
				{
					return null;
				}
			}
		}

		/// <summary>
		/// Used to force initialization of all static readonly fields in a NamedDbEnum class.
		/// </summary>
		/// <param name="type"></param>
		private static void ReflectAllStaticReadonlyValuesInType(Type type)
		{
			FieldInfo[] fields = type.GetFields(BindingFlags.Static | BindingFlags.Public);
			foreach (var fieldInfo in fields)
			{
				object value = fieldInfo.GetValue(null);
			}
		}

	}
}
