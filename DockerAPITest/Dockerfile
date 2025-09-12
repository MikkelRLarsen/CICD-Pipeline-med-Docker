FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["CICD-Pipeline-med-Docker/CICD-Pipeline-med-Docker.csproj", "CICD-Pipeline-med-Docker/"]
RUN dotnet restore "./CICD-Pipeline-med-Docker/CICD-Pipeline-med-Docker.csproj"
COPY . .
WORKDIR "/src/CICD-Pipeline-med-Docker"
RUN dotnet build "./CICD-Pipeline-med-Docker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./CICD-Pipeline-med-Docker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CICD-Pipeline-med-Docker.dll"]
