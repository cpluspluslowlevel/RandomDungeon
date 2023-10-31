using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace RandomDungeon.File
{

    public abstract class File
    {

        public abstract bool Save();

        public abstract bool Load();

    }

    public class JSONFile<T> : File
    {

        public T dataObject => m_dataObject;

        public string filePath { get; }



        public JSONFile(string filePath, T dataObject)
        {

            this.m_dataObject   = dataObject;
            this.filePath       = filePath;

            if (!System.IO.File.Exists(filePath))
            {
                var file = System.IO.File.Create(filePath);
                file.Close();
            }

        }


        public override bool Save()
        {

            var json = JsonUtility.ToJson(m_dataObject);
            System.IO.File.WriteAllText(filePath, json);

            return true;

        }



        public override bool Load()
        {

            var json = System.IO.File.ReadAllText(filePath);
            JsonUtility.FromJsonOverwrite(json, m_dataObject);

            return true;

        }

        private T m_dataObject = default(T);

    }

    public class CSV
    {

        public static List<List<string>> ParseCSV(string path)
        {

            List<List<string>> result = new List<List<string>>();

            StreamReader reader = new StreamReader(path);
            while (!reader.EndOfStream)
            {

                result.Add(new List<string>());
                
                var     line        = reader.ReadLine();
                var     tokenArray  = line.Split(",");
                foreach(var token in tokenArray)
                {
                    result.Last().Add(token);
                }

            }

            return result;

        }

    }

}