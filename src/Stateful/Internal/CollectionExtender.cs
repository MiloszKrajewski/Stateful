using System;
using System.Collections.Generic;

namespace Stateful.Internal
{
	internal static class CollectionExtender
	{
		#region enumerable

		/// <summary>Iterates over sequence of object and executes action for each item.</summary>
		/// <typeparam name="T">Type of item.</typeparam>
		/// <param name="collection">The collection.</param>
		/// <param name="action">The action.</param>
		public static void Iterate<T>(this IEnumerable<T> collection, Action<T> action)
		{
			foreach (var item in collection)
				action(item);
		}

		#endregion

		#region dictionary

		/// <summary>Get default value for any type.</summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="key">The key (ignored).</param>
		/// <returns>Default value for <typeparamref name="TValue"/></returns>
		private static TValue DefaultValue<TKey, TValue>(TKey key) { return default(TValue); }

		/// <summary>Tries to get value from dictionary. 
		/// If value does not exist has multiple ways to resolve this.</summary>
		/// <typeparam name="TKey">The type of the key.</typeparam>
		/// <typeparam name="TValue">The type of the value.</typeparam>
		/// <param name="map">The dictionary.</param>
		/// <param name="key">The key.</param>
		/// <param name="mode">The retrieval mode (<see cref="TryGetMode"/>).</param>
		/// <param name="factory">The factory method to create new value.</param>
		/// <returns>Retrieved or created value.</returns>
		/// <exception cref="System.ArgumentException">Thrown if value already exists in CreateOrFail mode.</exception>
		/// <exception cref="System.Collections.Generic.KeyNotFoundException">Throw if value does not exist in GetOrThrow mode.</exception>
		public static TValue TryGet<TKey, TValue>(
			this IDictionary<TKey, TValue> map, TKey key,
			TryGetMode mode = TryGetMode.GetOrDefault,
			Func<TKey, TValue> factory = null)
		{
			factory = factory ?? DefaultValue<TKey, TValue>;

			if (mode == TryGetMode.CreateAndReplace)
				return map[key] = factory(key);

			TValue value;
			var found = map.TryGetValue(key, out value);

			if (found)
			{
				if (mode == TryGetMode.CreateOrFail)
				{
					throw new ArgumentException(
						string.Format("Key '{0}' already exists in dictionary.", key));
				}
				return value;
			}

			switch (mode)
			{
				case TryGetMode.GetOrDefault:
					return factory(key);
				case TryGetMode.GetOrThrow:
					throw new KeyNotFoundException(
						string.Format("Key '{0}' could not be found in dictionary.", key));
				default:
					return map[key] = factory(key);
			}
		}

		#endregion
	}
}
