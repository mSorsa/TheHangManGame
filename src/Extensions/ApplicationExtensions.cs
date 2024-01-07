namespace TheHangManGame.Extensions;

public static class ApplicationExtensions
{
    public static void Configure(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSession();
        
        // Configure the HTTP request pipeline.
        if (!env.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();
    }
}