using System;
using System.Collections.Generic;
using System.Text;

namespace LibTask1Core
{
    public class ResultInfo
    {
        public string imageName;
        public object result;
        public List<string> classes;

        public ResultInfo(object new_result, List<string> new_classes, string new_imageName)
        {
            this.result = new_result;
            this.classes = new_classes;
            this.classes.Sort();
            this.imageName = new_imageName;
        }


        public void printResult()
        {
            Console.WriteLine("Classes:");
            foreach (var item in classes)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine();
        }

        public string toString()
        {
            string str = "";
            foreach (var item in classes)
            {
                str += item + "\n";
            }
            return str;
        }

        //public bool CompareClasses(List<string> curClasses)
        public bool CompareClasses(string curClass)
        {
            for (int i = 0; i < this.classes.Count; i++)
            {
                if (!Convert.ToBoolean(string.Compare(this.classes[i], curClass)))
                    return true;
            }
            return false;
        }
    }
}
