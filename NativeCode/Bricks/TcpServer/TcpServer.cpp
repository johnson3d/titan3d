#include "TcpServer.h"
#include "TcpConnect.h"

#define new VNEW

#if defined(PLATFORM_WIN)
	#if defined(_DEBUG)
		#pragma comment(lib, "HPSocket4C_D.lib")
	#else
		#pragma comment(lib, "HPSocket4C.lib")
	#endif
#else
	
#endif

NS_BEGIN

TcpServerManager	TcpServerManager::smInstance;
void TcpServerManager::RegServer(TcpServer* server)
{
	VAutoVSLLock lk(mLocker);
	for (auto i : mServers)
	{
		if (i == server) 
		{
			return;
		}
	}

	server->AddRef();
	mServers.push_back(server);
}

void TcpServerManager::UnregServer(TcpServer* server)
{
	VAutoVSLLock lk(mLocker);
	for (int i = 0; i < mServers.size(); i++)
	{
		if (mServers[i] == server)
		{
			mServers.erase(mServers.begin()+i);
			return;
		}
	}
}

FOnTcpConnectAccept TcpServer::OnTcpConnectAccept = nullptr;
FOnTcpConnectClosed TcpServer::OnTcpConnectClosed = nullptr;
FOnTcpServerListen TcpServer::OnTcpServerListen = nullptr;
FOnTcpServerShutdown TcpServer::OnTcpServerShutdown = nullptr;
TcpServer::TcpServer()
{
	mListener = nullptr;
	mServer = nullptr;
	GCHandle = nullptr;
}

TcpServer::~TcpServer()
{
	::Destroy_HP_TcpPullServer(mServer);
	::Destroy_HP_TcpPullServerListener(mListener);
	ASSERT(GCHandle == nullptr);
}

bool TcpServer::StartServer(const char* ip, USHORT port)
{
	mListener = ::Create_HP_TcpPullServerListener();
	mServer = ::Create_HP_TcpPullServer(mListener);
	::HP_Set_FN_Server_OnPrepareListen(mListener, OnPrepareListen);
	::HP_Set_FN_Server_OnAccept(mListener, OnAccept);
	::HP_Set_FN_Server_OnSend(mListener, OnSend);
	::HP_Set_FN_Server_OnPullReceive(mListener, OnReceive);
	::HP_Set_FN_Server_OnClose(mListener, OnClose);
	::HP_Set_FN_Server_OnShutdown(mListener, OnShutdown);

	TcpServerManager::GetInstance()->RegServer(this);
	if (::HP_Server_Start(mServer, ip, port) == FALSE)
	{
		return false;
	}
	return true;
}

void TcpServer::StopServer()
{
	::HP_Server_Stop(mServer);	
}

void TcpServer::OnShutdown()
{
	VAutoVSLLock lk(mLocker);
	for (auto& i : mTcpConnects)
	{
		if (OnTcpConnectClosed != nullptr)
		{
			OnTcpConnectClosed(i.second->mServer, i.second, 2);
		}
		i.second->Release();
	}
	mTcpConnects.clear();
}

En_HP_HandleResult TcpServer::OnPrepareListen(HP_Server pSender, SOCKET soListen)
{
	TCHAR szAddress[50];
	int iAddressLen = sizeof(szAddress) / sizeof(TCHAR);
	USHORT usPort;
	::HP_Server_GetListenAddress(pSender, szAddress, &iAddressLen, &usPort);

	auto server = TcpServerManager::GetInstance()->FindServer(pSender);
	ASSERT(server);
	if (OnTcpServerListen != nullptr)
		OnTcpServerListen(server);
	return HR_OK;
}

En_HP_HandleResult TcpServer::OnAccept(HP_Server pSender, HP_CONNID dwConnID, SOCKET soClient)
{
	auto server = TcpServerManager::GetInstance()->FindServer(pSender);
	ASSERT(server);
	auto tcpConn = server->CreateTcpConnect(dwConnID);
	ASSERT(tcpConn);

	::HP_Server_SetConnectionExtra(pSender, dwConnID, tcpConn);

	if(OnTcpConnectAccept!=nullptr)
	{
		OnTcpConnectAccept(server, tcpConn);
	}
	return HR_OK;
}

En_HP_HandleResult TcpServer::OnSend(HP_Server pSender, HP_CONNID dwConnID, const BYTE* pData, int iLength)
{
	//::PostOnSend(dwConnID, pData, iLength);
	return HR_OK;
}

En_HP_HandleResult TcpServer::OnReceive(HP_Server pSender, HP_CONNID dwConnID, int iLength)
{
	auto conn = GetConnect(pSender, dwConnID);
	if (conn == nullptr)
		return En_HP_HandleResult::HR_ERROR;
	
	while (iLength > 0)
	{
		En_HP_FetchResult result = En_HP_FetchResult::FR_OK;
		if (iLength > conn->mRcvTmpBuffer.size())
		{
			result = ::HP_TcpPullServer_Fetch(pSender, dwConnID, &conn->mRcvTmpBuffer[0], (int)conn->mRcvTmpBuffer.size());
			if (result != FR_OK)
			{
				break;
			}
			iLength -= (int)conn->mRcvTmpBuffer.size();
			conn->PushData(&conn->mRcvTmpBuffer[0], (int)conn->mRcvTmpBuffer.size());
		}
		else
		{
			result = ::HP_TcpPullServer_Fetch(pSender, dwConnID, &conn->mRcvTmpBuffer[0], iLength);
			if (result != FR_OK)
			{
				break;
			}
			iLength = 0;
			conn->PushData(&conn->mRcvTmpBuffer[0], iLength);
		}		
	}
	return HR_OK;
}

En_HP_HandleResult TcpServer::OnClose(HP_Server pSender, HP_CONNID dwConnID, En_HP_SocketOperation enOperation, int iErrorCode)
{
	auto conn = GetConnect(pSender, dwConnID);
	ASSERT(conn);

	if (OnTcpConnectClosed != nullptr)
	{
		OnTcpConnectClosed(conn->mServer, conn, 1);
	}
	conn->mServer->CloseTcpConnect(dwConnID);
	return HR_OK;
}

En_HP_HandleResult TcpServer::OnShutdown(HP_Server pSender)
{
	auto server = TcpServerManager::GetInstance()->FindServer(pSender);
	ASSERT(server);
	if (OnTcpServerShutdown != nullptr)
		OnTcpServerShutdown(server);
	TcpServerManager::GetInstance()->UnregServer(server);
	server->OnShutdown();
	
	return HR_OK;
}

TcpConnect* TcpServer::GetConnect(HP_Server pSender, HP_CONNID dwConnID)
{
	PVOID pInfo = nullptr;
	::HP_Server_GetConnectionExtra(pSender, dwConnID, &pInfo);
	return (TcpConnect*)pInfo;
}

TcpConnect* TcpServer::CreateTcpConnect(HP_CONNID dwConnID)
{
	VAutoVSLLock lk(mLocker);
	auto iter = mTcpConnects.find(dwConnID);
	if (iter != mTcpConnects.end())
	{
		return nullptr;
	}
	TcpConnect* conn = new TcpConnect();
	conn->mServer = this;
	conn->mConnId = dwConnID;
	conn->mConnected = true;
	mTcpConnects.insert(std::make_pair(dwConnID, conn));
	return conn;
}

void TcpServer::CloseTcpConnect(HP_CONNID dwConnID)
{
	VAutoVSLLock lk(mLocker);
	auto iter = mTcpConnects.find(dwConnID);
	if (iter != mTcpConnects.end())
	{
		return;
	}
	auto conn = iter->second;
	mTcpConnects.erase(iter);
	conn->Release();
}

NS_END