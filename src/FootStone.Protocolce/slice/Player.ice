// **********************************************************************
//
// Copyright (c) 2003-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#pragma once

#include "common.ice"
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
			string id;
			string name;
			int    serverId;
			int    level;
			
			string zoneId;
			RoleMaster roleMaster;
			ItemList items;
		};

		interface IPlayerPush
		{
			void hpChanged(int hp);
		}

		interface IPlayer
		{
			["amd"] idempotent PlayerInfo GetPlayerInfo() throws PlayerNotExsit;

			["amd"] void SetPlayerName(string name)	throws PlayerNotExsit;
    
		};

	
	};
};