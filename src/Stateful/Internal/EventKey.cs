using System;

namespace Stateful.Internal
{
	internal struct EventKey: IEquatable<EventKey>
	{
		#region fields

		private readonly Type _stateType;
		private readonly Type _eventType;

		#endregion

		#region constructor

		public EventKey(Type stateType, Type eventType)
		{
			_stateType = stateType.EnsureNotNull("stateType");
			_eventType = eventType.EnsureNotNull("eventType");
		}

		#endregion

		#region public interface

		public Type StateType { get { return _stateType; } }
		public Type EventType { get { return _eventType; } }

		#endregion

		#region overrides

		/// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
		/// <returns>true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.</returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(EventKey other)
		{
			return _stateType == other._stateType && _eventType == other._eventType;
		}

		/// <summary>Indicates whether this instance and a specified object are equal.</summary>
		/// <param name="other">Another object to compare to.</param>
		/// <returns>true if <paramref name="other" /> and this instance are the same type and represent the same value; otherwise, false.</returns>
		public override bool Equals(object other)
		{
			if (ReferenceEquals(other, null))
				return false;
			if (!(other is EventKey))
				return false;
			return Equals((EventKey)other);
		}

		/// <summary>Returns the hash code for this instance.</summary>
		/// <returns>A 32-bit signed integer that is the hash code for this instance.</returns>
		public override int GetHashCode()
		{
			return _stateType.GetHashCode() * 13 + _eventType.GetHashCode() * 7 + 3;
		}

		/// <summary>Returns the fully qualified type name of this instance.</summary>
		/// <returns>A <see cref="T:System.String"/> containing a fully qualified type name.</returns>
		public override string ToString()
		{
			return string.Format("EventKey({0}, {1})", _stateType.Name, _eventType.Name);
		}

		#endregion
	}
}
