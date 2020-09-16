using Nop.Services.Plugins;

namespace Nop.Services.Authentication.MultiFactor
{
    /// <summary>
    /// Represents method for the multifactor authentication
    /// </summary>
    public partial interface IMultiFactorAuthenticationMethod : IPlugin
    {
        /// <summary>
        ///  Gets a multifactor authentication type
        /// </summary>
        MultiFactorAuthenticationType MultiFactorAuthenticationType { get; }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store
        /// </summary>
        /// <returns>View component name</returns>
        string GetPublicViewComponentName();

        /// <summary>
        /// Gets a name of a view component for displaying plugin in login page
        /// </summary>
        /// <returns>View component name</returns>
        string GetLoginViewComponentName();
    }
}
