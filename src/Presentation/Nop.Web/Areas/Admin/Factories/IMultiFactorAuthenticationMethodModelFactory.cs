using Nop.Web.Areas.Admin.Models.MultiFactorAuthentication;

namespace Nop.Web.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the multifactor authentication method model factory
    /// </summary>
    public partial interface IMultiFactorAuthenticationMethodModelFactory
    {
        /// <summary>
        /// Prepare multifactor authentication method search model
        /// </summary>
        /// <param name="searchModel">Multifactor authentication method search model</param>
        /// <returns>Multifactor authentication method search model</returns>
        MultiFactorAuthenticationMethodSearchModel PrepareMultiFactorAuthenticationMethodSearchModel(
            MultiFactorAuthenticationMethodSearchModel searchModel);

        /// <summary>
        /// Prepare paged multifactor authentication method list model
        /// </summary>
        /// <param name="searchModel">Multifactor authentication method search model</param>
        /// <returns>Multifactor authentication method list model</returns>
        MultiFactorAuthenticationMethodListModel PrepareMultiFactorAuthenticationMethodListModel(
            MultiFactorAuthenticationMethodSearchModel searchModel);
    }
}
