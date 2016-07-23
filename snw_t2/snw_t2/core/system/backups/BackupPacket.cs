namespace snw.core.system.backups
{
    public class BackupPacket
    {
        public BackupPacket() { }
        public BackupPacket(string name) { this.Name = name; }
        public BackupPacket(string name, byte[] data) { this.Name = name; this.Data = data; }
        public string Name { set; get; }
        public byte[] Data { set; get; }
    }
}