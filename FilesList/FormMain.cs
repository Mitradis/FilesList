using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace FilesList
{
    public partial class FormMain : Form
    {
        List<string> filesList = new List<string>();
        int pathLength = 0;

        public FormMain()
        {
            InitializeComponent();
        }

        void button1_Click(object sender, EventArgs e)
        {
            button1.Text = "Работает";
            button1.Enabled = false;
            checkBox1.Enabled = false;
            checkBox2.Enabled = false;
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                pathLength = pathAddSlash(folderBrowserDialog1.SelectedPath).Length;
                searchFolder(folderBrowserDialog1.SelectedPath);
                DirectoryInfo info = new DirectoryInfo(folderBrowserDialog1.SelectedPath);
                string file = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), info.Parent != null ? info.Name + ".txt" : info.Name.Remove(1) + ".txt");
                try
                {
                    File.WriteAllLines(file, filesList);
                }
                catch
                {
                    MessageBox.Show("Не удалось записать файл: " + file);
                }
                filesList.Clear();
            }
            checkBox2.Enabled = true;
            checkBox1.Enabled = true;
            button1.Enabled = true;
            button1.Text = "Путь";
        }

        void searchFolder(string path)
        {
            getFilesList(path);
            foreach (string line in getDirectories(path))
            {
                if (Directory.Exists(line) && (new DirectoryInfo(line).Attributes & FileAttributes.System) != FileAttributes.System)
                {
                    searchFolder(line);
                }
            }
        }

        void getFilesList(string path)
        {
            foreach (string file in getFiles(path))
            {
                if (!File.GetAttributes(file).HasFlag(FileAttributes.System))
                {
                    try
                    {
                        filesList.Add(file.Remove(0, pathLength) + (checkBox2.Checked ? "\t" + getCRC(file) : "") + (checkBox1.Checked ? "\t" + new FileInfo(file).Length + "\t" + new FileInfo(file).LastWriteTime : ""));
                    }
                    catch
                    {
                        filesList.Add(file.Remove(0, pathLength) + "\tNO ACCESS TO FILE");
                    }
                }
            }
        }

        string[] getFiles(string path)
        {
            try
            {
                return Directory.GetFiles(path);
            }
            catch
            {
                return new string[] { };
            }
        }

        string[] getDirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch
            {
                return new string[] { };
            }
        }

        string pathAddSlash(string path)
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

        string getCRC(string file)
        {
            string line = "";
            FileStream fs = File.OpenRead(file);
            line = String.Format("{0:X}", calculateCRC(fs));
            fs.Close();
            while (line.Length < 8)
            {
                line = "0" + line;
            }
            return line;
        }

        uint calculateCRC(Stream stream)
        {
            const int buffer_size = 1024;
            const uint POLYNOMIAL = 0xEDB88320;
            uint result = 0xFFFFFFFF;
            uint Crc32;
            byte[] buffer = new byte[buffer_size];
            uint[] table = new uint[256];
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
                    table[i] = Crc32;
                }
                int count = stream.Read(buffer, 0, buffer_size);
                while (count > 0)
                {
                    for (int i = 0; i < count; i++)
                    {
                        result = ((result) >> 8) ^ table[(buffer[i]) ^ ((result) & 0x000000FF)];
                    }
                    count = stream.Read(buffer, 0, buffer_size);
                }
            }
            buffer = null;
            table = null;
            stream.Close();
            return ~result;
        }
    }
}
