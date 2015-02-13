using System;

namespace Stateful
{
	public static partial class StateMachine<TContext, TState, TEvent>
	{
		#region event context

		public interface IEventContext<out TActualState, out TActualEvent>: 
			IStateContext<TActualState>
			where TActualState: TState
			where TActualEvent: TEvent
		{
			TActualEvent Event { get; }
		}

		private class EventContext<TActualState, TActualEvent>:
			IEventContext<TActualState, TActualEvent>
			where TActualState: TState
			where TActualEvent: TEvent
		{
			public TContext Context { get; set; }
			public TActualState State { get; set; }
			public TActualEvent Event { get; set; }
		}

		private static IEventContext<TActualState, TActualEvent> MakeContext<TActualState, TActualEvent>(
			TContext context, TState state, TEvent @event)
			where TActualState: TState
			where TActualEvent: TEvent
		{
			return new EventContext<TActualState, TActualEvent> {
				Context = context,
				State = (TActualState)state,
				Event = (TActualEvent)@event
			};
		}

		#endregion

		#region event configuration

		public interface IEventConfiguration
		{
			Type StateType { get; }
			Type EventType { get; }

			string Name { get; }

			Func<TContext, TState, TEvent, bool> OnValidate { get; }
			Action<TContext, TState, TEvent> OnTrigger { get; }
			Func<TContext, TState, TEvent, TState> OnExecute { get; }

			bool IsLoop { get; }
		}

		private class EventConfiguration: IEventConfiguration
		{
			public EventConfiguration(Type stateType, Type eventType)
			{
				StateType = stateType;
				EventType = eventType;
			}

			public EventConfiguration(IEventConfiguration other)
			{
				StateType = other.StateType;
				EventType = other.EventType;
				OnValidate = other.OnValidate;
				OnTrigger = other.OnTrigger;
				OnExecute = other.OnExecute;
				IsLoop = other.IsLoop;
			}

			public Type StateType { get; private set; }
			public Type EventType { get; private set; }

			public string Name { get; set; }

			public Func<TContext, TState, TEvent, bool> OnValidate { get; set; }
			public Action<TContext, TState, TEvent> OnTrigger { get; set; }
			public Func<TContext, TState, TEvent, TState> OnExecute { get; set; }

			public bool IsLoop { get; set; }

			public override string ToString()
			{
				return string.Format(
					"EventConfiguration(Type:{0}/{1}, Name:'{2}', OnValidate:{3}, OnTrigger:{4}, OnExecute:{5}, IsLoop:{6})",
					StateType.Name, EventType.Name,
					Name,
					OnValidate != null, OnTrigger != null, OnExecute != null,
					IsLoop);
			}
		}

		public interface IEventConfigurator<out TActualState, out TActualEvent>
			where TActualState: TState
			where TActualEvent: TEvent
		{
			IEventConfigurator<TActualState, TActualEvent> Name(string name);
			IEventConfigurator<TActualState, TActualEvent> OnTrigger(
				Action<IEventContext<TActualState, TActualEvent>> action);
			IEventConfigurator<TActualState, TActualEvent> When(
				Func<IEventContext<TActualState, TActualEvent>, bool> predicate);
			IEventConfigurator<TActualState, TActualEvent> Goto(
				Func<IEventContext<TActualState, TActualEvent>, TState> action);
			IEventConfigurator<TActualState, TActualEvent> Loop();
		}

		private class EventConfigurator<TActualState, TActualEvent>:
			IEventConfigurator<TActualState, TActualEvent>
			where TActualState: TState
			where TActualEvent: TEvent
		{
			private readonly EventConfiguration _configuration;

			public EventConfigurator(EventConfiguration configuration)
			{
				_configuration = configuration;
			}

			private string TextId
			{
				get
				{
					return string.Format(
						"Event({0}, {1})", _configuration.StateType.Name, _configuration.EventType.Name);
				}
			}

			public IEventConfigurator<TActualState, TActualEvent> Name(string name)
			{
				if (name == null)
					throw new ArgumentNullException("name");
				if (_configuration.Name != null)
					throw new InvalidOperationException(
						string.Format("{0}.Name has been already defined.", TextId));
				_configuration.Name = name;
				return this;
			}

			public IEventConfigurator<TActualState, TActualEvent> OnTrigger(
				Action<IEventContext<TActualState, TActualEvent>> action)
			{
				if (action == null)
					throw new ArgumentNullException("action");
				if (_configuration.OnTrigger != null)
					throw new InvalidOperationException(
						string.Format("{0}.OnTrigger has been already defined.", TextId));
				_configuration.OnTrigger = (c, s, e) => action(MakeContext<TActualState, TActualEvent>(c, s, e));
				return this;
			}

			public IEventConfigurator<TActualState, TActualEvent> When(
				Func<IEventContext<TActualState, TActualEvent>, bool> predicate)
			{
				if (predicate == null)
					throw new ArgumentNullException("predicate");
				if (_configuration.OnValidate != null)
					throw new InvalidOperationException(
						string.Format("{0}.When has been already defined.", TextId));
				_configuration.OnValidate = (c, s, e) => predicate(MakeContext<TActualState, TActualEvent>(c, s, e));
				return this;
			}

			public IEventConfigurator<TActualState, TActualEvent> Goto(
				Func<IEventContext<TActualState, TActualEvent>, TState> action)
			{
				if (action == null)
					throw new ArgumentNullException("action");
				if (_configuration.OnExecute != null)
					throw new InvalidOperationException(
						string.Format("{0}.Goto/Loop has been already defined.", TextId));
				_configuration.OnExecute = (c, s, e) => action(MakeContext<TActualState, TActualEvent>(c, s, e));
				_configuration.IsLoop = false;
				return this;
			}

			public IEventConfigurator<TActualState, TActualEvent> Loop()
			{
				if (_configuration.OnExecute != null)
					throw new InvalidOperationException(
						string.Format("{0}.Goto/Loop has been already defined.", TextId));
				_configuration.OnExecute = LoopHandler; // technically it is not needed, but it unifies Loop and Goto
				_configuration.IsLoop = true;
				return this;
			}

			private static TState LoopHandler(TContext context, TState state, TEvent @event)
			{
				return state;
			}

			public override string ToString()
			{
				return string.Format("EventConfigurator({0})", _configuration);
			}
		}

		#endregion
	}
}
