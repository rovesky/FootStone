using System;
using System.Collections.Generic;
using System.Text;

namespace TestAOI
{
    // 双链表（对象）
    class DoubleNode
    {
        public DoubleNode(string key, int x, int y)
        {
            this.key = key;
            this.x = x;
            this.y = y;
            xPrev = xNext = null;
        }

        public DoubleNode xPrev;
        public DoubleNode xNext;

        public DoubleNode yPrev;
        public DoubleNode yNext;

        public string key;
        public int x; // 位置（x坐标）
        public int y; // 位置（y坐标）

        public Dictionary<string, DoubleNode> observers = new Dictionary<string, DoubleNode>();
    }




    // 地图/场景
    class Scene
    {
        public Scene(int xMap,int yMap)
        {
            this.xMap = xMap;
            this.yMap = yMap;
            //this._map = new int[xMap, yMap];

            this._head = new DoubleNode("[head]", 0, 0); // 带头尾的双链表(可优化去掉头尾)
            this._tail = new DoubleNode("[tail]", 0, 0);
            _head.xNext = _tail;
            _head.yNext = _tail;
            _tail.xPrev = _head;
            _tail.yPrev = _head;
        }

        // 对象加入(新增)
        public DoubleNode Add(string name, int x, int y)
        {

            DoubleNode node = new DoubleNode(name, x, y);
            _add(node);
            return node;
        }

        // 对象离开(删除)
        public void Leave(DoubleNode node)
        {
            node.xPrev.xNext = node.xNext;
            node.xNext.xPrev = node.xPrev;
            node.yPrev.yNext = node.yNext;
            node.yNext.yPrev = node.yPrev;

            node.xPrev = null;
            node.xNext = null;
            node.yPrev = null;
            node.yNext = null;
        }

        // 对象移动
        public void Move(DoubleNode node, int dx, int dy)
        {
            Leave(node);
            node.x += dx;
            if (node.x < 0)
                node.x = 0;
            if (node.x > this.xMap - 1)
                node.x = this.xMap - 1;
            node.y += dy;
            if (node.y < 0)
                node.y = 0;
            if (node.y > this.yMap - 1)
                node.y = this.yMap - 1;
            _add(node);
        }

        // 获取范围内的AOI (参数为查找范围)
        public List<DoubleNode> RefreshAOI(DoubleNode node, int xAreaLen, int yAreaLen)
        {
            List<DoubleNode> ret = new List<DoubleNode>();

            // 往后找
            DoubleNode cur = node.xNext;
            while (cur != _tail)
            {
       
                if ((cur.x - node.x) > xAreaLen)
                {
                    break;
                }
                else
                {
                    int inteval = 0;
                    inteval = node.y - cur.y;
                    if (inteval >= -yAreaLen && inteval <= yAreaLen)
                    {
                        ret.Add(cur);
                        cur.observers.Add(node.key, node);
                    }
                }
                cur = cur.xNext;
            }

            // 往前找
            cur = node.xPrev;
            while (cur != _head)
            {
                if ((node.x - cur.x) > xAreaLen)
                {
                    break;
                }
                else
                {
                    int inteval = 0;
                    inteval = node.y - cur.y;
                    if (inteval >= -yAreaLen && inteval <= yAreaLen)
                    {
                        ret.Add(cur);
                        cur.observers.Add(node.key, node);
                    }
                }            
                cur = cur.xPrev;
            }
            return ret;
          }

        // 获取范围内的AOI (参数为查找范围)
        public void PrintAOI(DoubleNode node, int xAreaLen, int yAreaLen)
        {
            int searchCount = 0;
            int entityCount = 0;
            Console.Out.Write("Cur is: " + node.key + "（" + node.x + "," + node.y + ")\n");
            Console.Out.Write("Print AOI:\n");

            // 往后找
            DoubleNode cur = node.xNext;
            while (cur != _tail)
            {
                searchCount++;
                if ((cur.x - node.x) > xAreaLen)
                {
                    break;
                }
                else
                {
                    int inteval = 0;
                    inteval = node.y - cur.y;
                    if (inteval >= -yAreaLen && inteval <= yAreaLen)
                    {
                        entityCount++;
                        Console.Out.Write("\t" + cur.key + "(" + cur.x + "," + cur.y + ")\n");
                    }
                }
                cur = cur.xNext;
            }

            // 往前找
            cur = node.xPrev;
            while (cur != _head)
            {
                if ((node.x - cur.x) > xAreaLen)
                {
                    break;
                }
                else
                {
                    int inteval = 0;
                    inteval = node.y - cur.y;
                    if (inteval >= -yAreaLen && inteval <= yAreaLen)
                    {
                        entityCount++;
                        Console.Out.Write("\t" + cur.key + "(" + cur.x + "," + cur.y + ")\n" );
                    }
                }
                searchCount++;
                cur = cur.xPrev;
            }
            Console.Out.Write("\t" +"AOI search count:"+searchCount +",entity count:"+entityCount);
        }
        public void PrintMap()
        {
            this._map = new int[xMap, yMap];
            // 打印x轴链表
            DoubleNode cur = _head.xNext;
            while (cur != _tail)
            {
                _map[cur.x, cur.y]++;
                cur = cur.xNext;
            }
            Console.Out.Write("\n   ");
            for (int j = 0; j < xMap; ++j)
            {
                Console.Out.Write(" " + string.Format("{0:00}", j));
            }
            Console.Out.Write("\n");
            for (int y = 0; y < yMap; ++y)
            {
                Console.Out.Write(string.Format("{0:00}", y) + "-");
                for (int x = 0; x < xMap; ++x)
                {
                    Console.Out.Write("  "+_map[x,y]);
                }
                Console.Out.Write("\n");
            }
        }

        // 调试代码
        public void PrintLink()  // 打印链表(从头开始)
        {
            // 打印x轴链表
            DoubleNode cur = _head.xNext;
            while (cur != _tail)
            {             
                Console.Out.Write(cur.key + "(" + (cur.x) + "," + (cur.y) + ") . ");
                cur = cur.xNext;
            }
            Console.Out.Write("end\n");

            // 打印y轴链表
            cur = _head.yNext;
            while (cur != _tail)
            {
                Console.Out.Write(cur.key + "(" + cur.x + "," + cur.y + ") . ");
                cur = cur.yNext;
            }
            Console.Out.Write("end\n");
        }

        private DoubleNode _head;
        private DoubleNode _tail;
        private int[,] _map;
        private int xMap;
        private int yMap;

        private void _add(DoubleNode node)
        {
            // x轴处理
            DoubleNode cur = _head.xNext;
            while (cur != null)
            {
                if ((cur.x > node.x) || cur == _tail) // 插入数据
                {
                    node.xNext = cur;
                    node.xPrev = cur.xPrev;
                    cur.xPrev.xNext = node;
                    cur.xPrev = node;
                    break;
                }
                cur = cur.xNext;
            }

            // y轴处理
            cur = _head.yNext;
            while (cur != null)
            {
                if ((cur.y > node.y) || cur == _tail) // 插入数据
                {
                    node.yNext = cur;
                    node.yPrev = cur.yPrev;
                    cur.yPrev.yNext = node;
                    cur.yPrev = node;
                    break;
                }
                cur = cur.yNext;
            }
        }
    }

}
