﻿
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
		   
			["amd"] ISession* CreateSession(string account,string password);

			void Shutdown();
		}
	}
}