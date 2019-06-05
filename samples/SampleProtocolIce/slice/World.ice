
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
		exception WorldException
		{	
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


		interface World
		{
      	  ["amd"] ServerList GetServerListRequest() throws WorldException;
		 
		  ["amd"] PlayerList GetPlayerListRequest(int serverId) throws WorldException;		
		}
    
	}

}