FROM microsoft/dotnet:2.2-sdk AS build-env
WORKDIR /app

# Copy necessary files and restore as distinct layer
COPY *.csproj ./
RUN dotnet restore -s https://api.nuget.org/v3/index.json -s https://www.myget.org/F/grid-portal-infrastructure/auth/e5bb1dde-d6f4-4385-aba8-c29dc1b93fd2/api/v3/index.json

# Copy everything else and build
COPY . ./
RUN dotnet publish -c Release -o out

# Build runtime image
FROM microsoft/dotnet:2.2-aspnetcore-runtime
COPY --from=build-env /app/out .

# Expose ports
EXPOSE 5127/tcp
ENV ASPNETCORE_URLS http://*:5127
HEALTHCHECK --interval=30s --timeout=3s --retries=1 CMD curl --silent --fail http://localhost:5127/hc || exit 1

# Configure environment variables
ENV WEBSITE_SITE_NAME OrderServiceAPI

# Start
ENTRYPOINT ["dotnet", "OrderService.dll"]
