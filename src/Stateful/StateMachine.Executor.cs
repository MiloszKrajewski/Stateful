using System;
using System.Collections.Generic;
using System.Linq;
using Stateful.Internal;

namespace Stateful
{
	public static partial class StateMachine<TContext, TState, TEvent>
	{
		#region Executor

		public interface IExecutor
		{
			TContext Context { get; }
			TState State { get; }

			void Fire(TEvent @event);
		}

		private class Executor: IExecutor
		{
			#region fields

			private readonly StateConfiguration[] _stateConfigList;
			private readonly EventConfiguration[] _eventConfigList;

			private readonly IDictionary<Type, StateExecutor[]> _stateConfigCache =
				new Dictionary<Type, StateExecutor[]>();
			private readonly IDictionary<EventKey, EventExecutor[]> _eventConfigCache =
				new Dictionary<EventKey, EventExecutor[]>();

			#endregion

			public TContext Context { get; private set; }
			public TState State { get; private set; }

			public Executor(
				IConfigurationProvider configuration,
				TContext context, TState state)
			{
				configuration.EnsureNotNull("configuration");
				state.EnsureNotNull("state");

				_stateConfigList = configuration.States.Select(s => new StateConfiguration(s)).ToArray();
				_eventConfigList = configuration.Events.Select(e => new EventConfiguration(e)).ToArray();
				Context = context;
				State = state;

				OnEnter();
			}

			public void Fire(TEvent @event)
			{
				@event.EnsureNotNull("event");
				OnFire(@event);
			}

			private void OnEnter()
			{
				var stateType = State.GetType();
				var configs = _stateConfigCache.TryGet(stateType, TryGetMode.GetOrCreate, CacheStateConfiguration);
				configs.Iterate(s => s.Enter(Context, State));
			}

			private void OnExit()
			{
				var stateType = State.GetType();
				var configs = _stateConfigCache.TryGet(stateType, TryGetMode.GetOrCreate, CacheStateConfiguration);
				configs.Reverse().Iterate(s => s.Exit(Context, State));
			}

			private void OnFire(TEvent @event)
			{
				var stateType = State.GetType();
				var eventType = @event.GetType();
				var eventKey = new EventKey(stateType, eventType);

				var configs = // all rules applicable for this state/event
					_eventConfigCache.TryGet(eventKey, TryGetMode.GetOrCreate, CacheEventConfiguration)
					.Where(e => e.Validate(Context, State, @event)).ToArray();
				var transitions = // only ones which claim to decide where to go next
					configs.Where(e => e.IsTransition).Reverse().ToArray();

				var firstTransition = transitions.Length <= 0 ? null : transitions[0];
				if (firstTransition == null)
					throw new InvalidOperationException(
						string.Format(
							"Unexpected event '{0}' in state '{1}'. No transitions defined.", 
							eventType.Name, stateType.Name));

				var secondTransition = transitions.Length <= 1 ? null : transitions[1];
				if (secondTransition != null && secondTransition.CompareTo(firstTransition) <= 0)
					throw new InvalidOperationException(
						string.Format(
							"Unhandled event '{0}' in state '{1}'. Ambigious transitions defined.", 
							eventType.Name, stateType.Name));

				configs.Iterate(e => e.Trigger(Context, State, @event));

				if (firstTransition.IsLoop)
					// for 'loop' transition we stay in the same state without triggering OnEnter nor OnExit
					return; 

				OnExit();
				State = firstTransition.Execute(Context, State, @event);
				OnEnter();
			}

			private StateExecutor[] CacheStateConfiguration(Type stateType)
			{
				var result = _stateConfigList
					.Where(s => stateType.InheritsFrom(s.StateType))
					.Select(s => new StateExecutor(stateType, s))
					.ToArray();
				Array.Sort(result);
				Array.Reverse(result);
				return result;
			}

			private EventExecutor[] CacheEventConfiguration(EventKey eventKey)
			{
				var stateType = eventKey.StateType;
				var eventType = eventKey.EventType;
				var result = _eventConfigList
					.Where(e => stateType.InheritsFrom(e.StateType) && eventType.InheritsFrom(e.EventType))
					.Select(e => new EventExecutor(stateType, eventType, e))
					.ToArray();
				Array.Sort(result);
				Array.Reverse(result);
				return result;
			}
		}

		#endregion

		#region public interface

		public static IExecutor NewExecutor(
			IConfigurationProvider configuration, TContext context, TState state)
		{
			return new Executor(configuration, context, state);
		}

		#endregion
	}
}
