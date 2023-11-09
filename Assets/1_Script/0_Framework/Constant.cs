using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomDungeon
{

    public class Constant
    {

        #region Directory
        public const string DIRECTORY_PATH_ROOT              = "";
        public const string DIRECTORY_PATH_DATA              = DIRECTORY_PATH_ROOT + "Data";
        public const string DIRECTORY_PATH_JSON              = DIRECTORY_PATH_DATA + "\\JSON";
        public const string DIRECTORY_PATH_CSV               = DIRECTORY_PATH_DATA + "\\CSV";
        #endregion

        #region Tile
        public const int TILE_IDENTIFIER_EMPTY              = 0;
        public const int TILE_IDENTIFIER_OUT                = 1;
        public const int TILE_IDENTIFIER_BIOM_WALL          = 2;
        public const int TILE_IDENTIFIER_BIOM_FLOOR         = 3;
        #endregion

    }

}
