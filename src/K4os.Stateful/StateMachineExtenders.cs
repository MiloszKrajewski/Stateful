namespace K4os.Stateful
{
	public static class StateMachineExtenders
	{
		public static StateMachine<TContext, TState, TEvent>.IExecutor
			NewExecutor<TContext, TState, TEvent>(
				this StateMachine<TContext, TState, TEvent>.IConfigurationProvider configuration,
				TContext context,
				TState state) =>
			StateMachine<TContext, TState, TEvent>.NewExecutor(configuration, context, state);
	}
}
