using System.Collections.Generic;
using Nop.Core.Configuration;

namespace Nop.Core.Domain.Customers
{
    /// <summary>
    /// Multifactor authentication settings
    /// </summary>
    public class MultiFactorAuthenticationSettings : ISettings
    {
        #region Ctor

        public MultiFactorAuthenticationSettings()
        {
            ActiveAuthenticationMethodSystemNames = new List<string>();
        }

        #endregion

        /// <summary>
        /// Gets or sets system names of active multifactor authentication methods
        /// </summary>
        public List<string> ActiveAuthenticationMethodSystemNames { get; set; }

    }
}
