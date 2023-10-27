using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RandomDungeon.Framework
{

    public class Framework
    {

        public Data.User userData => m_userDataFile.dataObject;

        #region Singleton
        //Singleton

        static public Framework Instance => I;

        static private Framework I = new Framework();

        private Framework()
        {
            Initialize();
        }

        ///////////////////////////////////////////////////
        #endregion

        private void Initialize()
        {

            m_userDataFile.Load();

        }


        //Data
        private File.JSONFile<Data.User> m_userDataFile = new File.JSONFile<Data.User>(Constant.DIRECTORY_PATH_JSON + "\\User.json", new Data.User());

    }

}