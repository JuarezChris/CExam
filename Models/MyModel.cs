using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace Dojo_Activity.Models
{
    public class Active
    {
       [Key]
        public int ActivityId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [MinLength(2, ErrorMessage = "Title must be at least 2 characters.")]
        public string Title { get; set; }


        [Required(ErrorMessage = "Duration is required")]
        
        public int Duration { get; set; }

        [Required]
        [RestrictedDate(ErrorMessage = "Please select valid Upcoming Date, for Activity !!!")]
        
        public DateTime ActivityDate { get; set; } 

        [Required]
        [MinLength(10, ErrorMessage = "Description must be at least 2 characters.")]
        public string Description { get; set; }
        [Required]
         public string Hour { get; set; }
        public List<Join> JoinList { get; set; }
        public int ActivityPlanner { get; set; }

    }

    public class Join
    {
        [Key]
        public int JoinId { get; set; }
        public int ActivityId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public Active Active { get; set; }

    }

     public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MinLength(2, ErrorMessage = "Must be longer than 2")]
        public string FirstName { get; set; }
        [Required]
        [MinLength(2, ErrorMessage = "Must be longer than 2")]
        public string LastName { get; set; }
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        [Required]
        [MinLength(8, ErrorMessage = "Password must be 8 characters or longer!")]
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        // Will not be mapped to your users table!
        [NotMapped]
        [Compare("Password")]
        [DataType(DataType.Password)]
        public string Confirm { get; set; }
        public List<Join> Atending { get; set; }

    }

    public class LoginUser
    {
        // Other fields
        [Required]
        [Display(Name = "Email")]
        public string LoginEmail { get; set; }
        [Required]
        [Display(Name = "Password")]
        public string LoginPassword { get; set; }
    }

    public class RestrictedDate : ValidationAttribute 
        {
        //validation to have a past data, not future
        public override bool IsValid (object submittedDate) 
            {
            DateTime date = (DateTime) submittedDate;
            return date > DateTime.Now;
            }
        }
}