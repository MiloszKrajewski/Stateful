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
			var configurator = StateMachine<object, object, object>.NewConfigurator();
			var stateConfigurator = configurator.In<TState>();
		}

		[Test]
		public void CanConfigureEventUsingOnKeyword()
		{
			var configurator = StateMachine<TContext, TState, TEvent>.NewConfigurator();
			var event1Configurator = configurator.On<TState, TEvent>();
			var event2Configurator = configurator.In<TState>().On<TEvent>();
		}
	}
}
