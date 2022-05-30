using Xunit;
using SimpleStore2;
using System;
using System.Collections.Generic;

namespace SimpleStoreTests
{
    public class UnitTest1
    {
        private class TestClass {
            public int ID { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public byte[] RandomBytes { get; set; }
            public override bool Equals(object? obj)
            {
                TestClass tc=obj as TestClass;
                if(tc == null) return false;
                if(tc.ID != this.ID) return false;
                if (tc.Name != this.Name) return false;
                if (tc.Email != this.Email) return false;
                if (tc.RandomBytes == null && this.RandomBytes != null) return false;
                if (tc.RandomBytes == this.RandomBytes) return true;
                if(tc.RandomBytes.Length != this.RandomBytes.Length) return false;
                for(var i =0; i < tc.RandomBytes.Length; i++)
                {
                    if (tc.RandomBytes[i] != this.RandomBytes[i]) return false;
                }
                return true;
            }
        }
        private List<string> tempfiles = new List<string>();
        TestClass obj1 = new TestClass() { Email="jokogarcia@gmail.com",Name="Joaquin",ID=0,RandomBytes=GetByteArray(120)};
        TestClass obj2 = new TestClass() { Email = "pilyandel@gmail.com", Name = "Pily", ID = 9, RandomBytes = GetByteArray(20) };
        string masterTestFile;
       #region Constructor and Destructor
        public UnitTest1()
        {
            var command = "del *.tmp";
            System.Diagnostics.Process.Start("cmd.exe", "/C " + command);
            masterTestFile = "masterTestFile.tmp";
            using (var store = new SimpleStore<TestClass>(masterTestFile))
            {
                store.AddOrUpdate(0, obj1);
                store.AddOrUpdate(0, obj2);
            }
        }
        
        #endregion
        #region Success Cases
        [Fact]
        public void ConstructStoreFromExistingFile()
        {
            using (var t = new SimpleStore<TestClass>(masterTestFile))
                Assert.NotNull(t);
        }
        [Fact]
        public void ReadDataFromExistingFile()
        {
            using (var t = new SimpleStore<TestClass>(masterTestFile))
            {
                var d1 = t.Get(1);
                Assert.Equal(d1, obj1);
            }
                
        }
        [Fact]
        public void DataIsPersisted()
        {
            TestClass obj3 = new TestClass()
            {
                Email = "kissmy@ss.com",
                Name = "Cursory Foxxy",
                ID = 987
            };
            string path;
            uint id;
            using(var t = getDisposableTestStore())
            {
                id = t.AddOrUpdate(0, obj3);
                path = t.GetFilePath();
            }
            using (var t = new SimpleStore<TestClass>(path))
            {
                Assert.Equal(t.Get(id), obj3);
            }
        }
        [Fact]

        public void WriteDataToNewFileAndRead()
        {
            string newfilepath = createTempFile();
            using (var d = new SimpleStore<TestClass>(newfilepath))
            {
                var id = d.AddOrUpdate(0, obj2);
                Assert.True(id > 0);
                Assert.Equal(d.Get(id), obj2);
            }
                
        }

        [Fact]
        public void DeleteItemFromFile()
        {
            using (var d = getDisposableTestStore())
            {
                d.Delete(2);
                Assert.Throws<SimpleStore.Exceptions.ItemNotFoundException>(() => d.Get(2));
            }
            
        }
        [Fact]
        public void DestroyArchive()
        {
            using (var d = getDisposableTestStore())
            {
                Assert.True(System.IO.File.Exists(tempfiles[tempfiles.Count - 1]));
                d.DestroyForeverNoJoke();
                Assert.Throws<SimpleStore.Exceptions.StoreDisposedException>(() => d.Get(1));
                Assert.False(System.IO.File.Exists(tempfiles[tempfiles.Count - 1]));
            }
            
        }
        #endregion
        #region Fail Cases
        [Fact]
        public void CreateFromWrongFormat_ThrowsFileFormatException()
        {
            Assert.Throws<SimpleStore.Exceptions.FileformatException>(()=> new SimpleStore<string>(masterTestFile));
        }
        [Fact]
        public void CreateFromOrdinaryTextFile_ThrowsFileFormatException()
        {
            var f = createTempFile();
            System.IO.File.WriteAllText(f, "I am random text on a random textfile\nLOL");
            Assert.Throws<SimpleStore.Exceptions.FileformatException>(() => new SimpleStore<TestClass>(f));

        }
        [Fact]
        public void ReadInvalidId_ThrowsItemNotFoundException()
        {
            using (var t = getDisposableTestStore())
            {
                Assert.Throws<SimpleStore.Exceptions.ItemNotFoundException>(()=> t.Get(69));
            }
        }
        #endregion
        #region Aux functions
        private SimpleStore<TestClass> getDisposableTestStore()
        {
            var tempFileName = createTempFile();
            System.IO.File.Copy(masterTestFile, tempFileName, true);
            return new SimpleStore<TestClass>(tempFileName);
        }
       
        private string createTempFile()
        {
            var r = new Random((int)DateTime.UtcNow.Ticks);

            var tempFileName = $"aTempFile{r.Next()}.tmp";
            tempfiles.Add(tempFileName);
            return tempFileName;
        }
        private static byte[] GetByteArray(int sizeInKb)
        {
            Random rnd = new Random(4);
            byte[] b = new byte[sizeInKb * 1024]; // convert kb to byte
            rnd.NextBytes(b);
            return b;
        }
        #endregion
        /*
         TODO:
        Success cases:
        Construct from existing file-
        Read data from existing file-
        Write data to new file and read back from memory using returned ID-
        Delete data from file and attempt to read deleted data from memory-
        Destroy archive

        Failure cases:
        Attempt to construct from file of another SimpleStore Format. Should trhow FileFormatException
        Attempt to construct from an ordinary text file. Should throw FileFormatException
        Attempt to read invalid ID
        5r4t
         */
    }
}