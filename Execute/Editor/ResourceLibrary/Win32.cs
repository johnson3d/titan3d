using System;
using System.Runtime.InteropServices;

namespace ResourceLibrary
{
    public class Win32
    {
        #region WinMsg

        public const int WM_NULL = 0x0000; // 
        public const int WM_CREATE = 0x0001; //应用程序创建一个窗口 
        public const int WM_DESTROY = 0x0002; //一个窗口被销毁 
        public const int WM_MOVE = 0x0003; //移动一个窗口 
        public const int WM_SIZE = 0x0005; //改变一个窗口的大小 
        public const int WM_ACTIVATE = 0x0006; //一个窗口被激活或失去激活状态； 
        public const int WM_SETFOCUS = 0x0007; //获得焦点后 
        public const int WM_KILLFOCUS = 0x0008; //失去焦点 
        public const int WM_ENABLE = 0x000A; //改变enable状态 
        public const int WM_SETREDRAW = 0x000B; //设置窗口是否能重画 
        public const int WM_SETTEXT = 0x000C; //应用程序发送此消息来设置一个窗口的文本 
        public const int WM_GETTEXT = 0x000D; //应用程序发送此消息来复制对应窗口的文本到缓冲区 
        public const int WM_GETTEXTLENGTH = 0x000E; //得到与一个窗口有关的文本的长度（不包含空字符） 
        public const int WM_PAINT = 0x000F; //要求一个窗口重画自己 
        public const int WM_CLOSE = 0x0010; //当一个窗口或应用程序要关闭时发送一个信号 
        public const int WM_QUERYENDSESSION = 0x0011; //当用户选择结束对话框或程序自己调用ExitWindows函数 
        public const int WM_QUIT = 0x0012; //用来结束程序运行或当程序调用postquitmessage函数 
        public const int WM_QUERYOPEN = 0x0013; //当用户窗口恢复以前的大小位置时，把此消息发送给某个图标 
        public const int WM_ERASEBKGND = 0x0014; //当窗口背景必须被擦除时（例在窗口改变大小时） 
        public const int WM_SYSCOLORCHANGE = 0x0015; //当系统颜色改变时，发送此消息给所有顶级窗口 
        public const int WM_ENDSESSION = 0x0016; // 当系统进程发出WM_QUERYENDSESSION消息后，此消息发送给应用程序，通知它对话是否结束 
        public const int WM_SYSTEMERROR = 0x0017; // 
        public const int WM_SHOWWINDOW = 0x0018; //当隐藏或显示窗口是发送此消息给这个窗口 
        public const int WM_ACTIVATEAPP = 0x001C; //发此消息给应用程序哪个窗口是激活的，哪个是非激活的； 
        public const int WM_FONTCHANGE = 0x001D; //当系统的字体资源库变化时发送此消息给所有顶级窗口 
        public const int WM_TIMECHANGE = 0x001E; //当系统的时间变化时发送此消息给所有顶级窗口 
        public const int WM_CANCELMODE = 0x001F; //发送此消息来取消某种正在进行的摸态（操作） 
        public const int WM_SETCURSOR = 0x0020; //如果鼠标引起光标在某个窗口中移动且鼠标输入没有被捕获时，就发消息给某个窗口
        public const int WM_MOUSEACTIVATE = 0x0021; //当光标在某个非激活的窗口中而用户正按着鼠标的某个键发送此消息给当前窗口 
        public const int WM_CHILDACTIVATE = 0x0022; //发送此消息给MDI子窗口当用户点击此窗口的标题栏，或当窗口被激活，移动，改变大小 
        public const int WM_QUEUESYNC = 0x0023; //此消息由基于计算机的训练程序发送，通过WH_JOURNALPALYBACK的hook程序分离出用户输入消息 
        public const int WM_GETMINMAXINFO = 0x0024; //此消息发送给窗口当它将要改变大小或位置； 
        public const int WM_PAINTICON = 0x0026; //发送给最小化窗口当它图标将要被重画 
        public const int WM_ICONERASEBKGND = 0x0027; //此消息发送给某个最小化窗口，仅当它在画图标前它的背景必须被重画 
        public const int WM_NEXTDLGCTL = 0x0028; //发送此消息给一个对话框程序去更改焦点位置 
        public const int WM_SPOOLERSTATUS = 0x002A; //每当打印管理列队增加或减少一条作业时发出此消息 
        public const int WM_DRAWITEM = 0x002B; //当button，combobox，listbox，menu的可视外观改变时发送此消息给这些空件的所有者 
        public const int WM_MEASUREITEM = 0x002C; //当button, combo box, list box, list view control, or menu item 被创建时发送此消息给控件的所有者 
        public const int WM_DELETEITEM = 0x002D; // 当the list box 或combo box 被销毁或当某些项被删除通过LB_DELETESTRING, LB_RESETCONTENT, CB_DELETESTRING, or CB_RESETCONTENT 消息 
        public const int WM_VKEYTOITEM = 0x002E; //此消息有一个LBS_WANTKEYBOARDINPUT风格的发出给它的所有者来响应WM_KEYDOWN消息 
        public const int WM_CHARTOITEM = 0x002F; //此消息由一个LBS_WANTKEYBOARDINPUT风格的列表框发送给他的所有者来响应WM_CHAR消息 
        public const int WM_SETFONT = 0x0030; //当绘制文本时程序发送此消息得到控件要用的颜色 
        public const int WM_GETFONT = 0x0031; //应用程序发送此消息得到当前控件绘制文本的字体 
        public const int WM_SETHOTKEY = 0x0032; //应用程序发送此消息让一个窗口与一个热键相关连 
        public const int WM_GETHOTKEY = 0x0033; //应用程序发送此消息来判断热键与某个窗口是否有关联 
        public const int WM_QUERYDRAGICON = 0x0037; //此消息发送给最小化窗口，当此窗口将要被拖放而它的类中没有定义图标，应用程序能 
        //返回一个图标或光标的句柄，当用户拖放图标时系统显示这个图标或光标 
        public const int WM_COMPAREITEM = 0x0039; //发送此消息来判定combobox或listbox新增加的项的相对位置 
        public const int WM_GETOBJECT = 0x003D; //WM_COMPACTING = 0x0041; //显示内存已经很少了 
        public const int WM_WINDOWPOSCHANGING = 0x0046; //发送此消息给那个窗口的大小和位置将要被改变时，来调用setwindowpos函数或其它窗口管理函数 
        public const int WM_WINDOWPOSCHANGED = 0x0047; //发送此消息给那个窗口的大小和位置已经被改变时，来调用setwindowpos函数或其它窗口管理函数 
        public const int WM_POWER = 0x0048; //（适用于16位的windows）当系统将要进入暂停状态时发送此消息 
        public const int WM_COPYDATA = 0x004A; //当一个应用程序传递数据给另一个应用程序时发送此消息 
        public const int WM_CANCELJOURNAL = 0x004B; //当某个用户取消程序日志激活状态，提交此消息给程序 
        public const int WM_NOTIFY = 0x004E; //当某个控件的某个事件已经发生或这个控件需要得到一些信息时，发送此消息给它的父窗口 
        public const int WM_INPUTLANGCHANGEREQUEST = 0x0050; //当用户选择某种输入语言，或输入语言的热键改变 
        public const int WM_INPUTLANGCHANGE = 0x0051; //当平台现场已经被改变后发送此消息给受影响的最顶级窗口 
        public const int WM_TCARD = 0x0052; //当程序已经初始化windows帮助例程时发送此消息给应用程序 
        public const int WM_HELP = 0x0053; //此消息显示用户按下了F1，如果某个菜单是激活的，就发送此消息个此窗口关联的菜单，否则就 
        //发送给有焦点的窗口，如果当前都没有焦点，就把此消息发送给当前激活的窗口 
        public const int WM_USERCHANGED = 0x0054; //当用户已经登入或退出后发送此消息给所有的窗口，当用户登入或退出时系统更新用户的具体 
        //设置信息，在用户更新设置时系统马上发送此消息； 
        public const int WM_NOTIFYformAT = 0x0055; //公用控件，自定义控件和他们的父窗口通过此消息来判断控件是使用ANSI还是UNICODE结构 
        //在WM_NOTIFY消息，使用此控件能使某个控件与它的父控件之间进行相互通信 
        public const int WM_CONTEXTMENU = 0x007B; //当用户某个窗口中点击了一下右键就发送此消息给这个窗口 
        public const int WM_styleCHANGING = 0x007C; //当调用SETWINDOWLONG函数将要改变一个或多个窗口的风格时发送此消息给那个窗口 
        public const int WM_styleCHANGED = 0x007D; //当调用SETWINDOWLONG函数一个或多个窗口的风格后发送此消息给那个窗口 
        public const int WM_DISPLAYCHANGE = 0x007E; //当显示器的分辨率改变后发送此消息给所有的窗口 
        public const int WM_GETICON = 0x007F; //此消息发送给某个窗口来返回与某个窗口有关连的大图标或小图标的句柄； 
        public const int WM_SETICON = 0x0080; //程序发送此消息让一个新的大图标或小图标与某个窗口关联； 
        public const int WM_NCCREATE = 0x0081; //当某个窗口第一次被创建时，此消息在WM_CREATE消息发送前发送； 
        public const int WM_NCDESTROY = 0x0082; //此消息通知某个窗口，非客户区正在销毁 
        public const int WM_NCCALCSIZE = 0x0083; //当某个窗口的客户区域必须被核算时发送此消息 
        public const int WM_NCHITTEST = 0x0084; //移动鼠标，按住或释放鼠标时发生 
        public const int WM_NCPAINT = 0x0085; //程序发送此消息给某个窗口当它（窗口）的框架必须被绘制时； 
        public const int WM_NCACTIVATE = 0x0086; //此消息发送给某个窗口仅当它的非客户区需要被改变来显示是激活还是非激活状态； 
        public const int WM_GETDLGCODE = 0x0087; //发送此消息给某个与对话框程序关联的控件，widdows控制方位键和TAB键使输入进入此控件 
        //通过响应WM_GETDLGCODE消息，应用程序可以把他当成一个特殊的输入控件并能处理它 
        public const int WM_NCMOUSEMOVE = 0x00A0; //当光标在一个窗口的非客户区内移动时发送此消息给这个窗口//非客户区为：窗体的标题栏及窗的边框体 
        public const int WM_NCLBUTTONDOWN = 0x00A1; // 
        //当光标在一个窗口的非客户区同时按下鼠标左键时提交此消息 
        public const int WM_NCLBUTTONUP = 0x00A2; //当用户释放鼠标左键同时光标某个窗口在非客户区十发送此消息； 
        public const int WM_NCLBUTTONDBLCLK = 0x00A3; //当用户双击鼠标左键同时光标某个窗口在非客户区十发送此消息 
        public const int WM_NCRBUTTONDOWN = 0x00A4; //当用户按下鼠标右键同时光标又在窗口的非客户区时发送此消息 
        public const int WM_NCRBUTTONUP = 0x00A5; //当用户释放鼠标右键同时光标又在窗口的非客户区时发送此消息 
        public const int WM_NCRBUTTONDBLCLK = 0x00A6; //当用户双击鼠标右键同时光标某个窗口在非客户区十发送此消息 
        public const int WM_NCMBUTTONDOWN = 0x00A7; //当用户按下鼠标中键同时光标又在窗口的非客户区时发送此消息 
        public const int WM_NCMBUTTONUP = 0x00A8; //当用户释放鼠标中键同时光标又在窗口的非客户区时发送此消息 
        public const int WM_NCMBUTTONDBLCLK = 0x00A9; //当用户双击鼠标中键同时光标又在窗口的非客户区时发送此消息 
        public const int WM_KEYFIRST = 0x0100; // 
        public const int WM_KEYDOWN = 0x0100; //按下一个键 
        public const int WM_KEYUP = 0x0101; //释放一个键 
        public const int WM_CHAR = 0x0102; //按下某键，并已发出WM_KEYDOWN，WM_KEYUP消息 
        public const int WM_DEADCHAR = 0x0103; //当用translatemessage函数翻译WM_KEYUP消息时发送此消息给拥有焦点的窗口 
        public const int WM_SYSKEYDOWN = 0x0104; //当用户按住ALT键同时按下其它键时提交此消息给拥有焦点的窗口； 
        public const int WM_SYSKEYUP = 0x0105; //当用户释放一个键同时ALT 键还按着时提交此消息给拥有焦点的窗口 
        public const int WM_SYSCHAR = 0x0106; //当WM_SYSKEYDOWN消息被TRANSLATEMESSAGE函数翻译后提交此消息给拥有焦点的窗口 
        public const int WM_SYSDEADCHAR = 0x0107; //当WM_SYSKEYDOWN消息被TRANSLATEMESSAGE函数翻译后发送此消息给拥有焦点的窗口 
        public const int WM_KEYLAST = 0x0108; // 
        public const int WM_INITDIALOG = 0x0110; //在一个对话框程序被显示前发送此消息给它，通常用此消息初始化控件和执行其它任务 
        public const int WM_COMMAND = 0x0111; //当用户选择一条菜单命令项或当某个控件发送一条消息给它的父窗口，一个快捷键被翻译 
        public const int WM_SYSCOMMAND = 0x0112; //当用户选择窗口菜单的一条命令或当用户选择最大化或最小化时那个窗口会收到此消息 
        public const int WM_TIMER = 0x0113; //发生了定时器事件 
        public const int WM_HSCROLL = 0x0114; //当一个窗口标准水平滚动条产生一个滚动事件时发送此消息给那个窗口，也发送给拥有它的控件 
        public const int WM_VSCROLL = 0x0115; //当一个窗口标准垂直滚动条产生一个滚动事件时发送此消息给那个窗口也，发送给拥有它的控件WM_INITMENU = 0x0116; // 
        //当一个菜单将要被激活时发送此消息，它发生在用户菜单条中的某项或按下某个菜单键，它允许 
        //程序在显示前更改菜单 
        public const int WM_INITMENUPOPUP = 0x0117; //当一个下拉菜单或子菜单将要被激活时发送此消息，它允许程序在它显示前更改菜单，而不要 
        // 改变全部 
        public const int WM_MENUSELECT = 0x011F; //当用户选择一条菜单项时发送此消息给菜单的所有者（一般是窗口） 
        public const int WM_MENUCHAR = 0x0120; //当菜单已被激活用户按下了某个键（不同于加速键），发送此消息给菜单的所有者； 
        public const int WM_ENTERIDLE = 0x0121; //当一个模态对话框或菜单进入空载状态时发送此消息给它的所有者，一个模态对话框或菜单进入空载状态就是在处理完一条或几条先前的消息后没有消息它的列队中等待 
        public const int WM_MENURBUTTONUP = 0x0122; // 
        public const int WM_MENUDRAG = 0x0123; // 
        public const int WM_MENUGETOBJECT = 0x0124; // 
        public const int WM_UNINITMENUPOPUP = 0x0125; // 
        public const int WM_MENUCOMMAND = 0x0126; // 
        public const int WM_CHANGEUISTATE = 0x0127; // 
        public const int WM_UPDATEUISTATE = 0x0128; // 
        public const int WM_QUERYUISTATE = 0x0129; // 
        public const int WM_CTLCOLORMSGBOX = 0x0132; //在windows绘制消息框前发送此消息给消息框的所有者窗口，通过响应这条消息，所有者窗口可以 
        //通过使用给定的相关显示设备的句柄来设置消息框的文本和背景颜色 
        public const int WM_CTLCOLOREDIT = 0x0133; //当一个编辑型控件将要被绘制时发送此消息给它的父窗口；通过响应这条消息，所有者窗口可以 
        //通过使用给定的相关显示设备的句柄来设置编辑框的文本和背景颜色 
        public const int WM_CTLCOLORLISTBOX = 0x0134; //当一个列表框控件将要被绘制前发送此消息给它的父窗口；通过响应这条消息，所有者窗口可以 
        //通过使用给定的相关显示设备的句柄来设置列表框的文本和背景颜色 
        public const int WM_CTLCOLORBTN = 0x0135; //当一个按钮控件将要被绘制时发送此消息给它的父窗口；通过响应这条消息，所有者窗口可以 
        //通过使用给定的相关显示设备的句柄来设置按纽的文本和背景颜色 
        public const int WM_CTLCOLORDLG = 0x0136; //当一个对话框控件将要被绘制前发送此消息给它的父窗口；通过响应这条消息，所有者窗口可以 
        //通过使用给定的相关显示设备的句柄来设置对话框的文本背景颜色 
        public const int WM_CTLCOLORSCROLLBAR = 0x0137; //当一个滚动条控件将要被绘制时发送此消息给它的父窗口；通过响应这条消息，所有者窗口可以 
        //通过使用给定的相关显示设备的句柄来设置滚动条的背景颜色 
        public const int WM_CTLCOLORSTATIC = 0x0138; //当一个静态控件将要被绘制时发送此消息给它的父窗口；通过响应这条消息，所有者窗口可以 
        //通过使用给定的相关显示设备的句柄来设置静态控件的文本和背景颜色 
        public const int WM_MOUSEFIRST = 0x0200; // 
        public const int WM_MOUSEMOVE = 0x0200; //移动鼠标 
        public const int WM_LBUTTONDOWN = 0x0201; //按下鼠标左键 
        public const int WM_LBUTTONUP = 0x0202; //释放鼠标左键 
        public const int WM_LBUTTONDBLCLK = 0x0203; //双击鼠标左键 
        public const int WM_RBUTTONDOWN = 0x0204; //按下鼠标右键 
        public const int WM_RBUTTONUP = 0x0205; //释放鼠标右键 
        public const int WM_RBUTTONDBLCLK = 0x0206; //双击鼠标右键 
        public const int WM_MBUTTONDOWN = 0x0207; //按下鼠标中键 
        public const int WM_MBUTTONUP = 0x0208; //释放鼠标中键 
        public const int WM_MBUTTONDBLCLK = 0x0209; //双击鼠标中键 
        public const int WM_MOUSEWHEEL = 0x020A; //当鼠标轮子转动时发送此消息个当前有焦点的控件 
        public const int WM_MOUSELAST = 0x020A; // 
        public const int WM_PARENTNOTIFY = 0x0210; //当MDI子窗口被创建或被销毁，或用户按了一下鼠标键而光标在子窗口上时发送此消息给它的父窗口 
        public const int WM_ENTERMENULOOP = 0x0211; //发送此消息通知应用程序的主窗口that已经进入了菜单循环模式 
        public const int WM_EXITMENULOOP = 0x0212; //发送此消息通知应用程序的主窗口that已退出了菜单循环模式 
        public const int WM_NEXTMENU = 0x0213; // 
        public const int WM_SIZING = 532; //当用户正在调整窗口大小时发送此消息给窗口；通过此消息应用程序可以监视窗口大小和位置
        //也可以修改他们 
        public const int WM_CAPTURECHANGED = 533; //发送此消息给窗口当它失去捕获的鼠标时； 
        public const int WM_MOVING = 534; //当用户在移动窗口时发送此消息，通过此消息应用程序可以监视窗口大小和位置 
        //也可以修改他们； 
        public const int WM_POWERBROADCAST = 536; //此消息发送给应用程序来通知它有关电源管理事件； 
        public const int WM_DEVICECHANGE = 537; //当设备的硬件配置改变时发送此消息给应用程序或设备驱动程序 
        public const int WM_IME_STARTCOMPOSITION = 0x010D; // 
        public const int WM_IME_ENDCOMPOSITION = 0x010E; // 
        public const int WM_IME_COMPOSITION = 0x010F; // 
        public const int WM_IME_KEYLAST = 0x010F; // 
        public const int WM_IME_SETCONTEXT = 0x0281; // 
        public const int WM_IME_NOTIFY = 0x0282; // 
        public const int WM_IME_CONTROL = 0x0283; // 
        public const int WM_IME_COMPOSITIONFULL = 0x0284; // 
        public const int WM_IME_SELECT = 0x0285; // 
        public const int WM_IME_CHAR = 0x0286; // 
        public const int WM_IME_REQUEST = 0x0288; // 
        public const int WM_IME_KEYDOWN = 0x0290; // 
        public const int WM_IME_KEYUP = 0x0291; // 
        public const int WM_MDICREATE = 0x0220; //应用程序发送此消息给多文档的客户窗口来创建一个MDI 子窗口 
        public const int WM_MDIDESTROY = 0x0221; //应用程序发送此消息给多文档的客户窗口来关闭一个MDI 子窗口 
        public const int WM_MDIACTIVATE = 0x0222; //应用程序发送此消息给多文档的客户窗口通知客户窗口激活另一个MDI子窗口，当客户窗口收到 
        //此消息后，它发出WM_MDIACTIVE消息给MDI子窗口（未激活）激活它； 
        public const int WM_MDIRESTORE = 0x0223; //程序发送此消息给MDI客户窗口让子窗口从最大最小化恢复到原来大小 
        public const int WM_MDINEXT = 0x0224; //程序发送此消息给MDI客户窗口激活下一个或前一个窗口 
        public const int WM_MDIMAXIMIZE = 0x0225; //程序发送此消息给MDI客户窗口来最大化一个MDI子窗口； 
        public const int WM_MDITILE = 0x0226; //程序发送此消息给MDI客户窗口以平铺方式重新排列所有MDI子窗口 
        public const int WM_MDICASCADE = 0x0227; //程序发送此消息给MDI客户窗口以层叠方式重新排列所有MDI子窗口 
        public const int WM_MDIICONARRANGE = 0x0228; //程序发送此消息给MDI客户窗口重新排列所有最小化的MDI子窗口 
        public const int WM_MDIGETACTIVE = 0x0229; //程序发送此消息给MDI客户窗口来找到激活的子窗口的句柄 
        public const int WM_MDISETMENU = 0x0230; //程序发送此消息给MDI客户窗口用MDI菜单代替子窗口的菜单 
        public const int WM_ENTERSIZEMOVE = 0x0231; // 
        public const int WM_EXITSIZEMOVE = 0x0232; // 
        public const int WM_DROPFILES = 0x0233; // 
        public const int WM_MDIREFRESHMENU = 0x0234; // 
        public const int WM_MOUSEHOVER = 0x02A1; // 
        public const int WM_MOUSELEAVE = 0x02A3; // 
        public const int WM_CUT = 0x0300; //程序发送此消息给一个编辑框或combobox来删除当前选择的文本 
        public const int WM_COPY = 0x0301; //程序发送此消息给一个编辑框或combobox来复制当前选择的文本到剪贴板 
        public const int WM_PASTE = 0x0302; //程序发送此消息给editcontrol或combobox从剪贴板中得到数据 
        public const int WM_CLEAR = 0x0303; //程序发送此消息给editcontrol或combobox清除当前选择的内容； 
        public const int WM_UNDO = 0x0304; //程序发送此消息给editcontrol或combobox撤消最后一次操作 
        public const int WM_RENDERformAT = 0x0305; // 
        public const int WM_RENDERALLformATS = 0x0306; // 
        public const int WM_DESTROYCLIPBOARD = 0x0307; //当调用ENPTYCLIPBOARD函数时发送此消息给剪贴板的所有者 
        public const int WM_DRAWCLIPBOARD = 0x0308; //当剪贴板的内容变化时发送此消息给剪贴板观察链的第一个窗口；它允许用剪贴板观察窗口来 
        //显示剪贴板的新内容； 
        public const int WM_PAINTCLIPBOARD = 0x0309; //当剪贴板包含CF_OWNERDIPLAY格式的数据并且剪贴板观察窗口的客户区需要重画； 
        public const int WM_VSCROLLCLIPBOARD = 0x030A; // 
        public const int WM_SIZECLIPBOARD = 0x030B; //当剪贴板包含CF_OWNERDIPLAY格式的数据并且剪贴板观察窗口的客户区域的大小已经改变是此消息通过剪贴板观察窗口发送给剪贴板的所有者； 
        public const int WM_ASKCBformATNAME = 0x030C; //通过剪贴板观察窗口发送此消息给剪贴板的所有者来请求一个CF_OWNERDISPLAY格式的剪贴板的名字 
        public const int WM_CHANGECBCHAIN = 0x030D; //当一个窗口从剪贴板观察链中移去时发送此消息给剪贴板观察链的第一个窗口； 
        public const int WM_HSCROLLCLIPBOARD = 0x030E; // 
        //此消息通过一个剪贴板观察窗口发送给剪贴板的所有者；它发生在当剪贴板包含CFOWNERDISPALY格式的数据并且有个事件在剪贴板观察窗的水平滚动条上；所有者应滚动剪贴板图象并更新滚动条的值； 
        public const int WM_QUERYNEWPALETTE = 0x030F; //此消息发送给将要收到焦点的窗口，此消息能使窗口在收到焦点时同时有机会实现他的逻辑调色板 
        public const int WM_PALETTEISCHANGING = 0x0310; //当一个应用程序正要实现它的逻辑调色板时发此消息通知所有的应用程序 
        public const int WM_PALETTECHANGED = 0x0311; //此消息在一个拥有焦点的窗口实现它的逻辑调色板后发送此消息给所有顶级并重叠的窗口，以此 
        //来改变系统调色板 
        public const int WM_HOTKEY = 0x0312; //当用户按下由REGISTERHOTKEY函数注册的热键时提交此消息 
        public const int WM_PRINT = 791; //应用程序发送此消息仅当WINDOWS或其它应用程序发出一个请求要求绘制一个应用程序的一部分； 
        public const int WM_PRINTCLIENT = 792; // 
        public const int WM_HANDHELDFIRST = 856; // 
        public const int WM_HANDHELDLAST = 863; // 
        public const int WM_PENWINFIRST = 0x0380; // 
        public const int WM_PENWINLAST = 0x038F; // 
        public const int WM_COALESCE_FIRST = 0x0390; // 
        public const int WM_COALESCE_LAST = 0x039F; // 
        public const int WM_DDE_FIRST = 0x03E0; // 
        public const int WM_DDE_INITIATE = WM_DDE_FIRST + 0; //一个DDE客户程序提交此消息开始一个与服务器程序的会话来响应那个指定的程序和主题名； 
        public const int WM_DDE_TERMINATE = WM_DDE_FIRST + 1; //一个DDE应用程序（无论是客户还是服务器）提交此消息来终止一个会话； 
        public const int WM_DDE_ADVISE = WM_DDE_FIRST + 2; //一个DDE客户程序提交此消息给一个DDE服务程序来请求服务器每当数据项改变时更新它 
        public const int WM_DDE_UNADVISE = WM_DDE_FIRST + 3; //一个DDE客户程序通过此消息通知一个DDE服务程序不更新指定的项或一个特殊的剪贴板格式的项 
        public const int WM_DDE_ACK = WM_DDE_FIRST + 4; //此消息通知一个DDE（动态数据交换）程序已收到并正在处理WM_DDE_POKE, WM_DDE_EXECUTE, WM_DDE_DATA, WM_DDE_ADVISE, WM_DDE_UNADVISE, or WM_DDE_INITIAT消息 
        public const int WM_DDE_DATA = WM_DDE_FIRST + 5; //一个DDE服务程序提交此消息给DDE客户程序来传递个一数据项给客户或通知客户的一条可用数据项 
        public const int WM_DDE_REQUEST = WM_DDE_FIRST + 6; //一个DDE客户程序提交此消息给一个DDE服务程序来请求一个数据项的值； 
        public const int WM_DDE_POKE = WM_DDE_FIRST + 7; //一个DDE客户程序提交此消息给一个DDE服务程序，客户使用此消息来请求服务器接收一个未经同意的数据项；服务器通过答复WM_DDE_ACK消息提示是否它接收这个数据项； 
        public const int WM_DDE_EXECUTE = WM_DDE_FIRST + 8; //一个DDE客户程序提交此消息给一个DDE服务程序来发送一个字符串给服务器让它象串行命令一样被处理，服务器通过提交WM_DDE_ACK消息来作回应； 
        public const int WM_DDE_LAST = WM_DDE_FIRST + 8; // 
        public const int WM_APP = 0x8000; // 
        public const int WM_USER = 0x0400; //此消息能帮助应用程序自定义私有消息；

        #endregion

        /// <summary>
        /// 带有外边框和标题的windows的样式  
        /// </summary>
        public const int WS_CAPTION = 0X00C0000;
        /// <summary>  
        /// 系统菜单  
        /// </summary>  
        public const int WS_SYSMENU = 0x00080000;
        public const int WS_CHILD = 0x40000000;
        /// <summary>
        /// window 扩展样式 分层显示
        /// </summary>
        public const int WS_EX_LAYERED = 0x00080000;
        /// <summary>
        /// 带有alpha的样式 
        /// </summary>
        public const int LWA_ALPHA = 0x00000002;
        /// <summary>
        /// 颜色键
        /// </summary>
        public const int LWA_COLORKEY = 0x00000001;
        /// <summary>
        /// window的基本样式
        /// </summary>
        public const int GWL_STYLE = -16;
        /// <summary>
        /// window的扩展样式
        /// </summary>
        public const int GWL_EXSTYLE = -20;
        public const int GCL_STYLE = (-26);
        public const int CS_DROPSHADOW = 0x00020000;

        public const int SPI_SETDROPSHADOW = 0x1025;

        public enum HitTest : int
        {
            HTERROR = -2,
            HTTRANSPARENT = -1,
            HTNOWHERE = 0,
            HTCLIENT = 1,
            HTCAPTION = 2,
            HTSYSMENU = 3,
            HTGROWBOX = 4,
            HTSIZE = HTGROWBOX,
            HTMENU = 5,
            HTHSCROLL = 6,
            HTVSCROLL = 7,
            HTMINBUTTON = 8,
            HTMAXBUTTON = 9,
            HTLEFT = 10,
            HTRIGHT = 11,
            HTTOP = 12,
            HTTOPLEFT = 13,
            HTTOPRIGHT = 14,
            HTBOTTOM = 15,
            HTBOTTOMLEFT = 16,
            HTBOTTOMRIGHT = 17,
            HTBORDER = 18,
            HTREDUCE = HTMINBUTTON,
            HTZOOM = HTMAXBUTTON,
            HTSIZEFIRST = HTLEFT,
            HTSIZELAST = HTBOTTOMRIGHT,
            HTOBJECT = 19,
            HTCLOSE = 20,
            HTHELP = 21,
        }

        // Retrieves a handle to the display monitor that is nearest to the window
        public const int MONITOR_DEFAULTTONEAREST = 2;

        // Retrieves a handle to the display monitor
        [DllImport("user32.dll")]
        public static extern IntPtr MonitorFromWindow(IntPtr hwnd, int dwFlags);
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);

        // RECT structure, Rectangle used by MONITORINFOEX
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        // MONITORINFOEX structure, Monitor information used by GetMonitorInfo function
        [StructLayout(LayoutKind.Sequential)]
        public class MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor; // The display monitor rectangle
            public RECT rcWork; // The working area rectangle
            public int dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
            public char[] szDevice;
        }

        // Point structure, Point information used by MINMAXINFO structure
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
        }

        // MINMAXINFO structure, Window's maximum size and position information
        [StructLayout(LayoutKind.Sequential)]
        public struct MINMAXINFO
        {
            public POINT ptReserved;
            public POINT ptMaxSize; // The maximized size of the window
            public POINT ptMaxPosition; // The position of the maximized window
            public POINT ptMinTrackSize;
            public POINT ptMaxTrackSize;
        }

        // Get the working area of the specified monitor
        [DllImport("user32.dll")]
        public static extern bool GetMonitorInfo(HandleRef hmonitor, [In, Out] MONITORINFOEX monitorInfo);
        [DllImport("User32.dll ", EntryPoint = "SetParent")]
        public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);
        [DllImport("user32.dll ", EntryPoint = "ShowWindow")]
        public static extern int ShowWindow(IntPtr hwnd, int nCmdShow);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int SetClassLong(IntPtr hwnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int GetClassLong(IntPtr hwnd, int nIndex);
        /// <summary>
        /// 设置窗体的样式
        /// </summary>
        /// <param name="handle">操作窗体的句柄</param>
        /// <param name="oldStyle">进行设置窗体的样式类型</param>
        /// <param name="newStyle">新样式</param>
        [DllImport("user32.dll")]
        public static extern void SetWindowLong(IntPtr handle, int oldStyle, int newStyle);
        /// <summary>
        /// 获取窗体指定的样式
        /// </summary>
        /// <param name="handle">操作窗体的句柄</param>
        /// <param name="style">要进行返回的样式</param>
        /// <returns>当前window的样式</returns>
        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr handle, int style);
        /// <summary>
        /// 设置窗体的工作区域
        /// </summary>
        /// <param name="handle">操作窗体的句柄</param>
        /// <param name="handleRegion">操作窗体区域的句柄</param>
        /// <param name="regraw">if set to <c>true</c> [regraw]</param>
        /// <returns>返回值</returns>
        [DllImport("user32.dll")]
        public static extern int SetWindowRgn(IntPtr handle, IntPtr handleRegion, bool regraw);
        /// <summary>
        /// 创建带有圆角的区域
        /// </summary>
        /// <param name="x1">左上角坐标的X值</param>
        /// <param name="y1">左上角坐标的Y值</param>
        /// <param name="x2">右下角坐标的X值</param>
        /// <param name="y2">右下角坐标的Y值</param>
        /// <param name="width">圆角椭圆的 width</param>
        /// <param name="height">圆角椭圆的 height</param>
        /// <returns>hRgn的句柄</returns>
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRoundRectRgn(int x1, int y1, int x2, int y2, int width, int height);
        /// <summary>
        /// Sets the layered window attributes
        /// </summary>
        /// <param name="handle">要进行操作的窗口句柄</param>
        /// <param name="colorKey">RGB的值</param>
        /// <param name="alpha">Alpha的值，透明度</param>
        /// <param name="flags">附带参数</param>
        /// <returns>true or false</returns>
        [DllImport("user32.dll")]
        public static extern bool SetLayeredWindowAttributes(IntPtr handle, uint colorKey, byte alpha, int flags);

        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        [DllImport("kernel32.dll")]
        public static extern int GetLastError();

        // During drag-and-drop operations, the position of the mouse cannot be 
        // reliably determined through GetPosition. This is because control of 
        // the mouse (possibly including capture) is held by the originating 
        // element of the drag until the drop is completed, with much of the 
        // behavior controlled by underlying Win32 calls. As a workaround, you 
        // might need to use Win32 externals such as GetCursorPos.
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(ref POINT point);
        [DllImport("user32.dll")]
        public static extern int SetCursorPos(int x, int y);

        [DllImport("User32.dll")]
        static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        public static double GetPhysicalUnitScale()
        {
            IntPtr dDC = GetDC(IntPtr.Zero); // Get desktop DC
            int dpi = GetDeviceCaps(dDC, 88);
            ReleaseDC(IntPtr.Zero, dDC);
            return 96d / (double)dpi;
        }

        [DllImport("user32.dll")]
        static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);
        public static POINT GetCursorPosInWPF()
        {
            var point = new POINT();
            if (GetCursorPos(ref point))
            {
                // get the system DPI
                IntPtr dDC = GetDC(IntPtr.Zero); // Get desktop DC
                int dpi = GetDeviceCaps(dDC, 88);
                bool rv = ReleaseDC(IntPtr.Zero, dDC);

                // WPF's physical unit size is calculated by taking the 
                // "Device-Independant Unit Size" (always 1/96)
                // and scaling it by the system DPI
                double physicalUnitSize = (96d / (double)dpi);
                point.X = (int)(physicalUnitSize * (double)point.X);
                point.Y = (int)(physicalUnitSize * (double)point.Y);
            }
            return point;
        }

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
        public static int GetWindowZOrder(IntPtr hwnd)
        {
            const uint GW_HWNDPREV = 3;
            const uint GW_HWNDLAST = 1;

            var lowestHwnd = GetWindow(hwnd, GW_HWNDLAST);

            var z = 0;
            var hwndTmp = lowestHwnd;
            while (hwndTmp != IntPtr.Zero)
            {
                if (hwnd == hwndTmp)
                {
                    return z;
                }

                hwndTmp = GetWindow(hwndTmp, GW_HWNDPREV);
                z++;
            }

            return int.MinValue;
        }

        [DllImport("user32.dll")]
        public static extern bool BringWindowToTop(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(int x, int y);

        //[DllImport("User32.dll", CharSet = CharSet.Auto)]
        //public static extern bool PeekMessage(out Message msg, IntPtr hWnd, uint messageFilterMin, uint messageFilterMax, uint flags);
        [DllImport("user32.dll", EntryPoint = "SendMessageA")]
        private static extern int SendMessage(IntPtr hwnd, int wMsg, IntPtr wParam, string lParam);
        [DllImport("user32.dll")]
        public static extern IntPtr PostMessage(IntPtr hwnd, int wMsg, int wParam, int lParam);
    }
}
