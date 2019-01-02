
// **********************************************************************
//
// Copyright (c) 2003-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************
#pragma once
#include "Player.ice"


module FootStone
{
	module GrainInterfaces
	{  
		exception AccountException
		{
			string message;
		}

		struct RegisterInfo
		{
			string account;
			string password;
		}

		struct LoginInfo
		{
			string account;
			string password;
		}

		struct ServerInfo
		{
			int    id;
			string name;
			int    status;
		}
		["clr:generic:List"]sequence<ServerInfo> ServerList;

		struct PlayerShortInfo
		{
			string playerId;
			string name;
			int    gender;
			int    modelId;
		}
		["clr:generic:List"]sequence<PlayerShortInfo> PlayerList;

		interface Account
		{
      
		  ["amd"] void LoginRequest(LoginInfo info) throws AccountException;

		  ["amd"] void RegisterRequest(RegisterInfo info) throws AccountException;

		  ["amd"] ServerList GetServerListRequest() throws AccountException;

		  ["amd"] PlayerList GetPlayerListRequest(int serverId) throws AccountException;
		
	  	  ["amd"] void SelectPlayerRequest(string playerId) throws AccountException;		
		
	      ["amd"] string CreatePlayerRequest(string name,int serverId) throws AccountException;

		}
    
	}

}