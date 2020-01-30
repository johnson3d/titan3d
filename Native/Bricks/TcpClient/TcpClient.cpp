#include "TcpClient.h"
#include "../../CSharpAPI.h"

#define new VNEW

NS_BEGIN

RTTI_IMPL(EngineNS::TCPClient, EngineNS::VIUnknown);

using namespace std;

TCPClient::TCPClient()
{
	mSocket = -1;
	mPort = 0;
	mAddress = "";
}

TCPClient::~TCPClient()
{
	Disconnect();
}

vBOOL TCPClient::Connect(const char* address, int port, int timeout)
{
	mServerAddr.sin_family = AF_INET;
	mServerAddr.sin_port = htons(port);
	inet_pton(AF_INET, address, &mServerAddr.sin_addr);
	memset(&(mServerAddr.sin_zero), 0, 8);

	if ((mSocket = socket(AF_INET, SOCK_STREAM, 0)) == -1)
	{
		return FALSE;
	}
#if PLATFORM_WIN
	unsigned long ul = 1;
	if (ioctlsocket(mSocket, FIONBIO, (unsigned long*)&ul) == SOCKET_ERROR)
	{
		VFX_LTRACE(ELTT_Network, "ioctlsocket fail");
		return -1;
	}
	if (connect(mSocket, (struct sockaddr*) & mServerAddr, sizeof(struct sockaddr)) == SOCKET_ERROR)
	{
		fd_set r;
		FD_ZERO(&r);
		FD_SET(mSocket, &r);
		struct timeval timeo = { timeout/1000, 0 };
		auto ret = select(0, &r, 0, 0, &timeo);
		if (ret < 0) //需要注意select函数第一个参数在winsock被忽略了，//在linux必须是sock+1;
		{
			VFX_LTRACE(ELTT_Network, "connect fail");
			return FALSE;
		}
		else
		{
			VFX_LTRACE(ELTT_Network, "connect success");
			ul = 0;
			ioctlsocket(mSocket, FIONBIO, (unsigned long*)&ul);
			return TRUE;
		}
	}
	else
	{
		ul = 0;
		ioctlsocket(mSocket, FIONBIO, (unsigned long*)&ul);
		return TRUE;
	}
#else
	int retval;
	fcntl(mSocket, F_SETFL, fcntl(mSocket, F_GETFL) | O_NONBLOCK); //将fd设置为非阻塞
	/*auto ret = */connect(mSocket, (struct sockaddr*) & mServerAddr, sizeof(struct sockaddr));
	/*if (ret == 0)
	{
		return TRUE;
	}*/
	if (errno != EINPROGRESS)
	{
		return FALSE;
	}
	fd_set set;
	FD_ZERO(&set);
	FD_SET(mSocket, &set);
	timeval timeo = { timeout / 1000, 0 };
	retval = select(mSocket + 1, NULL, &set, NULL, &timeo);
	if (retval == -1) 
	{
		VFX_LTRACE(ELTT_Network, "select");
		return FALSE;
	}else if(retval == 0) {
		VFX_LTRACE(ELTT_Network, "timeoutn");
		return FALSE;
	}
	fcntl(mSocket, F_SETFL, fcntl(mSocket, F_GETFL) ^ O_NONBLOCK);
	return TRUE;
#endif
}

int TCPClient::Send(char* p, UINT size)
{
	return send(mSocket, p, size, 0);
}

void TCPClient::Disconnect()
{
#ifdef PLATFORM_WIN
	closesocket(mSocket);
#else
	close(mSocket);
#endif
}

vBOOL TCPClient::WaitData(int* errCode)
{
	fd_set readset;
	FD_ZERO(&readset);            //每次循环都要清空集合，否则不能检测描述符变化
	FD_SET(mSocket, &readset);     //添加描述符 
	int maxfd = (int)mSocket + 1;
	int check_timeval = 1;
	struct timeval timeout = { check_timeval,0 }; //阻塞式select, 等待1秒，1秒轮询

	*errCode = select(maxfd, &readset, NULL, NULL, &timeout);   // 非阻塞模式
	if (FD_ISSET(mSocket, &readset))  //测试sock是否可读，即是否网络上有数据
	{
		return TRUE;
	}
	timeout.tv_sec = check_timeval;    // 必须重新设置，因为超时时间到后会将其置零
	return FALSE;
}

int TCPClient::RecvData(void* pBuffer, UINT bufferSize)
{
	return recv(mSocket, (char*)pBuffer, (int)bufferSize, 0);
}

NS_END

using namespace EngineNS;

extern "C"
{
	CSharpReturnAPI3(vBOOL, EngineNS, TCPClient, Connect, const char*, int, int );
	CSharpAPI0(EngineNS, TCPClient, Disconnect);

	CSharpReturnAPI2(int, EngineNS, TCPClient, Send, char*, UINT);
	CSharpReturnAPI1(vBOOL, EngineNS, TCPClient, WaitData, int*);
	CSharpReturnAPI2(int, EngineNS, TCPClient, RecvData, void*, UINT);
}