namespace HexaEngine.Editor.Projects
{
    using System.Collections.Specialized;
    using System.IO;
    using System.Xml.Serialization;

    public class HexaDirectory : HexaParent
    {
        private bool isSelected;

        public HexaDirectory()
        {
        }

        public HexaDirectory(string name, HexaParent parent)
        {
            Name = name;
            Parent = parent;
            Directory.CreateDirectory(GetAbsolutePath());
        }

        [XmlIgnore]
        public override IntPtr Icon => IntPtr.Zero;

        public override event NotifyCollectionChangedEventHandler? CollectionChanged;

        public override void Import(string path)
        {
            string filename = Path.GetFileName(path);
            string newPath = GetAbsolutePath(filename);
            File.Copy(path, newPath, true);
            HexaFile file = new(filename, this);
            Items.Add(file);
        }

        public HexaDirectory CreateFolder(string name)
        {
            HexaDirectory directory = new(name, this);
            Items.Add(directory);
            return directory;
        }

        public override void Add(HexaItem item)
        {
            Items.Add(item);
            Save();
        }

        public override void Remove(HexaItem item)
        {
            Items.Remove(item);
            Save();
        }

        public override void Move(HexaItem item)
        {
            if (item.Parent == null) return;
            string oldPath = item.GetAbsolutePath();
            string newPath = GetAbsolutePath(item.Name);
            File.Move(oldPath, newPath, true);
            item.Parent.Items.Remove(item);
            item.Parent = this;
            Items.Add(item);
            Save();
        }

        public override void Delete()
        {
            Parent.Items.Remove(this);
            Save();
            Directory.Delete(GetAbsolutePath(), true);
        }

        public override void Rename(string newName)
        {
            string oldPath = GetAbsolutePath();
            string newPath = Parent.GetAbsolutePath(newName);
            Directory.Move(oldPath, newPath);
            Name = newName;
            Save();
        }

        public override string GetAbsolutePath(string path)
        {
            return Path.Combine(GetAbsolutePath(), path);
        }

        public override string GetAbsolutePath()
        {
            return Parent.GetAbsolutePath(Name);
        }

        public override void Save()
        {
            FindRoot<HexaProject>().Save();
        }

        internal override void BuildParentTree()
        {
            foreach (var item in Items)
            {
                item.Parent = this;
                if (item is HexaParent parent)
                    parent.BuildParentTree();
            }
        }

        public override T FindRoot<T>() where T : class
        {
            if (this is T t)
                return t;
            else if (Parent is not null)
                return Parent.FindRoot<T>();
            return null;
        }
    }
}