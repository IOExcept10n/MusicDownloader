using System;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Indicates that a class should be registered as a scoped service in a dependency injection container.
    /// A scoped service is created once per request within the scope.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class ScopedAttribute : Attribute
    {
    }

    /// <summary>
    /// Indicates that a class should be registered as a singleton service in a dependency injection container.
    /// A singleton service is created once and shared throughout the application's lifetime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class SingletonAttribute : Attribute
    {
    }

}
