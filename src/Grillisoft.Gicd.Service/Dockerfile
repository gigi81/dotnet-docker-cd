# Multi-stage build for Grillisoft.Gicd.Service
# Build context should be the repository root when running docker build -f src/Grillisoft.Gicd.Service/Dockerfile .

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /build

# Copy solution and common MSBuild props to leverage layer caching
COPY *.slnx ./
COPY *.props ./

# Copy project files
COPY src ./src
COPY tests ./tests

# Speed up dotnet by skipping first-run work and using a known NuGet packages folder
ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    NUGET_PACKAGES=/root/.nuget/packages

# Restore packages using BuildKit cache mounts for NuGet. Requires BuildKit (docker buildx) to be effective.
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.nuget/NuGetFallbackFolder \
    dotnet restore --use-current-runtime

# Copy everything and publish
WORKDIR /build/src/Grillisoft.Gicd.Service
RUN --mount=type=cache,target=/root/.nuget/packages \
    --mount=type=cache,target=/root/.nuget/NuGetFallbackFolder \
    dotnet publish -c Release -o /app/publish --no-restore

# Build runtime image
FROM ghcr.io/gigi81/gicd-base:latest AS runtime
WORKDIR /app

COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_ENVIRONMENT=Production

EXPOSE 80
ENTRYPOINT ["dotnet", "gicd.dll"]


