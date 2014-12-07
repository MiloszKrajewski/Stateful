using System;
using System.Collections.Generic;
using System.Linq;
using Stateful.Internal;

namespace Stateful
{
	public static partial class StateMachine<TContext, TState, TEvent>
	{
		#region configuration

		public interface IConfigurationProvider
		{
			IEnumerable<IStateConfiguration> States { get; }
			IEnumerable<IEventConfiguration> Events { get; }
		}

		public interface IConfigurator: IConfigurationProvider
		{
			IStateConfigurator<TActualState> ConfigureState<TActualState>()
				where TActualState: TState;

			IEventConfigurator<TActualState, TActualEvent> ConfigureEvent<TActualState, TActualEvent>()
				where TActualState: TState
				where TActualEvent: TEvent;
		}

		private class Configurator: IConfigurator
		{
			#region fields

			private readonly IDictionary<Type, StateConfiguration> _states =
				new Dictionary<Type, StateConfiguration>();

			private readonly IDictionary<EventKey, IList<EventConfiguration>> _events =
				new Dictionary<EventKey, IList<EventConfiguration>>();

			#endregion

			#region IConfigurator implementation

			public IStateConfigurator<TActualState> ConfigureState<TActualState>()
				where TActualState: TState
			{
				var stateType = typeof(TActualState);
				var stateData = _states.TryGet(stateType, TryGetMode.CreateOrFail, t => new StateConfiguration(t));
				return new StateConfigurator<TActualState>(stateData);
			}

			public IEventConfigurator<TActualState, TActualEvent> ConfigureEvent<TActualState, TActualEvent>()
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

			public IEnumerable<IStateConfiguration> States { get { return _states.Values; } }
			public IEnumerable<IEventConfiguration> Events { get { return _events.Values.SelectMany(x => x); } }

			#endregion
		}

		#endregion

		#region public interface

		public static IConfigurator NewConfigurator()
		{
			return new Configurator();
		}

		#endregion
	}
}