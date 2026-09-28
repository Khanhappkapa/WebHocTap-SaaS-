# ============================================================
# Dockerfile cho ASP.NET Core 8 — Deploy lên Render
# Multi-stage build: build → publish → runtime
# ============================================================

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj trước để tận dụng Docker layer cache
COPY ["WebHocTap(SaaS).csproj", "./"]
RUN dotnet restore

# Copy toàn bộ source và build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime (image nhẹ hơn, không có SDK)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy output từ stage build
COPY --from=build /app/publish .

# Render sử dụng biến PORT, ASP.NET Core cần lắng nghe đúng port
ENV ASPNETCORE_URLS=http://+:${PORT:-10000}
ENV ASPNETCORE_ENVIRONMENT=Production

# Expose port (Render tự gán PORT)
EXPOSE 10000

ENTRYPOINT ["dotnet", "WebHocTap(SaaS).dll"]
