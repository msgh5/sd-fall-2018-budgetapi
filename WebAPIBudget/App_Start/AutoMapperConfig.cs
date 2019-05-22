﻿using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPIBudget.Models;
using WebAPIBudget.Models.Domain;

namespace WebAPIBudget.App_Start
{
    public static class AutoMapperConfig
    {
        public static void Init()
        {
            Mapper.Initialize(p =>
            {
                p.CreateMap<Household, CreateEditHouseholdBindingModel>().ReverseMap();
                p.CreateMap<Household, HouseholdViewModel>().ReverseMap();
                p.CreateMap<ApplicationUser, HouseholdUserViewModel>().ReverseMap();
                p.CreateMap<Category, CreateCategoryBindingModel>().ReverseMap();
                p.CreateMap<Category, EditCategoryBindingModel>().ReverseMap();
                p.CreateMap<Category, CategoryViewModel>().ReverseMap();
            });
        }
    }
}