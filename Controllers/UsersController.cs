using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Database;
using LatihanAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace LatihanAPI.Controllers
{
    [Authorize(AuthenticationSchemes = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly Context db = new Context();
        public static IConfigurationRoot Configuration { get; set; }
        private readonly IFileProvider fileProvider;
        public UsersController(IFileProvider fileProvider)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            this.fileProvider = fileProvider;
        }

        [HttpGet]
        [Route("GetData")]
        public async Task<List<UsersTable>> GetData()
        {
            List<UsersTable> users = await db.Users.ToListAsync();
            return users;
        }

        [HttpGet]
        [Route("GetDataLimit")]
        public async Task<UsersModel> GetDataLimit(string search, int start = 0, int take = 0)
        {
            take = (take != 0) ? take : 25;
            int skip = (start * take);
            List<UsersTable> users = await db.Users.ToListAsync();
            if (!string.IsNullOrEmpty(search))
            {
                users = users.Where(s => s.JobTitle.ToLower().Contains(search.ToLower())).ToList();
            }
            string next = null;
            if (users.Any())
            {
                users = users.Skip(skip).Take(take).ToList();
                if (((start + 1) * take) < users.Count())
                {
                    next = $"{Startup.restUrl}/Users/?start={start + 1}&take={take}";
                }
            }
            UsersModel model = new UsersModel()
            {
                Next = next,
                Start = start,
                Value = users
            };
            return model;
        }

        [HttpGet]
        [Route("GetDataSPLimit")]
        public async Task<UsersModel> GetDataSPLimit(string search, int start = 0, int take = 0)
        {
            int total = 0;
            take = (take != 0) ? take : 25;
            int skip = (start * take);

            SqlParameter paramSkip = new SqlParameter("@Skip", skip);
            SqlParameter paramTake = new SqlParameter("@Take", take);
            SqlParameter paramSearch = new SqlParameter("@Search", (!string.IsNullOrEmpty(search)) ? search : "");
            SqlParameter paramList = new SqlParameter("@IsCount", false);
            SqlParameter paramCount = new SqlParameter("@IsCount", true);

            //List<UsersTable> users = await db.Users.ToListAsync();
            List<UsersTable> users = await db.Users.FromSqlRaw("SPGetUser @IsCount, @Skip, @Take, @Search", paramList, paramSkip, paramTake, paramSearch).AsNoTracking().ToListAsync();
            List<SPUserCount> userCount = await db.SPUserCounts.FromSqlRaw("SPGetUser @IsCount, @Skip, @Take, @Search", paramCount, paramSkip, paramTake, paramSearch).AsNoTracking().ToListAsync();
            total = userCount.Sum(x => x.Number);

            IQueryable<UsersTable> data = null;
            string next = null;
            if (users.Any())
            {
                data = users.AsQueryable();
                if (((start + 1) * take) < total)
                {
                    next = $"{Startup.restUrl}/Inbox/?search={search}&start={start + 1}&take={take}";
                }
            }
            UsersModel model = new UsersModel()
            {
                Next = next,
                Start = start,
                Data = data,
                Total = total
            };
            return model;
        }

        /// <param name="model"></param>
        [HttpPost]
        [Route("InsertUser")]
        public async Task<string> InsertUser(UserPostModel model)
        {
            try
            {
                UsersTable user = new UsersTable()
                {
                    UserId = Guid.NewGuid().ToString(),
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    DisplayName = model.DisplayName,
                    EmailAddress = model.EmailAddress,
                    JobTitle = model.JobTitle
                };
                await db.Users.AddAsync(user);
                await db.SaveChangesAsync();
                return user.DisplayName + " successfully added";
            }
            catch (Exception x)
            {
                return x.Message.ToString();
            }
        }

        [HttpPut]
        [Route("UpdateUserById")]
        public async Task<string> UpdateUserById(UsersTable model)
        {
            try
            {
                UsersTable user = await db.Users.FindAsync(model.UserId);
                if (user != null)
                {
                    user.DisplayName = model.DisplayName;
                    user.EmailAddress = model.EmailAddress;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.JobTitle = model.JobTitle;
                    await  db.SaveChangesAsync();
                    return "Succesfully edit " + model.DisplayName;
                }
                else
                {
                    return "Cannot find detail user with ID " + model.UserId;
                }
            }
            catch (Exception x)
            {
                return x.Message.ToString();
            }
        }

        [HttpDelete]
        [Route("DeleteUserById")]
        public async Task<string> DeleteUserById(string id)
        {
            try
            {
                UsersTable user = await db.Users.FindAsync(id);
                if (user != null)
                {
                    db.Users.Remove(user);
                    await db.SaveChangesAsync();
                    return "Succesfully delete " +user.DisplayName;
                }
                else
                {
                    return "Cannot find detail user with ID " + id;
                }
            }
            catch (Exception x)
            {
                return x.Message.ToString();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }


    }
}