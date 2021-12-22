FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Rinkudesu.Services.Links/Rinkudesu.Services.Links.csproj"
WORKDIR "/src/Rinkudesu.Services.Links"
RUN dotnet build "Rinkudesu.Services.Links.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Rinkudesu.Services.Links.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS final
EXPOSE 80
WORKDIR /app
COPY --from=publish /app/publish .
HEALTHCHECK --interval=20s --start-period=5s --retries=3 CMD curl --fail http://localhost/health || exit 1
ENTRYPOINT ["dotnet", "Rinkudesu.Services.Links.dll"]
