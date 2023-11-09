using RandomDungeon.File;
using RandomDungeon.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RandomDungeon.Framework
{

    public class Framework
    {

        #region Singleton
        //Singleton

        static public Framework Instance => I;

        static private Framework I = new Framework();

        private Framework()
        {
        }

        ///////////////////////////////////////////////////
        #endregion


        public User                     userData => m_userDataFile.dataObject;
        public List<TileProperty>       tilePropertyList => m_tilePropertyDataFile.dataObject;


        public void Initialize()
        {

            m_userDataFile.Load();
            m_tilePropertyDataFile.Load();

        }


        //Data
        private JSONFile<User>              m_userDataFile          = new JSONFile<User>(Constant.DIRECTORY_PATH_JSON + "\\User.json", new User());
        private CSVFile<TileProperty>       m_tilePropertyDataFile  = new CSVFile<TileProperty>(Constant.DIRECTORY_PATH_CSV + "\\TileProperty.csv", new List<TileProperty>());

    }

}