# Sử dụng base image .NET 8 runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Sử dụng .NET SDK để build dự án
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Định nghĩa biến cấu hình build
ARG BUILD_CONFIGURATION=Release

# Đặt thư mục làm việc
WORKDIR /src

# Sao chép solution file trước để restore dependency
COPY ["BabyCare/BabyCare.sln", "BabyCare/"]

# Copy từng project file để tránh cache sai
COPY ["BabyCare/BabyCare.API/BabyCare.API.csproj", "BabyCare/BabyCare.API/"]
COPY ["BabyCare/BabyCare.Core/BabyCare.Core.csproj", "BabyCare/BabyCare.Core/"]
COPY ["BabyCare/BabyCare.Services/BabyCare.Services.csproj", "BabyCare/BabyCare.Services/"]
COPY ["BabyCare/BabyCare.Contract.Services/BabyCare.Contract.Services.csproj", "BabyCare/BabyCare.Contract.Services/"]
COPY ["BabyCare/BabyCare.Repositories/BabyCare.Repositories.csproj", "BabyCare/BabyCare.Repositories/"]
COPY ["BabyCare/BabyCare.Contract.Repositories/BabyCare.Contract.Repositories.csproj", "BabyCare/BabyCare.Contract.Repositories/"]
COPY ["BabyCare/BabyCare.ModelViews/BabyCare.ModelViews.csproj", "BabyCare/BabyCare.ModelViews/"]

# Restore dependencies
WORKDIR /src/BabyCare
RUN dotnet restore "BabyCare.sln"

# Sao chép toàn bộ mã nguồn vào container
COPY . .

# Build project
WORKDIR "/src/BabyCare/BabyCare.API"
RUN dotnet build "BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish ứng dụng để chuẩn bị deploy
FROM build AS publish
RUN dotnet publish "BabyCare.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final image để chạy ứng dụng
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BabyCare.API.dll"]
