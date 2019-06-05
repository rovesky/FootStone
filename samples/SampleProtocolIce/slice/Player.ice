// **********************************************************************
//
// Copyright (c) 2003-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#pragma once

#include <common.ice>
#include "RoleMaster.ice"

module FootStone
{
	module GrainInterfaces
	{
	

		struct Item
		{
			string id;
			string name;
			int    type;
		};

		["clr:generic:List"]sequence<Item> ItemList;
		
	
		struct PlayerInfo
		{
			string account;
			string playerId;
			string name;
			int    gameId;
			int    level;
			
			string zoneId;
			RoleMaster roleMaster;
			ItemList items;
		};

		struct PlayerCreateInfo
		{
			string name;
			int    profession;
		}

		interface IPlayerPush
		{
			void hpChanged(int hp);
		}

		interface IPlayer
		{  	
	        ["amd"] string CreatePlayerRequest(int gameId,PlayerCreateInfo info);

		    ["amd"] void SelectPlayerRequest(string playerId) throws PlayerNotExsit;		
		
			["amd"] idempotent PlayerInfo GetPlayerInfo() throws PlayerNotExsit;

			["amd"] void SetPlayerName(string name)	throws PlayerNotExsit;
    
		};

	
	};
};