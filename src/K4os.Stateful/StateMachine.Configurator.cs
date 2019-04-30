using System;
using System.Collections.Generic;
using K4os.Stateful.Internal;
using System.Linq;

namespace K4os.Stateful
{
	public static partial class StateMachine<TContext, TState, TEvent>
	{
		#region configuration

		/// <summary>
		/// Provides state machine configuration for IExecutor.
		/// </summary>
		public interface IConfigurationProvider
		{
			IEnumerable<IStateConfiguration> States { get; }
			IEnumerable<IEventConfiguration> Events { get; }
		}

		/// <inheritdoc />
		/// <summary>
		/// Allow to configure state machine.
		/// </summary>
		public interface IConfigurator: IConfigurationProvider
		{
			IStateConfigurator<TActualState> In<TActualState>()
				where TActualState: TState;

			IEventConfigurator<TActualState, TActualEvent> On<TActualState, TActualEvent>()
				where TActualState: TState
				where TActualEvent: TEvent;
		}

		/// <inheritdoc />
		/// <summary>
		/// Implementation of <see cref="IConfigurator" />
		/// </summary>
		private class Configurator: IConfigurator
		{
			#region fields

			private readonly IDictionary<Type, StateConfiguration> _states =
				new Dictionary<Type, StateConfiguration>();

			private readonly IDictionary<EventKey, IList<EventConfiguration>> _events =
				new Dictionary<EventKey, IList<EventConfiguration>>();

			#endregion

			#region IConfigurator implementation

			public IStateConfigurator<TActualState> In<TActualState>()
				where TActualState: TState
			{
				var stateType = typeof(TActualState);
				var stateData = _states.TryGet(stateType, TryGetMode.GetOrCreate, t => new StateConfiguration(t));
				return new StateConfigurator<TActualState>(this, stateData);
			}

			public IEventConfigurator<TActualState, TActualEvent> On<TActualState, TActualEvent>()
				where TActualState: TState
				where TActualEvent: TEvent
			{
				var eventMap = _events;
				var stateType = typeof(TActualState);
				var eventType = typeof(TActualEvent);
				var eventKey = new EventKey(stateType, eventType);
				var eventList = eventMap.TryGet(eventKey, TryGetMode.GetOrCreate, _ => new List<EventConfiguration>());
				var eventData = new EventConfiguration(stateType, eventType);
				eventList.Add(eventData);
				return new EventConfigurator<TActualState, TActualEvent>(eventData);
			}

			#endregion

			#region IConfigurationProvider implementation

			public IEnumerable<IStateConfiguration> States => _states.Values;
			public IEnumerable<IEventConfiguration> Events => _events.Values.SelectMany(x => x);

			#endregion
		}

		#region public interface

		/// <summary>Create new state machine configurator.</summary>
		/// <returns>New configurator.</returns>
		public static IConfigurator NewConfigurator()
		{
			return new Configurator();
		}

		#endregion

		#endregion
	}
}