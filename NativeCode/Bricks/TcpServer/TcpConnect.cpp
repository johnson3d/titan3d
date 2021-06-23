#include "TcpConnect.h"
#include "TcpServer.h"

#define new VNEW

NS_BEGIN

FOnTcpConnectRcvData TcpConnect::OnTcpConnectRcvData = nullptr;

TcpConnect::TcpConnect()
{
	mConnId = 0;
	mServer = nullptr;
	mConnected = false;
	GCHandle = nullptr;
}

TcpConnect::~TcpConnect()
{
	mConnId = 0;
	mServer = nullptr;
	mConnected = false;
	ASSERT(GCHandle==nullptr);
}

void TcpConnect::PushData(BYTE* ptr, int size)
{

}

bool TcpConnect::Send(BYTE* ptr, int size)
{
	if (!::HP_Server_Send(mServer->GetCoreObject(), mConnId, ptr, size))
		return false;
	return true;
}

void TcpConnect::Disconnect()
{
	::HP_Server_Disconnect(mServer->GetCoreObject(), mConnId, TRUE);
	mConnected = false;
}

NS_END