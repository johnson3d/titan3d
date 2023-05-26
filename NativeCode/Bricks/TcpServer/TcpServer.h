#pragma once
#include "../../Base/IUnknown.h"
#include "../../Base/thread/vfxcritical.h"

#if defined(PLATFORM_WIN)
#define HPSOCKET_STATIC_LIB
#include "../../../3rd/native/HPSocket/Windows/include/HPSocket4C.h"
#else
#endif

NS_BEGIN

class TcpConnect;
class TcpServer;

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void (*FOnTcpConnectAccept)(TcpServer* pServer, TcpConnect* pConnect);

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void (*FOnTcpConnectClosed)(TcpServer* pServer, TcpConnect* pConnect, int state);

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void (*FOnTcpServerListen)(TcpServer* pServer);

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void (*FOnTcpServerShutdown)(TcpServer* pServer);

class TR_CLASS()
TcpServer : public IWeakReference
{
public:
	void* GCHandle;
protected:
	VSLLock mLocker;
	HP_TcpPullServer mServer;
	HP_TcpPullServerListener mListener;
	std::map<HP_CONNID, TcpConnect*> mTcpConnects;
	
	static En_HP_HandleResult WINAPI OnPrepareListen(HP_Server pSender, SOCKET soListen);
	static En_HP_HandleResult WINAPI OnAccept(HP_Server pSender, HP_CONNID dwConnID, SOCKET soClient);
	static En_HP_HandleResult WINAPI OnSend(HP_Server pSender, HP_CONNID dwConnID, const BYTE * pData, int iLength);
	static En_HP_HandleResult WINAPI OnReceive(HP_Server pSender, HP_CONNID dwConnID, int iLength);
	static En_HP_HandleResult WINAPI OnClose(HP_Server pSender, HP_CONNID dwConnID, En_HP_SocketOperation enOperation, int iErrorCode);
	static En_HP_HandleResult WINAPI OnShutdown(HP_Server pSender);

	static TcpConnect* GetConnect(HP_Server pSender, HP_CONNID dwConnID);
	static FOnTcpConnectAccept OnTcpConnectAccept;
	static FOnTcpConnectClosed OnTcpConnectClosed;
	static FOnTcpServerListen OnTcpServerListen;
	static FOnTcpServerShutdown OnTcpServerShutdown;
public:
	TcpServer();
	~TcpServer();
	HP_TcpPullServer GetCoreObject() {
		return mServer;
	}
	bool StartServer(const char* ip, USHORT port);
	void StopServer();

	void OnShutdown();
	TcpConnect* CreateTcpConnect(HP_CONNID dwConnID);
	void CloseTcpConnect(HP_CONNID dwConnID);

	static void SetOnTcpConnectAccept(FOnTcpConnectAccept fn) {
		OnTcpConnectAccept = fn;
	}
	static void SetOnTcpConnectClosed(FOnTcpConnectClosed fn) {
		OnTcpConnectClosed = fn;
	}
	static void SetOnTcpServerListen(FOnTcpServerListen fn) {
		OnTcpServerListen = fn;
	}
	static void SetOnTcpServerShutdown(FOnTcpServerShutdown fn) {
		OnTcpServerShutdown = fn;
	}
};

class TR_CLASS()
	TcpServerManager : public IWeakReference
{
	VSLLock mLocker;
	std::vector<TcpServer*>	mServers;
	static TcpServerManager smInstance;
public:
	static TcpServerManager* GetInstance() {
		return &smInstance;
	}
	void RegServer(TcpServer* server);
	void UnregServer(TcpServer * server);
	TcpServer* FindServer(HP_TcpPullServer server) {
		for (auto i : mServers)
		{
			if (i->GetCoreObject() == server) {
				return i;
			}
		}
		return nullptr;
	}
};

NS_END