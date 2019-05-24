#pragma once

#include "Account.ice"

module Sample
{

class  SampleLoginData extends FootStone::GrainInterfaces::LoginData
{
	string code; 
}

}