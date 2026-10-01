using System.Text.Json;
using dotnet_cie2.Extensions;
using dotnet_cie2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace dotnet_cie2.Controllers
{
    public class CourseController : Controller
    {
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private const string COURSE_CACHE_KEY = "CourseList_CacheKey";
        private const string COOKIE_STUDENT_NAME = "StudentName";
        private const string SESSION_SELECTED_COURSE = "SelectedCourseName";
        private const string SESSION_SELECTED_COURSE_ID = "SelectedCourseId";

        public CourseController(IMemoryCache memoryCache, IHttpClientFactory httpClientFactory)
        {
            _memoryCache = memoryCache;
            _httpClientFactory = httpClientFactory;
        }

        // 1. Course List page - Demonstrating IMemoryCache (5-minute expiration)
        public IActionResult Index()
        {
            bool isFromCache = true;
            DateTime cacheTime;

            if (!_memoryCache.TryGetValue(COURSE_CACHE_KEY, out List<Course>? courses))
            {
                isFromCache = false;
                courses = GetInitialCourses();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                    .SetPriority(CacheItemPriority.Normal);

                _memoryCache.Set(COURSE_CACHE_KEY, courses, cacheEntryOptions);
                _memoryCache.Set(COURSE_CACHE_KEY + "_Time", DateTime.Now, cacheEntryOptions);
            }

            if (!_memoryCache.TryGetValue(COURSE_CACHE_KEY + "_Time", out cacheTime))
            {
                cacheTime = DateTime.Now;
            }

            ViewBag.IsFromCache = isFromCache;
            ViewBag.CacheTimestamp = cacheTime;
            ViewBag.StudentNameCookie = Request.Cookies[COOKIE_STUDENT_NAME];
            ViewBag.SelectedCourseSession = HttpContext.Session.GetString(SESSION_SELECTED_COURSE);

            return View(courses);
        }

        // Action to manually clear IMemoryCache (for testing/demo)
        public IActionResult ClearCache()
        {
            _memoryCache.Remove(COURSE_CACHE_KEY);
            _memoryCache.Remove(COURSE_CACHE_KEY + "_Time");
            TempData["SuccessMessage"] = "IMemoryCache has been cleared successfully!";
            return RedirectToAction(nameof(Index));
        }

        // 2. Course Details page - Demonstrating Query String parameter & Response Caching
        [ResponseCache(Duration = 300, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "id", "courseId" })]
        public IActionResult Details(int? id, int? courseId)
        {
            // Support passing CourseId via Query String (e.g. /Course/Details?id=1 or /Course/Details?courseId=1)
            int targetId = courseId ?? id ?? 1;

            List<Course> courses;
            if (!_memoryCache.TryGetValue(COURSE_CACHE_KEY, out courses!))
            {
                courses = GetInitialCourses();
            }

            var course = courses.FirstOrDefault(c => c.CourseId == targetId);
            if (course == null)
            {
                return NotFound($"Course with ID {targetId} was not found.");
            }

            // Save selected course in Session as required in State Management
            HttpContext.Session.SetString(SESSION_SELECTED_COURSE, course.CourseName);
            HttpContext.Session.SetInt32(SESSION_SELECTED_COURSE_ID, course.CourseId);

            ViewBag.QueryStringCourseId = targetId;
            ViewBag.StudentNameCookie = Request.Cookies[COOKIE_STUDENT_NAME];
            ViewBag.SelectedCourseSession = HttpContext.Session.GetString(SESSION_SELECTED_COURSE);
            ViewBag.RenderTimestamp = DateTime.Now.ToString("HH:mm:ss.fff");

            return View(course);
        }

        // GET: Enroll in a course (Renders form with Hidden Field for CourseId)
        [HttpGet]
        public IActionResult Enroll(int courseId)
        {
            List<Course> courses;
            if (!_memoryCache.TryGetValue(COURSE_CACHE_KEY, out courses!))
            {
                courses = GetInitialCourses();
            }

            var course = courses.FirstOrDefault(c => c.CourseId == courseId) ?? courses.First();

            // Retrieve student name from Cookie if present
            string? savedStudentName = Request.Cookies[COOKIE_STUDENT_NAME];

            var model = new EnrollmentViewModel
            {
                CourseId = course.CourseId, // This will be bound to Hidden Field in the form
                StudentName = savedStudentName ?? string.Empty,
                CourseDetails = course
            };

            return View(model);
        }

        // POST: Process Course Enrollment (Receives CourseId via Hidden Field)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Enroll(EnrollmentViewModel model)
        {
            List<Course> courses;
            if (!_memoryCache.TryGetValue(COURSE_CACHE_KEY, out courses!))
            {
                courses = GetInitialCourses();
            }

            model.CourseDetails = courses.FirstOrDefault(c => c.CourseId == model.CourseId);

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Cookie State Management: Store Student Name in Cookie (expires in 7 days)
            CookieOptions cookieOptions = new CookieOptions
            {
                Expires = DateTime.Now.AddDays(7),
                HttpOnly = true,
                IsEssential = true
            };
            Response.Cookies.Append(COOKIE_STUDENT_NAME, model.StudentName, cookieOptions);

            // 2. Session State Management: Store Selected Course in Session
            if (model.CourseDetails != null)
            {
                HttpContext.Session.SetString(SESSION_SELECTED_COURSE, model.CourseDetails.CourseName);
                HttpContext.Session.SetInt32(SESSION_SELECTED_COURSE_ID, model.CourseDetails.CourseId);
            }

            TempData["SuccessMessage"] = $"Enrollment Successful! Welcome, {model.StudentName}.";
            return RedirectToAction(nameof(Confirmation), new { courseId = model.CourseId });
        }

        // GET: Enrollment Confirmation Page
        public IActionResult Confirmation(int courseId)
        {
            List<Course> courses;
            if (!_memoryCache.TryGetValue(COURSE_CACHE_KEY, out courses!))
            {
                courses = GetInitialCourses();
            }

            var course = courses.FirstOrDefault(c => c.CourseId == courseId);
            ViewBag.StudentNameCookie = Request.Cookies[COOKIE_STUDENT_NAME];
            ViewBag.SelectedCourseSession = HttpContext.Session.GetString(SESSION_SELECTED_COURSE);

            return View(course);
        }

        // Action to save/update Student Name Cookie explicitly
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SaveStudentCookie(string studentName, string returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(studentName))
            {
                CookieOptions cookieOptions = new CookieOptions
                {
                    Expires = DateTime.Now.AddDays(7),
                    HttpOnly = true,
                    IsEssential = true
                };
                Response.Cookies.Append(COOKIE_STUDENT_NAME, studentName, cookieOptions);
                TempData["SuccessMessage"] = $"Student name '{studentName}' saved in cookie!";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction(nameof(Index));
        }

        // Action to clear Cookie & Session
        public IActionResult ClearState()
        {
            Response.Cookies.Delete(COOKIE_STUDENT_NAME);
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Cookie and Session state cleared successfully!";
            return RedirectToAction(nameof(Index));
        }

        // 3. Asynchronous Web Request - Fetch users from https://jsonplaceholder.typicode.com/users
        public async Task<IActionResult> Users()
        {
            List<ExternalUser> users = new List<ExternalUser>();
            string? errorMessage = null;

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                var response = await client.GetAsync("https://jsonplaceholder.typicode.com/users");

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    users = JsonSerializer.Deserialize<List<ExternalUser>>(jsonString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    }) ?? new List<ExternalUser>();
                }
                else
                {
                    errorMessage = $"Failed to load users. Status Code: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Error fetching remote user data: {ex.Message}";
            }

            ViewBag.ErrorMessage = errorMessage;
            ViewBag.StudentNameCookie = Request.Cookies[COOKIE_STUDENT_NAME];
            ViewBag.SelectedCourseSession = HttpContext.Session.GetString(SESSION_SELECTED_COURSE);

            return View(users);
        }

        // Helper method to generate sample course data
        private static List<Course> GetInitialCourses()
        {
            return new List<Course>
            {
                new Course
                {
                    CourseId = 101,
                    CourseName = "ASP.NET Core MVC Masterclass",
                    Duration = "8 Weeks",
                    Fees = 499.99m,
                    DiscountPercentage = 15m,
                    Description = "Learn full-stack ASP.NET Core MVC development with C#, Entity Framework Core, Dependency Injection, and modern web application security practices.",
                    Category = "Web Development",
                    Level = "Intermediate",
                    Icon = "bi-code-slash"
                },
                new Course
                {
                    CourseId = 102,
                    CourseName = "Full-Stack Web API & Microservices",
                    Duration = "10 Weeks",
                    Fees = 649.00m,
                    DiscountPercentage = 20m,
                    Description = "Build scalable RESTful Web APIs, implement gRPC, JWT Authentication, API Gateways, and deployment strategies using Docker and Azure.",
                    Category = "Backend & Cloud",
                    Level = "Advanced",
                    Icon = "bi-cpu"
                },
                new Course
                {
                    CourseId = 103,
                    CourseName = "C# & .NET 10 Fundamental Programming",
                    Duration = "6 Weeks",
                    Fees = 299.50m,
                    DiscountPercentage = 10m,
                    Description = "Master C# syntax, Object-Oriented Design principles, LINQ, Extension Methods, Asynchronous Async/Await programming patterns.",
                    Category = "Programming Languages",
                    Level = "Beginner",
                    Icon = "bi-terminal"
                },
                new Course
                {
                    CourseId = 104,
                    CourseName = "UI/UX & Soft UI Design for Web Apps",
                    Duration = "4 Weeks",
                    Fees = 199.00m,
                    DiscountPercentage = 25m,
                    Description = "Craft clean modern interfaces with Neumorphism, Soft UI shadow aesthetics, accessibility, responsive CSS Grid, and custom Bootstrap styling.",
                    Category = "Frontend Design",
                    Level = "All Levels",
                    Icon = "bi-palette"
                },
                new Course
                {
                    CourseId = 105,
                    CourseName = "Cloud Computing with Azure & DevOps",
                    Duration = "12 Weeks",
                    Fees = 799.00m,
                    DiscountPercentage = 18m,
                    Description = "Architect cloud solutions using Azure App Services, SQL Databases, CI/CD pipelines with GitHub Actions, and Infrastructure as Code.",
                    Category = "Cloud & DevOps",
                    Level = "Advanced",
                    Icon = "bi-cloud-arrow-up"
                }
            };
        }
    }
}
