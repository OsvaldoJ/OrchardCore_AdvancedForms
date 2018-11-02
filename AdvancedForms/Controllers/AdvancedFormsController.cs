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
using Microsoft.AspNetCore.Http;

namespace AdvancedForms.Controllers
{
    public class AdvancedFormsController : Controller, IUpdateModel
    {

        private readonly IContentManager _contentManager;
        private readonly IContentItemDisplayManager _contentItemDisplayManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly IContentAliasManager _contentAliasManager;
        private readonly YesSql.ISession _session;
        private const string _id = "AdvancedFormSubmissions";

        public AdvancedFormsController(
            IContentManager contentManager,
            IContentItemDisplayManager contentItemDisplayManager,
            IAuthorizationService authorizationService,
            IContentAliasManager contentAliasManager,
            YesSql.ISession session
            )
        {
            _authorizationService = authorizationService;
            _contentItemDisplayManager = contentItemDisplayManager;
            _contentManager = contentManager;
            _contentAliasManager = contentAliasManager;
            _session = session;
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

        [Route("AdvancedForms/{alias}/Edit/{id}")]
        public async Task<IActionResult> Edit(string alias, string id)
        {
            if (String.IsNullOrWhiteSpace(alias))
            {
                await Index();
            }
            else if (String.IsNullOrWhiteSpace(id))
            {
                await Display(alias);
            }

            var contentItemId = await _contentAliasManager.GetContentItemIdAsync("slug:AdvancedForms/" + alias);

            var contentItem = await _contentManager.GetAsync(contentItemId, VersionOptions.Published);
            var subContentItem = await _contentManager.GetAsync(id, VersionOptions.Latest);

            if (contentItem == null)
            {
                return NotFound();
            }

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.SubmitForm, subContentItem))
            {
                return Unauthorized();
            }

            var model = new AdvancedFormViewModel
            {
                Id = contentItemId,
                Title = contentItem.Content.AdvancedForm.Title,
                Container = contentItem.Content.AdvancedForm.Container.Html,
                Description = contentItem.Content.AdvancedForm.Description.Html,
                Instructions = contentItem.Content.AdvancedForm.Instructions.Html,
                SubmissionId = subContentItem.ContentItemId,
                Submission = subContentItem.Content.AdvancedFormSubmissions.Submission.Html
            };

            return View("Display", model);

        }
        [HttpPost]
        [Route("AdvancedForms/Entry")]
        public async Task<IActionResult> Entry(string submission, string title, string id)
        {
            var contentItem = await _contentManager.NewAsync(_id);

            if (!await _authorizationService.AuthorizeAsync(User, Permissions.SubmitForm, contentItem))
            {
                return Unauthorized();
            }

            var subObject = JObject.Parse(submission);

            string subTitle = title + "_" + DateTime.Now.ToUniversalTime().ToString(); 
            var advFormSub = new AdvancedFormSubmissions(subObject["data"].ToString(), subObject["metadata"].ToString(), subTitle);
            var titlePart = new TitlePart(subTitle);
            contentItem.Content.AdvancedFormSubmissions = JToken.FromObject(advFormSub);
            contentItem.Content.TitlePart = JToken.FromObject(titlePart);

            if (!ModelState.IsValid)
            {
                _session.Cancel();
                return StatusCode(StatusCodes.Status406NotAcceptable);
            }

            await _contentManager.CreateAsync(contentItem, VersionOptions.Draft);

            await _contentManager.PublishAsync(contentItem);
            return StatusCode(StatusCodes.Status201Created);  
        }        
    }
}
