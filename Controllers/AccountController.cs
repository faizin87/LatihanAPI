using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LatihanAPI.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace LatihanAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly Context db = new Context();
        public static IConfigurationRoot Configuration { get; set; }
        private readonly IFileProvider fileProvider;
        public AccountController(IFileProvider fileProvider)
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");
            Configuration = builder.Build();
            this.fileProvider = fileProvider;
        }

        [HttpPost]
        public ReturnSignIn Office365(string email)
        {
            var data = db.Users.FirstOrDefault(x => x.EmailAddress.Equals(email));
            if (data != null)
            {
                return new ReturnSignIn()
                {
                    Name = data.DisplayName,
                    Oid = data.UserId,
                    Token = TokenHelper.Get(data.EmailAddress, data.DisplayName),
                    JobTitle = data.JobTitle
                };
            };
            return null;
        }
        public class ReturnSignIn
        {
            public string Oid { get; set; }
            public string Name { get; set; }
            public string JobTitle { get; set; }
            public object Token { get; set; }
        }

    }
}