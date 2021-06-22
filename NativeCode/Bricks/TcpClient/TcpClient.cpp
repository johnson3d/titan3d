#include "TcpClient.h"

#define new VNEW

NS_BEGIN

using namespace std;

TcpClient::TcpClient()
{
	mSocket = -1;
	mPort = 0;
	mAddress = "";
}

TcpClient::~TcpClient()
{
	Disconnect();
}

vBOOL TcpClient::Connect(const char* address, int port, int timeout)
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
		if (ret < 0)
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
	fcntl(mSocket, F_SETFL, fcntl(mSocket, F_GETFL) | O_NONBLOCK);
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

int TcpClient::Send(char* p, UINT size)
{
	return send(mSocket, p, size, 0);
}

void TcpClient::Disconnect()
{
#ifdef PLATFORM_WIN
	closesocket(mSocket);
#else
	close(mSocket);
#endif
}

vBOOL TcpClient::WaitData(int* errCode)
{
	fd_set readset;
	FD_ZERO(&readset);
	FD_SET(mSocket, &readset);
	int maxfd = (int)mSocket + 1;
	int check_timeval = 1;
	struct timeval timeout = { check_timeval,0 }; 

	*errCode = select(maxfd, &readset, NULL, NULL, &timeout);
	if (FD_ISSET(mSocket, &readset))
	{
		return TRUE;
	}
	timeout.tv_sec = check_timeval;
	return FALSE;
}

int TcpClient::RecvData(void* pBuffer, UINT bufferSize)
{
	return recv(mSocket, (char*)pBuffer, (int)bufferSize, 0);
}

NS_END
