using AutoMapper;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using WebAPIBudget.Models;
using WebAPIBudget.Models.Domain;

namespace WebAPIBudget.Controllers
{
    [Authorize]
    public class CategoryController : ApiController
    {
        private ApplicationDbContext Context;

        public CategoryController()
        {
            Context = new ApplicationDbContext();
        }

        [HttpPost]
        public IHttpActionResult Create(CreateCategoryBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = User.Identity.GetUserId();
            var household = Context
                .Households
                .FirstOrDefault(p => p.Id == model.HouseholdId);

            if (household == null || household.OwnerId != userId)
            {
                return NotFound();
            }
            
            var category = Mapper.Map<Category>(model);
            category.DateCreated = DateTime.Now;

            Context.Categories.Add(category);
            Context.SaveChanges();

            var result = Mapper.Map<CategoryViewModel>(category);

            return Ok(result);
        }

        [HttpPost]
        public IHttpActionResult Edit(int id, EditCategoryBindingModel model)
        {
            var userId = User.Identity.GetUserId();

            var category = Context
                .Categories
                .FirstOrDefault(p => p.Id == id &&
                p.Household.OwnerId == userId);

            if (category == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Mapper.Map(model, category);
            category.DateUpdated = DateTime.Now;

            Context.SaveChanges();

            var result = Mapper.Map<CategoryViewModel>(category);

            return Ok(result);
        }

        [HttpPost]
        public IHttpActionResult Delete(int id)
        {
            var userId = User.Identity.GetUserId();

            var category = Context
                .Categories
                .FirstOrDefault(p => p.Id == id &&
                p.Household.OwnerId == userId);

            if (category == null)
            {
                return NotFound();
            }

            Context.Categories.Remove(category);
            Context.SaveChanges();
            return Ok();
        }

        [HttpGet]
        public IHttpActionResult View(int id)
        {
            var userId = User.Identity.GetUserId();

            var categories = Context
                .Categories
                .Where(p => p.HouseholdId == id &&
                (p.Household.OwnerId == userId
                || p.Household.Members.Any(t => t.Id == userId)))
                .ToList();

            if (categories == null)
            {
                return NotFound();
            }

            var result = Mapper.Map<List<CategoryViewModel>>(categories);

            return Ok(result);
        }
    }
}
