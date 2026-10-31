using Microsoft.Extensions.DependencyInjection;

using OnForkHub.Core.Attributes;

namespace OnForkHub.CrossCutting.Tests.DependencyInjection.Fixtures;

[AutoRegister(AsImplementedInterfaces = false, AsSelf = true, Lifetime = ServiceLifetime.Singleton)]
public sealed class SelfOnlyClock;
