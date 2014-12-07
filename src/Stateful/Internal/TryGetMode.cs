using System;
using System.Collections.Generic;

namespace Stateful.Internal
{
	/// <summary>Retrieval mode for .TryGet</summary>
	internal enum TryGetMode
	{
		/// <summary>Gets the value or returns new one (but does not add it to dictionary).</summary>
		GetOrDefault,

		/// <summary>Gets the value or throws <see cref="KeyNotFoundException"/>.</summary>
		GetOrThrow,

		/// <summary>Gets the value or creates, adds and returns new one.</summary>
		GetOrCreate,

		/// <summary>Creates and adds new value or throws <see cref="ArgumentException"/> (if key already exists).</summary>
		CreateOrFail,

		/// <summary>Creates and add new value, regardless if it already was in dictionary.</summary>
		CreateAndReplace,
	}
}