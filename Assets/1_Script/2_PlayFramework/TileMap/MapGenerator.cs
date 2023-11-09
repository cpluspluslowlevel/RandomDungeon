using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.U2D;

public class MapGenerator
{

    [System.Serializable]
    public struct MAP_GENERATION_PROPERTY
    {

        public float cellSizeX => mapSize.x / cellNumberX;
        public float cellSizeY => mapSize.y / cellNumberY;

        public GameObject debuggingGridPrefab;
        public GameObject debuggingGridParent;

        public Vector2    mapSize;
        public Vector2    mapPivot;
        public int        mapGenerationPatitionNumber;

        public int        cellNumberX;
        public int        cellNumberY;
        public Vector2    cellSplitRandomRangeX;
        public Vector2    cellSplitRandomRangeY;

        public Vector2    roomRandomRangeX;
        public Vector2    roomRandomRangeY;

        public int        corriderSize;

    }

    public class Node
    {

        public int CenterX => GridRect.x + GridRect.width / 2;
        public int CenterY => GridRect.y + GridRect.height / 2;

        public bool IsLeaf => ChildLeft == null;

        public RectInt              GridRect = new RectInt();
        public RectInt              RoomRect;
        public RectInt              CorridorRect;

        public bool                 IsSplitHorizon;

        public Node                 ChildLeft   = null;
        public Node                 ChildRight  = null;

        //디버깅용 입니다.
        public GameObject           Grid;
        public GameObject           Room;
        public GameObject           Corridor;


        public RectInt CalculateRect()
        {

            RectInt rect = new RectInt();

            if (IsLeaf)
            {
                return RoomRect;
            }

            var leftRect    = ChildLeft.CalculateRect();
            var rightRect   = ChildRight.CalculateRect();
            rect.min        = new Vector2Int(System.Math.Min(leftRect.min.x, rightRect.min.x), System.Math.Min(leftRect.min.y, rightRect.min.y));
            rect.max        = new Vector2Int(System.Math.Max(leftRect.max.x, rightRect.max.x), System.Math.Max(leftRect.max.y, rightRect.max.y));

            return rect;

        }

        public int RecursiveOverlapRoom(RectInt rect)
        {
            if(IsLeaf)
            {
                return GridRect.Overlaps(rect) ? 1 : 0;
            }
            else
            {
                return ChildLeft.RecursiveOverlapRoom(rect) + ChildRight.RecursiveOverlapRoom(rect);
            }
        }


    }

    public void GenerateMap()
    {

        CreateTree();

        
        //영역을 분할합니다.
        var siblingNumber   = 1;
        int begin           = 0;
        bool isHorizonSplit = Random.Range(0, 2) % 2 == 0;
        for (int i = 0; i < m_property.mapGenerationPatitionNumber; ++i)
        {

            begin = (int)Mathf.Pow(2, i) - 1;
            for (int j = 0; j < siblingNumber; ++j)
            {

                //포화 이진 트리이기 때문에 자식, 부모의 인덱스를 계산할 수 있습니다.
                var index = begin + j;
                var childLeft = index * 2 + 1;
                var childRight = childLeft + 1;

                //자신을 분할해 자식을 설정합니다.
                Split(m_node_array[index], isHorizonSplit);


            }
            
            isHorizonSplit = !isHorizonSplit;
            siblingNumber *= 2;

        }

        //분할된 영역에 방을 생성합니다.
        siblingNumber   = (int)Mathf.Pow(2, m_property.mapGenerationPatitionNumber);
        begin           = (int)Mathf.Pow(2, m_property.mapGenerationPatitionNumber) - 1;
        for (int i = 0; i < siblingNumber; ++i)
        {
            GenerateRoom(m_node_array[begin + i]);
            //UpdateLineRenderer(m_node_array[begin + i]);
        }


        //방들을 연결합니다.

        for (int i = (int)Mathf.Pow(2, m_property.mapGenerationPatitionNumber) - 1 - 1; i >= 0; --i)
        {
            GenerateCorridor(m_node_array[i]);
            //UpdateLineRenderer(_node_array[i]);
        }

    }


    private void CreateTree()
    {

        //포화 이진 트리 구조이기에 노드의 개수를 알 수 있으니 미리 생성해 놓습니다.
        m_node_array = new Node[(int)Mathf.Pow(2, m_property.mapGenerationPatitionNumber + 1) - 1];
        for (int i = 0; i < m_node_array.Length; ++i)
        {

            m_node_array[i] = new Node();

            //디버깅 용도
            //m_node_array[i].Grid     = GameObject.Instantiate(m_property.debuggingGridPrefab);
            //m_node_array[i].Room     = GameObject.Instantiate(m_property.debuggingGridPrefab);
            //m_node_array[i].Corridor = GameObject.Instantiate(m_property.debuggingGridPrefab);
            //m_node_array[i].Grid.transform.SetParent(m_property.debuggingGridParent.transform);
            //m_node_array[i].Room.transform.SetParent(m_property.debuggingGridParent.transform);
            //m_node_array[i].Corridor.transform.SetParent(m_property.debuggingGridParent.transform);

        }

        int   parentNodeCount = (int)Mathf.Pow(2, m_property.mapGenerationPatitionNumber) - 1;
        for (int i = 0; i < parentNodeCount; ++i)
        {
            m_node_array[i].ChildLeft  = m_node_array[i * 2 + 1];
            m_node_array[i].ChildRight = m_node_array[i * 2 + 2];
        }

        //루트 노드를 맵 전체 크기로 설정합니다.
        m_node_array[0].GridRect.x       = 0;
        m_node_array[0].GridRect.y       = 0;
        m_node_array[0].GridRect.width   = m_property.cellNumberX;
        m_node_array[0].GridRect.height  = m_property.cellNumberY;

    }

    private void Split(Node node, bool isHorizonSplit)
    {

        var left    = node.ChildLeft;
        var right   = node.ChildRight;
        node.IsSplitHorizon = isHorizonSplit;
        if (node.IsSplitHorizon)
        {

            int splitY = (int)(node.GridRect.height * Random.Range(m_property.cellSplitRandomRangeY.x, m_property.cellSplitRandomRangeY.y));

            left.GridRect.x         = node.GridRect.x;
            left.GridRect.y         = node.GridRect.y;
            left.GridRect.width     = node.GridRect.width;
            left.GridRect.height    = splitY;

            right.GridRect.x        = node.GridRect.x;
            right.GridRect.y        = node.GridRect.y + splitY;
            right.GridRect.width    = node.GridRect.width;
            right.GridRect.height   = node.GridRect.height - splitY;

        }
        else
        {

            var splitX = (int)(node.GridRect.width * Random.Range(m_property.cellSplitRandomRangeX.x, m_property.cellSplitRandomRangeX.y));

            left.GridRect.x         = node.GridRect.x;
            left.GridRect.y         = node.GridRect.y;
            left.GridRect.width     = splitX;
            left.GridRect.height    = node.GridRect.height;

            right.GridRect.x        = node.GridRect.x + splitX;
            right.GridRect.y        = node.GridRect.y;
            right.GridRect.width    = node.GridRect.width - splitX;
            right.GridRect.height   = node.GridRect.height;

        }

    }


    private void GenerateRoom(Node node)
    {

        node.RoomRect           = new RectInt();
        node.RoomRect.width     = (int)(node.GridRect.width * Random.Range(m_property.roomRandomRangeX.x, m_property.roomRandomRangeX.y));
        node.RoomRect.height    = (int)(node.GridRect.height * Random.Range(m_property.roomRandomRangeX.y, m_property.roomRandomRangeX.y));
        node.RoomRect.x         = Random.Range(node.GridRect.x + 1, node.GridRect.x + node.GridRect.width - node.RoomRect.width - 1);
        node.RoomRect.y         = Random.Range(node.GridRect.y + 1, node.GridRect.y + node.GridRect.height - node.RoomRect.height - 1);

    }

    private void GenerateCorridor(Node node)
    {

        node.CorridorRect = new RectInt();

        Node    left            = node.ChildLeft;
        Node    right           = node.ChildRight;
        RectInt leftRect        = left.CalculateRect();
        RectInt rightRect       = right.CalculateRect();
        int maxBegin;
        int minEnd;
        if (node.IsSplitHorizon)
        {

            maxBegin    = Mathf.Max(leftRect.x + m_property.corriderSize, rightRect.x + m_property.corriderSize);
            minEnd      = Mathf.Min(leftRect.x + leftRect.width - m_property.corriderSize, rightRect.x + rightRect.width - m_property.corriderSize);

            node.CorridorRect.x         = Random.Range(maxBegin, minEnd + 1);
            node.CorridorRect.width     = m_property.corriderSize;
            node.CorridorRect.y         = leftRect.y;
            node.CorridorRect.height    = (int)Mathf.Abs(rightRect.y + rightRect.height - leftRect.y);

            CuttingCorridorTop(node, left);
            CuttingCorridorBottom(node, right);

        }
        else
        {

            maxBegin    = Mathf.Max(leftRect.y + m_property.corriderSize, rightRect.y + m_property.corriderSize);
            minEnd      = Mathf.Min(leftRect.y + leftRect.height - m_property.corriderSize, rightRect.y + rightRect.height - m_property.corriderSize);

            node.CorridorRect.x         = leftRect.x;
            node.CorridorRect.width     = (int)Mathf.Abs(rightRect.x + rightRect.width - leftRect.x);
            node.CorridorRect.y         = Random.Range(maxBegin, minEnd + 1);
            node.CorridorRect.height    = m_property.corriderSize;

            CuttingCorridorLeft(node, left);
            CuttingCorridorRight(node, right);

        }

    }

    private void CuttingCorridorLeft(Node corridorNode, Node node)
    {

        if (node.IsLeaf)
        {
            if (corridorNode.CorridorRect.Overlaps(node.RoomRect))
            {
                corridorNode.CorridorRect.min = new Vector2Int(node.RoomRect.max.x, corridorNode.CorridorRect.min.y);
            }
        }
        else
        {
            if (corridorNode.CorridorRect.Overlaps(node.CorridorRect))
            {
                corridorNode.CorridorRect.min = new Vector2Int(node.CorridorRect.max.x, corridorNode.CorridorRect.min.y);

            }
            CuttingCorridorLeft(corridorNode, node.ChildLeft);
            CuttingCorridorLeft(corridorNode, node.ChildRight);
        }

    }

    private void CuttingCorridorRight(Node corridorNode, Node node)
    {

        if (node.IsLeaf)
        {
            if (corridorNode.CorridorRect.Overlaps(node.RoomRect))
            {
                corridorNode.CorridorRect.max = new Vector2Int(node.RoomRect.min.x, corridorNode.CorridorRect.max.y);
            }
        }
        else
        {
            if (corridorNode.CorridorRect.Overlaps(node.CorridorRect))
            {
                corridorNode.CorridorRect.max = new Vector2Int(node.CorridorRect.min.x, corridorNode.CorridorRect.max.y);
            }
            CuttingCorridorRight(corridorNode, node.ChildLeft);
            CuttingCorridorRight(corridorNode, node.ChildRight);
        }

    }

    private void CuttingCorridorTop(Node corridorNode, Node node)
    {

        if (node.IsLeaf)
        {
            if (corridorNode.CorridorRect.Overlaps(node.RoomRect))
            {
                corridorNode.CorridorRect.min = new Vector2Int(corridorNode.CorridorRect.min.x, node.RoomRect.max.y);
            }
        }
        else
        {
            if (corridorNode.CorridorRect.Overlaps(node.CorridorRect))
            {
                corridorNode.CorridorRect.min = new Vector2Int(corridorNode.CorridorRect.min.x, node.CorridorRect.max.y);

            }
            CuttingCorridorTop(corridorNode, node.ChildLeft);
            CuttingCorridorTop(corridorNode, node.ChildRight);
        }

    }

    private void CuttingCorridorBottom(Node corridorNode, Node node)
    {

        if (node.IsLeaf)
        {
            if (corridorNode.CorridorRect.Overlaps(node.RoomRect))
            {
                corridorNode.CorridorRect.max = new Vector2Int(corridorNode.CorridorRect.max.x, node.RoomRect.min.y);
            }
        }
        else
        {
            if (corridorNode.CorridorRect.Overlaps(node.CorridorRect))
            {
                corridorNode.CorridorRect.max = new Vector2Int(corridorNode.CorridorRect.max.x, node.CorridorRect.min.y);
            }
            CuttingCorridorBottom(corridorNode, node.ChildLeft);
            CuttingCorridorBottom(corridorNode, node.ChildRight);
        }

    }

    private void UpdateLineRenderer(Node node)
    {

        var gridLineRenderer = node.Grid.GetComponent<LineRenderer>();
        
        var left                        = (float)node.GridRect.x / m_property.cellNumberX * m_property.mapSize.x;
        var right                       = (float)(node.GridRect.x + node.GridRect.width) / m_property.cellNumberX * m_property.mapSize.x;
        var top                         = (float)(node.GridRect.y + node.GridRect.height) / m_property.cellNumberY * m_property.mapSize.y;
        var bottom                      = (float)node.GridRect.y / m_property.cellNumberY * m_property.mapSize.y;
        gridLineRenderer.enabled        = false;
        gridLineRenderer.startColor     = Color.blue;
        gridLineRenderer.endColor       = Color.blue;
        gridLineRenderer.positionCount  = 4;
        gridLineRenderer.SetPosition(0, new Vector2(left,   top) - m_property.mapPivot);
        gridLineRenderer.SetPosition(1, new Vector2(right,  top) - m_property.mapPivot);
        gridLineRenderer.SetPosition(2, new Vector2(right,  bottom) - m_property.mapPivot);
        gridLineRenderer.SetPosition(3, new Vector2(left,   bottom) - m_property.mapPivot);
        
        
        //Room
        var roomLineRenderer = node.Room.GetComponent<LineRenderer>();
        
        left                            = (float)node.RoomRect.x / m_property.cellNumberX * m_property.mapSize.x;
        right                           = (float)(node.RoomRect.y + node.RoomRect.width) / m_property.cellNumberX * m_property.mapSize.x;
        top                             = (float)(node.RoomRect.y + node.RoomRect.height) / m_property.cellNumberY * m_property.mapSize.y;
        bottom                          = (float)node.RoomRect.y / m_property.cellNumberY * m_property.mapSize.y;
        roomLineRenderer.startColor     = Color.red;
        roomLineRenderer.endColor       = Color.red;
        roomLineRenderer.positionCount  = 4;
        roomLineRenderer.SetPosition(0, new Vector2(left,   top) - m_property.mapPivot);
        roomLineRenderer.SetPosition(1, new Vector2(right,  top) - m_property.mapPivot);
        roomLineRenderer.SetPosition(2, new Vector2(right,  bottom) - m_property.mapPivot);
        roomLineRenderer.SetPosition(3, new Vector2(left,   bottom) - m_property.mapPivot);
        
        //Corrior
        var corridorLineRenderer = node.Corridor.GetComponent<LineRenderer>();
        
        left                                = (float)node.CorridorRect.x / m_property.cellNumberX * m_property.mapSize.x;
        right                               = (float)(node.CorridorRect.x + node.CorridorRect.width) / m_property.cellNumberX * m_property.mapSize.x;
        top                                 = (float)(node.CorridorRect.y + node.CorridorRect.height) / m_property.cellNumberY * m_property.mapSize.y;
        bottom                              = (float)node.CorridorRect.y / m_property.cellNumberY * m_property.mapSize.y;
        corridorLineRenderer.startColor     = Color.red;
        corridorLineRenderer.endColor       = Color.red;
        corridorLineRenderer.positionCount  = 4;
        corridorLineRenderer.SetPosition(0, new Vector2(left,   top) - m_property.mapPivot);
        corridorLineRenderer.SetPosition(1, new Vector2(right,  top) - m_property.mapPivot);
        corridorLineRenderer.SetPosition(2, new Vector2(right,  bottom) - m_property.mapPivot);
        corridorLineRenderer.SetPosition(3, new Vector2(left,   bottom) - m_property.mapPivot);
        

    }

    public MAP_GENERATION_PROPERTY m_property;

    public Node[] m_node_array;

}
