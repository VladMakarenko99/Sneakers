# Build stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app

# Copy and restore dependencies
COPY . .
RUN dotnet restore

# Publish the application
RUN dotnet publish -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

# Set environment variables if required (e.g., for database connection string)

# Expose port if required (e.g., for HTTP traffic)

# Define the entry point
ENTRYPOINT ["dotnet", "practice.dll"]