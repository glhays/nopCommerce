using System.Collections.Generic;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Plugins;

namespace Nop.Services.Authentication.MultiFactor
{
    /// <summary>
    /// Represents an multifactor authentication plugin manager implementation
    /// </summary>
    public partial class MultiFactorAuthenticationPluginManager : PluginManager<IMultiFactorAuthenticationMethod>, IMultiFactorAuthenticationPluginManager
    {
        #region Fields

        private readonly MultiFactorAuthenticationSettings _multiFactorAuthSettings;

        #endregion

        #region Ctor

        public MultiFactorAuthenticationPluginManager(MultiFactorAuthenticationSettings multiFactorAuthSettings,
            ICustomerService customerService,
            IPluginService pluginService) : base(customerService, pluginService)
        {
            _multiFactorAuthSettings = multiFactorAuthSettings;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Load active multifactor authentication methods
        /// </summary>
        /// <param name="customer">Filter by customer; pass null to load all plugins</param>
        /// <param name="storeId">Filter by store; pass 0 to load all plugins</param>
        /// <returns>List of active multifactor authentication methods</returns>
        public virtual IList<IMultiFactorAuthenticationMethod> LoadActivePlugins(Customer customer = null, int storeId = 0)
        {
            return LoadActivePlugins(_multiFactorAuthSettings.ActiveAuthenticationMethodSystemNames, customer, storeId);
        }

        /// <summary>
        /// Check whether the passed multifactor authentication method is active
        /// </summary>
        /// <param name="authenticationMethod">Authentication method to check</param>
        /// <returns>Result</returns>
        public virtual bool IsPluginActive(IMultiFactorAuthenticationMethod authenticationMethod)
        {
            return IsPluginActive(authenticationMethod, _multiFactorAuthSettings.ActiveAuthenticationMethodSystemNames);
        }

        /// <summary>
        /// Check whether the multifactor authentication method with the passed system name is active
        /// </summary>
        /// <param name="systemName">System name of authentication method to check</param>
        /// <param name="customer">Filter by customer; pass null to load all plugins</param>
        /// <param name="storeId">Filter by store; pass 0 to load all plugins</param>
        /// <returns>Result</returns>
        public virtual bool IsPluginActive(string systemName, Customer customer = null, int storeId = 0)
        {
            var authenticationMethod = LoadPluginBySystemName(systemName, customer, storeId);
            return IsPluginActive(authenticationMethod);
        }

        #endregion

    }
}
