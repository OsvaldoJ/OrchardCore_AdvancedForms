using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using OrchardCore.Mvc.ActionConstraints;
using Microsoft.Extensions.Logging;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.ContentManagement.Metadata;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Settings;
using YesSql;
using OrchardCore.Admin;
using AdvancedForms.ViewModels;

namespace AdvancedForms.Controllers
{

    [Admin]
    public class AdminController : Controller, IUpdateModel
    {
        private const string _id = "AdvancedForm";
        private readonly IContentManager _contentManager;
        private readonly IContentDefinitionManager _contentDefinitionManager;
        private readonly ISiteService _siteService;
        private readonly ISession _session;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly INotifier _notifier;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger _logger;

        public AdminController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IContentDefinitionManager contentDefinitionManager,
            ISiteService siteService,
            INotifier notifier,
            ISession session,
            IShapeFactory shapeFactory,
            ILogger<AdminController> logger,
            IHtmlLocalizer<AdminController> localizer,
            IAuthorizationService authorizationService
            )
        {
            _authorizationService = authorizationService;
            _notifier = notifier;
            _contentItemDisplayManager = contentItemDisplayManager;
            _session = session;
            _siteService = siteService;
            _contentManager = contentManager;
            _contentDefinitionManager = contentDefinitionManager;
            _logger = logger; 
            T = localizer;
        }

        public IHtmlLocalizer T { get; }

        public async Task<IActionResult> Create()
        {
            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageOwnAdvancedForms))
            {
                return Unauthorized();
            }

            return View();
        }

        [HttpPost, ActionName("Create")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> CreateAndPublishPOST(AdvancedFormViewModel viewModel)
        {
            // pass a dummy content to the authorization check to check for "own" variations
            var dummyContent = await _contentManager.NewAsync(_id);

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdvancedForms, dummyContent))
            {
                return Unauthorized();
            }

            //var x = new DefaultContentManager(_contentDefinitionManager, null, _session, null, null);   
            //x.CreateAsync(dummyContent.ContentItem, )

            return await CreatePOST(viewModel, async contentItem =>
            {
                await _contentManager.PublishAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                _notifier.Success(T["Your Adavanced Form has been published."]);
            });
        }

        private async Task<IActionResult> CreatePOST(AdvancedFormViewModel viewModel, Func<ContentItem, Task> conditionallyPublish)
        {
            var contentItem = await _contentManager.NewAsync(_id);

            //if (!await _authorizationService.AuthorizeAsync(User, Permissions.EditContent, contentItem))
            //{
            //    return Unauthorized();
            //}

            contentItem.Content.AdvancedForm.Title = viewModel.Title;
            contentItem.Content.AdvancedForm.Description = "{\"Text\":" + viewModel.Description + "}";
            contentItem.Content.AdvancedForm.Instructions = "{\"Text\":" + viewModel.Instructions + "}";
            contentItem.Content.AdvancedForm.Container = "{\"Text\":" + viewModel.Container + "}";

            //var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this, true);
            
            //if (!ModelState.IsValid)
            //{
            //    _session.Cancel();
            //    return View(new AdvancedFormViewModel());
            //}

            await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);

            await conditionallyPublish(contentItem);
            return View(viewModel);
        }
    }

    public class MyPart : ContentPart
    {
        public string Text { get; set; }
    }
}
