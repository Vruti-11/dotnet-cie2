var builder = WebApplication.CreateBuilder(args);

// Add MVC services
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Register HttpClient for Asynchronous Web Requests (JSONPlaceholder)
builder.Services.AddHttpClient();

// Register MemoryCache for course list caching (IMemoryCache - 5 min expiration)
builder.Services.AddMemoryCache();

// Register Response Caching for Course Details page
builder.Services.AddResponseCaching();

// Register Session state services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable Response Caching middleware
app.UseResponseCaching();

// Enable Session middleware
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Course}/{action=Index}/{id?}");

app.Run();
