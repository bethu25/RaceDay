# Use the .NET 8 SDK image to build the application.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory.
WORKDIR /src

# Copy the MVC project file.
COPY ["RaceDay.MVC.csproj", "./"]

# Restore project dependencies.
RUN dotnet restore "RaceDay.MVC.csproj"

# Copy the remaining project files.
COPY . .

# Publish the MVC application.
RUN dotnet publish "RaceDay.MVC.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Use the smaller ASP.NET Core runtime image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

# Set the application directory.
WORKDIR /app

# Copy the published application.
COPY --from=build /app/publish .

# Configure the application to listen on port 8080.
ENV ASPNETCORE_URLS=http://+:8080

# Expose port 8080.
EXPOSE 8080

# Start the MVC application.
ENTRYPOINT ["dotnet", "RaceDay.MVC.dll"]