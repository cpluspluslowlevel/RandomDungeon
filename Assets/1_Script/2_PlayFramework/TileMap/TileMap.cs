using RandomDungeon.File;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

namespace RandomDungeon.PlayFramework
{


    public enum TileClassify
    {

    }

    public class Tile
    {

        public int identifier;
        public int index;

        internal static int GetClassify(Tile tile)
        {
            throw new System.NotImplementedException();
        }
    }

    public struct TILEMAP_PROPERTY
    {
    }


    /***************************************************
     * 타일맵
     **************************************************/
    public class TileMap : MonoBehaviour
    {

        public TILEMAP_PROPERTY                             property => m_property;

        public MapGenerator.MAP_GENERATION_PROPERTY         mapGenerationProperty => m_mapGenerationProperty;
        

        public MeshFilter meshFilter => m_meshFilter;


        private void Awake()
        {

            Debug.Assert(m_meshFilter != null);

            GenerateDungeon();
            CreateTileMap();


        }

        private bool GenerateDungeon()
        {

            MapGenerator generator = new MapGenerator();
            generator.m_property = m_mapGenerationProperty;
            generator.GenerateMap();

            //배열을 만들고 빈 값으로 채웁니다.
            m_tileList.Capacity = m_mapGenerationProperty.cellNumberX * m_mapGenerationProperty.cellNumberY;
            for (int y = 0; y < generator.m_property.cellNumberY; ++y)
            {
                for (int x = 0; x < generator.m_property.cellNumberX; ++x)
                {
                    m_tileList.Add(new Tile());
                    m_tileList.Last().identifier    = Constant.TILE_IDENTIFIER_EMPTY;
                    m_tileList.Last().index         = y * generator.m_property.cellNumberX + x;
                }
            }

            //방들을 만듭니다.
            int siblingNumber = (int)Mathf.Pow(2, generator.m_property.mapGenerationPatitionNumber);
            int begin = siblingNumber - 1;
            CreateRoomClass(siblingNumber);
            for (int i = 0; i < siblingNumber; ++i)
            {
                GenerateRoom(generator.m_node_array[begin + i], i);
            }

            //통로를 만듭니다.
            int parentNumber = (int)Mathf.Pow(2, generator.m_property.mapGenerationPatitionNumber) - 1;
            for (int i = 0; i < parentNumber; ++i)
            {
                GenerateCorridor(generator.m_node_array[i]);
            }

            //방, 통로에 벽을 만듭니다.
            GenerateWall();

            foreach (var room in m_roomList)
            {
                room.Start();
            }

            return true;

        }

        private void CreateTileMap()
        {

            Vector2 pt = Vector2.zero;
            int     offsetY = 0;

            for(int y = 0; y < m_mapGenerationProperty.cellNumberY; ++y)
            {
                offsetY = y * m_mapGenerationProperty.cellNumberX;
                for(int x = 0; x < m_mapGenerationProperty.cellNumberX; ++x)
                {
                    UpdateGeneratedTile(m_tileList[offsetY + x]);
                }
            }

            int planeNumberX = m_mapGenerationProperty.cellNumberX + 1;
            int planeNumberY = m_mapGenerationProperty.cellNumberY + 1;
            for (int y = 0; y < planeNumberY; ++y)
            {
                offsetY = y * m_mapGenerationProperty.cellNumberX;
                for (int x = 0; x < planeNumberX; ++x)
                {
                    //MakeMash(m_tileList[offsetY + x]);
                }
            }

        }

        //맵 생성기로 생성된 타일을 실제 타일로 변환
        private void UpdateGeneratedTile(Tile tile)
        {

            switch(tile.identifier)
            {
            case Constant.TILE_IDENTIFIER_BIOM_WALL:
                break;
            case Constant.TILE_IDENTIFIER_BIOM_FLOOR:
                break;
            }

        }

        private void MakeMash(Tile tile)
        {

            switch(Tile.GetClassify(tile))
            {

            }

        }

        private void CreateRoomClass(int roomNumber)
        {

            m_roomList.Add(new PlayerStartRoom());
            m_roomList.Add(new BossRoom());

            int monsterRoomNumber = (roomNumber - 2) / 2;
            for (int i = 0; i < monsterRoomNumber; ++i)
            {
                m_roomList.Add(new MonsterRoom());
            }

            int emptyRoomNumber = roomNumber - 2 - monsterRoomNumber;
            for (int i = 0; i < emptyRoomNumber; ++i)
            {
                m_roomList.Add(new EmptyRoom());
            }
            m_roomList = m_roomList.OrderBy(x => Random.Range(0, 100)).ToList();

        }

        private void GenerateRoom(MapGenerator.Node node, int index)
        {

            Debug.Assert(node.IsLeaf);

            for (int y = node.RoomRect.min.y; y <= node.RoomRect.max.y; ++y)
            {
                for (int x = node.RoomRect.min.x; x <= node.RoomRect.max.x; ++x)
                {
                    m_tileList[y * m_mapGenerationProperty.cellNumberX + x].identifier = Constant.TILE_IDENTIFIER_BIOM_FLOOR;
                }
            }

            m_roomList[index].tileMap       = this;
            m_roomList[index].Index         = index;
            m_roomList[index].tileIndexRect = node.RoomRect;

        }

        private void GenerateCorridor(MapGenerator.Node node)
        {

            Debug.Assert(!node.IsLeaf);

            for (int y = 0; y < node.CorridorRect.height; ++y)
            {
                for (int x = 0; x < node.CorridorRect.width; ++x)
                {

                    var tileIndex = new Vector3Int(node.CorridorRect.x + x, node.CorridorRect.y + y, 0);

                }
            }

        }

        private void GenerateWall()
        {

            //벽을 생성하는 타일을 구분하는 맵을 만듭니다.
            bool[] isFloorMap = new bool[m_mapGenerationProperty.cellNumberX * m_mapGenerationProperty.cellNumberY];
            for (int y = 0; y < m_mapGenerationProperty.cellNumberY; ++y)
            {
                for (int x = 0; x < m_mapGenerationProperty.cellNumberX; ++x)
                {
                    var index = y * m_mapGenerationProperty.cellNumberX + x;
                    isFloorMap[index] = m_tileList[index].identifier == Constant.TILE_IDENTIFIER_BIOM_FLOOR;
                }
            }


            //밑에 if문을 간단히 하기 위해 for문을[0, w], [0, h]형태로 구성해 마지막 인덱스를 구할 때 -1을 할 필요없게 합니다.
            var w = m_mapGenerationProperty.cellNumberX - 1;
            var h = m_mapGenerationProperty.cellNumberY - 1;
            for (int y = 0; y <= h; ++y)
            {
                for (int x = 0; x <= w; ++x)
                {

                    var index = y * m_mapGenerationProperty.cellNumberX + x;

                    //주변에 벽을 생성하는 타일이 있다면 벽을 만듭니다.
                    //맵 밖을 넘어가는 인덱스일 경우 맵 안으로 값을 자릅니다.
                    //OR연산자로 구성되기에 같은 타일을 두번 계산해도 문제는 발생하지 않습니다.
                    if (!isFloorMap[index])
                    {

                        if (isFloorMap[System.Math.Max(y - 1, 0) * m_mapGenerationProperty.cellNumberX + System.Math.Max(x - 1, 0)] ||
                            isFloorMap[System.Math.Max(y - 1, 0) * m_mapGenerationProperty.cellNumberX + (x + 0)] ||
                            isFloorMap[System.Math.Max(y - 1, 0) * m_mapGenerationProperty.cellNumberX + System.Math.Min(x + 1, w)] ||
                            isFloorMap[y * m_mapGenerationProperty.cellNumberX + System.Math.Max(x - 1, 0)] ||
                            isFloorMap[y * m_mapGenerationProperty.cellNumberX + System.Math.Min(x + 1, w)] ||
                            isFloorMap[System.Math.Min(y + 1, h) * m_mapGenerationProperty.cellNumberX + System.Math.Max(x - 1, 0)] ||
                            isFloorMap[System.Math.Min(y + 1, h) * m_mapGenerationProperty.cellNumberX + (x + 0)] ||
                            isFloorMap[System.Math.Min(y + 1, h) * m_mapGenerationProperty.cellNumberX + System.Math.Min(x + 1, w)])
                        {
                            m_tileList[index].identifier = Constant.TILE_IDENTIFIER_BIOM_WALL;
                        }
                    }

                }
            }

        }


        [SerializeField] private TILEMAP_PROPERTY                       m_property;
        [SerializeField] private MapGenerator.MAP_GENERATION_PROPERTY   m_mapGenerationProperty;
        private List<Tile>                                              m_tileList = new List<Tile>();
        private List<Room>                                              m_roomList = new List<Room>();

        [SerializeField] private MeshFilter                             m_meshFilter;

    }

}
