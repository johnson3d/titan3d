#pragma once
#include "TcpServer.h"

NS_BEGIN

TR_CALLBACK(SV_CallConvention = System.Runtime.InteropServices.CallingConvention.Cdecl)
typedef void (*FOnTcpConnectRcvData)(TcpConnect* pConnect, BYTE* ptr, int size);

class TR_CLASS()
TcpConnect : public IWeakReference
{
public:
	void* GCHandle;
public:
	HP_CONNID		mConnId;
	TcpServer*		mServer;

	bool			mConnected;
	std::vector<BYTE>	mRcvTmpBuffer;

	static FOnTcpConnectRcvData OnTcpConnectRcvData;
public:
	TcpConnect();
	~TcpConnect();
	
	static void SetOnTcpConnectRcvData(FOnTcpConnectRcvData fn) {
		OnTcpConnectRcvData = fn;
	}
	void PushData(BYTE* ptr, int size);
	bool Send(BYTE * ptr, int size);
	void Disconnect();
};

NS_END