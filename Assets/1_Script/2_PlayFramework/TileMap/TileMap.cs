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
     * Ÿ�ϸ�
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

            //�迭�� ����� �� ������ ä��ϴ�.
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

            //����� ����ϴ�.
            int siblingNumber = (int)Mathf.Pow(2, generator.m_property.mapGenerationPatitionNumber);
            int begin = siblingNumber - 1;
            CreateRoomClass(siblingNumber);
            for (int i = 0; i < siblingNumber; ++i)
            {
                GenerateRoom(generator.m_node_array[begin + i], i);
            }

            //��θ� ����ϴ�.
            int parentNumber = (int)Mathf.Pow(2, generator.m_property.mapGenerationPatitionNumber) - 1;
            for (int i = 0; i < parentNumber; ++i)
            {
                GenerateCorridor(generator.m_node_array[i]);
            }

            //��, ��ο� ���� ����ϴ�.
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

        //�� ������� ������ Ÿ���� ���� Ÿ�Ϸ� ��ȯ
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

            //���� �����ϴ� Ÿ���� �����ϴ� ���� ����ϴ�.
            bool[] isFloorMap = new bool[m_mapGenerationProperty.cellNumberX * m_mapGenerationProperty.cellNumberY];
            for (int y = 0; y < m_mapGenerationProperty.cellNumberY; ++y)
            {
                for (int x = 0; x < m_mapGenerationProperty.cellNumberX; ++x)
                {
                    var index = y * m_mapGenerationProperty.cellNumberX + x;
                    isFloorMap[index] = m_tileList[index].identifier == Constant.TILE_IDENTIFIER_BIOM_FLOOR;
                }
            }


            //�ؿ� if���� ������ �ϱ� ���� for����[0, w], [0, h]���·� ������ ������ �ε����� ���� �� -1�� �� �ʿ���� �մϴ�.
            var w = m_mapGenerationProperty.cellNumberX - 1;
            var h = m_mapGenerationProperty.cellNumberY - 1;
            for (int y = 0; y <= h; ++y)
            {
                for (int x = 0; x <= w; ++x)
                {

                    var index = y * m_mapGenerationProperty.cellNumberX + x;

                    //�ֺ��� ���� �����ϴ� Ÿ���� �ִٸ� ���� ����ϴ�.
                    //�� ���� �Ѿ�� �ε����� ��� �� ������ ���� �ڸ��ϴ�.
                    //OR�����ڷ� �����Ǳ⿡ ���� Ÿ���� �ι� ����ص� ������ �߻����� �ʽ��ϴ�.
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
