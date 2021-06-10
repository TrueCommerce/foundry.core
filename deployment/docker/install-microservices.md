# How to install Foundry Microservices

**Requirements**: docker machine

&nbsp;

## Connect to Docker machine

If using Foundry Docker QA VM...

1. In Hyper-V Manager select "Foundry QA Docker" VM and at the bottom switch to "Networking" tab there you will see VM IP address, need to remember one for the "External Switch", for example: 10.0.1.13

2. In host Windows start either cmd or powershell

```bash
ssh [VM IP address] -l [user name]
```

- for example:

```bash
ssh 10.0.1.13 -l qatester
```

3. after entering password it should open linux ssh remote terminal

&nbsp;

## Generate JWS Secret

```bash
docker run -it --rm truecommerce/foundry.core.tools /service:security /generatejwtsecret
```

> Remember JWT Secret Key somewhere, it will be used to configure MicroServices and Core

&nbsp;

## Migrate Core DB data to Microservices DBs

These tools will automatically (_if necessary_) create microservices tables in DB and migrate all the related data from old Core DB. Make sure that you are specified appropriate microservices version. Can share Core DB and microservices DBs if needed, their tables do not overlap.

> press 'y' to continue when asked ... also can use optional /Y to skip all questions
> also can use optional /skipcorecheck parameter to skip old Core Tenants integrity check ... useful for huge db when upgrading

- Migrate Security Microservice data

```bash
docker run -it --rm truecommerce/foundry.core.tools:[microservices version] /service:security /migratedb /sqlconnection:"[MicroServices DB connection string]" /coresqlconnection:"[Core DB connection string]]"
```

&nbsp;

## Download deployment file

1. Download Docker-Compose Foundry deployment file from TrueCommerce official github repository

2. Use curl or any other utility to get file from url

```bash
curl -SL https://raw.githubusercontent.com/TrueCommerce/foundry.core/master/deployment/docker/foundry.core-compose.yml -o foundry.core-compose.yml
```

&nbsp;

## Configure deployment file

1. Open foundry.core-compose.yml in nano editor (or any other installed editor)

```bash
nano foundry.core-compose.yml
```

2. Assign rabbitmq default user name and password in the following lines: - ```bash

```yaml
- RABBITMQ_DEFAULT_USER=<rabbitmq user>
```

```yaml
- RABBITMQ_DEFAULT_PASS=<rabbitmq password>
```

```yaml
- Foundry__RabbitMQ__User=<rabbitmq user>
```

```yaml
- Foundry__RabbitMQ__Password=<rabbitmq password>
```

3. Set Security MicroServices DB connection string in the following lines

- one for foundry_core_security_maintenance
- and one for foundry_core_security_microservice

```yaml
- Foundry__Security__SQLConnection=<Core DB connection string>
```

4. Set JWT Secret  
   Copy generated JWT Secret into foundry_core_security_microservice the following line

```yaml
- Foundry__Security__JwtSecret=<Jwt Secret>
```

5. Replace Foundry Microservices version with the appropriate one for testing / deployment
   > by default this file will container the latest official release (or beta before release)
   > image: truecommerce/foundry.core.security.maintenance:2.1
   > image: truecommerce/foundry.core.security.microservice:2.1

&nbsp;

## Configure https (optional)

1. Uncomment the following lines in foundry_core_security_microservice
   > just remove # sign

```yaml
#- ASPNETCORE_URLS=https://+;http://+
#- ASPNETCORE_Kestrel**Certificates**Default**Password=<certificate password>
#- ASPNETCORE_Kestrel**Certificates**Default**Path=/https/<certificate file name>
#volumes:
#- <certificate folder path>:/https/
```

2. Get real certificate or generate dev one as described in https://github.com/dotnet/dotnet-docker/blob/master/samples/aspnetapp/aspnetcore-docker-https.md

   > Need to generate one inside docker VM or copy real one into docker VM

   - set <certificate password> to certificate password
   - set <certificate file name> to certificate (.pfx) file name
   - set <certificate folder path> to the path to the folder where certificate file stored

3. sample configuration

```yaml
  - ASPNETCORE_URLS=https://+;http://+
  - ASPNETCORE_Kestrel**Certificates**Default**Password=test123
  - ASPNETCORE_Kestrel**Certificates**Default**Path=/https/aspnetapp.pfx
volumes:
  - ~/.aspnet/https:/https/
```

4. Replace

```yaml
ports: - "30100:80"
```

with

```yaml
ports: - "30100:443"
```

5. Now security microservice will be available at the https://[host]:30100

6. Exit and save file

&nbsp;

## Deploy Microservices

1. Deploy all microservices and detach

```bash
docker-compose -p '<tag, usually dev or qa>' -f foundry.core-compose.yml up -d
```

2. List all deployed services

```bash
docker container ls -a
```

&nbsp;

## Test containers

**Security MicroService**

1. Start any browser

2. Open swagger playground for Security MicroService
   http://[docker machine ip]:30100/swagger/index.html
3. Show JSON string with healthcheck
   http://[docker machine ip]:30100/healthcheck

**Security Maintenance MicroService**

1. List all containers

```bash
docker container ls -a
```

2. Find container id for truecommerce/foundry.core.security.maintenance

3 Show service logs

```bash
docker logs [container id ... can use just a first few characters from id, do not have to copy the whole id]
```

**Kibana UI**

1. Start any browser and go to [docker machine ip]:5601

   > it should open Kibana UI (it can take a while to start Kibana, just refresh page if not started yet)
   > Foundry is using Kibana for logging
   > for more information about Kibana, please, visit official web site: https://www.elastic.co/products/kibana

&nbsp;

## Stop and remove MicroServices (if needed)

```bash
docker-compose -p '<tag, usually dev or qa>' -f foundry.core-compose.yml down
```
