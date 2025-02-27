  builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<IProfileCache, ProfileCache>();
    builder.Services.AddSingleton<IProfileCacheService, ProfileCacheService>();

    // Adiciona os outros serviços
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(SwaggerConfig.Configure);
    builder.Services.AddSingleton<IProfileService, ProfileService>();
    builder.Services.AddSingleton<IProfileRepository, ProfileRepository>();

    // Constrói a aplicação
    var app = builder.Build();

    // Adiciona o middleware de tratamento global de exceções
    app.UseGlobalExceptionHandling();

    // Configura ambiente de desenvolvimento
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ValidProfiles API v1"));
    }

    // Configuração da pipeline de requisições HTTP
    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseAuthorization();

    // Configuração do Serilog para logging de requisições
    app.UseSerilogRequestLogging();

    app.MapControllers();

    // Executa a aplicação
    app.Run();
    
    return 0;
}
catch (Exception ex)
{
    Log.Fatal(ex, "A aplicação falhou ao iniciar");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}