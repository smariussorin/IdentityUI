﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SSRD.IdentityUI.Admin.Areas.IdentityAdmin.Models;
using SSRD.IdentityUI.Admin.Areas.IdentityAdmin.Models.DataTable;
using SSRD.IdentityUI.Admin.Areas.IdentityAdmin.Models.User;
using SSRD.IdentityUI.Core.Helper;
using SSRD.IdentityUI.Core.Interfaces.Services;
using SSRD.IdentityUI.Core.Interfaces.Services.Auth;
using SSRD.IdentityUI.Core.Models.Result;
using SSRD.IdentityUI.Core.Services.Auth.Session.Models;
using SSRD.IdentityUI.Core.Services.User.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSRD.IdentityUI.Admin.Areas.IdentityAdmin.Interfaces.User;

namespace SSRD.IdentityUI.Admin.Areas.IdentityAdmin.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserDataService _userDataService;
        private readonly IManageUserService _manageUserService;
        private readonly IAddUserService _addUserService;
        private readonly ISessionService _sessionService;

        public UserController(IUserDataService userDataService, IManageUserService manageUserService, IAddUserService addUserService, ISessionService sessionService)
        {
            _userDataService = userDataService;
            _manageUserService = manageUserService;
            _addUserService = addUserService;
            _sessionService = sessionService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] DataTableRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Result<DataTableResult<UserListViewModel>> result = _userDataService.GetAll(request);
            if(result.Failure)
            {
                ModelState.AddErrors(result.Errors);
                return BadRequest(ModelState);
            }

            return Ok(result.Value);
        }

        [HttpGet]
        public IActionResult Details(string id)
        {
            if(!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            Result<UserDetailsViewModel> result = _userDataService.GetDetailsViewModel(id);
            if(result.Failure)
            {
                ModelState.AddErrors(result.Errors);
                return RedirectToAction(nameof(Index));
            }

            return View(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(string id, EditUserRequest model)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            Result editResult = await _manageUserService.EditUser(id, model, GetUserId());
            Result<UserDetailsViewModel> userResult = _userDataService.GetDetailsViewModel(id);
            if (userResult.Failure)
            {
                ModelState.AddErrors(userResult.Errors);
                return RedirectToAction(nameof(Index));
            }

            if (editResult.Failure)
            {
                ModelState.AddErrors(editResult.Errors);

                userResult.Value.StatusAlert = StatusAlertViewExtension.Get(editResult);
                return View("Details", userResult.Value);
            }

            userResult.Value.StatusAlert = StatusAlertViewExtension.Get("User updated");

            return View("Details", userResult.Value);
        }

        [HttpPost]
        public async Task<IActionResult> SendVerifyEmail(string id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            Result result = await _manageUserService.SendEmilVerificationMail(new SendEmailVerificationMailRequest(id), GetUserId());
            Result<UserDetailsViewModel> userResult = _userDataService.GetDetailsViewModel(id);
            if (userResult.Failure)
            {
                ModelState.AddErrors(userResult.Errors);
                return RedirectToAction(nameof(Index));
            }

            if (result.Failure)
            {
                ModelState.AddErrors(result.Errors);

                userResult.Value.StatusAlert = StatusAlertViewExtension.Get(result);
                return View("Details", userResult.Value);
            }

            userResult.Value.StatusAlert = StatusAlertViewExtension.Get("Verify Email mail sent");

            return View("Details", userResult.Value);
        }

        [HttpPost]
        public IActionResult Unlock(string id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            Result result = _manageUserService.UnlockUser(new UnlockUserRequest(id), GetUserId());
            Result<UserDetailsViewModel> userResult = _userDataService.GetDetailsViewModel(id);
            if (userResult.Failure)
            {
                ModelState.AddErrors(userResult.Errors);
                return RedirectToAction(nameof(Index));
            }

            if (result.Failure)
            {
                ModelState.AddErrors(result.Errors);

                userResult.Value.StatusAlert = StatusAlertViewExtension.Get(result);
                return View("Details", userResult.Value);
            }

            userResult.Value.StatusAlert = StatusAlertViewExtension.Get("User unlocked");

            return View("Details", userResult.Value);
        }

        [HttpGet]
        public IActionResult Credentials(string id)
        {
            if(!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            Result<UserCredentialsViewModel> result = _userDataService.GetCredetialsViewModel(id);
            if (result.Failure)
            {
                ModelState.AddErrors(result.Errors);
                return RedirectToAction(nameof(Index));
            }

            return View(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> SetNewPassword([FromRoute] string id, [FromBody] SetNewPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Result result = await _manageUserService.SetNewPassword(id, request, GetUserId());
            if (result.Failure)
            {
                ModelState.AddErrors(result.Errors);
                return BadRequest(ModelState);
            }

            return Ok(new EmptyResult());
        }

        [HttpGet]
        public IActionResult Roles(string id)
        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }

            Result<UserRolesViewModel> result = _userDataService.GetRolesViewModel(id);
            if (result.Failure)
            {
                ModelState.AddErrors(result.Errors);
                return RedirectToAction(nameof(Index));
            }

            return View(result.Value);
        }

        [HttpGet]
        public IActionResult GetRoles([FromRoute] string id)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Result<RoleViewModel> result = _userDataService.GetRoles(id);
            if(result.Failure)
            {
                ModelState.AddErrors(result.Errors);
                return BadRequest(ModelState);
            }

            return Ok(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> AddRoles([FromRoute] string id, [FromBody] RoleRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Result result = await _manageUserService.AddRoles(id, request.Roles, GetUserId());
            if(result.Failure)
            {
                ModelState.AddErrors(result.Errors);
                return BadRequest(ModelState);
            }

            return Ok(new EmptyResult());
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRoles([FromRoute] string id, [FromBody] RoleRequest request)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Result result = await _manageUserService.RemoveRoles(id, request.Roles, GetUserId());
            if(result.Failure)
            {
                ModelState.AddErrors(result.Errors);
                return BadRequest(ModelState);
            }

            return Ok(new EmptyResult());
        }

        [HttpGet]
        public IActionResult New()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> New(NewUserRequest newUserRequest)
        {
            if(!ModelState.IsValid)
            {
                return View();
            }

            Result<string> result = await _addUserService.AddUser(newUserRequest , GetUserId());
            if(result.Failure)
            {
                ModelState.AddErrors(result.Errors);
                return View(new NewUserViewModel(StatusAlertViewExtension.Get(result)));
            }

            return RedirectToAction(nameof(Details), new { id = result.Value });
        }

        [HttpGet]
        public IActionResult Sessions([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Result<UserSessionViewModel> result = _userDataService.GetUserSessionViewModel(id);
            if(result.Failure)
            {
                ModelState.AddErrors(result.Errors);
                return View();
            }

            return View(result.Value);
        }

        [HttpGet]
        public IActionResult GetSessions([FromRoute] string id, [FromQuery] DataTableRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Result<DataTableResult<SessionViewModel>> result = _userDataService.GetSessions(id, request);
            if(result.Failure)
            {
                ModelState.AddErrors(result.Errors);
                return BadRequest(ModelState);
            }

            return Ok(result.Value);
        }

        [HttpPost]
        public IActionResult LogoutSession([FromBody] LogoutSessionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Result result = _sessionService.Logout(request, GetUserId());
            if(result.Failure)
            {
                ModelState.AddErrors(result.Errors);
                return BadRequest(ModelState);
            }

            return Ok(new EmptyResult());
        }

        [HttpPost]
        public async Task<IActionResult> LogoutUser([FromBody] LogoutUserSessionsRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Result result = await _sessionService.LogoutUser(request, GetUserId());
            if (result.Failure)
            {
                ModelState.AddErrors(result.Errors);
                return BadRequest(ModelState);
            }

            return Ok(new EmptyResult());
        }
    }
}
