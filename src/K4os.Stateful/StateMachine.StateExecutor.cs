using System;
using K4os.Stateful.Internal;

namespace K4os.Stateful
{
	public static partial class StateMachine<TContext, TState, TEvent>
	{
		#region StateExecutor

		private class StateExecutor: IComparable<StateExecutor>
		{
			private readonly StateConfiguration _configuration;
			private readonly Type _type;
			private readonly int _distance;

			public StateExecutor(Type stateType, StateConfiguration data)
			{
				_configuration = data;
				_type = stateType;
				_distance = stateType.DistanceFrom(data.StateType);
			}

			public void Enter(TContext context, TState state)
			{
				var enter = _configuration.OnEnter;
				enter?.Invoke(context, state);
			}

			public void Exit(TContext context, TState state)
			{
				var exit = _configuration.OnExit;
				exit?.Invoke(context, state);
			}

			/// <summary>Compares the current object with another object of the same type.</summary>
			/// <returns>A value that indicates the relative order of the objects being compared. The return value has the following meanings: Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.Zero This object is equal to <paramref name="other"/>. Greater than zero This object is greater than <paramref name="other"/>. </returns>
			/// <param name="other">An object to compare with this object.</param>
			public int CompareTo(StateExecutor other) => _distance.CompareTo(other._distance);

			public override string ToString() =>
				$"StateExecutor(Type:{_type.Name}({_distance}), {_configuration})";
		}

		#endregion
	}
}
