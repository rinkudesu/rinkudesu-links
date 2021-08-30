FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Rinkudesu.Services.Links/Rinkudesu.Services.Links.csproj"
WORKDIR "/src/Rinkudesu.Services.Links"
RUN dotnet build "Rinkudesu.Services.Links.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rinkudesu.Services.Links.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS final
EXPOSE 80
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Rinkudesu.Services.Links.dll"]
