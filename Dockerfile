
FROM node as react-build
WORKDIR /client

COPY ./src/Apps/Jev.OpenIddict.Web/jev-openiddict-web/ ./

RUN npm i
RUN npx webpack --env production=true output=dist

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env-sdk
WORKDIR /app

# Copy csproj and restore as distinct layers
COPY /src/ ./
COPY ./ ./

RUN dotnet restore

COPY --from=react-build ./client/dist/ ./src/Apps/Jev.OpenIddict.Web/wwwroot/

RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env-sdk /app/out .
ENTRYPOINT ["dotnet", "Jev.OpenIddict.Web.dll"]