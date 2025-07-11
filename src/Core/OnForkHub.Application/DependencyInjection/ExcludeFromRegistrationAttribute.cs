namespace OnForkHub.Application.DependencyInjection;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class ExcludeFromRegistrationAttribute : Attribute { }
