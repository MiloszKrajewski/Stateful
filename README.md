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

Because it wasn't, I decided I would like to have a State Machine with slightly different approach. 

StateMachine
---
`StateMachine` is actually a wrapper class for both `IConfigurator` and `IExecutor` providing data types for: Context (your data), State (base class for all the states) and Event (base class for all events).

Interface:

	public static class StateMachine<TContext, TState, TEvent>
	{
		IConfigurator NewConfigurator();
	} 

Example:

	var configurator = StateMachine<Context, State, Event>.NewConfigurator();   

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

	var configurator = StateMachine<Context, State, Event>.NewConfigurator();   
	configurator.In<State1>()
		.OnEnter(c => Console.WriteLine("entering State1"))
		.OnExit(c => Console.WriteLine("exiting State1"));
	configure.On<State1, Event1>()
		.OnTrigger(c => Console.WriteLine("received Event1 in State1"))
		.Goto(c => new State2());

StateMachine.IStateConfigurator
---
Allows to configure state with `OnEnter`, `OnExit` and `On<TActualEvent>` handlers.
   
Interface:

	public static class StateMachine<TContext, TState, TEvent>
	{
		public interface IStateConfigurator<out TActualState>
			where TActualState: TState
		{
			IStateConfigurator<TActualState> OnEnter(
				Action<IStateContext<TActualState>> context);
			IStateConfigurator<TActualState> OnExit(
				Action<IStateContext<TActualState>> context);
			IEventConfigurator<TActualState, TActualEvent> On<TActualEvent>()
				where TActualEvent: TEvent;
		}
	}

Example:

	var configurator = StateMachine<Context, State, Event>.NewConfigurator();
	
	// note: both constructs below do the same thing
	configurator.In<State1>().On<Event1>().Goto(c => new State2());
	configurator.On<State1, Event1>().Goto(c => new State2());

StateMachine.IEventConfigurator
---
Allows to configure event. Use `When` to filter events, and `OnTrigger` to execute any code when event is triggered. `Goto` is used to make transition to another state, while `Loop` is used to stay in the same state without triggering state handlers. For example, after both `.Goto(c => c.State)` and `.Loop()` state machine will stay in the same state the `.Goto(...)` transition will trigger both `OnExit` and `OnEnter` handlers (in this order).

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

	var configurator = StateMachine<Context, State, Event>.NewConfigurator();
	
	configurator.In<State1>().On<Event1>()
		.OnTrigger(c => Console.WriteLine("Triggered!"))
		.When(c => c.Event.Sender.Name == "Mike")
		.Goto(c => new TState2("Got here because of Mike"));

IStateContext and IEventContext
---
Handlers in both State and Event definition (`OnEnter`, `OnExit`, `OnTrigger`, `When`, `Goto`) take `IStateContext` and `IEventContext` respectively.

	public interface IStateContext<out TActualState>
		where TActualState: TState
	{
		TContext Context { get; }
		TActualState State { get; }
	}

	public interface IEventContext<out TActualState, out TActualEvent>: 
		IStateContext<TActualState>
		where TActualState: TState
		where TActualEvent: TEvent
	{
		TActualEvent Event { get; }
	}

They allow to access Context, State and Event when trigger, for example:

	configurator.In<State1>()
		.OnEnter(c => Console.WriteLine(c.State.Name)); // c.State is State1 

	configurator.In<State1>().On<Event1>()
		.OnTrigger(c => Console.WriteLine("Triggered!"))
		.When(c => c.Event.Sender.Name == "Mike") // c.Event is Event1
		.Goto(c => new State2("Got here because of Mike"));
 

StateMachine.IExecutor
---
Executor is the object allowing to walk through states or in other word execute state machine by feeding it with events.

Interface:

	public interface IExecutor
	{
		TContext Context { get; }
		TState State { get; }

		void Fire(TEvent @event);
	}

So, as you can see it allows to fire event, access state machine's context and current state. Usually, the loop used for execution would be something link this:

	foreach (var e in eventStream)
	{
		if (executor.State is TerminalState)
			break;
		executor.Fire(e);
	}

Note, `TerminalState` is your terminal state, whatever is your terminal state. The events may be fed with `IEnumerable<TEvent>` (as in this example) or provided by `IObservable<TEvent>`. In some state machines it would be important to generate Idle-like event so I would suggest blocking queue with a loop like this:

	while (true)
	{
		if (executor.State is TerminalState)
			break;
		TEvent e;
		var success = eventQueue.TryTake(out e, timeout);
		executor.Fire(success ? e : new IdleEvent());
	}   

Hierarchical state machine
---
Hierarchy comes naturally to this state machine as events and states are classes so you can use class hierarchy to build events. Let's say you have:

	class Event {}
	class IdleEvent: Event {}
	class SomeEvent: Event {}

	class State {}
	class StateA: State {}
	class StateB: State {}
	class StateC: State {}

configuring the state machine like this:

	config.On<State, IdleEvent>()
		.OnTrigger(c => Console.WriteLine("idle..."))
		.Loop();
	config.On<StateC, IdleEvent>()
		.OnTrigger(c => Console.WriteLine("idle in C - going to A"))
		.Goto(c => new StateA());

will handle `IdleEvent` in all states the same way (by printing 'idle...' and staying in the same state), except for `StateC` where it will go to `StateA`.
Definition "closer" in terms of inheritance distance takes precedence. The distance to interface is a little bit tricky so it is advised to not use interface for rule definition. It will work but results may be surprising sometimes.
Let's imagine a class A which implements interface I and a class B which inherits from A. It is not possible to determine if B implements I on it's own (distance 1) or only because A implements it (distance 2). Stateful assumes longest inheritance path otherwise distance would be always 1.

Notes on performance
---
This is not the fastest state machine in the world. Approach of using classes for both states and events and allowing hierarchical definitions has it's price. It tries to cache list of potential rules, so it does calculate "inheritance distance" only once per concrete type. There is still some reflection used though, so if you need very fast switching, fine grained state machine use something or roll your own. 

I would NOT recommend Stateful to implement you own Regular Expression engine. 

Simple Calculator, working example
---
Check unit tests for working example of simple calculator. With execution method shown below:

	public int Execute(string expression)
	{
		var calculator = CreateCalculator();

		foreach (var e in expression) // char-by-char
		{
			calculator.Fire(e);
			if (calculator.State is Result)
				break;
		}

		var result = calculator.State as Result;
		if (result == null)
			throw new ArgumentException("Expression ended prematurely");

		return result.Number;
	}

I was able to evaluate some simple expressions:

	[Test]
	public void SomeExpressions()
	{
		Assert.AreEqual(123, Execute("123="));
		Assert.AreEqual(123 + 546, Execute("123+546="));
		Assert.AreEqual(-123 - 546, Execute("-123-546="));
		Assert.AreEqual(-123 * -356, Execute("-123*-356="));
	}

See `CalculatorTests.cs` for details.
