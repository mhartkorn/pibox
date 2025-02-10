using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using PiBox.Hosting.Abstractions;
using PiBox.Hosting.Abstractions.DependencyInjection;

namespace PiBox.Testing
{
    public static class TestingDefaults
    {
        public static ServiceCollection ServiceCollection()
        {
            var sc = new ServiceCollection();
            sc.AddSingleton<TimeProvider, FakeTimeProvider>();
            sc.AddLogging();
            sc.AddTransient(typeof(IFactory<>), typeof(Factory<>));
            sc.AddSingleton<GlobalStatusCodeOptions>();
            return sc;
        }

        public static ServiceProvider ServiceProvider() => ServiceCollection().BuildServiceProvider();
    }
}
