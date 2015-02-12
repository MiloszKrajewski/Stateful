using System;
using NUnit.Framework;

namespace Stateful.Tests
{
	[TestFixture]
	class ConfigurationTests
	{
		private class TContext { }

		private class TState { }

		private class TEvent { }

		[Test]
		public void CanConfigureStateUsingInKeyword()
		{
			var configurator = StateMachine<TContext, TState, TEvent>.NewConfigurator();
			var stateConfigurator = configurator.In<TState>();
		}

		[Test]
		public void CanConfigureEventUsingOnKeyword()
		{
			var configurator = StateMachine<TContext, TState, TEvent>.NewConfigurator();
			var event1Configurator = configurator.On<TState, TEvent>();
			var event2Configurator = configurator.In<TState>().On<TEvent>();
		}

		[Test]
		public void CannotConfigureOnEnterTwice()
		{
			var configurator = StateMachine<TContext, TState, TEvent>.NewConfigurator();
			configurator.In<TState>().OnEnter(c => { });
			Assert.Throws<InvalidOperationException>(() => configurator.In<TState>().OnEnter(c => { }));
		}

		[Test]
		public void CannotConfigureOnExitTwice()
		{
			var configurator = StateMachine<TContext, TState, TEvent>.NewConfigurator();
			configurator.In<TState>().OnExit(c => { });
			Assert.Throws<InvalidOperationException>(() => configurator.In<TState>().OnExit(c => { }));
		}
	}
}
