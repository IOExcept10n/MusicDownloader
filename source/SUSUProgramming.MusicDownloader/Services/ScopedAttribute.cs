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
}
