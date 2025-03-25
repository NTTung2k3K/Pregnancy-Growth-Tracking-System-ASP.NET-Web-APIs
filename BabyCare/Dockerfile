# Dùng .NET 8 base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Dùng .NET SDK để build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Chỉ copy file .csproj trước rồi restore (để tận dụng caching)
COPY ["BabyCare.API/BabyCare.API.csproj", "BabyCare.API/"]
WORKDIR "/src/BabyCare.API"
RUN dotnet restore "BabyCare.API.csproj"

# Copy toàn bộ source code vào container
COPY . .

# Build project
RUN dotnet build "BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish project
FROM build AS publish
RUN dotnet publish "BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image setup
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Khởi chạy ứng dụng
ENTRYPOINT ["dotnet", "BabyCare.API.dll"]
