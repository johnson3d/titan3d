#pragma once

#include "../../Base/IUnknown.h"

#ifdef PLATFORM_WIN
#include <WS2tcpip.h>
#pragma comment(lib,"Ws2_32.lib")
#else
#include <sys/socket.h>
typedef int SOCKET;
#include <netinet/in.h>
#include<arpa/inet.h>
#endif

NS_BEGIN

class TR_CLASS()
TcpClient : public VIUnknown
{
private:
	SOCKET			mSocket;
	std::string		mAddress;
	int				mPort;
	sockaddr_in		mServerAddr;
public:
	TcpClient();
	~TcpClient();
	vBOOL Connect(const char* address, int port, int timeout);
	void Disconnect();

	int Send(TR_META(SV_NoStringConverter) char* p, UINT size);

	vBOOL WaitData(int* errCode);
	int RecvData(void* pBuffer, UINT bufferSize);
};

NS_END