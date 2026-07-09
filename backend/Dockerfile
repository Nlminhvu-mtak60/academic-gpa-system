# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files and restore dependencies
COPY ["src/AcademicGPA.Domain/AcademicGPA.Domain.csproj", "src/AcademicGPA.Domain/"]
COPY ["src/AcademicGPA.Application/AcademicGPA.Application.csproj", "src/AcademicGPA.Application/"]
COPY ["src/AcademicGPA.Infrastructure/AcademicGPA.Infrastructure.csproj", "src/AcademicGPA.Infrastructure/"]
COPY ["src/AcademicGPA.API/AcademicGPA.API.csproj", "src/AcademicGPA.API/"]
COPY ["Directory.Packages.props", "./"]
RUN dotnet restore "src/AcademicGPA.API/AcademicGPA.API.csproj"

# Copy remaining source code and publish
COPY . .
RUN dotnet publish "src/AcademicGPA.API/AcademicGPA.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 10000
ENV ASPNETCORE_ENVIRONMENT=Production
# Render injects PORT env var; default to 10000 for local Docker testing
ENTRYPOINT dotnet AcademicGPA.API.dll --urls "http://+:${PORT:-10000}"
