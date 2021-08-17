FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY ["Rinkudesu.Services.Links/Rinkudesu.Services.Links.csproj", "Rinkudesu.Services.Links/"]
RUN dotnet restore "Rinkudesu.Services.Links/Rinkudesu.Services.Links.csproj"
COPY . .
WORKDIR "/src/Rinkudesu.Services.Links"
RUN dotnet build "Rinkudesu.Services.Links.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rinkudesu.Services.Links.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rinkudesu.Services.Links.dll"]
