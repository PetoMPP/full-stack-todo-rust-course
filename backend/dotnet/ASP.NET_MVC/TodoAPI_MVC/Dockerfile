FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
VOLUME /code
WORKDIR /source

COPY *.sln .
COPY TodoAPI_MVC/*.csproj ./TodoAPI_MVC/
COPY TodoAPI_MVC_Tests/*.csproj ./TodoAPI_MVC_Tests/
RUN dotnet restore

COPY TodoAPI_MVC/. ./TodoAPI_MVC/
COPY TodoAPI_MVC_Tests/. ./TodoAPI_MVC_Tests/
WORKDIR /source/TodoAPI_MVC
RUN dotnet publish -c release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:6.0
ENV ASPNETCORE_URLS=http://*:5000/
EXPOSE 5000
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "TodoAPI_MVC.dll"]
