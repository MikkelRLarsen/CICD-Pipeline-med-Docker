FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DockerAPITest/DockerAPITest.csproj", "DockerAPITest/"]
RUN dotnet restore "./DockerAPITest/DockerAPITest.csproj"
COPY . .
WORKDIR "/src/DockerAPITest"
RUN dotnet build "./DockerAPITest.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./DockerAPITest.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DockerAPITest.dll"]
