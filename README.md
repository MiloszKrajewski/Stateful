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

