// **********************************************************************
//
// Copyright (c) 2003-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#pragma once

module FootStone
{
	module GrainInterfaces
	{
		exception PlayerNotExsit
		{
		}

		struct Item
		{
			string id;
			string name;
			int    type;
		}

		["clr:generic:List"]sequence<Item> ItemList;
		
		struct PlayerMaster
		{
			int hp;
			int mp;
		}
		struct PlayerInfo
		{
			string Key;
			string Name;
			PlayerMaster  playerMaster;
			ItemList items;
		}


		interface Player
		{
			["amd"] idempotent PlayerInfo getPlayerInfo(string playerId)
				throws PlayerNotExsit;

    
		}
	}
}