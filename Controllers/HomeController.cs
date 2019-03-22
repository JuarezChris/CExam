using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Dojo_Activity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Dojo_Activity.Controllers
{
    public class HomeController : Controller
    {
        private MyContext dbContext;

        // here we can "inject" our context service into the constructor
        public HomeController(MyContext context)
        {
            dbContext = context;
        }
        [Route("")]
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("create")]
        public IActionResult Create(User newUser)
        {
            if (ModelState.IsValid)
            {
                if (dbContext.users.Any(u => u.Email == newUser.Email))
                {
                    ModelState.AddModelError("Email", "Email already in use!");
                    return View("Index");
                }
                PasswordHasher<User> Hasher = new PasswordHasher<User>();
                newUser.Password = Hasher.HashPassword(newUser, newUser.Password);
                dbContext.users.Add(newUser);
                dbContext.SaveChanges();
                HttpContext.Session.SetString("LoggedIn", "LoggedIn");
                HttpContext.Session.SetInt32("UserId", newUser.UserId);


                var userInDb = dbContext.users.FirstOrDefault(u => u.Email == newUser.Email);

                return RedirectToAction("Home");
            }
            else
            {
                return View("Index");
            }
        }

        [Route("login")]
        [HttpPost]
        public IActionResult Login(LoginUser user)
        {
            if (ModelState.IsValid)
            {

                var userInDb = dbContext.users.FirstOrDefault(u => u.Email == user.LoginEmail);
                if (userInDb == null)
                {

                    ModelState.AddModelError("LoginEmail", "Invalid Email/Password");
                    return View("login");
                }
                var hasher = new PasswordHasher<LoginUser>();


                var result = hasher.VerifyHashedPassword(user, userInDb.Password, user.LoginPassword);


                if (result == 0)
                {

                    ModelState.AddModelError("LoginPassword", "Invalid Email/Password");
                    return View("login");

                }
                HttpContext.Session.SetString("LoggedIn", "LoggedIn");
                HttpContext.Session.SetInt32("UserId", userInDb.UserId);

                return RedirectToAction("Home");
            }
            else
            {
                return View("Index");
            }
        }

        [Route("logout")]
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index");
        }

        [Route("Home")]
        [HttpGet]
        public IActionResult Home(User currUser)
        {

            if (HttpContext.Session.GetString("LoggedIn") == null)
            {
                return RedirectToAction("Index");
            }

            //     Active Bye = dbContext.activities.SingleOrDefault(m => m.ActivityId == );
            //    if (Bye.DateTime.AddMinutes(30) < DateTime.Now)
            //    {
            //        TempData["TooLate"] = "You cannot delete your message after 30 minutes.";
            //        return RedirectToAction("Dashboard");
            //    }
            //    else
            //    {
            //        dbContext.Remove(Bye);
            //        dbContext.SaveChanges();
            //    }
            //    return RedirectToAction("");

            IEnumerable<Active> AllActivities = dbContext.activities
            .Include(r => r.JoinList)
            .ThenInclude(u => u.User)
            .ToList()
            .OrderBy(c => c.ActivityDate);

            List<User> Allusers = dbContext.users
            .ToList();

            //  .OrderBy(c => c.ActivityDate.TimeOfDay)
            // .OrderBy(c => c.ActivityDate.Date)
            // .OrderBy(c => c.ActivityDate.Year)
            // List<Active>order = AllActivities.OrderByDescending(c => c.ActivityDate).ToList();
            ViewBag.AllUsers = Allusers;
            ViewBag.currUser = currUser;
            ViewBag.activities = AllActivities;
        
            User user = dbContext.users.FirstOrDefault(a => a.UserId == (int)HttpContext.Session.GetInt32("UserId"));
                 
                   ViewBag.name = user;
            ViewBag.User = (int)HttpContext.Session.GetInt32("UserId");
            return View();
        }

        [Route("newActivity")]
        [HttpGet]
        public IActionResult NewActivity()
        {
            if (HttpContext.Session.GetString("LoggedIn") == null)
            {
                return RedirectToAction("Index");
            }
            return View();
        }

        [Route("addActivity")]
        [HttpPost]
        public IActionResult addActivity(Active newActivity)
        {
            if (ModelState.IsValid)
            {
                newActivity.ActivityPlanner = (int)HttpContext.Session.GetInt32("UserId");
                dbContext.activities.Add(newActivity);
                dbContext.SaveChanges();
                return RedirectToAction("DisplayActivity", new { id = newActivity.ActivityId });
            }
            return View("NewActivity");
        }

        [Route("DisplayActivity/{id}")]
        [HttpGet]
        public IActionResult DisplayActivity(int id)
        {
            if (HttpContext.Session.GetString("LoggedIn") == null)
            {
                return RedirectToAction("Index");
            }

            Active theActivity = dbContext.activities
            .Include(j => j.JoinList)
            .ThenInclude(u => u.User)
            .FirstOrDefault(w => w.ActivityId == id);
            ViewBag.theActivity = theActivity;

            List<Active> AllActivities = dbContext.activities
           .Include(r => r.JoinList)
           .ThenInclude(u => u.User)
           .ToList();

            List<User> Allusers = dbContext.users
            .ToList();

            ViewBag.AllUsers = Allusers;

             User user = dbContext.users.FirstOrDefault(a => a.UserId == (int)HttpContext.Session.GetInt32("UserId"));
                 
                   ViewBag.name = user;

            ViewBag.activities = AllActivities;


            ViewBag.User = (int)HttpContext.Session.GetInt32("UserId");
            return View("DisplayActivity", theActivity);

        }

        [Route("join/{id}/{uid}")]
        [HttpPost]
        public IActionResult Join(Join newJoin, int id, int uid)
        {

            Active newActive = dbContext.activities.Include(a => a.JoinList).ThenInclude(b => b.User).FirstOrDefault(x => x.ActivityId == id);
            User newUser = dbContext.users.Include(a => a.Atending).ThenInclude(b => b.Active).FirstOrDefault(us => us.UserId == uid);
            foreach (var joined in newUser.Atending)
            {
                if (joined.Active.ActivityDate.Date == newActive.ActivityDate.Date)
                {
                    ModelState.AddModelError("ActivityDate", "Date Cannot be in the past");
                    return RedirectToAction("Home", new { id = (int)HttpContext.Session.GetInt32("UserId") });

                }
            }

            newJoin.ActivityId = (int)id;
            newJoin.UserId = (int)HttpContext.Session.GetInt32("UserId");
            dbContext.joinTable.Add(newJoin);
            dbContext.SaveChanges();
            return RedirectToAction("Home");
        }

        [Route("leave/{id}")]
        [HttpPost]
        public IActionResult Leave(int id)
        {
            IEnumerable<Join> attendee = dbContext.joinTable.Where(a => a.ActivityId == id);
            Join flaker = attendee.SingleOrDefault(user => user.UserId == (int)HttpContext.Session.GetInt32("UserId"));

            dbContext.joinTable.Remove(flaker);


            dbContext.SaveChanges();


            return RedirectToAction("Home");
        }


        [Route("delete/{id}")]
        [HttpPost]
        public IActionResult Delete(int id)
        {
            Active RetrievedActivity = dbContext.activities.SingleOrDefault(w => w.ActivityId == id);
            dbContext.activities.Remove(RetrievedActivity);

            dbContext.SaveChanges();
            return RedirectToAction("Home");
        }
    }
}