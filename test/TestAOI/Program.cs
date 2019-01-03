using System;
using System.Collections.Generic;
using System.Threading;
using System.Timers;

namespace TestAOI
{
    class Program
    {

        static List<DoubleNode> nodes = new List<DoubleNode>();
        static Scene scene;
        static Random rd = new Random();
        static int index = 0;
        static double totalTime = 0;
        static void Main(string[] args)
        {
          
            int xMap = 50;
            int yMap = 50;
            int entityCount = 200;
            scene = new Scene(xMap, yMap);
            scene.PrintMap();
            // 增加
            DoubleNode node = scene.Add("node0", 3, 3);
            for (int i = 1; i < entityCount; ++i)
            {
                nodes.Add(scene.Add("node"+i, rd.Next(0, xMap), rd.Next(0, yMap)));
            }

            int viewX = 1;
            int viewY = 1;
            //    scene.PrintLink();
            scene.PrintMap();
            scene.RefreshAOI(node, viewX, viewY);

            // 移动
            Console.Out.Write("\n[MOVE]\n");
            scene.Move(node, 62, -14);
            scene.PrintMap();
         //   scene.RefreshAOI(node, viewX, viewY);

            // 删除
            Console.Out.Write("\n[LEAVE]\n");
            scene.Leave(node);
            scene.PrintMap();

            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(onTimmer);
            aTimer.Interval = 33;
            aTimer.AutoReset = true;//执行一次 false，一直执行true  
            //是否执行System.Timers.Timer.Elapsed事件  
            aTimer.Enabled = true;
            Console.ReadLine();
             
        }

        private static void onTimmer(object source, System.Timers.ElapsedEventArgs e)
        {
           
            index++;
            var startTime = DateTime.Now;
            int i = 0;
            int moveCount = 0;
            foreach(DoubleNode node in nodes)
            {
                i++;
                if(i%10 == 0)
                {
                    int dx = rd.Next(-1, 2);
                    int dy = rd.Next(-1, 2);
                    if(dx != 0 && dy!= 0)
                    {
                        moveCount++;
                    }
                    scene.Move(node,dx,dy);
                }
                node.observers.Clear();
            }

            foreach (DoubleNode node in nodes)
            {
                scene.RefreshAOI(node, 2, 2);           
            }


            if (index % 1000000 == 0)
            {
                Console.WriteLine("------------------------------------------------------------");
                foreach (DoubleNode node in nodes)
                {
                    Console.Write(node.key + "(" + node.x + "," + node.y + "):");
                    foreach (DoubleNode ob in node.observers.Values)
                    {
                        Console.WriteLine("ob:" + ob.key + "(" + ob.x + "," + ob.y + "),");
                    }
                    Console.Write("\n");
                }

                scene.PrintMap();
            }


            //   Thread.Sleep(300);
            var curTime = (DateTime.Now - startTime).TotalMilliseconds;
            totalTime += curTime;
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId + " use time:"+ curTime + ",avg time:"+ (totalTime/index)+ ",moveCount:"+ moveCount);
        }

    }
}
