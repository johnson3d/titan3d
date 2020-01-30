#include "Core_IOS.h"
#import <Foundation/Foundation.h>
//安装并配置使用 iOS 进行构建的工具
//https://docs.microsoft.com/zh-cn/visualstudio/cross-platform/install-and-configure-tools-to-build-using-ios?view=vs-2017

int TestOC()
{
	NSArray *cachePaths = NSSearchPathForDirectoriesInDomains(NSCachesDirectory, NSUserDomainMask, YES);

    NSString *cacheDir = [cachePaths objectAtIndex:0];

	return 0;
}