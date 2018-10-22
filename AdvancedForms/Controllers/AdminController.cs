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
using AdvancedForms.Models;
using Newtonsoft.Json.Linq;
using OrchardCore.ContentManagement.Metadata.Settings;
using Microsoft.AspNetCore.Routing;

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

            return View(new AdvancedFormViewModel());
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

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdvancedForms, contentItem))
            {
                return Unauthorized();
            }

            var advForm = new AdvancedForm(viewModel.Description, viewModel.Instructions, 
                viewModel.Container, viewModel.Title);
            var titlePart = new TitlePart(viewModel.Title);
            contentItem.Content.AdvancedForm = JToken.FromObject(advForm);
            contentItem.Content.TitlePart = JToken.FromObject(titlePart);
            contentItem.Content.AutoroutePart.Path = CreatePath(viewModel.Title);

            var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this, true);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View(viewModel);
            }

            await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);
           
            await conditionallyPublish(contentItem);
            return View(viewModel);
        }

        public async Task<IActionResult> Edit(string contentItemId, string returnUrl)
        {
            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdvancedForms, contentItem))
            {
                return Unauthorized();
            }

            var model = new AdvancedFormViewModel
            {
                Id = contentItemId,
                Title = contentItem.Content.AdvancedForm.Title,
                Container = contentItem.Content.AdvancedForm.Container.Html,
                Description = contentItem.Content.AdvancedForm.Description.Html,
                Instructions = contentItem.Content.AdvancedForm.Instructions.Html
            };

            return View("Create", model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Save")]
        public Task<IActionResult> EditPOST(AdvancedFormViewModel viewModel, string returnUrl)
        {
            return EditPOST(viewModel, returnUrl, contentItem =>
            {
                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                _notifier.Success(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? T["Your content draft has been saved."]
                    : T["Your {0} draft has been saved.", typeDefinition.DisplayName]);

                return Task.CompletedTask;
            });
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("submit.Publish")]
        public async Task<IActionResult> EditAndPublishPOST(AdvancedFormViewModel viewModel, string returnUrl)
        {
            string contentItemId = viewModel.Id;

            var content = await _contentManager.GetAsync(contentItemId, VersionOptions.Latest);

            if (content == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdvancedForms, content))
            {
                return Unauthorized();
            }

            return await EditPOST(viewModel, returnUrl, async contentItem =>
            {
                await _contentManager.PublishAsync(contentItem);

                var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

                _notifier.Success(string.IsNullOrWhiteSpace(typeDefinition.DisplayName)
                    ? T["Your content has been published."]
                    : T["Your {0} has been published.", typeDefinition.DisplayName]);
            });
        }

        private async Task<IActionResult> EditPOST(AdvancedFormViewModel viewModel, string returnUrl, Func<ContentItem, Task> conditionallyPublish)
        {
            string contentItemId = viewModel.Id;

            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.DraftRequired);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ManageAdvancedForms, contentItem))
            {
                return Unauthorized();
            }

            var advForm = new AdvancedForm(viewModel.Description, viewModel.Instructions,
                viewModel.Container, viewModel.Title);
            var titlePart = new TitlePart(viewModel.Title);
            contentItem.Content.AdvancedForm = JToken.FromObject(advForm);
            contentItem.Content.TitlePart = JToken.FromObject(titlePart);
            contentItem.Content.AutoroutePart = CreatePath(viewModel.Title);
            contentItem.Content.AutoroutePart.Path = CreatePath(viewModel.Title);

            var model = new AdvancedFormViewModel
            {
                Id = viewModel.Id,
                Title = contentItem.Content.AdvancedForm.Title,
                Container = contentItem.Content.AdvancedForm.Container.Html,
                Description = contentItem.Content.AdvancedForm.Description.Html,
                Instructions = contentItem.Content.AdvancedForm.Instructions.Html
            };               

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return View("Create", model);
            }

            await conditionallyPublish(contentItem);

            // The content item needs to be marked as saved (again) in case the drivers or the handlers have
            // executed some query which would flush the saved entities. In this case the changes happening in handlers 
            // would not be taken into account.
            _session.Save(contentItem);
            
            var typeDefinition = _contentDefinitionManager.GetTypeDefinition(contentItem.ContentType);

            if (returnUrl == null)
            {
                return RedirectToAction("Edit", new RouteValueDictionary { { "ContentItemId", viewModel.Id } });
            }
            else
            {
                return LocalRedirect(returnUrl);
            }
        }

        private string CreatePath(string title)
        {
            if (!string.IsNullOrEmpty(title))
            {
                title = GetType().Namespace + "/" + title.Replace(" ","-");
            }
            return title; 
        }
    }               
}
