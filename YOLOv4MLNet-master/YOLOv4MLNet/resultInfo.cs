using System;
using System.Collections.Generic;
using System.Text;

namespace YOLOv4MLNet
{
    class ResultInfo
    {
        public object result;
        public List <string> classes;
        
        public ResultInfo(object new_result, List<string> new_classes)
        {
            result = new_result;
            classes = new_classes;
        }


        public  void printResult()
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
