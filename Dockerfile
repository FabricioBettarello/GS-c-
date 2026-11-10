FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY Directory.Build.props ./
COPY src/AlagaAlerta.Domain/AlagaAlerta.Domain.csproj src/AlagaAlerta.Domain/
COPY src/AlagaAlerta.Application/AlagaAlerta.Application.csproj src/AlagaAlerta.Application/
COPY src/AlagaAlerta.Infrastructure/AlagaAlerta.Infrastructure.csproj src/AlagaAlerta.Infrastructure/
COPY src/AlagaAlerta.Api/AlagaAlerta.Api.csproj src/AlagaAlerta.Api/
RUN dotnet restore src/AlagaAlerta.Api/AlagaAlerta.Api.csproj
COPY src ./src
RUN dotnet publish src/AlagaAlerta.Api/AlagaAlerta.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "AlagaAlerta.Api.dll"]
