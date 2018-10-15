FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["DemoService.Microservice/DemoService.Microservice.csproj", "DemoService.Microservice/"]
COPY ["DemoService.Core/DemoService.Core.csproj", "DemoService.Core/"]
COPY ["DemoService.SQL/DemoService.SQL.csproj", "DemoService.SQL/"]
RUN dotnet restore "DemoService.Microservice/DemoService.Microservice.csproj"
COPY . .
WORKDIR "/src/DemoService.Microservice"
RUN dotnet build "DemoService.Microservice.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DemoService.Microservice.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DemoService.Microservice.dll"]