
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
			void RecvData(Bytes data);
		}
   
		["amd"]interface IZone
		{ 
		    void SendData(Bytes data);
			
			EndPointZone BindZone(string zoneId,string playerId);
			
			void PlayerEnter( );
        
			void PlayerLeave();	
		}  		
	}
}