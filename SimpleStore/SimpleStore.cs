using SimpleStore.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimpleStore2
{
    public class SimpleStore<T>:IDisposable
    {
        private string filepath;
        private Dictionary<UInt32, T> _memoryStorage;
        public class DiskRecord
        {
            public UInt32 Id { get; set; }
            public DateTime DateAdded { get; set; }
            public T Data { get; set; }
            public bool IsDeleted { get; set; }=false;
        }
        private System.IO.StreamWriter _fs;
        private UInt32 biggestId;
        private bool disposed=false;

        public SimpleStore(string filePath)
        {
            this.filepath = filePath;
            
            _memoryStorage= new Dictionary<UInt32, T>();
            biggestId=0;
            InitializeFromDisk(filePath);
            _fs = new System.IO.StreamWriter(filePath,true);
        }
        ~SimpleStore()
        {
            if(_fs!=null)
                try
                {
                    _fs.Close();
                }catch(Exception ex)
                {
                    throw;
                }
        }
        private void InitializeFromDisk(string filePath)
        {
            if(!System.IO.File.Exists(filePath))
                System.IO.File.Create(filePath).Close();
            using (var reader = new System.IO.StreamReader(filePath))
            {
                try
                {
                    while (true)
                    {
                        var line = reader.ReadLine();
                        if (line == null) 
                            break;
                        var r = System.Text.Json.JsonSerializer.Deserialize<DiskRecord>(line);

                        if (r.IsDeleted)
                        {
                            _memoryStorage.Remove(r.Id);
                        }
                        else
                        {
                            MemoryAddOrReplace(r.Id, r.Data);
                        }

                    }
                }catch(Exception ex)
                {
                    throw new FileformatException() { };
                }
            }
        }
        private void PersistToDisk(DiskRecord r) {
            var line = System.Text.Json.JsonSerializer.Serialize(r);
            _fs.WriteLine(line);
        }
        private UInt32 MemoryAddOrReplace(UInt32 id, T data)
        {
            if(id<1 || !_memoryStorage.ContainsKey(id))
            {
                id = ++biggestId;
                _memoryStorage.Add(id, data);
            }
            _memoryStorage[id] = data;
            return id;
        }
        
        public T Get(UInt32 id)
        {
            ThrowIfDisposed();
            try
            {
                return _memoryStorage[id];
            }catch(KeyNotFoundException)
            {
                throw new ItemNotFoundException(id);
            }
        }
        
        public uint AddOrUpdate(UInt32 id, T data)
        {
            ThrowIfDisposed();
            var newid = MemoryAddOrReplace(id, data);
            PersistToDisk(new DiskRecord { Id = newid, Data = data, DateAdded = DateTime.UtcNow });
            return newid;
        }
        public void Delete(UInt32 id)
        {
            ThrowIfDisposed();
            _memoryStorage.Remove(id);
            PersistToDisk(new DiskRecord { Id = id, DateAdded = DateTime.UtcNow, Data = { }, IsDeleted = true }) ;
        }
        public void DestroyForeverNoJoke()
        {
            ThrowIfDisposed();
            _memoryStorage.Clear();
            _fs.Flush();
            _fs.Close();
            this.disposed = true;
            System.IO.File.Delete(filepath);

        }
        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw new StoreDisposedException();
            }
        }
        public void Dispose()
        {
            if (_fs != null)
            {
                try
                {
                    _fs.Flush();
                    _fs.Close();
                }
                catch (ObjectDisposedException)
                {
                    //do nothing
                }
            }
          
            this.disposed = true;
            _memoryStorage.Clear();
        }
        public string GetFilePath() => this.filepath;
        
    }
}
