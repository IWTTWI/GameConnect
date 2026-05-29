using Microsoft.EntityFrameworkCore;
using GameConnect.Data;
using GameConnect.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=midnightshop.db"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();});});
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();
    if (!context.SocialLinks.Any()){
        context.SocialLinks.AddRange(
            new SocialLink { Name = "Telegram", Url = "https://t.me/midnightshop", IconClass = "fab fa-telegram" },
            new SocialLink { Name = "Discord", Url = "https://discord.gg/midnight", IconClass = "fab fa-discord" },
            new SocialLink { Name = "VK", Url = "https://vk.com/midnightshop", IconClass = "fab fa-vk" },
            new SocialLink { Name = "YouTube", Url = "https://youtube.com/midnightshop", IconClass = "fab fa-youtube" });
        await context.SaveChangesAsync();}
    if (!context.Products.Any())
    {
        context.Products.AddRange(
            new Product { Name = "1000 Coins", Description = "Игровая валюта", Price = 99, PriceInCoins = 0, Type = "currency" },
            new Product { Name = "XP Boost", Description = "Ускорение прокачки на 24 часа", Price = 149, PriceInCoins = 500, Type = "boost" },
            new Product { Name = "Premium Pack", Description = "Набор премиум предметов", Price = 499, PriceInCoins = 1500, Type = "other" });
        await context.SaveChangesAsync();}}
app.UseCors("AllowAll");
app.UseStaticFiles();
app.MapGet("/api/users/{id}", async (long id, AppDbContext db) =>{
    var user = await db.Users.FindAsync(id);
    return user is null ? Results.NotFound() : Results.Ok(user);});
app.MapPost("/api/users", async (User user, AppDbContext db) =>{
    user.RegisteredAt = DateTime.UtcNow;
    db.Users.Add(user);
    await db.SaveChangesAsync();
    return Results.Created($"/api/users/{user.Id}", user);});
app.MapPut("/api/users/{id}", async (long id, User updatedUser, AppDbContext db) =>{
    var user = await db.Users.FindAsync(id);
    if (user is null) return Results.NotFound();
    user.Username = updatedUser.Username;
    user.FirstName = updatedUser.FirstName;
    user.LastName = updatedUser.LastName;
    user.Balance = updatedUser.Balance;
    user.MidnightCoins = updatedUser.MidnightCoins;
    await db.SaveChangesAsync();
    return Results.Ok(user);});
app.MapGet("/api/products", async (AppDbContext db) =>{
    return await db.Products.Where(p => p.IsAvailable).ToListAsync();});
app.MapGet("/api/products/{id}", async (int id, AppDbContext db) =>{
    var product = await db.Products.FindAsync(id);
    return product is null ? Results.NotFound() : Results.Ok(product);});
app.MapPost("/api/products", async (Product product, AppDbContext db) =>{
    db.Products.Add(product);
    await db.SaveChangesAsync();
    return Results.Created($"/api/products/{product.Id}", product);});
app.MapPut("/api/products/{id}", async (int id, Product updatedProduct, AppDbContext db) =>{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();
    product.Name = updatedProduct.Name;
    product.Description = updatedProduct.Description;
    product.Price = updatedProduct.Price;
    product.PriceInCoins = updatedProduct.PriceInCoins;
    product.Type = updatedProduct.Type;
    product.ImageUrl = updatedProduct.ImageUrl;
    product.IsAvailable = updatedProduct.IsAvailable;
    await db.SaveChangesAsync();
    return Results.Ok(product);});
app.MapDelete("/api/products/{id}", async (int id, AppDbContext db) =>{
    var product = await db.Products.FindAsync(id);
    if (product is null) return Results.NotFound();

    db.Products.Remove(product);
    await db.SaveChangesAsync();
    return Results.Ok();});
app.MapGet("/api/socials", async (AppDbContext db) =>{
    return await db.SocialLinks.ToListAsync();});
app.MapPost("/api/socials", async (SocialLink social, AppDbContext db) =>{
    db.SocialLinks.Add(social);
    await db.SaveChangesAsync();
    return Results.Created($"/api/socials/{social.Id}", social);});
app.MapDelete("/api/socials/{id}", async (int id, AppDbContext db) =>{
    var social = await db.SocialLinks.FindAsync(id);
    if (social is null) return Results.NotFound();
    db.SocialLinks.Remove(social);
    await db.SaveChangesAsync();
    return Results.Ok();});
app.MapPost("/api/users/{userId}/purchase", async (long userId, int productId, string paymentType, AppDbContext db) =>{
    var user = await db.Users.FindAsync(userId);
    var product = await db.Products.FindAsync(productId);
    if (user is null || product is null) return Results.NotFound();
    if (paymentType == "money"){
        if (user.Balance < product.Price) return Results.BadRequest("Недостаточно средств");
        user.Balance -= product.Price;}
    else if (paymentType == "coins"){
        if (user.MidnightCoins < product.PriceInCoins) return Results.BadRequest("Недостаточно coins");
        user.MidnightCoins -= product.PriceInCoins;}
    user.Items.Add(new UserItem{
        Name = product.Name,
        Type = product.Type,
        Quantity = 1,
        PurchasedAt = DateTime.UtcNow});
    await db.SaveChangesAsync();
    return Results.Ok(user);});
app.MapFallbackToFile("index.html");
app.Run();