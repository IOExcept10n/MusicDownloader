// Copyright 2024 (c) IOExcept10n (contact https://github.com/IOExcept10n)
// Distributed under MIT license. See LICENSE.md file in the project root for more information
using System;

namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Indicates that a class should be registered as a singleton service in a dependency injection container.
    /// A singleton service is created once and shared throughout the application's lifetime.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    internal class SingletonAttribute : Attribute
    {
    }
}