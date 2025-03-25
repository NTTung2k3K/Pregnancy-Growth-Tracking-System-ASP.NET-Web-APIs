# Dùng base image cho ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Dùng SDK để build project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy toàn bộ solution để đảm bảo context đúng
COPY . .

# Chạy restore với đúng thư mục
WORKDIR /src/BabyCare.API
RUN dotnet restore

# Build ứng dụng
RUN dotnet build -c $BUILD_CONFIGURATION -o /app/build

# Publish ứng dụng
FROM build AS publish
RUN dotnet publish -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage để chạy app
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BabyCare.API.dll"]
