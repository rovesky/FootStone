start dotnet FootStone.Core.GameServer.dll --Ice.Config=config
ping 127.0.0.1 -n 8 -w 1000 > nul
start dotnet FootStone.Core.FrontServer.dll --Ice.Config=config
