// See https://aka.ms/new-console-template for more information

using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RetryHttpClient;

// The Microsoft.Extensions.Logging package provides this one-liner
// to add logging services.
var serviceCollection = new ServiceCollection()
    .AddLogging(cfg => cfg.AddConsole())
    .Configure<LoggerFilterOptions>(cfg => cfg.MinLevel = LogLevel.Information);

var containerBuilder = new ContainerBuilder();

// Once you've registered everything in the ServiceCollection, call
// Populate to bring those registrations into Autofac. This is
// just like a foreach over the list of things in the collection
// to add them to Autofac.
containerBuilder.Populate(serviceCollection);

// Make your Autofac registrations. Order is important!
// If you make them BEFORE you call Populate, then the
// registrations in the ServiceCollection will override Autofac
// registrations; if you make them AFTER Populate, the Autofac
// registrations will override. You can make registrations
// before or after Populate, however you choose.
containerBuilder.RegisterType<RetryPolicyRegistry>().As<IRetryPolicyRegistry>();
containerBuilder.RegisterType<ApiClient>().As<IApiClient>();

// Creating a new AutofacServiceProvider makes the container
// available to your app using the Microsoft IServiceProvider
// interface so you can use those abstractions rather than
// binding directly to Autofac.
var container = containerBuilder.Build();
var serviceProvider = new AutofacServiceProvider(container);

var apiClient = container.Resolve<IApiClient>();
var result = await apiClient.GetAsync("https://httpstat.us/503").ConfigureAwait(false);

Console.WriteLine($"Hello, World! {result}");