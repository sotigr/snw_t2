using System.Collections.ObjectModel;
namespace snw.core.system.backups
{
    public class BackupManager : Collection<BackupPacket>
    {
        public BackupManager(string path)
        {
            SavePath = path;
        }
        public string SavePath { set; get; }
        public bool Save()
        {
            foreach (BackupPacket packet in this)
            {
                if (SavePath.EndsWith("\\"))
                {
                    if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(SavePath + packet.Name)))
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(SavePath + packet.Name));
                    System.IO.File.WriteAllBytes(SavePath + packet.Name, packet.Data);
                }
                else
                {
                    if (!System.IO.Directory.Exists(System.IO.Path.GetDirectoryName(SavePath + "\\" + packet.Name)))
                        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(SavePath + "\\" + packet.Name));
                    System.IO.File.WriteAllBytes(SavePath + "\\" + packet.Name, packet.Data);
                }
            }
            return true;
        }
    }
}