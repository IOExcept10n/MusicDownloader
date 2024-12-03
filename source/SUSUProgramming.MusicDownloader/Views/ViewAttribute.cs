using System;

namespace SUSUProgramming.MusicDownloader.Views
{
    /// <summary>
    /// Marks class as view ready to work with services collection in app DI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ViewAttribute : Attribute
    {
    }
}
