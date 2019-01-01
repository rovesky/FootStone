// **********************************************************************
//
// Copyright (c) 2003-2018 ZeroC, Inc. All rights reserved.
//
// **********************************************************************

#pragma once

//#include "Common.ice"

module FootStone
{
	module GrainInterfaces
	{

		struct MasterProperty
		{
			int str;
			int agil;
			int intel;
		}
		
		struct RoleMaster
		{
			MasterProperty property;

		}

		interface IRoleMasterPush
		{
			void PropertyChanged(MasterProperty property);
		}

		interface IRoleMaster
		{
			["amd"] idempotent MasterProperty GetProperty() ;//throws PlayerNotExsit;

			["amd"] void SetProperty(MasterProperty property)	;//throws PlayerNotExsit;
    
		};

	
	};
};