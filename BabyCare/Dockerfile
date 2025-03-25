# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src/BabyCare
COPY ["BabyCare/BabyCare.sln", "BabyCare/"]
COPY ["BabyCare/BabyCare.API/BabyCare.API.csproj", "BabyCare/BabyCare.API/"]
COPY ["BabyCare/BabyCare.Core/BabyCare.Core.csproj", "BabyCare/BabyCare.Core/"]
COPY ["BabyCare/BabyCare.Services/BabyCare.Services.csproj", "BabyCare/BabyCare.Services/"]
COPY ["BabyCare/BabyCare.Contract.Services/BabyCare.Contract.Services.csproj", "BabyCare/BabyCare.Contract.Services/"]
COPY ["BabyCare/BabyCare.Repositories/BabyCare.Repositories.csproj", "BabyCare/BabyCare.Repositories/"]
COPY ["BabyCare/BabyCare.Contract.Repositories/BabyCare.Contract.Repositories.csproj", "BabyCare/BabyCare.Contract.Repositories/"]
COPY ["BabyCare/BabyCare.ModelViews/BabyCare.ModelViews.csproj", "BabyCare/BabyCare.ModelViews/"]
RUN dotnet restore "BabyCare/BabyCare.sln"
COPY . .
WORKDIR "/src/BabyCare/BabyCare.API"
RUN dotnet build "./BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BabyCare.API.dll"]