FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY Backend.slnx Directory.Packages.props ./
COPY src/Backend.csproj src/
RUN dotnet restore Backend.slnx
COPY src/ src/
RUN dotnet publish src/Backend.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 60355
ENV ASPNETCORE_URLS=http://+:60355
USER $APP_UID
ENTRYPOINT ["dotnet", "Backend.dll"]
