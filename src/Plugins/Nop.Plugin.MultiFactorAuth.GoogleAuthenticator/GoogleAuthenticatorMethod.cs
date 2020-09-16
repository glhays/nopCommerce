using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;

namespace Nop.Plugin.MultiFactorAuth.GoogleAuthenticator
{
    /// <summary>
    /// Represents method for the multifactor authentication with Google Authenticator
    /// </summary>
    public class GoogleAuthenticatorMethod : BasePlugin, IMultiFactorAuthenticationMethod
    {
        #region Fields

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IUrlHelperFactory _urlHelperFactory;

        #endregion

        #region Ctor

        public GoogleAuthenticatorMethod(IActionContextAccessor actionContextAccessor,
            ILocalizationService localizationService,
            ISettingService settingService,
            IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _localizationService = localizationService;
            _settingService = settingService;
            _urlHelperFactory = urlHelperFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext).RouteUrl(GoogleAuthenticatorDefaults.ConfigurationRouteName);
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in public store
        /// </summary>
        /// <returns>View component name</returns>
        public string GetPublicViewComponentName()
        {
            return GoogleAuthenticatorDefaults.VIEW_COMPONENT_NAME;
        }

        /// <summary>
        /// Gets a name of a view component for displaying plugin in login page
        /// </summary>
        /// <returns>View component name</returns>
        public string GetLoginViewComponentName()
        {
            return GoogleAuthenticatorDefaults.VERIFICATION_VIEW_COMPONENT_NAME;
        }

        /// <summary>
        /// Gets a name of a view component for displaying verification page
        /// </summary>
        /// <returns>View component name</returns>
        public string GetVerificationViewComponentName()
        {
            return GoogleAuthenticatorDefaults.VERIFICATION_VIEW_COMPONENT_NAME;
        }

        /// <summary>
        /// Install the plugin
        /// </summary>
        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new GoogleAuthenticatorSettings()
            {
                QRPixelsPerModule = GoogleAuthenticatorDefaults.DefaultQRPixelsPerModule
            });

            //locales
            _localizationService.AddLocaleResource(new Dictionary<string, string>
            {
                //admin config 
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.QRPixelsPerModule"] = "QRPixelsPerModule",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.QRPixelsPerModule.Hint"] = "Sets the number of pixels per unit. The module is one square in the QR code. By default is 3 for a 171x171 pixel image.",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Instructions"] = "To use Google Authenticator, the app is first installed on a smartphone. The plugin provides a shared secret key to the user over a secure channel, to be stored in the Google Authenticator app. This secret key will be used for all future logins to the site.",

                //db fields
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Fields.Customer"] = "Customer",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Fields.SecretKey"] = "Secret key",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Fields.StoreId"] = "Store ID",

                //customer config
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Customer.VerificationToken"] = "Google Authenticator Code",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Customer.ManualSetupCode"] = "Manual entry setup code",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Customer.SendCode"] = "Confirm",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Customer.Instruction"] = "Please download the app Google Authenticator to scan this QR code. If you already have a verified account with Google Authenticator, then you can change the parameters of your account by registering on this page. Your data in the system will be updated.",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Customer.InstructionManual"] = "You can not scan code? You can add the entry manually, please provide the following details to the application on your phone.",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Customer.Account"] = "Account: ",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Customer.TypeKey"] = "Time based : Yes",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Customer.Key"] = "Key: ",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.MFAMethodDescription"] = "Google Authenticator is a software-based authenticator by Google that implements two-step verification services, for authenticating users",

                //validators
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Fields.Code.Required"] = "Field cannot be empty. Enter the code from the Google Authenticator app no your mobile phone.",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Fields.Code.Wrong"] = "Field must be 6 digits. Enter the code from the Google Authenticator app no your mobile phone.",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Token.Unsuccessful"] = "Invalid token or its lifetime has expired.",
                ["Plugins.MultiFactorAuth.GoogleAuthenticator.Token.Successful"] = "Configuration of Google Authenticator for current customer saved successful."
            });

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<GoogleAuthenticatorSettings>();

            //locales
            _localizationService.DeleteLocaleResources("Plugins.MultiFactorAuth.GoogleAuthenticator");

            base.Uninstall();
        }

        #endregion

        #region Properies

        public MultiFactorAuthenticationType MultiFactorAuthenticationType => MultiFactorAuthenticationType.ApplicationVerification;

        /// <summary>
        /// Gets a multi-factor authentication method description that will be displayed on customer info pages in the public store
        /// </summary>
        public string MultiFactorAuthenticationMethodDescription => _localizationService.GetResource("Plugins.MultiFactorAuth.GoogleAuthenticator.MFAMethodDescription");

        #endregion
    }
}
