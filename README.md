Stateful
===
**State Machine Construction Kit for .NET**

Why yet another state machine library?
---

I looked at several other state machine implementations:

* [Windows Workflow Foundation](http://msdn.microsoft.com/en-gb/vstudio/jj684582.aspx) (of course)
* [Appccelerate.StateMachine](https://github.com/appccelerate/statemachine)
* [bbv.Common.StateMachine](https://code.google.com/p/bbvcommon/wiki/StateMachineTutorial)
* [Stateless](https://github.com/nblumhardt/stateless)
* [SimpleStateMachine](http://simplestatemachine.codeplex.com)
* [Solid.State](https://code.google.com/p/solid-state)
* [StateMachineToolkit](https://github.com/OmerMor/StateMachineToolkit/tree/master/src/StateMachineToolkit)

Unfortunately, none of them satisfied my needs. My requirements were:

* **Events should be able to carry data** - for example, hypothetical event `KeyPressed` should also carry information which key has been pressed;
* **States should be able hold data** - for example, state collecting key presses (let's call it `EnteringText`) should be able to hold a list of keys pressed so far;
* **Guard statements should have access to both current state and event** - for example, `KeyPressed` event may cause transition to different state depending which key has been pressed;

Windows Workflow Foundation is just scary, apart from the fact that State Machine is just not available in .NET 4.0.           

Stateful has been inspired by [Stateless](https://github.com/nblumhardt/stateless). Actually, I would most likely settle with [Stateless](https://github.com/nblumhardt/stateless) if it was passing event arguments to `.If(...)` predicate.

Because it wasn't I decided I would like to have a State Machine with slightly different approach.

StateMachine
---
`StateMachine` is actually a wrapper class for both `IConfigurator` and `IExecutor` providing data types for: Context (your data), State (base class for all the states) and Event (base class for all events).

Interface:

	public static class StateMachine<TContext, TState, TEvent>
	{
		IConfigurator NewConfigurator();
	} 

Example:

	var configurator = StateMachine<TContext, TState, TEvent>.NewConfigurator();   

StateMachine.IConfigurator
---
`IConfigurator` is an interface used to configure states and events. It has two methods: `In<TActualState>()` and `On<TActualState, TActualEvent>()`. `In` is used to configure state, while `On` is used to configure event.

Interface:

	public static class StateMachine<TContext, TState, TEvent>
	{
		public interface IConfigurator
		{
			IStateConfigurator<TActualState> In<TActualState>()
				where TActualState: TState;
	
			IEventConfigurator<TActualState, TActualEvent> On<TActualState, TActualEvent>()
				where TActualState: TState
				where TActualEvent: TEvent;
		}
	}

Example:

	var configurator = StateMachine<TContext, TState, TEvent>.NewConfigurator();   
	configurator.In<TState1>()
		.OnEnter(c => Console.WriteLine("entering TState1"))
		.OnExit(c => Console.WriteLine("exiting TState1"));
	configure.On<TState1, TEvent1>()
		.OnTrigger(c => Console.WriteLine("received TEvent1 in TState1"))
		.Goto(c => new TState2());

StateMachine.IStateConfigurator
---
Allows to configure state with `OnEnter`, `OnExit` and `On<TActualEvent>` handlers.
   
Interface:

	public static class StateMachine<TContext, TState, TEvent>
	{
		public interface IStateConfigurator<out TActualState>
			where TActualState: TState
		{
			IStateConfigurator<TActualState> OnEnter(Action<IStateContext<TActualState>> context);
			IStateConfigurator<TActualState> OnExit(Action<IStateContext<TActualState>> context);
			IEventConfigurator<TActualState, TActualEvent> On<TActualEvent>()
				where TActualEvent: TEvent;
		}
	}

Example:

	var configurator = StateMachine<TContext, TState, TEvent>.NewConfigurator();
	
	// note: both constructs below do the same thing
	configurator.In<TState1>().On<TEvent1>().Goto(c => new TState2());
	configurator.On<TState1, TEvent1>().Goto(c => new TState2());

StateMachine.IEventConfigurator
---
Allows to configure event. Use `When` to filter events, and `OnTrigger` to execute any code when event is triggered. `Goto` is used to make transition to another state, while `Loop` is used to stay in the same state without triggering `OnExit` and `OnEnter` handlers. 

Interface:

	public static class StateMachine<TContext, TState, TEvent>
	{
		public interface IEventConfigurator<out TActualState, out TActualEvent>
			where TActualState: TState
			where TActualEvent: TEvent
		{
			IEventConfigurator<TActualState, TActualEvent> OnTrigger(
				Action<IEventContext<TActualState, TActualEvent>> action);
			IEventConfigurator<TActualState, TActualEvent> When(
				Func<IEventContext<TActualState, TActualEvent>, bool> predicate);
			IEventConfigurator<TActualState, TActualEvent> Goto(
				Func<IEventContext<TActualState, TActualEvent>, TState> action);
			IEventConfigurator<TActualState, TActualEvent> Loop();
		}
	}

Example:

	var configurator = StateMachine<TContext, TState, TEvent>.NewConfigurator();
	
	configurator.In<TState1>().On<TEvent1>()
		.When(c => c.Event.Sender.Name == "Mike")
		.Goto(c => new TState2("Got here because of Mike"));

StateMachine.IExecutor
---
