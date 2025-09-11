# DockerAPITest — CI/CD med GitHub Actions, Docker Hub og Watchtower

Dette projekt viser, trin for trin, hvordan man sætter en fuld **CI/CD pipeline** op for et .NET API eller Blazor Server projekt ved hjælp af:

- **Docker** (bygning af images)
- **GitHub Actions** (automatisk build & push)
- **Docker Hub** (host af images)
- **Watchtower** (automatisk opdatering på server/VM)

Målet er, at du kan følge opskriften 1:1, også selvom du aldrig har arbejdet med CI/CD eller Docker før.

---

## 🔧 Trin 1: Lav en Dockerfile (C# API eller Blazor Server)

Når du opretter et nyt projekt i Visual Studio (fx *ASP.NET Core Web API* eller *Blazor Server*), kan du under **“Enable Docker Support”** krydse af for Docker.  
Hvis du allerede har et eksisterende projekt, kan du tilføje en **Dockerfile** manuelt i projektmappen.  

Eksempel på Dockerfile for et .NET 8 API:

```dockerfile
# Byg image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "./DockerAPITest/DockerAPITest.csproj"
RUN dotnet publish "./DockerAPITest/DockerAPITest.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DockerAPITest.dll"]
```

Gem filen som:  
```
DockerAPITest/Dockerfile
```

---

## 🔑 Trin 2: Opret Docker Hub repo og Access Token

1. Log ind på [Docker Hub](https://hub.docker.com).  
2. Opret et nyt repository, fx `thenuker2/weatherforecast_api`.  
3. Under **Account Settings → Security → Access Tokens**, opret et token og kopier det.  

---

## 🎬 Trin 3: Tilføj GitHub Secrets og Actions workflow

👉 Følg denne video for trin-for-trin guide:  
[YouTube: GitHub Actions Docker Build & Push Tutorial](https://www.youtube.com/watch?v=x7f9x30W_dI)

I dit GitHub repository:

- Gå til **Settings → Secrets and variables → Actions → New repository secret**
- Tilføj:
  - `DOCKER_HUB_TOKEN` → dit Docker Hub token

---

## ⚙️ Trin 4: Opret GitHub Actions workflow

### 📝 Hvad er GitHub Actions?

**GitHub Actions** er GitHubs indbyggede værktøj til **CI/CD** (Continuous Integration / Continuous Deployment).  
Det betyder, at man kan automatisere ting som:

- Bygning af kode (fx compile dit .NET projekt)
- Test af kode (fx unit tests)
- Deployment (fx bygge Docker images og uploade til Docker Hub)

Man beskriver en workflow-fil i **YAML** (fx `.github/workflows/docker-publish.yml`), hvor man definerer:
- **Hvornår** noget skal ske (fx når der pushes til `master`)
- **Hvilke jobs** der skal køres (fx byg Docker image)
- **Hvilke steps** der udføres (fx `docker build` → `docker push`)

Fordelen er, at det hele sker **automatisk i skyen**, uden at du behøver gøre det manuelt på din egen PC.


Lav en fil i dit repo:  
```
.github/workflows/docker-publish.yml
```

Indsæt dette (din YAML):

```yaml
name: Docker Image CI

on:
  push:
    branches: [ "master" ]
  pull_request:
    branches: [ "master" ]

jobs:

  build:

    runs-on: ubuntu-latest

    steps:
    - uses: actions/checkout@v4
    - name: Build the Docker image
      run: docker build . -t thenuker2/weatherforecast_api:latest -f DockerAPITest/Dockerfile
    - name: Push Image to DockerHub
      run: |
       docker login -u thenuker2 -p ${{ secrets.DOCKER_HUB_TOKEN }}
       docker push thenuker2/weatherforecast_api:latest
```

---

## 🖥️ Trin 5: Test på udviklingsmaskinen

Før du sætter Watchtower op, bør du teste at image’et virker lokalt:

```bash
# byg lokalt
docker build -t thenuker2/weatherforecast_api:latest -f DockerAPITest/Dockerfile .

# kør image lokalt
docker run -d -p 5000:80 --name weatherapi thenuker2/weatherforecast_api:latest
```

Gå derefter til `http://localhost:5000` i browseren og tjek at dit API/Blazor kører.

---

## 🔄 Trin 6: Opsæt produktion + Watchtower

Når din VM er klar med Docker installeret, kan du køre Watchtower **med** eller **uden** docker-compose.

### ✅ Variant 1: Watchtower med docker-compose

Lav en fil på din VM:  
`docker-compose.yml`

```yaml
version: '3.8'

services:
  app:
    image: thenuker2/weatherforecast_api:latest
    container_name: weatherapi
    restart: always
    ports:
      - "80:80"

  watchtower:
    image: containrrr/watchtower
    container_name: watchtower
    restart: always
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock
      - /home/ubuntu/.docker:/root/.docker   # <- til Docker login credentials
    command: --interval 30 --cleanup --debug
```

Kør derefter:

```bash
docker compose up -d
```

---

### ✅ Variant 2: Watchtower uden docker-compose (direkte `docker run`)

Start først din app:

```bash
docker run -d   --name weatherapi   --restart always   -p 80:80   thenuker2/weatherforecast_api:latest
```

Start derefter Watchtower:

```bash
docker run -d   --name watchtower   --restart always   -v /var/run/docker.sock:/var/run/docker.sock   -v /home/ubuntu/.docker:/root/.docker   containrrr/watchtower   --interval 30   --cleanup   --debug
```

---

## 🧪 Trin 7: Verificér at det virker

1. **Find IP-adressen på din VM**  
   Kør følgende på din VM:
   ```bash
   ip addr show
   ```
   eller
   ```bash
   hostname -I
   ```
   Du får noget i stil med:
   ```
   192.168.1.45
   ```

2. **Kald API’et fra din udviklingsmaskine**  
   Åbn browseren eller brug `curl` til at teste:  
   ```bash
   curl http://192.168.1.45/api/weatherforecast
   ```
   eller gå direkte i browseren:
   ```
   http://192.168.1.45/api/weatherforecast
   ```

   Hvis alt er sat korrekt op, vil du nu se dit API’s JSON-output (fx WeatherForecast-data).

3. **Deploy et nyt build for at teste Watchtower**  
   - Lav en ændring i dit projekt (fx tilføj en ekstra property i WeatherForecast).  
   - Push ændringen til `master`.  
   - GitHub Actions bygger og pusher automatisk det nye image.  
   - Watchtower opdager det og genstarter containeren på VM’en.  

   Prøv igen at tilgå:
   ```
   http://192.168.1.45/api/weatherforecast
   ```
   Du burde nu se ændringen — uden at du selv har genstartet noget manuelt 🎉

---

## 🛠️ Troubleshooting — Watchtower

1. **Docker login og credentials**  
   Når du kører `docker login`, vil terminalen vise hvor dine login-oplysninger gemmes (typisk i `~/.docker/config.json`).  
   Denne path skal mountes ind i Watchtower for at det kan trække private images:

   ```yaml
   volumes:
     - /var/run/docker.sock:/var/run/docker.sock
     - /home/ubuntu/.docker:/root/.docker
   ```

2. **Watchtower opdaterer ikke**  
   - Tjek logs: `docker logs -f watchtower`  
   - Sørg for at image faktisk ændrer sig (prøv at pushe et nyt tag).  

3. **Permission denied**  
   - Sørg for at `-v /var/run/docker.sock:/var/run/docker.sock` er med.  

4. **Netværksproblemer**  
   - Test: `docker pull thenuker2/weatherforecast_api:latest` manuelt på serveren.  

---

## 📚 Ressourcer
- [YouTube — CI/CD med GitHub Actions og Docker Hub](https://www.youtube.com/watch?v=x7f9x30W_dI)  
- [Watchtower GitHub Repo](https://github.com/containrrr/watchtower)  
- [SSH Remote Commands Action (alternativ til Watchtower)](https://github.com/marketplace/actions/ssh-remote-commands)  

---

## ✅ Konklusion

Med ovenstående opsætning får du:
1. En Dockerfile til dit projekt.  
2. GitHub Actions workflow, der bygger og pusher images til Docker Hub.  
3. En produktionsserver med Watchtower, der selv holder containere opdateret (enten med eller uden compose).  
4. En verificeringstest, hvor du kan tilgå dit API via VM’ens IP og se opdateringerne slå igennem.  

→ **Resultat:** Fuldt CI/CD flow uden manuel opdatering! 🚀
