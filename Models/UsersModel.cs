using Database;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace LatihanAPI.Models
{
    public class UsersModel
    {
        public string Next { get; set; }
        public int? Start { get; set; }
        public int? Total { get; set; }
        public IQueryable<UsersTable> Data { get; set; }
        public List<UsersTable> Value { get; set; }
    }

    public class SPUserCount
    {
        [Key]
        public int Number { get; set; }
    }

    public class UserPostModel
    {

        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string DisplayName { get; set; }
        [Required]
        public string JobTitle { get; set; }
        [Required]
        public string EmailAddress { get; set; }
    }
}
