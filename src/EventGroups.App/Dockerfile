#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
RUN mkdir /db
VOLUME /db
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["EventGroups.App/EventGroups.App.csproj", "EventGroups.App/"]
COPY ["EventGroups.Storage/EventGroups.Storage.csproj", "EventGroups.Storage/"]
COPY ["LoggerMessages.Common/LoggerMessages.Common.csproj", "LoggerMessages.Common/"]
RUN dotnet restore "EventGroups.App/EventGroups.App.csproj"
COPY . .
WORKDIR "/src/EventGroups.App"
RUN dotnet build "EventGroups.App.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "EventGroups.App.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EventGroups.App.dll"]