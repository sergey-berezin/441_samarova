using System;
using System.Collections.Generic;
using System.Text;

namespace YOLOv4MLNet
{
    class resultInfo
    {
        static object result;
        static List <string> classes;
        
        public resultInfo(object new_result, List<string> new_classes)
        {
            result = new_result;
            classes = new_classes;
        }
        public static void printResult()
        {
            Console.WriteLine("Classes:");
            foreach (var item in classes)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine();
        }
    }
}
