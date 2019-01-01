
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
		sequence<byte> Bytes;

		struct EndPointZone
		{
			string ip;
			int    port;
		}

		interface IZonePush
		{
			void ZoneSync(Bytes data);
		}
   
		interface IZone
		{ 
		    ["amd"]EndPointZone PlayerEnter(string zoneId);

		    void Move(Bytes data);
		
		}  		
	}
}