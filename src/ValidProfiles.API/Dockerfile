FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ValidProfiles.API/ValidProfiles.API.csproj", "ValidProfiles.API/"]
COPY ["ValidProfiles.Application/ValidProfiles.Application.csproj", "ValidProfiles.Application/"]
COPY ["ValidProfiles.Domain/ValidProfiles.Domain.csproj", "ValidProfiles.Domain/"]
COPY ["ValidProfiles.Infrastructure/ValidProfiles.Infrastructure.csproj", "ValidProfiles.Infrastructure/"]
COPY ["ValidProfiles.Shared/ValidProfiles.Shared.csproj", "ValidProfiles.Shared/"]
RUN dotnet restore "./ValidProfiles.API/ValidProfiles.API.csproj"
COPY . .
WORKDIR "/src/ValidProfiles.API"
RUN dotnet build "./ValidProfiles.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ValidProfiles.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ValidProfiles.API.dll"]