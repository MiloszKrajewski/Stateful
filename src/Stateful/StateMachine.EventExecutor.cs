using System;
using Stateful.Internal;

namespace Stateful
{
	public static partial class StateMachine<TContext, TState, TEvent>
	{
		#region EventExecutor

		private class EventExecutor: IComparable<EventExecutor>
		{
			private readonly EventConfiguration _configuration;
			private readonly int _stateDistance;
			private readonly int _eventDistance;
			private readonly Type _stateType;
			private readonly Type _eventType;

			public EventExecutor(Type stateType, Type eventType, EventConfiguration configuration)
			{
				_configuration = configuration;
				_stateType = stateType;
				_eventType = eventType;
				_stateDistance = stateType.DistanceFrom(configuration.StateType);
				_eventDistance = eventType.DistanceFrom(configuration.EventType);
			}

			private bool IsFallback { get { return _configuration.OnValidate == null; } }
			public bool IsTransition { get { return _configuration.OnExecute != null || _configuration.IsLoop; } }
			public bool IsLoop { get { return _configuration.IsLoop; } }

			public bool Validate(TContext context, TState state, TEvent @event)
			{
				var action = _configuration.OnValidate;
				return action == null || action(context, state, @event);
			}

			public void Trigger(TContext context, TState state, TEvent @event)
			{
				var trigger = _configuration.OnTrigger;
				if (trigger != null)
					trigger(context, state, @event);
			}

			public TState Execute(TContext context, TState state, TEvent @event)
			{
				var transition = _configuration.OnExecute;
				// no if here, we do want exception in such case
				return transition(context, state, @event);
			}

			/// <summary>Compares the current object with another object of the same type.</summary>
			/// <returns>A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. </returns>
			/// <param name="other">An object to compare with this object.</param>
			public int CompareTo(EventExecutor other)
			{
				int c;
				if ((c = _stateDistance.CompareTo(other._stateDistance)) != 0)
					return c;
				if ((c = _eventDistance.CompareTo(other._eventDistance)) != 0)
					return c;
				if ((c = IsFallback.CompareTo(other.IsFallback)) != 0)
					return c;
				return 0;
			}

			public override string ToString()
			{
				return string.Format(
					"EventExecutor(Type:{0}({1})/{2}({3}), {4})",
					_stateType.Name, _stateDistance,
					_eventType.Name, _eventDistance,
					_configuration);
			}
		}

		#endregion
	}
}
