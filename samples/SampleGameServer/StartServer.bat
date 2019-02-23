start dotnet SampleGameServer.dll --Ice.Config=config
ping 127.0.0.1 -n 8 -w 1000 > nul
start dotnet SampleFrontServer.dll --Ice.Config=config
