#pragma once

#include "../../BaseHead.h"
#include "../../Core/debug/vfxdebug.h"

NS_BEGIN

#ifdef PLATFORM_WIN
#include <WS2tcpip.h>
#pragma comment(lib,"Ws2_32.lib")
#else
#include <sys/socket.h>
typedef int SOCKET;
#include <netinet/in.h>
#include<arpa/inet.h>
#endif

class TCPClient : public VIUnknown
{
private:
	SOCKET			mSocket;
	std::string		mAddress;
	int				mPort;
	sockaddr_in		mServerAddr;
public:
	RTTI_DEF(TCPClient, 0x22ba61be5cdce42c, true);
	TCPClient();
	~TCPClient();
	vBOOL Connect(const char* address, int port, int timeout);
	void Disconnect();

	int Send(char* p, UINT size);

	vBOOL WaitData(int* errCode);
	int RecvData(void* pBuffer, UINT bufferSize);

	void ReceiveLoop();
	//std::string receive(int size = 4096);
	//std::string read();
	//void exit();
};

NS_END