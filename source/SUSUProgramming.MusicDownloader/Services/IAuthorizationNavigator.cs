namespace SUSUProgramming.MusicDownloader.Services
{
    /// <summary>
    /// Represents a helper class for the authorization navigation flow.
    /// </summary>
    internal interface IAuthorizationNavigator
    {
        /// <summary>
        /// Opens main authorization page.
        /// </summary>
        void OpenAuthorizationPage();

        /// <summary>
        /// Opens the page that indicates authorization success.
        /// </summary>
        void OnAuthorizationSucceeded();

        /// <summary>
        /// Opens the page that indicates authorization fault.
        /// </summary>
        void OnAuthorizationFailed();
    }
}
