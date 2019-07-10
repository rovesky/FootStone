
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
	 
		exception AccountException
		{
			string message;
		}

		struct RegisterInfo
		{
			string account;
			string password;
		}	

		class LoginData
		{
		}

		["amd"]interface IAccount
		{
  
		   void LoginRequest(string account,string pwd) throws AccountException;

		   void RegisterRequest(string account,RegisterInfo info) throws AccountException;

		   void TestLoginRequest(string account,string pwd,LoginData data) throws AccountException;

		}   	
	}
}