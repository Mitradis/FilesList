using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FilesList
{
    public partial class FormMain : Form
    {
        List<string> filesList = new List<string>();
        int pathLeight = 0;

        public FormMain()
        {
            InitializeComponent();
        }

        private void searthFolder(string path)
        {
            foreach (string line in Directory.GetDirectories(path))
            {
                if (!line.Contains("$RECYCLE.BIN") && !line.Contains("System Volume Information"))
                {
                    searthFolder(line);
                    getFilesList(line);
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
                        filesList.Add(line.Remove(0, pathLeight) + "\t" + getCRC(line) + "\t" + new FileInfo(line).Length + "\t" + new FileInfo(line).LastWriteTime);
                    }
                    else
                    {
                        filesList.Add(line.Remove(0, pathLeight) + "\t" + new FileInfo(line).Length + "\t" + new FileInfo(line).LastWriteTime);
                    }
                }
                else
                {
                    if (checkBox2.Checked)
                    {
                        filesList.Add(line.Remove(0, pathLeight) + "\t" + getCRC(line));
                    }
                    else
                    {
                        filesList.Add(line.Remove(0, pathLeight));
                    }
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            filesList.Clear();
            folderBrowserDialog1.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (Directory.Exists(folderBrowserDialog1.SelectedPath))
                {
                    if (folderBrowserDialog1.SelectedPath.EndsWith(@"\"))
                    {
                        pathLeight = folderBrowserDialog1.SelectedPath.Length;
                    }
                    else
                    {
                        pathLeight = folderBrowserDialog1.SelectedPath.Length + 1;
                    }
                    getFilesList(folderBrowserDialog1.SelectedPath);
                    searthFolder(folderBrowserDialog1.SelectedPath);
                    File.WriteAllLines(Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + @"\_list.txt", filesList);
                }
            }
        }

        private string getCRC(string file)
        {
            FileStream streamFile = File.OpenRead(file);
            return string.Format("{0:X}", calculateCRC(streamFile));
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
    }
}
