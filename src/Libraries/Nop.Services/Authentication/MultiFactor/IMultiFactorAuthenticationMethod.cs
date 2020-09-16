using Microsoft.Azure.Storage.Blob.Protocol;
using Nop.Services.Plugins;

namespace Nop.Services.Authentication.MultiFactor
{
    /// <summary>
    /// Represents method for the multifactor authentication
    /// </summary>
    public partial interface IMultiFactorAuthenticationMethod : IPlugin
    {
        #region Methods

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

        #endregion

        #region Properties

        /// <summary>
        /// Gets a multi-factor authentication method description that will be displayed on customer info pages in the public store
        /// </summary>
        string MultiFactorAuthenticationMethodDescription { get; }

        #endregion
    }
}
