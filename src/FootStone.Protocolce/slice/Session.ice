
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
		    void AddPush(ISessionPush* sessionPush);
		
			void Destroy();
		}  

		interface ISessionFactory
		{		   
			ISession* CreateSession(string account,string password);

			void Shutdown();
		}
	}
}