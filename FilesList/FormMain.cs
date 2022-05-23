using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FilesList
{
    public partial class FormMain : Form
    {
        List<string> filesList = new List<string>();
        string logOut = pathAddSlash(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)) + "_list.txt";
        string lastFolder = null;
        int pathLength = 0;

        public FormMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (lastFolder != null)
            {
                if (Directory.Exists(lastFolder))
                {
                    folderBrowserDialog1.SelectedPath = lastFolder;
                }
            }
            else
            {
                folderBrowserDialog1.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            }
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (Directory.Exists(folderBrowserDialog1.SelectedPath))
                {
                    lastFolder = folderBrowserDialog1.SelectedPath;
                    if (folderBrowserDialog1.SelectedPath.EndsWith(@"\") || folderBrowserDialog1.SelectedPath.EndsWith("/"))
                    {
                        pathLength = folderBrowserDialog1.SelectedPath.Length;
                    }
                    else
                    {
                        pathLength = folderBrowserDialog1.SelectedPath.Length + 1;
                    }
                    searchFolder(folderBrowserDialog1.SelectedPath);
                    try
                    {
                        File.WriteAllLines(logOut, filesList);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Не удалось записать файл: " + logOut + Environment.NewLine + ex.Message);
                    }
                }
            }
            filesList.Clear();
        }

        private void searchFolder(string path)
        {
            getFilesList(path);
            foreach (string line in Directory.GetDirectories(path))
            {
                if (!new DirectoryInfo(line).Attributes.HasFlag(FileAttributes.System))
                {
                    searchFolder(line);
                }
            }
        }

        private void getFilesList(string path)
        {
            foreach (string line in Directory.GetFiles(path))
            {
                if (checkBox1.Checked)
                {
                    if (checkBox2.Checked)
                    {
                        filesList.Add(line.Remove(0, pathLength) + "\t" + getCRC(line) + "\t" + new FileInfo(line).Length + "\t" + new FileInfo(line).LastWriteTime);
                    }
                    else
                    {
                        filesList.Add(line.Remove(0, pathLength) + "\t" + new FileInfo(line).Length + "\t" + new FileInfo(line).LastWriteTime);
                    }
                }
                else
                {
                    if (checkBox2.Checked)
                    {
                        filesList.Add(line.Remove(0, pathLength) + "\t" + getCRC(line));
                    }
                    else
                    {
                        filesList.Add(line.Remove(0, pathLength));
                    }
                }
            }
        }

        private string getCRC(string file)
        {
            FileStream streamFile = File.OpenRead(file);
            string line = string.Format("{0:X}", calculateCRC(streamFile));
            streamFile.Close();
            while (line.Length != 8)
            {
                line = "0" + line;
            }
            return line;
        }

        private static uint calculateCRC(Stream stream)
        {
            const int buffer_size = 1024;
            const uint POLYNOMIAL = 0xEDB88320;
            uint result = 0xFFFFFFFF;
            uint Crc32;
            byte[] buffer = new byte[buffer_size];
            uint[] table_CRC32 = new uint[256];
            unchecked
            {
                for (int i = 0; i < 256; i++)
                {
                    Crc32 = (uint)i;
                    for (int j = 8; j > 0; j--)
                    {
                        if ((Crc32 & 1) == 1)
                        {
                            Crc32 = (Crc32 >> 1) ^ POLYNOMIAL;
                        }
                        else
                        {
                            Crc32 >>= 1;
                        }
                    }
                    table_CRC32[i] = Crc32;
                }
                int count = stream.Read(buffer, 0, buffer_size);
                while (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        result = ((result) >> 8) ^ table_CRC32[(buffer[i]) ^ ((result) & 0x000000FF)];
                    }
                    count = stream.Read(buffer, 0, buffer_size);
                }
            }
            stream.Close();
            return ~result;
        }

        private static string pathAddSlash(string path)
        {
            if (!path.EndsWith("/") && !path.EndsWith(@"\"))
            {
                if (path.Contains("/"))
                {
                    path += "/";
                }
                else if (path.Contains(@"\"))
                {
                    path += @"\";
                }
            }
            return path;
        }
    }
}
