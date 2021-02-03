using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LatihanAPI.Helpers
{
    public class TokenHelper
    {
        public static string Get(string upn, string name)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("B554E0544B72761E4EA11D512F1401B13E4F14F10588F68FE6F777DDB57BBCD9"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(upn,
              name,
              null,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
