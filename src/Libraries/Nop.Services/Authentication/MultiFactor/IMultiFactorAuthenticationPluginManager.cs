using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Plugins;

namespace Nop.Services.Authentication.MultiFactor
{
    /// <summary>
    /// Represents an multifactor authentication plugin manager
    /// </summary>
    public partial interface IMultiFactorAuthenticationPluginManager : IPluginManager<IMultiFactorAuthenticationMethod>
    {
        /// <summary>
        /// Load active multifactor authentication methods
        /// </summary>
        /// <param name="customer">Filter by customer; pass null to load all plugins</param>
        /// <param name="storeId">Filter by store; pass 0 to load all plugins</param>
        /// <returns>List of active multifactor authentication methods</returns>
        IList<IMultiFactorAuthenticationMethod> LoadActivePlugins(Customer customer = null, int storeId = 0);

        /// <summary>
        /// Check whether the passed multifactor authentication method is active
        /// </summary>
        /// <param name="authenticationMethod">Multifactor authentication method to check</param>
        /// <returns>Result</returns>
        bool IsPluginActive(IMultiFactorAuthenticationMethod authenticationMethod);

        /// <summary>
        /// Check whether the multifactor authentication method with the passed system name is active
        /// </summary>
        /// <param name="systemName">System name of multifactor authentication method to check</param>
        /// <param name="customer">Filter by customer; pass null to load all plugins</param>
        /// <param name="storeId">Filter by store; pass 0 to load all plugins</param>
        /// <returns>Result</returns>
        bool IsPluginActive(string systemName, Customer customer = null, int storeId = 0);
    }
}
