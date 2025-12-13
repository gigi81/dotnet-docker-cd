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

# Restore packages
RUN dotnet restore --use-current-runtime

# Copy everything and publish
WORKDIR /build/src/Grillisoft.Gicd.Service
RUN dotnet publish -c Release -o /app/publish --no-restore

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./

ENV ASPNETCORE_URLS=http://+:80
ENV DOTNET_RUNNING_IN_CONTAINER=true

EXPOSE 80
ENTRYPOINT ["dotnet", "Grillisoft.Gicd.Service.dll"]
