﻿using AutoMapper;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using WebAPIBudget.Models;
using WebAPIBudget.Models.Domain;

namespace WebAPIBudget.Controllers
{
    [Authorize]
    public class HouseholdController : ApiController
    {
        private ApplicationDbContext Context;
        private EmailService EmailService;

        public HouseholdController()
        {
            Context = new ApplicationDbContext();
            EmailService = new EmailService();
        }

        [HttpPost]
        public IHttpActionResult Create(CreateEditHouseholdBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var household = Mapper.Map<Household>(model);
            household.DateCreated = DateTime.Now;
            household.OwnerId = User.Identity.GetUserId();

            Context.Households.Add(household);
            Context.SaveChanges();

            var result = Mapper.Map<HouseholdViewModel>(household);

            return Ok(result);
        }

        [HttpPost]
        public IHttpActionResult Edit(int id, CreateEditHouseholdBindingModel model)
        {
            var userId = User.Identity.GetUserId();
            var household = Context.Households.FirstOrDefault(p => p.Id == id);

            if (household == null)
            {
                return NotFound();
            }

            if (household.OwnerId != userId)
            {
                ModelState.AddModelError("", "You don't own this household");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Mapper.Map(model, household);
            household.DateUpdated = DateTime.Now;

            Context.SaveChanges();

            var result = Mapper.Map<HouseholdViewModel>(household);

            return Ok(result);
        }

        [HttpPost]
        public IHttpActionResult Invite(int id, InviteBindingModel model)
        {
            var userId = User.Identity.GetUserId();
            var household = Context.Households.FirstOrDefault(p => p.Id == id);

            if (household == null)
            {
                return NotFound();
            }

            if (household.OwnerId != userId)
            {
                ModelState.AddModelError("", "You don't own this household");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var invitedUser = Context.Users.FirstOrDefault(p => p.Email == model.Email);

            if (invitedUser == null)
            {
                ModelState.AddModelError("", "User doesn't not exist");
                return BadRequest(ModelState);
            }

            if (invitedUser.Id == userId)
            {
                ModelState.AddModelError("", "You cannot invite yourself to your own household");
                return BadRequest(ModelState);
            }

            if (household.Members.Any(p => p.Id == invitedUser.Id) ||
                household.InvitedUsers.Any(p => p.Id == invitedUser.Id))
            {
                ModelState.AddModelError("", "This user has already joined or has a pending invite");
                return BadRequest(ModelState);
            }

            household.InvitedUsers.Add(invitedUser);
            Context.SaveChanges();

            EmailService.Send(invitedUser.Email,
                $"You have been invited to participate in the household: {household.Name}",
                "Household invite");

            return Ok();
        }

        [HttpPost]
        public IHttpActionResult Delete(int id)
        {
            var userId = User.Identity.GetUserId();

            var household = Context.Households.FirstOrDefault(p => p.Id == id);

            if (household == null)
            {
                return NotFound();
            }

            if (household.OwnerId != userId)
            {
                ModelState.AddModelError("", "You don't own this household");
                return BadRequest(ModelState);
            }

            Context.Households.Remove(household);
            Context.SaveChanges();
            return Ok();
        }

        [HttpPost]
        public IHttpActionResult Join(int id)
        {
            var userId = User.Identity.GetUserId();
            var household = Context.Households.FirstOrDefault(p => p.Id == id);

            if (household == null)
            {
                return NotFound();
            }

            if (!household.InvitedUsers.Any(p => p.Id == userId))
            {
                ModelState.AddModelError("", "You are not invited to this household");
                return BadRequest(ModelState);
            }

            var user = Context.Users.FirstOrDefault(p => p.Id == userId);

            household.InvitedUsers.Remove(user);
            household.Members.Add(user);

            Context.SaveChanges();

            return Ok();
        }

        [HttpGet]
        public IHttpActionResult ViewUsers(int id)
        {
            var userId = User.Identity.GetUserId();

            var household = Context
                .Households
                .FirstOrDefault(p => p.Id == id &&
                (p.OwnerId == userId 
                || p.Members.Any(t => t.Id == userId)));

            if (household == null)
            {
                return NotFound();
            }

            var owner = Mapper.Map<HouseholdUserViewModel>(household.Owner);

            var result = Mapper.Map<List<HouseholdUserViewModel>>(household.Members);
            result.Add(owner);

            return Ok(result);
        }

        [HttpPost]
        public IHttpActionResult Leave(int id)
        {
            var userId = User.Identity.GetUserId();
            var household = Context.Households.FirstOrDefault(p => p.Id == id);

            if (household == null)
            {
                return NotFound();
            }

            if (household.OwnerId == userId)
            {
                ModelState.AddModelError("", "You cannot leave your own household");
                return BadRequest(ModelState);
            }

            if (!household.Members.Any(p => p.Id == userId))
            {
                ModelState.AddModelError("", "You are not a member of this household");
                return BadRequest(ModelState);
            }

            var user = Context.Users.FirstOrDefault(p => p.Id == userId);

            household.Members.Remove(user);

            Context.SaveChanges();

            return Ok();
        }

    }
}