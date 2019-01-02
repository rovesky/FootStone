
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
		
		interface ISessionPush
		{
			void SessionDestroyed();
		}
   
		interface ISession
		{      
		
		   ["amd"] void AddPush(ISessionPush* playerPush);
		
			void Destroy();
		}  

		interface ISessionFactory
		{
		   
			ISession* CreateSession(string name,string password);

			void Shutdown();
		}
	}
}