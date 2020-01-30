using System;
using System.Windows;

namespace EditorCommon
{
    public class MessageBox
    {
        /// <summary>
        /// 消息框显示的按钮类型
        /// </summary>
        public enum enMessageBoxButton
        {
            OK = 0,
            OKCancel = 1,
            //AbortRetryIgnore = 2,
            YesNoCancel = 3,
            YesNo = 4,
            //RetryCancel = 5,
            Yes_YesAll_No_NoAll = 6,
            Yes_YesAll_No_NoAll_Cancel = 7,
        }

        /// <summary>
        /// 消息框的返回值
        /// </summary>
        public enum enMessageBoxResult
        {
            None = 0,
            OK = 1,
            Cancel = 2,
            Abort = 3,
            Retry = 4,
            Ignore = 5,
            Yes = 6,
            No = 7,
            YesAll = 8,
            NoAll = 9,
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="messageString">消息内容</param>
        public static enMessageBoxResult Show(string messageString, Window owner = null)
        {
            MessageBoxWindow window = null;
            enMessageBoxResult retResult = enMessageBoxResult.None;
            if(EditorCommon.Program.MainDispatcher != null)
            {
                EditorCommon.Program.MainDispatcher.Invoke(new Action(() =>
                {
                    window = new MessageBoxWindow();
                    window.MessageString = messageString;
                    window.OKButtonVisibility = Visibility.Visible;
                    window.Owner = owner;
                    window.ShowDialog();
                    retResult = window.Result;
                }));
                return retResult;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(messageString);
                return enMessageBoxResult.OK;
            }
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="messageString">消息内容</param>
        /// <param name="caption">消息标题</param>
        public static enMessageBoxResult Show(string messageString, string caption, Window owner = null)
        {
            MessageBoxWindow window = null;
            enMessageBoxResult retResult = enMessageBoxResult.None;
            if(EditorCommon.Program.MainDispatcher != null)
            {
                EditorCommon.Program.MainDispatcher.Invoke(new Action(() =>
                {
                    window = new MessageBoxWindow();
                    window.MessageString = messageString;
                    window.Title = caption;
                    window.OKButtonVisibility = Visibility.Visible;
                    window.Owner = owner;
                    window.ShowDialog();
                    retResult = window.Result;
                }));
                return retResult;
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(messageString, caption);
                return enMessageBoxResult.OK;
            }
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="messageString">消息内容</param>
        /// <param name="buttons">消息框按钮</param>
        public static enMessageBoxResult Show(string messageString, enMessageBoxButton buttons, Window owner = null)
        {
            MessageBoxWindow window = null;
            enMessageBoxResult retResult = enMessageBoxResult.None;
            if(EditorCommon.Program.MainDispatcher != null)
            {
                EditorCommon.Program.MainDispatcher.Invoke(new Action(() =>
                {
                    window = new MessageBoxWindow();
                    window.MessageString = messageString;
                    window.Owner = owner;
                    switch (buttons)
                    {
                        case enMessageBoxButton.OK:
                            window.OKButtonVisibility = Visibility.Visible;
                            break;
                        case enMessageBoxButton.OKCancel:
                            window.OKButtonVisibility = Visibility.Visible;
                            window.CancelButtonVisibility = Visibility.Visible;
                            break;
                        case enMessageBoxButton.YesNo:
                            window.YesButtonVisibility = Visibility.Visible;
                            window.NoButtonVisibility = Visibility.Visible;
                            break;
                        case enMessageBoxButton.YesNoCancel:
                            window.YesButtonVisibility = Visibility.Visible;
                            window.NoButtonVisibility = Visibility.Visible;
                            window.CancelButtonVisibility = Visibility.Visible;
                            break;
                        case enMessageBoxButton.Yes_YesAll_No_NoAll:
                            window.YesButtonVisibility = Visibility.Visible;
                            window.YesAllButtonVisibility = Visibility.Visible;
                            window.NoButtonVisibility = Visibility.Visible;
                            window.NoAllButtonVisibility = Visibility.Visible;
                            break;
                        case enMessageBoxButton.Yes_YesAll_No_NoAll_Cancel:
                            window.YesButtonVisibility = Visibility.Visible;
                            window.YesAllButtonVisibility = Visibility.Visible;
                            window.NoButtonVisibility = Visibility.Visible;
                            window.NoAllButtonVisibility = Visibility.Visible;
                            window.CancelButtonVisibility = Visibility.Visible;
                            break;
                        default:
                            window.OKButtonVisibility = Visibility.Visible;
                            break;
                    }
                    window.ShowDialog();
                    retResult = window.Result;
                }));
                return retResult;
            }
            else
            {
                var result = System.Windows.Forms.MessageBox.Show(messageString, "", (System.Windows.Forms.MessageBoxButtons)buttons);
                return (enMessageBoxResult)result;
            }
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="messageString">消息内容</param>
        /// <param name="caption">消息标题</param>
        /// <param name="buttons">消息框按钮</param>
        public static enMessageBoxResult Show(string messageString, string caption, enMessageBoxButton buttons, Window owner = null)
        {
            MessageBoxWindow window = null;
            enMessageBoxResult retResult = enMessageBoxResult.None;
            if(EditorCommon.Program.MainDispatcher != null)
            {
                EditorCommon.Program.MainDispatcher.Invoke(new Action(() =>
                {
                    window = new MessageBoxWindow();
                    window.MessageString = messageString;
                    window.Title = caption;
                    window.Owner = owner;
                    switch (buttons)
                    {
                        case enMessageBoxButton.OK:
                            window.OKButtonVisibility = Visibility.Visible;
                            break;
                        case enMessageBoxButton.OKCancel:
                            window.OKButtonVisibility = Visibility.Visible;
                            window.CancelButtonVisibility = Visibility.Visible;
                            break;
                        case enMessageBoxButton.YesNo:
                            window.YesButtonVisibility = Visibility.Visible;
                            window.NoButtonVisibility = Visibility.Visible;
                            break;
                        case enMessageBoxButton.YesNoCancel:
                            window.YesButtonVisibility = Visibility.Visible;
                            window.NoButtonVisibility = Visibility.Visible;
                            window.CancelButtonVisibility = Visibility.Visible;
                            break;
                        case enMessageBoxButton.Yes_YesAll_No_NoAll:
                            window.YesButtonVisibility = Visibility.Visible;
                            window.YesAllButtonVisibility = Visibility.Visible;
                            window.NoButtonVisibility = Visibility.Visible;
                            window.NoAllButtonVisibility = Visibility.Visible;
                            break;
                        case enMessageBoxButton.Yes_YesAll_No_NoAll_Cancel:
                            window.YesButtonVisibility = Visibility.Visible;
                            window.YesAllButtonVisibility = Visibility.Visible;
                            window.NoButtonVisibility = Visibility.Visible;
                            window.NoAllButtonVisibility = Visibility.Visible;
                            window.CancelButtonVisibility = Visibility.Visible;
                            break;
                        default:
                            window.OKButtonVisibility = Visibility.Visible;
                            break;
                    }
                    window.ShowDialog();
                    retResult = window.Result;
                }));
                return retResult;
            }
            else
            {
                var result = System.Windows.Forms.MessageBox.Show(messageString, caption, (System.Windows.Forms.MessageBoxButtons)buttons);
                return (enMessageBoxResult)result;
            }
        }
    }
}
