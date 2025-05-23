FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY InsightFlow.sln ./
COPY src/InsightFlow.Core/InsightFlow.Core.csproj src/InsightFlow.Core/
COPY src/InsightFlow.Infrastructure/InsightFlow.Infrastructure.csproj src/InsightFlow.Infrastructure/
COPY src/InsightFlow.Api/InsightFlow.Api.csproj src/InsightFlow.Api/
RUN dotnet restore
COPY . .
RUN dotnet publish src/InsightFlow.Api/InsightFlow.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "InsightFlow.Api.dll"]
