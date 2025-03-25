# This stage is used when running from VS in fast mode (Default for Debug configuration) 
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base 
USER $APP_UID 
WORKDIR /app 
EXPOSE 8080 
EXPOSE 8081 
 
# This stage is used to build the service project 
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build 
ARG BUILD_CONFIGURATION=Release 
WORKDIR /src 

# Copy project files and restore dependencies
COPY ["BabyCare.API/BabyCare.API.csproj", "BabyCare.API/"]
COPY ["BabyCare.Core/BabyCare.Core.csproj", "BabyCare.Core/"]
COPY ["BabyCare.Services/BabyCare.Services.csproj", "BabyCare.Services/"]
COPY ["BabyCare.Contract.Services/BabyCare.Contract.Services.csproj", "BabyCare.Contract.Services/"]
COPY ["BabyCare.Repositories/BabyCare.Repositories.csproj", "BabyCare.Repositories/"]
COPY ["BabyCare.Contract.Repositories/BabyCare.Contract.Repositories.csproj", "BabyCare.Contract.Repositories/"]
COPY ["BabyCare.ModelViews/BabyCare.ModelViews.csproj", "BabyCare.ModelViews/"]
COPY ["BabyCare.WorkerService/BabyCare.WorkerService.csproj", "BabyCare.WorkerService/"]

# Restore dependencies
RUN dotnet restore "BabyCare.API/BabyCare.API.csproj"

# Copy all files
COPY . .

# Set working directory to API project
WORKDIR "/src/BabyCare.API"

# Build the project
RUN dotnet build "BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/build 
 
# Publish stage
FROM build AS publish 
ARG BUILD_CONFIGURATION=Release 
RUN dotnet publish "BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false 
 
# Final stage
FROM base AS final 
WORKDIR /app 
COPY --from=publish /app/publish . 
ENTRYPOINT ["dotnet", "BabyCare.API.dll"]