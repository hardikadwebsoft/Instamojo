using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Payments.Instamojo.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.Instamojo.Controllers
{
    public class PaymentInstamojo : BasePaymentController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public PaymentInstamojo(
            IOrderService orderService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<IActionResult> Configure()
        {
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<InstamojoSettings>(storeScope);
            var model = new ConfigurationModel
            {

                ClientId =  settings.ClientId ,
                SecretKey = settings.SecretKey,
                ActiveStoreScopeConfiguration = storeScope,
            };
            if (storeScope <= 0)
                return View("~/Plugins/Payments.Instamojo/Views/Configure.cshtml", model);

            model.ClientId_OverrideForStore = await _settingService.SettingExistsAsync(settings ,x => x.ClientId, storeScope);
            model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.SecretKey, storeScope);
           
            return View("~/Plugins/Payments.Instamojo/Views/Configure.cshtml", model);

        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<InstamojoSettings>(storeScope);

            //save settings
            settings.ClientId = model.ClientId;
            settings.SecretKey = model.SecretKey;
           
            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ClientId, model.ClientId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
          
            //now clear settings cache
            await _settingService.ClearCacheAsync();
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }


        #endregion
    }
}
