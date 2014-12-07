using System;

namespace Stateful
{
	public static partial class StateMachine<TContext, TState, TEvent>
	{
		#region state context

		public interface IStateContext<out TActualState>
			where TActualState: TState
		{
			TContext Context { get; }
			TActualState State { get; }
		}

		private class StateContext<TActualState>:
			IStateContext<TActualState>
			where TActualState: TState
		{
			public TContext Context { get; set; }
			public TActualState State { get; set; }
		}

		private static IStateContext<TActualState> MakeContext<TActualState>(
			TContext context, TState state)
			where TActualState: TState
		{
			return new StateContext<TActualState> { Context = context, State = (TActualState)state };
		}

		#endregion

		#region state configuration

		public interface IStateConfigurator<out TActualState>
			where TActualState: TState
		{
			IStateConfigurator<TActualState> Name(string name);
			IStateConfigurator<TActualState> OnEnter(Action<IStateContext<TActualState>> context);
			IStateConfigurator<TActualState> OnExit(Action<IStateContext<TActualState>> context);
		}

		public interface IStateConfiguration
		{
			Type StateType { get; }
			string Name { get; }
			Action<TContext, TState> OnEnter { get; }
			Action<TContext, TState> OnExit { get; }
		}

		private class StateConfiguration: IStateConfiguration
		{
			public StateConfiguration(Type stateType)
			{
				StateType = stateType;
			}

			public StateConfiguration(IStateConfiguration other)
			{
				StateType = other.StateType;
				OnEnter = other.OnEnter;
				OnExit = other.OnExit;
			}

			public Type StateType { get; private set; }
			public string Name { get; set; }
			public Action<TContext, TState> OnEnter { get; set; }
			public Action<TContext, TState> OnExit { get; set; }

			public override string ToString()
			{
				return string.Format(
					"StateConfiguration(Type:{0}, Name:'{1}', OnEnter:{2}, OnExit:{3}",
					StateType.Name, Name, OnEnter != null, OnExit != null);
			}
		}

		private class StateConfigurator<TActualState>: IStateConfigurator<TActualState>
			where TActualState: TState
		{
			private readonly StateConfiguration _configuration;

			public StateConfigurator(StateConfiguration configuration)
			{
				_configuration = configuration;
			}

			private string TextId { get { return string.Format("State({0})", _configuration.StateType.Name); } }

			public IStateConfigurator<TActualState> Name(string name)
			{
				if (_configuration.Name != null)
					throw new InvalidOperationException(
						string.Format("{0}.Name has been already defined.", TextId));
				_configuration.Name = name;
				return this;
			}

			public IStateConfigurator<TActualState> OnEnter(Action<IStateContext<TActualState>> action)
			{
				if (action == null)
					throw new ArgumentNullException("action");
				if (_configuration.OnEnter != null)
					throw new InvalidOperationException(
						string.Format("{0}.OnEnter has been already defined", TextId));
				_configuration.OnEnter = (c, s) => action(MakeContext<TActualState>(c, s));
				return this;
			}

			public IStateConfigurator<TActualState> OnExit(Action<IStateContext<TActualState>> action)
			{
				if (action == null)
					throw new ArgumentNullException("action");
				if (_configuration.OnExit != null)
					throw new InvalidOperationException(
						string.Format("{0}.OnExit has been already defined", TextId));
				_configuration.OnExit = (c, s) => action(MakeContext<TActualState>(c, s));
				return this;
			}

			public override string ToString()
			{
				return string.Format("StateConfigurator({0})", _configuration);
			}
		}

		#endregion
	}
}
