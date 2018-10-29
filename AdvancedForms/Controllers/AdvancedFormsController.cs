using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display;
using OrchardCore.DisplayManagement.ModelBinding;
using System.Threading.Tasks;
using AdvancedForms.ViewModels;
using Newtonsoft.Json.Linq;
using AdvancedForms.Models;

namespace AdvancedForms.Controllers
{
    public class AdvancedFormsController : Controller
    {

        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentAliasManager _contentAliasManager;
        private const string _id = "AdvancedFormSubmissions";

        public AdvancedFormsController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IAuthorizationService authorizationService,
            IContentAliasManager contentAliasManager
            )
        {
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentManager = contentManager;
            _contentAliasManager = contentAliasManager;
        }

        [Route("AdvancedForms")]
        [Route("AdvancedForms/Index")]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [Route("AdvancedForms/{alias}")]
        public async Task<IActionResult> Display(string alias)
        {
            if (String.IsNullOrWhiteSpace(alias))
            {
                await Index(); 
            }
      
            var contentItemId = await _contentAliasManager.GetContentItemIdAsync("slug:AdvancedForms/" + alias);

            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Published);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.ViewContent, contentItem))
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

            return View(model);

        }

        [HttpPost]
        [Route("AdvancedForms/Entry")]
        public async Task<ActionResult> Entry(string submission)
        {
            var contentItem = await _contentManager.NewAsync(_id);

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.SubmitForm, contentItem))
            {
                return Unauthorized();
            }

            var subObject = JObject.Parse(submission).ToString(Newtonsoft.Json.Formatting.None);

            string title = "#FormName_" + DateTime.Now.ToUniversalTime().ToString(); 
            var advForm = new AdvancedFormSubmissions(submission, submission, title);
            var titlePart = new TitlePart(title);
            contentItem.Content.AdvancedForm = JToken.FromObject(advForm);
            contentItem.Content.TitlePart = JToken.FromObject(titlePart);

            return View();
            
            //var model = await _contentItemDisplayManager.UpdateEditorAsync(contentItem, this, true);

            //if (!ModelState.IsValid)
            //{
            //    _session.Cancel();
            //    return View(viewModel);
            //}

            //await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);

            //await conditionallyPublish(contentItem);
            //return View(viewModel);
        }

    }
}
