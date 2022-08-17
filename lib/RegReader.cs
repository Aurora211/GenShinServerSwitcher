using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Genshin__.lib
{
    internal class RegReader
    {
        /// <summary>
        /// 判断键是否存在
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="root">检索根</param>
        /// <returns></returns>
        internal static bool isSubKeyExists(string key, RegistryKey root)
        {
            string[] subKeyNames = root.GetSubKeyNames();
            foreach (string subKey in subKeyNames)
            {
                if (subKey == key)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取原神安装路径
        /// </summary>
        /// <returns></returns>
        internal static string getGenShinInstallPath()
        {
            // 设置根注册表项目
            RegistryKey regKey = Registry.LocalMachine;
            try
            {
                // 进入已安装程序注册表列表
                regKey = regKey.OpenSubKey("SOFTWARE");
                regKey = regKey.OpenSubKey("Microsoft");
                regKey = regKey.OpenSubKey("Windows");
                regKey = regKey.OpenSubKey("CurrentVersion");
                regKey = regKey.OpenSubKey("Uninstall");
            }
            catch (Exception ex)
            {
                // 注册表结构异常！
                return ex.Message;
            }

            // 打开原神安装注册表
            // 检索国内服注册表信息
            if (isSubKeyExists("原神", regKey))
                regKey = regKey.OpenSubKey("原神");
            // 检索国际服注册表信息
            //else if (isSubKeyExists("Genshin Impact", regKey))
            //    regKey = regKey.OpenSubKey("Genshin Impact");
            else
                return "未获取到原神安装信息";

            // 获取启动器安装路径信息
            try
            {
                string path = regKey.GetValue("InstallPath").ToString();
                return path;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
