using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genshin__.lib
{
    internal class IniParser
    {
        private string filePath = null; // 用户将会传入的文件路径，与下一个二选一
        private FileStream fileStream = null; // 用户将会传入的文件数据流，与上一个二选一

        // INI文件数据项树
        internal IList<IniSection> iniData = new List<IniSection>();

        /// <summary>
        /// 根据文件路径解析INI文件
        /// </summary>
        /// <param name="path">文件路径</param>
        public IniParser(string path)
        {
            // 记录文件路径，创建流读写器
            filePath = path;
            StreamReader streamReader = new StreamReader(filePath);
            // 解析
            ParseIniFile(streamReader);
        }

        /// <summary>
        /// 根据文件数据流解析INI文件
        /// </summary>
        /// <param name="stream">文件数据流</param>
        public IniParser(FileStream stream)
        {
            // 记录文件数据流，创建流读写器
            fileStream = stream;
            StreamReader streamReader = new StreamReader(fileStream);
            // 解析
            ParseIniFile(streamReader);
        }

        /// <summary>
        /// 解析INI文件
        /// </summary>
        /// <param name="streamReader">文件流读取器</param>
        internal void ParseIniFile(StreamReader streamReader)
        {
            IniSection iniSection = null;
            IList<IniKey> iniKeys = new List<IniKey>();

            for (string line = streamReader.ReadLine(); line != null; line = streamReader.ReadLine())
            {
                line = line.Trim();
                if (line == string.Empty)
                    continue;
                if (line.StartsWith(";"))
                    continue;
                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    if (iniSection != null && iniKeys.Count != 0)
                    {
                        iniSection.keys = iniKeys;
                        iniData.Add(iniSection);
                    }
                    iniSection = new IniSection();
                    iniSection.section = line.Substring(1, line.Length - 2);
                    iniKeys.Clear();
                }
                else
                {
                    IniKey iniKey = new IniKey();
                    string[] keyPair = line.Split(new char[] { '=' }, 2);
                    iniKey.key = keyPair[0];
                    if (keyPair.Length > 1)
                        iniKey.value = keyPair[1];
                    iniKeys.Add(iniKey);
                }
            }
            if (iniSection != null && iniKeys.Count != 0)
            {
                iniSection.keys = iniKeys;
                iniData.Add(iniSection);
            }
            streamReader.Close();
            streamReader.Dispose();
        }

        /// <summary>
        /// 输出INI字符串
        /// </summary>
        /// <returns></returns>
        internal string GetIniString()
        {
            string iniString = string.Empty;
            foreach (IniSection section in iniData)
            {
                iniString += "[" + section.section + "]\n";
                foreach (IniKey key in section.keys)
                {
                    iniString += key.key + "=" + key.value + "\n";
                }
            }
            return iniString;
        }

        /// <summary>
        /// 将内容写入Ini文件
        /// </summary>
        /// <param name="path"></param>
        internal void SaveAsIniFile(string path)
        {
            File.WriteAllText(path, GetIniString(), Encoding.UTF8);
        }

        /// <summary>
        /// 列出所有Section的名称
        /// </summary>
        /// <returns></returns>
        internal string[] GetSectionNames()
        {
            IList<string> sections = new List<string>();
            foreach (IniSection section in iniData)
            {
                sections.Add(section.section);
            }
            return (string[])sections;
        }

        /// <summary>
        /// 计算共有多少Section
        /// </summary>
        /// <returns></returns>
        internal int GetSectionCounts()
        {
            int count = 0;
            foreach (IniSection section in iniData)
                count++;
            return count;
        }

        /// <summary>
        /// 获取指定Section下所有Key的名称
        /// </summary>
        /// <param name="sectionName">Section名称</param>
        /// <returns></returns>
        internal string[] GetKeyNames(string sectionName)
        {
            IList<string> keys = new List<string>();
            foreach (IniSection section in iniData) 
                if (section.section == sectionName)
                {
                    foreach (IniKey key in section.keys)
                        keys.Add(key.key);
                    return (string[])keys;
                }
            return null;
        }

        /// <summary>
        /// 获取指定Section下Key的数量
        /// </summary>
        /// <param name="sectionName">Section名称</param>
        /// <returns></returns>
        internal int GetKeyCounts(string sectionName)
        {
            int count = 0;
            foreach (IniSection section in iniData)
                if (section.section == sectionName)
                {
                    foreach (IniKey key in section.keys)
                        count++;
                    return count;
                }
            return -1;
        }

        /// <summary>
        /// 获取指定键值
        /// </summary>
        /// <param name="sectionName">Section名称</param>
        /// <param name="keyName">Key名称</param>
        /// <returns></returns>
        internal string GetKeyValue(string sectionName, string keyName)
        {
            foreach (IniSection section in iniData)
                if (section.section == sectionName)
                    foreach (IniKey key in section.keys)
                        if (key.key == keyName)
                            return key.value;
            return null;
        }

        /// <summary>
        /// 添加指定键值
        /// </summary>
        /// <param name="sectionName">Section名称</param>
        /// <param name="keyName">Key名称</param>
        /// <param name="keyValue">值</param>
        internal bool AddKeyValue(string sectionName, string keyName, string keyValue)
        {
            foreach (IniSection section in iniData)
                if (section.section == sectionName)
                {
                    IniKey iniKey = new IniKey();
                    iniKey.key = keyName;
                    iniKey.value = keyValue;
                    section.keys.Add(iniKey);
                    return true;
                }
            return false;
        }
        
        /// <summary>
        /// 移除指定节
        /// </summary>
        /// <param name="sectionName">Section名称</param>
        /// <returns></returns>
        internal bool RemoveSection(string sectionName)
        {
            foreach (IniSection section in iniData)
                if (section.section == sectionName)
                {
                    iniData.Remove(section);
                    return true;
                }
            return false;
        }

        /// <summary>
        /// 移除指定键值
        /// </summary>
        /// <param name="sectionName">Section名称</param>
        /// <param name="keyName">Key名称</param>
        /// <returns></returns>
        internal bool RemoveKeyValue(string sectionName, string keyName)
        {
            foreach (IniSection section in iniData)
                if (section.section == sectionName)
                    foreach (IniKey key in section.keys)
                        if (key.key == keyName)
                        {
                            section.keys.Remove(key);
                            return true;
                        }
            return false;
        }

        /// <summary>
        /// 设置指定键值
        /// </summary>
        /// <param name="sectionName">Section名称</param>
        /// <param name="keyName">Key名称</param>
        /// <param name="keyValue">值</param>
        /// <returns></returns>
        internal bool SetKeyValue(string sectionName, string keyName, string keyValue)
        {
            foreach (IniSection section in iniData)
                if (section.section == sectionName)
                    foreach (IniKey key in section.keys)
                        if (key.key == keyName)
                        {
                            key.value = keyValue;
                            return true;
                        }
            return false;
        }
    }

    internal class IniSection
    {
        internal string section { get; set; }
        internal IList<IniKey> keys { get; set; }
    }

    internal class IniKey
    {
        internal string key { get; set; }
        internal string value { get; set; }
    }
}
