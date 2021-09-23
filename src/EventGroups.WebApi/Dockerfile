#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
RUN mkdir /db
VOLUME /db
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["EventGroups.WebApi/EventGroups.WebApi.csproj", "EventGroups.WebApi/"]
COPY ["EventGroups.Storage/EventGroups.Storage.csproj", "EventGroups.Storage/"]
COPY ["LoggerMessages.Common/LoggerMessages.Common.csproj", "LoggerMessages.Common/"]
COPY ["EventGroups.Contract/EventGroups.Contract.csproj", "EventGroups.Contract/"]
RUN dotnet restore "EventGroups.WebApi/EventGroups.WebApi.csproj"
COPY . .
WORKDIR "/src/EventGroups.WebApi"
RUN dotnet build "EventGroups.WebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventGroups.WebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventGroups.WebApi.dll"]