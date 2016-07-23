using System; 
using System.Collections.ObjectModel;
namespace snw.core.system.backups
{ 
    public class BackupScheduler : Collection<BackupManager>
    {
        private byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        public void AddBackupPath(string path)
        {
            this.Add(new BackupManager(path));
        }
        public void Write(string name, string data)
        {
            BackupPacket packet = new BackupPacket(name, GetBytes(data));
            foreach (BackupManager mngr in this)
            {
                mngr.Add(packet);
            }
        }
        public void Write(string name, byte[] data)
        {
            BackupPacket packet = new BackupPacket(name, data);
            foreach (BackupManager mngr in this)
            {
                mngr.Add(packet);
            }
        }
        public void Write(string name, string data, bool autosave)
        {
            BackupPacket packet = new BackupPacket(name, GetBytes(data));
            foreach (BackupManager mngr in this)
            {
                mngr.Add(packet);
            }
            if (autosave)
            {
                this.Save();
            }
        }
        public void Write(string name, byte[] data, bool autosave)
        {
            BackupPacket packet = new BackupPacket(name, data);
            foreach (BackupManager mngr in this)
            {
                mngr.Add(packet);
            }
            if (autosave)
            {
                this.Save();
            }
        }
        public void Save()
        {
            foreach (BackupManager mngr in this)
            {
                mngr.Save();
                mngr.Clear();
            }
        }
        public string ReadString(string name)
        {
            try
            {
                string res = "";
                using (var reader = new System.IO.StreamReader(this[0].SavePath + "\\" + name, System.Text.Encoding.Unicode))
                {
                    res = reader.ReadToEnd();
                }
                return res;
            }
            catch (Exception ex) { return ""; }

        }
        public byte[] ReadBytes(string name)
        {
            return System.IO.File.ReadAllBytes(this[0].SavePath + "\\" + name);
        }
        public bool FileExists(string name)
        {
            return System.IO.File.Exists(this[0].SavePath + "\\" + name);
        }
        public bool DeleteFile(string name)
        {
            try
            {
                foreach (BackupManager mngr in this)
                {
                    if (System.IO.File.Exists(mngr.SavePath + "\\" + name))
                    {
                        System.IO.File.Delete(mngr.SavePath + "\\" + name);
                    }
                }
                return true;
            }
            catch { return false; }
        }
    }
}