# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base

# Set default values for USER_ID and GROUP_ID
#ARG USER_ID=2000
#ARG GROUP_ID=2000

# Create a group and user with the passed or default UID/GID
#RUN addgroup --gid ${GROUP_ID} apiuser && \
#    adduser --disabled-password --gecos "" --uid ${USER_ID} --gid ${GROUP_ID} apiuser

# Switch to the new non-root user
USER $APP_UID
WORKDIR /app
EXPOSE 5000
HEALTHCHECK --start-period=60s --interval=10s CMD curl --fail http://localhost:5000/health || exit 1


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy the necessary solution-wide configuration files
COPY OAuth/Directory.Build.props ./
COPY OAuth/Directory.Packages.props ./

# Copy project files and restore dependencies
COPY ["OAuth/src/Viebal.ECommerce.Course.OAuth.API/Viebal.ECommerce.Course.OAuth.API.csproj", "OAuth/src/Viebal.ECommerce.Course.OAuth.API/"]
COPY ["OAuth/src/Viebal.ECommerce.Course.OAuth.Contract/Viebal.ECommerce.Course.OAuth.Contract.csproj", "OAuth/src/Viebal.ECommerce.Course.OAuth.Contract/"]
COPY ["OAuth/src/Viebal.ECommerce.Course.OAuth.Domain/Viebal.ECommerce.Course.OAuth.Domain.csproj", "OAuth/src/Viebal.ECommerce.Course.OAuth.Domain/"]
COPY ["OAuth/src/Viebal.ECommerce.Course.OAuth.Infrastructure/Viebal.ECommerce.Course.OAuth.Infrastructure.csproj", "OAuth/src/Viebal.ECommerce.Course.OAuth.Infrastructure/"]
COPY ["OAuth/src/Viebal.ECommerce.Course.OAuth.SharedKernel/Viebal.ECommerce.Course.OAuth.SharedKernel.csproj", "OAuth/src/Viebal.ECommerce.Course.OAuth.SharedKernel/"]
COPY ["OAuth/src/Viebal.ECommerce.Course.OAuth.UseCase/Viebal.ECommerce.Course.OAuth.UseCase.csproj", "OAuth/src/Viebal.ECommerce.Course.OAuth.UseCase/"]

RUN dotnet restore "./OAuth/src/Viebal.ECommerce.Course.OAuth.API/Viebal.ECommerce.Course.OAuth.API.csproj"

COPY . .
WORKDIR "/src/OAuth/src/Viebal.ECommerce.Course.OAuth.API"
RUN dotnet build "./Viebal.ECommerce.Course.OAuth.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Viebal.ECommerce.Course.OAuth.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Viebal.ECommerce.Course.OAuth.API.dll"]