echo off
echo -----------------------EventGroups.App-------------------------------------
echo ----------------------------start------------------------------------------

docker build --force-rm -f EventGroups.App\Dockerfile -t eventgroupsapp:dev .
docker stop eventgroupsapp_instance
docker run --rm --name eventgroupsapp_instance -dt -v c:\GIT\loggermessages\data:/db -p 8000:80 -p 8001:443 -e ASPNETCORE_URLS=https://+:443;http://+:80 -e ASPNETCORE_HTTPS_PORT=8001 -v %USERPROFILE%\.aspnet\https:/https/ -e ASPNETCORE_Kestrel__Certificates__Default__Password="test" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx eventgroupsapp:dev

echo -----------------------EventGroups.App-------------------------------------
echo ----------------------------end--------------------------------------------
echo
echo -----------------------EventGroups.WebApi----------------------------------
echo ----------------------------start------------------------------------------

docker build --force-rm -f EventGroups.WebApi\Dockerfile -t eventgroupswebapi:dev .
docker stop eventgroupswebapi_instance
docker run --rm --name eventgroupswebapi_instance -dt -v c:\GIT\database:/db -p 8002:80 -p 8003:443 -e ASPNETCORE_URLS=https://+:443;http://+:80 -e ASPNETCORE_HTTPS_PORT=8003 -v %USERPROFILE%\.aspnet\https:/https/ -e ASPNETCORE_Kestrel__Certificates__Default__Password="test" -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/aspnetapp.pfx eventgroupswebapi:dev

echo -----------------------EventGroups.WebApi----------------------------------
echo ----------------------------end--------------------------------------------
