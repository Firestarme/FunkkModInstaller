using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.IO.Compression;
using FunkkModInstaller.Utilities;

namespace FunkkModInstaller.JSON
{
    public class JSONReader
    {

        private const string JSON_FILENAME = "PACK_MANIFEST.json";


       //READ functions
        public JSONDoc? TryReadFile(string path)
        {
            if (!File.Exists(path)) return null;

            Func<string> ReadFunc = () => File.ReadAllText(path);
            return TryRead(ReadFunc, path);
        }

        public JSONDoc? TryReadPackZip( string path)
        {
            if (!File.Exists(path)) return null; ;

            Func<string> ReadFunc = () =>
            {
                using (ZipArchive archive = ZipFile.OpenRead(path))
                {
                    ZipArchiveEntry readmeEntry = archive.GetEntry(JSON_FILENAME);
                    if (readmeEntry == null) { return null; }

                    using (StreamReader reader = new StreamReader(readmeEntry.Open()))
                    {
                        return reader.ReadToEnd();
                    }
                }
            };

            return TryRead(ReadFunc,path);
        }

        public JSONDoc? TryReadPackDirectory(string path)
        {
            var file_path = vPath.Combine(path, JSON_FILENAME);
            if (!Directory.Exists(path)) return null;

            Func<string> ReadFunc = () => File.ReadAllText(file_path);
            return TryRead(ReadFunc, file_path);
        }

        private JSONDoc? TryRead(Func<string> readfunc, string src_path)
        {
            JSONModPack? _doc;
            Hash16 _hash;

            try
            {
                string data = readfunc.Invoke();
                _doc = JsonSerializer.Deserialize<JSONModPack>(data);
                _hash = Hash16.ComputeHashFromString(data);
            }
            catch (Exception ex) 
            {
                //App.Console.PrintLn(1, $"Exception while parsing pack: {src_path}");
                //App.Console.Print(2, ex.Message);
                return null;
            }

            if (_doc == null)
            {
                //App.Console.PrintLn(1, $"Pack could not be parsed: {src_path}");
                return null;
            }

            return new JSONDoc(_doc, _hash); ;
        }

        // WRITE Functions
        public bool TryWritePackDirectory(string path, JSONModPack doc)
        {
            var file_path = vPath.Combine(path, JSON_FILENAME);
            if (!Directory.Exists(path)) return false;

            Func<string, bool> WriteFunc = (string data) =>
            {
                File.WriteAllText(file_path,data);
                return true;
            };

            return TryWrite(WriteFunc, file_path, doc);
        }

        public bool TryWriteFile(string path, JSONModPack doc)
        {
            Func<string, bool> WriteFunc = (string data) => { File.WriteAllText(path, data); return true; } ;
            return TryWrite(WriteFunc, path, doc);
        }

        private bool TryWrite(Func<string,bool> writefunc, string path, JSONModPack doc)
        {
            try
            {
                string data = JsonSerializer.Serialize(doc);
                return writefunc(data);
            }
            catch(Exception ex) 
            {
                //App.Console.PrintLn(1, $"Exception while saving pack: {path}");
                //App.Console.Print(2, ex.Message);
                return false;
            }
            
        }

        public class JSONDoc
        {
            public readonly JSONModPack JSONModPack;
            public readonly Hash16 Hash;

            public JSONDoc( JSONModPack jSONModPack, Hash16 hash)
            {
                JSONModPack = jSONModPack;
                Hash = hash;
            }
        }
    }

    public class JSONModPack
    {
        public string? ModPackName { get; set; }
        public JSONMod? Bepinex { get; set; }
        public IList<JSONMod>? Mods { get; set; }
    }

    public class JSONMod
    {
        public string? Name { get; set; }
        public string? GUID { get; set; }
        public string? Description { get; set; }
        public string? Version { get; set; }
        public bool? IsRequired { get; set; }
        public bool? InstallByDefault { get; set; }

        public IList<JSONTarget>? Targets { get; set; }
        public IList<string>? Dependencies { get; set; }
    }

    public class JSONTarget
    {
        public string? DestDirPath { get; set; }
        public IList<JSONModFile>? ModFiles { get; set; }
    }

    public class JSONModFile
    {
        public string? Filename { get; set; }
        public string? SrcPath { get; set; }
        public string? Version { get; set;}
    }
}
