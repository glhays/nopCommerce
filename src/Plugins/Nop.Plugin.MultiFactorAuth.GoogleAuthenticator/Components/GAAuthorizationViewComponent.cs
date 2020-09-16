using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.MultiFactorAuth.GoogleAuthenticator.Factories;
using Nop.Plugin.MultiFactorAuth.GoogleAuthenticator.Models;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.MultiFactorAuth.GoogleAuthenticator.Components
{
    /// <summary>
    /// Represents view component for setting GoogleAuthenticator
    /// </summary>
    [ViewComponent(Name = GoogleAuthenticatorDefaults.VIEW_COMPONENT_NAME)]
    public class GAAuthorizationViewComponent : NopViewComponent
    {
        #region Fields

        private readonly AuthorizationModelFactory _authorizationModelFactory;

        #endregion

        #region Ctor

        public GAAuthorizationViewComponent(AuthorizationModelFactory authorizationModelFactory)
        {
            _authorizationModelFactory = authorizationModelFactory;
        }

        #endregion

        #region Methods

        /// <summary>
        ///  Invoke view component
        /// </summary>
        /// <param name="widgetZone">Widget zone name</param>
        /// <param name="additionalData">Additional data</param>
        /// <returns>View component result</returns>
        public IViewComponentResult Invoke(string widgetZone, object additionalData)
        {
            var model = new AuthModel();
            model = _authorizationModelFactory.PrepareAuthModel(model);

            return View("~/Plugins/MultiFactorAuth.GoogleAuthenticator/Views/Customer/GAAuthorization.cshtml", model);
        }

        #endregion
    }
}
