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
		
		struct PlayerInfo
		{
			string Key;
			string Name;
		}
		interface Player
		{
			["amd"] idempotent PlayerInfo getPlayerInfo(string playerId)
				throws PlayerNotExsit;

    
		}
	}
}