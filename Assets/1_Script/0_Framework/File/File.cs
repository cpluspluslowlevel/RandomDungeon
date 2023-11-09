using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

namespace RandomDungeon.File
{
    public abstract class File<T>
    {

        public T dataObject => m_dataObject;

        public string filePath { get; }

        public File(string filePath, T dataObject)
        {

            this.m_dataObject   = dataObject;
            this.filePath       = filePath;

            if (!System.IO.File.Exists(filePath))
            {
                var file = System.IO.File.Create(filePath);
                file.Close();
            }

        }



        public abstract bool Save();

        public abstract bool Load();




        protected T m_dataObject = default(T);


    }

    public class JSONFile<T> : File<T>
    {


        public JSONFile(string filePath, T dataObject) : base(filePath, dataObject)
        {
        }



        #region File override
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        //File override

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

        //File override
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion

    }

    public class CSVFile<T> : File<List<T>> where T : new()
    {

        public CSVFile(string filePath, List<T> dataObjectList) : base(filePath, dataObjectList)
        {
        }

        #region File override
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        //File override

        public override bool Save()
        {
            return true;
        }

        public override bool Load()
        {

            var columnTypeList  = new List<KeyValuePair<string, FieldInfo>>();
            var line            = string.Empty;
            using (var reader = new StreamReader(filePath))
            {

                //첫 번째 행에서 열 이름과 타입 정보를 가져옵니다.
                line = reader.ReadLine();
                foreach(var name in line.Split(","))
                {
                    columnTypeList.Add(new KeyValuePair<string, FieldInfo>(name, typeof(T).GetField(name)));
                }

                //나머지 행을 읽어 데이터를 가져옵니다.
                while(!reader.EndOfStream)
                {

                    line            = reader.ReadLine();
                    var tokenList   = line.Split(",");

                    //행의 각 열에 해당하는 타입 정보를 이용해 토큰을 변환해 값을 넣습니다. 
                    var data = new T();
                    for (int i = 0; i < columnTypeList.Count; ++i)
                    {
                        columnTypeList[i].Value.SetValue(data, Convert.ChangeType(tokenList[i], columnTypeList[i].Value.FieldType));
                    }
                    dataObject.Add(data);

                }

            }

            return true;

        }

        //File override
        ///////////////////////////////////////////////////////////////////////////////////////////////////
        #endregion


    }

}