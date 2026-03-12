using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ConsoleApp2
{
    class Program
    {
        static int nextb;
        static int nextt;
        static string logicalqterm = " ";
        static int numberinway = 0;
        static bool proxod = true;
        static int iterations = 0;

        static public Dictionary<string, int> paramrazm = new Dictionary<string, int>();//Хранятся параметры размерности и их введенные значения
        static public ArrayList indata = new ArrayList();//Хранятся входные переменные
        static public Dictionary<string, string> outdata = new Dictionary<string, string>();//Хранятся выходные переменные и их значения в виде выражений
        static public Dictionary<string, string> vnutrperem = new Dictionary<string, string>();//Хранятся внутренние переменные блок-схемы и их значения в виде выражений
        static public Dictionary<int, string> way = new Dictionary<int, string>();//Хранится последний путь прохождения по цепочке условий, зависящих от входных данных
     
        static void Main(string[] args)
        {
            string path = "BS.txt";
            string countiesJson;
            vnutrperem.Add("empout", "1");
            
            using (StreamReader streamReader = new StreamReader(path, Encoding.UTF8))
            {
                countiesJson = streamReader.ReadToEnd();
            }
            
            List<string> countie1 = new List<string>();          
            List<string> countie2 = new List<string>();
            JSONParseDynamic2(countiesJson);//извлечение из блок-схемы параметров размерности и ввод их значений            
            List<string> countie3 = new List<string>();
            JSONParseDynamic3(countiesJson);//извлечение из блок-схемы входных переменных и размещение их в коллекции
            List<string> countie4 = new List<string>();
            JSONParseDynamic4(countiesJson);//извлечение из блок-схемы выходных переменных и размещение их в коллекции
            List<string> countie5 = new List<string>();
            while (proxod == true)
            {                
                vnutrperem["empout"] = "1";                
                JSONParseDynamic5(countiesJson);//проход по блок-схеме от начала до конца по всем путям               
                if (numberinway > 0)
                {
                    while (way[numberinway].IndexOf(",0") > 0)
                    {
                        way.Remove(numberinway);
                        numberinway = numberinway - 1;
                        if (numberinway == 0) break;
                    }
                }
                if (numberinway > 0)
                {
                    if (way[numberinway].IndexOf(",1") > 0)
                    {
                        way[numberinway] = way[numberinway].Substring(0, way[numberinway].IndexOf(",1") + 1) + "0";                        
                    }
                }
                numberinway = 0;
                logicalqterm = " ";
                if (way.Count == 0) proxod = false;
            }
            List<string> JSONParseDynamic2(string jsonText)//извлечение из блок-схемы параметров размерности и ввод их значений
            {
                dynamic jResults = JsonConvert.DeserializeObject(jsonText);

                foreach (var ver in jResults.Vertices)
                {
                    if (ver.Type == 4)
                    {
                        countie2.Add((string)ver.Content);
                    }

                }
                
                foreach (string inperem in countie2)
                {
                    int indexOfCharOpen = inperem.IndexOf("[");
                    int indexOfCharClose = inperem.IndexOf("]");
                    if (indexOfCharOpen >= 0)
                    {
                        string param = inperem.Substring(indexOfCharOpen + 1, indexOfCharClose - indexOfCharOpen - 1);
                        if (param.IndexOf(",") < 0)//блок-схема имеет один параметр размерности
                        {
                            try
                            {
                                bool b = false;
                                foreach (string c in paramrazm.Keys)
                                {
                                    if (c == param) { b = true; };
                                }
                                if (b == false)
                                {
                                    Console.WriteLine($"Введите значение параметра размерности {param}:");
                                    int paramValue = Int32.Parse(Console.ReadLine());
                                    paramrazm.Add(param, paramValue);//сохранение параметра размерности и его значения в словаре
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.ReadLine();
                            }
                        }
                            if (param.IndexOf(",") > 0)//блок-схема имеет несколько параметров размерности
                        {
                            string[] masparam = param.Split(new char[] { ',' });

                            foreach (string s in masparam)
                            {
                                try
                                {
                                    bool b = false;
                                    foreach (string c in paramrazm.Keys)
                                    {
                                        if (c == s) { b = true; };
                                    }
                                    if (b == false)
                                    {
                                        Console.WriteLine($"Введите значение параметра размерности {s}:");
                                        int sValue = Int32.Parse(Console.ReadLine());
                                        paramrazm.Add(s, sValue);//сохранение параметра размерности и его значения в словаре
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                    Console.ReadLine();
                                }
                            }
                        }
                    }
                }
                return countie2;

            }
            List<string> JSONParseDynamic3(string jsonText)//извлечение из блок-схемы входных переменных и размещение их в коллекции
            {
                dynamic jResults = JsonConvert.DeserializeObject(jsonText);

                foreach (var ver in jResults.Vertices)
                {
                    if (ver.Type == 4)
                    {

                        countie3.Add((string)ver.Content);
                    }
                }
                foreach (string inper in countie3)
                {
                    int indexOfCharOpen = inper.IndexOf("[");
                    int indexOfCharClose = inper.IndexOf("]");
                    if (indexOfCharOpen < 0) indata.Add(inper);
                    if ((indexOfCharOpen > 0)&(inper.IndexOf(",") < 0))
                    {
                        foreach (string c in paramrazm.Keys)
                        {
                            if (c == inper.Substring(indexOfCharOpen + 1, indexOfCharClose - indexOfCharOpen - 1))
                            {
                                for (int r = 1; r <= paramrazm[c]; r++)
                                {
                                    string inp = inper.Substring(0, indexOfCharOpen) + "(" + r + ")";
                                    indata.Add(inp);
                                }
                            }
                        }
                         
                    }
                    if ((indexOfCharOpen > 0) & (inper.IndexOf(",") > 0))
                    {
                        
                        string outper = inper.Substring(0, indexOfCharOpen);//обозначение входной переменной                        
                        string param = inper.Substring(indexOfCharOpen + 1, indexOfCharClose - indexOfCharOpen - 1);
                        string[] masparam = param.Split(new char[] { ',' });
                        int i = 0;
                        int[] indexoutdata = new int[10];
                        foreach (string s in masparam)
                        {                            
                            foreach (string c in paramrazm.Keys)
                            {
                                if (c == s)
                                {
                                    indexoutdata[i] = paramrazm[c];
                                    i++;                                    
                                }
                            }
                        }
                        i--;
                        int[] indexout = new int[10] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };//вспомогательный массив для перебора значений массива indexoutdata
                        string qterm;
                        bool endpereb;
                        do
                        {
                            qterm = outper;
                            string indexqterm = "(" + indexout[0];
                            for (int j = 1; j <= i; j++)
                            {
                                indexqterm = indexqterm + "," + indexout[j];
                            }
                            qterm = qterm + indexqterm + ")";
                            indata.Add(qterm);

                            endpereb = true;
                            for (int r = 0; r <= i; r++)
                            {
                                endpereb = endpereb & (indexout[r] == indexoutdata[r]);//условие окончания
                            }
                            
                            if (endpereb == true) break;//все наборы индексов для выходной переменной обработаны
                            for (int k = i; k >= 0; k--)
                            {
                                if (indexout[k] < indexoutdata[k])
                                {
                                    indexout[k]++;                                    
                                    for (int r = k + 1; r < 10; r++) indexout[r] = 1;                                    
                                    break;
                                }
                            }
                        }
                        while (endpereb == false);                                         
                    }                  
                }
                
                foreach (object o in indata)
                {
                    if ((string)o == "iterations")
                    {
                        Console.WriteLine("Введите количество итераций:");
                        iterations = Int32.Parse(Console.ReadLine());
                    }
                }                
                return countie3;
            }

            List<string> JSONParseDynamic4(string jsonText)//извлечение из блок-схемы выходных переменных и размещение их в коллекции
            {
                dynamic jResults = JsonConvert.DeserializeObject(jsonText);

                foreach (var ver in jResults.Vertices)
                {
                    if (ver.Type == 5)
                    {
                        countie4.Add((string)ver.Content);
                    }
                }
                foreach (string inperem in countie4)
                {
                    int indexOfCharOpen = inperem.IndexOf("[");
                    int indexOfCharClose = inperem.IndexOf("]");
                    if (indexOfCharOpen < 0) outdata.Add(inperem, "0");//сохранение выходной переменной и ее значения в словаре outdata, когда алгоритм не имеет размерности
                    if ((indexOfCharOpen > 0) & (inperem.IndexOf(",") < 0))
                    {
                        foreach (string c in paramrazm.Keys)
                        {
                            if (c == inperem.Substring(indexOfCharOpen + 1, indexOfCharClose - indexOfCharOpen - 1))
                            {
                                for (int r = 1; r <= paramrazm[c]; r++)
                                {
                                    string inp = inperem.Substring(0, indexOfCharOpen) + "(" + r + ")";
                                    outdata.Add(inp, "0");
                                }
                            }
                        }
                    }
                    if ((indexOfCharOpen > 0) & (inperem.IndexOf(",") > 0))
                    {
                        string outper = inperem.Substring(0, indexOfCharOpen);//обозначение выходной переменной                       
                        string param = inperem.Substring(indexOfCharOpen + 1, indexOfCharClose - indexOfCharOpen - 1);
                        string[] masparam = param.Split(new char[] { ',' });
                        int i = 0;
                        int[] indexoutdata = new int[10];
                        foreach (string s in masparam)
                        {                            
                            foreach (string c in paramrazm.Keys)
                            {
                                if (c == s)
                                {
                                    indexoutdata[i] = paramrazm[c];
                                    i++;                                    
                                }
                            }
                        }
                        i--;
                        int[] indexout = new int[10] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };//вспомогательный массив для перебора значений массива indexoutdata
                        string qterm;
                        bool endpereb;
                        do
                        {
                            qterm = outper;
                            string indexqterm = "(" + indexout[0];
                            for (int j = 1; j <= i; j++)
                            {
                                indexqterm = indexqterm + "," + indexout[j];
                            }
                            qterm = qterm + indexqterm + ")";
                            outdata.Add(qterm, "0");
                            endpereb = true;
                            for (int r = 0; r <= i; r++)
                            {
                                endpereb = endpereb & (indexout[r] == indexoutdata[r]);//условие окончания
                            }                            
                            if (endpereb == true) break;//все наборы индексов для выходной переменной обработаны
                            for (int k = i; k >= 0; k--)
                            {
                                if (indexout[k] < indexoutdata[k])
                                {
                                    indexout[k]++;                                   
                                    for (int r = k + 1; r < 10; r++) indexout[r] = 1;                                    
                                    break;
                                }
                            }
                        }
                        while (endpereb == false);
                    }                 
                }
                return countie4;
            }
            List<string> JSONParseDynamic5(string jsonText)//проход по блок-схеме от начала до конца по всем путям
            {
                dynamic jResults = JsonConvert.DeserializeObject(jsonText);
                int maxnumberbl = 0;//хранит количество блоков в блок-схеме
                nextb = 0;//инициализация переменной для хранения номера текущего блока
                foreach (var ver in jResults.Vertices)
                {
                    if (ver.Type == 0)
                    {                        
                        nextb = ver.Id;
                    }
                    if (ver.Id > maxnumberbl) { maxnumberbl = ver.Id; }
                }                
                if (nextb == 0)
                {
                    System.Console.WriteLine("Блок Start не cуществует.");
                }                
                while (nextb < maxnumberbl)
                { 
                foreach (var ver in jResults.Vertices)
                {
                    if (ver.Id == nextb)
                    {
                            if (ver.Type == 2)
                            {                                
                                string content = ver.Content;                                
                                int assignmentcharacter = content.IndexOf("=");
                                string leftcontent = content.Substring(0, assignmentcharacter);                                
                                if (leftcontent.IndexOf("[") < 0)
                                {                                    
                                    if (leftcontent.IndexOf("(") > 0)
                                    {
                                        int leftbracket = leftcontent.IndexOf("(");
                                        string leftcontentname = leftcontent.Substring(0, leftbracket);//имя переменной слева от скобки (                                        
                                        string leftotprisv = leftcontentname + "(";
                                        string vnutriskobok = leftcontent.Substring(leftcontent.IndexOf("(") + 1, leftcontent.IndexOf(")") - leftcontent.IndexOf("(") - 1);
                                        if (leftcontent.IndexOf(",") > 0)//несколько индексов
                                        {
                                            string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                            foreach (string s in masindex)
                                            {                                                
                                                foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов переменной слева от знака присваивания и определяются их значения
                                                {
                                                    if (c == s)
                                                    {
                                                        leftotprisv = leftotprisv + vnutrperem[s] + ",";                                                        
                                                    }
                                                }
                                            }
                                            leftotprisv = leftotprisv.Substring(0, leftotprisv.Length - 1) + ")";                                            
                                        }
                                        if (leftcontent.IndexOf(",") < 0)//один индекс
                                        {
                                            foreach (string c in vnutrperem.Keys)//ищется внутренняя переменная для индекса переменной слева от знака присваивания и определяется ее значение
                                            {
                                                if (c == vnutriskobok)
                                                {
                                                    leftotprisv = leftotprisv + vnutrperem[vnutriskobok];                                                    
                                                }
                                            }
                                            leftotprisv = leftotprisv.Substring(0, leftotprisv.Length) + ")";                                                                                        
                                        }
                                        leftcontent = leftotprisv;                                        
                                    }
                                    bool outp = false;
                                    foreach (string c in outdata.Keys)//проверяется, находится ли слева от знака присваивания выходная переменная
                                    {
                                        if (c == leftcontent) { outp = true; };
                                    }
                                    if (outp == true) //слева от знака присваивания выходная переменная
                                    {                                        
                                        string rightcontent = content.Substring(assignmentcharacter + 1, content.Length - assignmentcharacter - 1);                                        
                                        if ((rightcontent.IndexOf("+") < 0) & (rightcontent.IndexOf("-") < 0) & (rightcontent.IndexOf("*") < 0) & (rightcontent.IndexOf("/") < 0))
                                        {                                            
                                            if ((rightcontent.Substring(0, 1) == "0") || (rightcontent.Substring(0, 1) == "1") || (rightcontent.Substring(0, 1) == "2") || (rightcontent.Substring(0, 1) == "3") || (rightcontent.Substring(0, 1) == "4") || (rightcontent.Substring(0, 1) == "5") || (rightcontent.Substring(0, 1) == "6") ||
                                               (rightcontent.Substring(0, 1) == "7") || (rightcontent.Substring(0, 1) == "8") || (rightcontent.Substring(0, 1) == "9"))
                                            {
                                                outdata[leftcontent] = rightcontent;                                               
                                            }                                            
                                            bool rightcontentinp = false;
                                            foreach (string c in indata)//проверяется, справа от присваивания входная переменная?
                                            {
                                                if (c == rightcontent) { rightcontentinp = true; };
                                            }
                                            if (rightcontentinp == true)
                                            {                                                
                                                outdata[leftcontent] = rightcontent;                                                
                                            }
                                            
                                            if ((rightcontent.IndexOf("(") > 0) & (rightcontent.IndexOf(",") < 0))
                                            {
                                                string rightc = rightcontent.Substring(rightcontent.IndexOf("(") + 1, rightcontent.IndexOf(")") - rightcontent.IndexOf("(") - 1);
                                                
                                                if ((rightc.Substring(0, 1) == "0") || (rightc.Substring(0, 1) == "1") || (rightc.Substring(0, 1) == "2") || (rightc.Substring(0, 1) == "3") || (rightc.Substring(0, 1) == "4") || (rightc.Substring(0, 1) == "5") || (rightc.Substring(0, 1) == "6") ||
                                               (rightc.Substring(0, 1) == "7") || (rightc.Substring(0, 1) == "8") || (rightc.Substring(0, 1) == "9"))
                                                {
                                                    foreach (string c in vnutrperem.Keys)
                                                    {
                                                        //if (c == vnutrperem[rightcontent])
                                                        if (c == rightcontent)
                                                        {                                                            
                                                            outdata[leftcontent] = vnutrperem[rightcontent];                                                            
                                                        }
                                                    }                                                    
                                                }
                                                else
                                                {
                                                    foreach (string c in indata)
                                                    {
                                                        if (c == (rightcontent.Substring(0, rightcontent.IndexOf("(")) + "(" + vnutrperem[rightc] + ")"))
                                                        {
                                                            outdata[leftcontent] = rightcontent.Substring(0, rightcontent.IndexOf("(")) + "(" + vnutrperem[rightc] + ")";                                                            
                                                        }
                                                    }
                                                    foreach (string c in vnutrperem.Keys)
                                                    {
                                                        if (c == (rightcontent.Substring(0, rightcontent.IndexOf("(")) + "(" + vnutrperem[rightc] + ")"))
                                                        {
                                                            outdata[leftcontent] = vnutrperem[rightcontent.Substring(0, rightcontent.IndexOf("(")) + "(" + vnutrperem[rightc] + ")"];                                                            
                                                        }
                                                    }
                                                }                                                
                                            }
                                            
                                            if ((rightcontent.IndexOf("(") > 0) & (rightcontent.IndexOf(",") > 0))//справа переменная с нсколькими индексами
                                            {
                                                int leftbracket = rightcontent.IndexOf("(");
                                                string rightcontentname = rightcontent.Substring(0, leftbracket);//имя переменной справа от знака присваивания                                                
                                                string rightcontentv = rightcontentname + "(";
                                                string vnutriskobok = rightcontent.Substring(rightcontent.IndexOf("(") + 1, rightcontent.IndexOf(")") - rightcontent.IndexOf("(") - 1);
                                                string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                foreach (string s in masindex)
                                                {                                                    
                                                    foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов переменной справа от знака присваивания и определяются их значения
                                                    {
                                                        if (c == s)
                                                        {
                                                            rightcontentv = rightcontentv + vnutrperem[s] + ",";                                                            
                                                        }
                                                    }
                                                    foreach (string c in paramrazm.Keys)//ищутся внутренние переменные для индексов переменной справа от знака присваивания и определяются их значения
                                                    {
                                                        if (c == s)
                                                        {
                                                            rightcontentv = rightcontentv + paramrazm[s] + ",";                                                            
                                                        }
                                                    }
                                                }
                                                rightcontentv = rightcontentv.Substring(0, rightcontentv.Length - 1) + ")";                                                
                                                bool rcv = false;
                                                foreach (string c in vnutrperem.Keys)//определяется, является ли значение справа от знака присваивания внутренней переменной
                                                {
                                                    if (c == rightcontentv)
                                                    {
                                                        rcv = true;
                                                    }
                                                }
                                                if (rcv == true)
                                                {
                                                    outdata[leftcontent] = vnutrperem[rightcontentv];//изменено значение выходной переменной
                                                }                                                
                                                bool rcinp = false;
                                                foreach (string c in indata)//определяется, является ли значение справа от знака присваивания входной переменной
                                                {
                                                    if (c == rightcontentv)
                                                    {
                                                        rcinp = true;
                                                    }
                                                }
                                                if (rcinp == true)
                                                {
                                                    outdata[leftcontent] = rightcontentv;//изменено значение выходной переменной
                                                }                                                                                                
                                            }                                            
                                        }
                                        if ((rightcontent.IndexOf("+") > 0))
                                        {
                                            int locationoperation = rightcontent.IndexOf("+");
                                            string leftrightcontent = rightcontent.Substring(0, locationoperation);                                            
                                            string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                            
                                            if ((leftrightcontent.IndexOf("(") > 0))
                                            {
                                                int leftbracket = leftrightcontent.IndexOf("(");
                                                string leftrightcontentname = leftrightcontent.Substring(0, leftbracket);//Имя переменной слева от операции сложения                                                
                                                string leftsomnoj = leftrightcontentname + "(";
                                                string vnutriskobok = leftrightcontent.Substring(leftrightcontent.IndexOf("(") + 1, leftrightcontent.IndexOf(")") - leftrightcontent.IndexOf("(") - 1);
                                                string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                foreach (string s in masindex)
                                                {                                                    
                                                    foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов первого слагаемого и определяются их значения
                                                    {
                                                        if (c == s)
                                                        {
                                                            leftsomnoj = leftsomnoj + vnutrperem[s] + ",";                                                            
                                                        }
                                                    }
                                                }
                                                leftsomnoj = leftsomnoj.Substring(0, leftsomnoj.Length - 1) + ")";                                                
                                                if (outdata[leftsomnoj] != "0")
                                                {
                                                    //string leftvalue = "{" + "\"op\":\"+\",\"fO\":" + $"{outdata[leftsomnoj]},";                                                    
                                                    string leftvalue = " ";
                                                    if ((outdata[leftsomnoj].IndexOf("{") >= 0))
                                                    {
                                                        leftvalue = "{" + "\"op\":\"+\",\"fO\":" + $"{outdata[leftsomnoj]},";
                                                    }
                                                    else
                                                    {
                                                        leftvalue = "{" + "\"op\":\"+\",\"fO\":" + $"\"{outdata[leftsomnoj]}\",";
                                                    }                                                    
                                                    outdata[leftcontent] = leftvalue;//изменение значения выходной переменной                                                    
                                                }                                                
                                            }                                            
                                            if ((leftrightcontent.IndexOf("(") < 0))
                                            {                                                                                               
                                                bool leftrightcontentoutp = false;
                                                foreach (string c in outdata.Keys)//проверяется, находится ли слева от операции сложения выходная переменная
                                                {
                                                    if (c == leftrightcontent)
                                                    { leftrightcontentoutp = true; };
                                                }
                                                if (leftrightcontentoutp == true)
                                                {                                                                                                         

                                                        if ((outdata[leftrightcontent].IndexOf("{") >= 0))
                                                        {
                                                            outdata[leftcontent] = "{" + "\"op\":\"+\",\"fO\":" + $"{outdata[leftrightcontent]},";
                                                    }
                                                        else
                                                        {
                                                            outdata[leftcontent] = "{" + "\"op\":\"+\",\"fO\":" + $"\"{outdata[leftrightcontent]}\",";
                                                    }                                                                                                                                                                                                                       
                                                }
                                            }                                            
                                            if ((rightrightcontent.IndexOf("(") < 0))
                                            {                                                
                                                bool rightrightcontentvp = false;
                                                foreach (string c in vnutrperem.Keys)//проверяется, находится ли справа от операции сложения внутренняя переменная
                                                {
                                                    if (c == rightrightcontent)
                                                    { rightrightcontentvp = true; };
                                                }
                                                if (rightrightcontentvp == true)
                                                {
                                                    if (outdata[leftcontent].Substring(0, 1) == "{")//избавляемся от сложения с "0" при вычислении Cij
                                                {                                                    
                                                    if ((vnutrperem[rightrightcontent].IndexOf("{") >= 0))
                                                    {
                                                        outdata[leftcontent] = outdata[leftcontent] + $"\"sO\":{vnutrperem[rightrightcontent]}" + "}";
                                                    }
                                                    else
                                                    {
                                                        outdata[leftcontent] = outdata[leftcontent] + $"\"sO\":\"{vnutrperem[rightrightcontent]}\"" + "}";
                                                    }                                                    
                                                }
                                                else
                                                {
                                                    outdata[leftcontent] = vnutrperem[rightrightcontent];//изменение значения выходной переменной                                                    
                                                }                                    
                                                }
                                            }                                            
                                            if ((rightrightcontent.IndexOf("(") > 0))
                                            {
                                                int leftbracketr = rightrightcontent.IndexOf("(");
                                                string rightrightcontentname = rightrightcontent.Substring(0, leftbracketr);//имя переменной слева от скобки (                                                
                                                string vtoroeslagaemoe = rightrightcontentname + "(";
                                                string vnutriskobokr = rightrightcontent.Substring(rightrightcontent.IndexOf("(") + 1, rightrightcontent.IndexOf(")") - rightrightcontent.IndexOf("(") - 1);
                                                if (rightrightcontent.IndexOf(",") > 0)//несколько индексов
                                                {
                                                    string[] masindex = vnutriskobokr.Split(new char[] { ',' });
                                                    foreach (string s in masindex)
                                                    {                                                    
                                                        foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов второго слагаемого и определяются их значения
                                                        {
                                                            if (c == s)
                                                            {
                                                                vtoroeslagaemoe = vtoroeslagaemoe + vnutrperem[s] + ",";                                                                
                                                            }
                                                        }
                                                    }
                                                vtoroeslagaemoe = vtoroeslagaemoe.Substring(0, vtoroeslagaemoe.Length - 1) + ")";                                                
                                                }
                                                bool rightrightcontentinp = false;
                                                foreach (string c in indata)//проверяется, является ли второе слагаемое входной переменной
                                                {
                                                    if (c == vtoroeslagaemoe) { rightrightcontentinp = true; };
                                                }
                                                if (rightrightcontentinp == true)
                                                {                                                    
                                                    if (outdata[leftcontent].Substring(0, 1) == "{")
                                                    {                                                        
                                                        outdata[leftcontent] = outdata[leftcontent] + $"\"sO\":\"{vtoroeslagaemoe}\"" + "}";                                                        
                                                    }
                                                    else
                                                    {
                                                        outdata[leftcontent] = vtoroeslagaemoe;                                                        
                                                    }
                                                }
                                            }                                               
                                        }                                        
                                        if ((rightcontent.IndexOf("-") > 0))
                                        {
                                            int locationoperation = rightcontent.IndexOf("-");
                                            string leftrightcontent = rightcontent.Substring(0, locationoperation);                                            
                                            string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                            
                                            string leftrightcontentvalue = " ";
                                            if ((leftrightcontent.IndexOf("(") < 0))//уменьшаемое
                                            {                                                     
                                                if ((leftrightcontent.Substring(0, 1) == "0") || (leftrightcontent.Substring(0, 1) == "1") || (leftrightcontent.Substring(0, 1) == "2") || (leftrightcontent.Substring(0, 1) == "3") || (leftrightcontent.Substring(0, 1) == "4") || (leftrightcontent.Substring(0, 1) == "5") || (leftrightcontent.Substring(0, 1) == "6") ||
                                                   (leftrightcontent.Substring(0, 1) == "7") || (leftrightcontent.Substring(0, 1) == "8") || (leftrightcontent.Substring(0, 1) == "9"))
                                                {                                                    
                                                    leftrightcontentvalue = leftrightcontent;
                                                }                                               
                                            }
                                            if ((rightrightcontent.IndexOf("(") < 0))//вычитаемое
                                            {                                                
                                                bool rightrightcontentvp = false;
                                                foreach (string c in vnutrperem.Keys)//проверяется, находится ли справа от операции вычитания внутренняя переменная
                                                {
                                                    if (c == rightrightcontent)
                                                    { rightrightcontentvp = true; };
                                                }
                                                if (rightrightcontentvp == true)
                                                {
                                                    if ((vnutrperem[rightrightcontent].IndexOf("{")) >= 0)
                                                    {
                                                        outdata[leftcontent] = "{" + "\"op\":\"-\",\"fO\":" + $"\"{leftrightcontentvalue}\"" + ",\"sO\":" + $"{vnutrperem[rightrightcontent]}" + "}";
                                                    }
                                                    else
                                                    {
                                                        outdata[leftcontent] = "{" + "\"op\":\"-\",\"fO\":" + $"\"{leftrightcontentvalue}\"" + ",\"sO\":" + $"\"{vnutrperem[rightrightcontent]}\"" + "}";
                                                    }                                                    
                                                }                                                
                                            }
                                        }                                        
                                        if ((rightcontent.IndexOf("-") == 0))//операция унарный минус
                                        {
                                            int locationoperation = rightcontent.IndexOf("-");
                                            string leftrightcontent = rightcontent.Substring(0, locationoperation);                                            
                                            string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                                                                       
                                            if ((rightrightcontent.IndexOf("(") < 0))//вычитаемое не содержит индексов
                                            {
                                                bool rightrightcontentvp = false;
                                                foreach (string c in vnutrperem.Keys)//проверяется, находится ли справа от операции унарного минуса внутренняя переменная
                                                {
                                                    if (c == rightrightcontent)
                                                    { rightrightcontentvp = true; };
                                                }
                                                if (rightrightcontentvp == true)
                                                {
                                                    if ((vnutrperem[rightrightcontent].IndexOf("{")) >= 0)
                                                    {
                                                        outdata[leftcontent] = "{" + "\"op\":\"-\",\"od\":" + $"{vnutrperem[rightrightcontent]}" + "}";
                                                    }
                                                    else
                                                    {
                                                        outdata[leftcontent] = "{" + "\"op\":\"-\",\"od\":" + $"\"{vnutrperem[rightrightcontent]}\"" + "}";
                                                    }                                                    
                                                }
                                            }
                                        }                                        
                                        if ((rightcontent.IndexOf("*") > 0))
                                        {
                                            int locationoperation = rightcontent.IndexOf("*");
                                            string leftrightcontent = rightcontent.Substring(0, locationoperation);                                            
                                            string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                            
                                            if ((leftrightcontent.IndexOf("(") > 0) & (leftrightcontent.IndexOf(",") < 0))
                                            {
                                                int leftbracket = leftrightcontent.IndexOf("(");
                                                string leftrightcontentname = leftrightcontent.Substring(0, leftbracket);//имя переменной слева от операции умножения                                                
                                                string leftsomnoj = leftrightcontentname + "(";
                                                string vnutriskobok = leftrightcontent.Substring(leftrightcontent.IndexOf("(") + 1, leftrightcontent.IndexOf(")") - leftrightcontent.IndexOf("(") - 1);                                                
                                                bool vnutriskobokvp = false;
                                                foreach (string c in vnutrperem.Keys)//проверяется, является ли индексом первого сомножителя внутренняя переменная
                                                {
                                                    if (c == vnutriskobok)
                                                    {
                                                        vnutriskobokvp = true;
                                                    }
                                                }
                                                if (vnutriskobokvp == true)
                                                {
                                                    leftsomnoj = leftsomnoj + vnutrperem[vnutriskobok] + ")";                                                    
                                                    outdata[leftcontent] = "{" + "\"op\":\"*\",\"fO\":\"" + $"{leftsomnoj}\",";                                                                                                        
                                                }                                      
                                            }                                            
                                            if ((rightrightcontent.IndexOf("(") > 0) & (rightrightcontent.IndexOf(",") < 0))
                                            {
                                                int rightbracket = rightrightcontent.IndexOf("(");
                                                string rightrightcontentname = rightrightcontent.Substring(0, rightbracket);//имя переменной справа от операции умножения                                                
                                                string rightsomnoj = rightrightcontentname + "(";
                                                string vnutriskobok = rightrightcontent.Substring(rightrightcontent.IndexOf("(") + 1, rightrightcontent.IndexOf(")") - rightrightcontent.IndexOf("(") - 1);                                                
                                                bool vnutriskobokvp = false;
                                                foreach (string c in vnutrperem.Keys)//проверяется, является ли индексом первого сомножителя внутренняя переменная
                                                {
                                                    if (c == vnutriskobok)
                                                    {
                                                        vnutriskobokvp = true;
                                                    }
                                                }
                                                if (vnutriskobokvp == true)
                                                {
                                                    rightsomnoj = rightsomnoj + vnutrperem[vnutriskobok] + ")";                                                    
                                                    outdata[leftcontent] = outdata[leftcontent] + $"\"sO\":\"{rightsomnoj}\"" + "}";                                                    
                                                }
                                            }
                                            if ((rightrightcontent.IndexOf("(") < 0))
                                            {                                                
                                                if (outdata[leftcontent].Substring(0, 1) == "{")//избавляемся от сложения с "0" при вычислении Cij
                                                {                                                    
                                                    outdata[leftcontent] = outdata[leftcontent] + $"\"sO\":{vnutrperem[rightrightcontent]}" + "}";                                                    
                                                }
                                                else
                                                {
                                                    outdata[leftcontent] = vnutrperem[rightrightcontent];//изменение значения выходной переменной                                                    
                                                }
                                            }
                                        }                                        
                                        if ((rightcontent.IndexOf("/") > 0))
                                        {
                                            int locationoperation = rightcontent.IndexOf("/");
                                            string leftrightcontent = rightcontent.Substring(0, locationoperation);                                            
                                            string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                                                                       
                                            if ((leftrightcontent.IndexOf("(") < 0))//делимое
                                            {
                                                bool leftrightcontentvp = false;
                                                foreach (string c in vnutrperem.Keys)//проверяется, находится ли слева от операции деления внутренняя переменная
                                                {
                                                    if (c == leftrightcontent)
                                                    { leftrightcontentvp = true; };
                                                }
                                                if (leftrightcontentvp == true)
                                                {
                                                    if ((vnutrperem[leftrightcontent].IndexOf("{")) >= 0)
                                                    {
                                                        outdata[leftcontent] = "{" + "\"op\":\"/\",\"fO\":" + $"{vnutrperem[leftrightcontent]},";
                                                    }
                                                    else
                                                    {
                                                        outdata[leftcontent] = "{" + "\"op\":\"/\",\"fO\":" + $"\"{vnutrperem[leftrightcontent]}\",";
                                                    }                                                                                                        
                                                }
                                            }
                                            if ((rightrightcontent.IndexOf("(") < 0))//делитель
                                            {                                                
                                                bool rightrightcontentvp = false;
                                                foreach (string c in vnutrperem.Keys)//проверяется, находится ли справа от операции деления внутренняя переменная
                                                {
                                                    if (c == rightrightcontent)
                                                    { rightrightcontentvp = true; };
                                                }
                                                if (rightrightcontentvp == true)
                                                {
                                                    if ((vnutrperem[rightrightcontent].IndexOf("{")) >= 0)
                                                    {
                                                        outdata[leftcontent] = $"{outdata[leftcontent]}" + "\"sO\":" + $"{vnutrperem[rightrightcontent]}" + "}";
                                                    }
                                                    else
                                                    {
                                                        outdata[leftcontent] = $"{outdata[leftcontent]}" + "\"sO\":" + $"\"{vnutrperem[rightrightcontent]}\"" + "}";
                                                    }                                                    
                                                }
                                            }
                                        }
                                       /*if ((rightcontent.IndexOf("no real solution") == 0))
                                        {
                                            outdata[leftcontent] = "{\"v\":\"no real solution\"}";                                            
                                        }*/                                        
                                    }
                                    if (outp == false) //слева от знака присваивания не выходная переменная
                                    {                                        
                                        bool vnutp = false;
                                        foreach (string c in vnutrperem.Keys)//проверяется, находится ли слева от знака присваивания внутренняя переменная                                        {
                                            if (c == leftcontent) { vnutp = true; };
                                        }                                        
                                        if (vnutp == true) //слева от знака присваивания внутренняя переменная
                                        {                                            
                                            string rightcontent = content.Substring(assignmentcharacter + 1, content.Length - assignmentcharacter - 1);                                            
                                            if ((rightcontent.IndexOf("+") < 0) & (rightcontent.IndexOf("-") < 0) & (rightcontent.IndexOf("*") < 0) & (rightcontent.IndexOf("/") < 0) & (rightcontent.IndexOf("abs") < 0) & (rightcontent.IndexOf("sqrt") < 0))
                                            {
                                                if ((rightcontent.Substring(0, 1) == "0") || (rightcontent.Substring(0, 1) == "1") || (rightcontent.Substring(0, 1) == "2") || (rightcontent.Substring(0, 1) == "3") || (rightcontent.Substring(0, 1) == "4") || (rightcontent.Substring(0, 1) == "5") || (rightcontent.Substring(0, 1) == "6") ||
                                                   (rightcontent.Substring(0, 1) == "7") || (rightcontent.Substring(0, 1) == "8") || (rightcontent.Substring(0, 1) == "9"))
                                                {                                                    
                                                    vnutrperem[leftcontent] = rightcontent;                                                                                                    }
                                                
                                                if (rightcontent.IndexOf("(") < 0) 
                                                {
                                                    bool vnpr = false;
                                                    foreach (string c in vnutrperem.Keys)//определяется, является ли справа от знака присваивания переменная без индексов внутренней переменной
                                                    {
                                                        if (c == rightcontent)
                                                        {
                                                            vnpr = true;                                                            
                                                        }
                                                    }
                                                    if (vnpr == true)
                                                    {
                                                        vnutrperem[leftcontent] = vnutrperem[rightcontent];                                                        
                                                    }
                                                }                                                    
                                                    if ((rightcontent.IndexOf("(") > 0) & (rightcontent.IndexOf(",") < 0))//один индекс
                                                {
                                                    int rightbracket = rightcontent.IndexOf("(");
                                                    string rightcontentname = rightcontent.Substring(0, rightbracket);//имя переменной слева от скобки (
                                                    string rightotprisv = rightcontentname + "(";
                                                    string vnutriskobok = rightcontent.Substring(rightcontent.IndexOf("(") + 1, rightcontent.IndexOf(")") - rightcontent.IndexOf("(") - 1);
                                                    foreach (string c in vnutrperem.Keys)//ищется внутренняя переменная для индекса переменной справа от знака присваивания и определяется ее значение
                                                    {
                                                        if (c == vnutriskobok)
                                                        {
                                                            rightotprisv = rightotprisv + vnutrperem[vnutriskobok];                                                            
                                                        }
                                                    }
                                                    rightotprisv = rightotprisv.Substring(0, rightotprisv.Length) + ")";
                                                    rightcontent = rightotprisv;
                                                    bool vnpr = false;
                                                    foreach (string c in vnutrperem.Keys)//определяется, является ли справа от знака присваивания переменная с одним индексом внутренней переменной
                                                    {
                                                        if (c == rightotprisv)
                                                        {
                                                            vnpr = true;                                                            
                                                        }
                                                    }
                                                    bool outpr = false;
                                                    foreach (string c in outdata.Keys)//определяется, является ли справа от знака присваивания переменная с одним индексом выходной переменной
                                                    {
                                                        if (c == rightotprisv)
                                                        {
                                                            outpr = true;                                                            
                                                        }
                                                    }
                                                    string rightotprisvv = " ";
                                                    if (vnpr == true)
                                                    {
                                                        rightotprisvv = vnutrperem[rightotprisv];
                                                    }
                                                    if (outpr == true)
                                                    {
                                                        rightotprisvv = outdata[rightotprisv];                                                        
                                                    }
                                                    if ((rightotprisvv.Substring(0, 1) == "0") || (rightotprisvv.Substring(0, 1) == "1") || (rightotprisvv.Substring(0, 1) == "2") || (rightotprisvv.Substring(0, 1) == "3") || (rightotprisvv.Substring(0, 1) == "4") || (rightotprisvv.Substring(0, 1) == "5") || (rightotprisvv.Substring(0, 1) == "6") ||
                                                       (rightotprisvv.Substring(0, 1) == "7") || (rightotprisvv.Substring(0, 1) == "8") || (rightotprisvv.Substring(0, 1) == "9"))
                                                    {
                                                        rightcontent = rightotprisvv;
                                                    }
                                                    else
                                                    {                                                    
                                                            rightcontent = rightotprisv;
                                                    }                                                    
                                                    vnutrperem[leftcontent] = rightcontent;                                                    
                                                }                                                
                                                if ((rightcontent.IndexOf("(") > 0) & (rightcontent.IndexOf(",") > 0))//справа переменная с нсколькими индексами
                                                {
                                                    int leftbracket = rightcontent.IndexOf("(");
                                                    string rightcontentname = rightcontent.Substring(0, leftbracket);//имя переменной справа от знака присваивания                                                    
                                                    string rightcontentv = rightcontentname + "(";
                                                    string vnutriskobok = rightcontent.Substring(rightcontent.IndexOf("(") + 1, rightcontent.IndexOf(")") - rightcontent.IndexOf("(") - 1);
                                                    string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                    foreach (string s in masindex)
                                                    {                                                        
                                                        foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов переменной справа от знака присваивания и определяются их значения
                                                        {
                                                            if (c == s)
                                                            {
                                                                rightcontentv = rightcontentv + vnutrperem[s] + ",";                                                                
                                                            }
                                                        }
                                                    }
                                                    rightcontentv = rightcontentv.Substring(0, rightcontentv.Length - 1) + ")";                                                    
                                                    bool rcinp = false;
                                                    foreach (string c in indata)//определяется, является ли значение справа от знака присваивания входной переменной
                                                    {
                                                        if (c == rightcontentv)
                                                        {
                                                            rcinp = true;
                                                        }
                                                    }
                                                    if (rcinp == true)
                                                    {
                                                        vnutrperem[leftcontent] = rightcontentv;                                                        
                                                    }                                                    
                                                    bool rcv = false;
                                                    foreach (string c in vnutrperem.Keys)//определяется, является ли значение справа от знака присваивания внутренней переменной
                                                    {
                                                        if (c == rightcontentv)
                                                        {
                                                            rcv = true;
                                                        }
                                                    }
                                                    if (rcv == true)
                                                    {
                                                        vnutrperem[leftcontent] = vnutrperem[rightcontentv];                                                        
                                                    }
                                                    else
                                                    {
                                                        vnutrperem[leftcontent] = rightcontentv;                                                        
                                                    }                                                    
                                                }                                                
                                            }
                                            if ((rightcontent.IndexOf("+") > 0))
                                            {
                                                int locationoperation = rightcontent.IndexOf("+");
                                                string leftrightcontent = rightcontent.Substring(0, locationoperation);                                                
                                                string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                                
                                                string leftrightcontentvalue = "";
                                                string rightrightcontentvalue = "";
                                                bool leftrightcontentvp = false;
                                                bool rightrightcontentvp = false;
                                                if ((leftrightcontent.IndexOf("(") < 0))
                                                {                                                    
                                                    foreach (string c in vnutrperem.Keys)//проверяется, находится ли слева от операции сложения внутренняя переменная
                                                    {
                                                        if (c == leftrightcontent) { leftrightcontentvp = true; };
                                                    }
                                                    if (leftrightcontentvp == true)
                                                    {
                                                        leftrightcontentvalue = vnutrperem[leftrightcontent];                                                        
                                                    }                                                    
                                                }
                                                //Парсер второго слагаемого

                                                if ((rightrightcontent.IndexOf("(") < 0))
                                                {                                                    
                                                    if ((rightrightcontent.Substring(0, 1) == "0") || (rightrightcontent.Substring(0, 1) == "1") || (rightrightcontent.Substring(0, 1) == "2") || (rightrightcontent.Substring(0, 1) == "3") || (rightrightcontent.Substring(0, 1) == "4") || (rightrightcontent.Substring(0, 1) == "5") || (rightrightcontent.Substring(0, 1) == "6") ||
                                                       (rightrightcontent.Substring(0, 1) == "7") || (rightrightcontent.Substring(0, 1) == "8") || (rightrightcontent.Substring(0, 1) == "9"))
                                                    {                                                        
                                                        string rightvalue = ((Int32.Parse(leftrightcontentvalue)) + (Int32.Parse(rightrightcontent))).ToString();
                                                        vnutrperem[leftcontent] = rightvalue;                                                        
                                                    }                                                    
                                                    foreach (string c in vnutrperem.Keys)//проверяется, находится ли справа от операции сложения внутренняя переменная
                                                    {
                                                        if (c == rightrightcontent) { rightrightcontentvp = true; };
                                                    }
                                                    if (rightrightcontentvp == true)
                                                    {
                                                        rightrightcontentvalue = vnutrperem[rightrightcontent];                                                                                                                
                                                    }
                                                    if ((leftrightcontentvp == true) & (rightrightcontentvp == true))
                                                    {
                                                        if ((leftrightcontentvalue.Substring(0, 1) == "0") || (leftrightcontentvalue.Substring(0, 1) == "1") || (leftrightcontentvalue.Substring(0, 1) == "2") || (leftrightcontentvalue.Substring(0, 1) == "3") || (leftrightcontentvalue.Substring(0, 1) == "4") || (leftrightcontentvalue.Substring(0, 1) == "5") || (leftrightcontentvalue.Substring(0, 1) == "6") ||
                                                        (leftrightcontentvalue.Substring(0, 1) == "7") || (leftrightcontentvalue.Substring(0, 1) == "8") || (leftrightcontentvalue.Substring(0, 1) == "9"))
                                                        {
                                                            if ((rightrightcontentvalue.Substring(0, 1) == "0") || (rightrightcontentvalue.Substring(0, 1) == "1") || (rightrightcontentvalue.Substring(0, 1) == "2") || (rightrightcontentvalue.Substring(0, 1) == "3") || (rightrightcontentvalue.Substring(0, 1) == "4") || (rightrightcontentvalue.Substring(0, 1) == "5") || (rightrightcontentvalue.Substring(0, 1) == "6") ||
                                                            (rightrightcontentvalue.Substring(0, 1) == "7") || (rightrightcontentvalue.Substring(0, 1) == "8") || (rightrightcontentvalue.Substring(0, 1) == "9"))
                                                            {
                                                                string rightvalue = ((Int32.Parse(leftrightcontentvalue)) + (Int32.Parse(rightrightcontentvalue))).ToString();
                                                                vnutrperem[leftcontent] = rightvalue;                                                                
                                                            }
                                                        }
                                                        else
                                                        {                                                            
                                                            if ((leftrightcontentvalue.IndexOf("{") < 0) & (rightrightcontentvalue.IndexOf("{") < 0))
                                                            {
                                                                vnutrperem[leftcontent] = "{" + "\"op\":\"+\",\"fO\":\"" + $"{leftrightcontentvalue}\"," + "\"sO\":\"" + $"{ rightrightcontentvalue}\"" + "}";
                                                            }
                                                            if ((leftrightcontentvalue.IndexOf("{") < 0) & (rightrightcontentvalue.IndexOf("{") >= 0))
                                                            {
                                                                vnutrperem[leftcontent] = "{" + "\"op\":\"+\",\"fO\":\"" + $"{leftrightcontentvalue}\"," + "\"sO\":" + $"{ rightrightcontentvalue}" + "}";
                                                            }
                                                            if ((leftrightcontentvalue.IndexOf("{") >= 0) & (rightrightcontentvalue.IndexOf("{") < 0))
                                                            {
                                                                vnutrperem[leftcontent] = "{" + "\"op\":\"+\",\"fO\":" + $"{leftrightcontentvalue}," + "\"sO\":\"" + $"{ rightrightcontentvalue}\"" + "}";
                                                            }
                                                            if ((leftrightcontentvalue.IndexOf("{") >= 0) & (rightrightcontentvalue.IndexOf("{") >= 0))
                                                            {
                                                                vnutrperem[leftcontent] = "{" + "\"op\":\"+\",\"fO\":" + $"{leftrightcontentvalue}," + "\"sO\":" + $"{ rightrightcontentvalue}" + "}";
                                                            }                                                                                                                    
                                                        }
                                                    }                                                    
                                                }
                                                if ((leftrightcontent.IndexOf("(") > 0) & (leftrightcontent.IndexOf(",") < 0))
                                                {
                                                    int leftbracket = leftrightcontent.IndexOf("(");
                                                    string leftrightcontentname = leftrightcontent.Substring(0, leftbracket);//имя переменной слева от операции сложения
                                                    string leftslag = leftrightcontentname + "(";
                                                    string vnutriskobok = leftrightcontent.Substring(leftrightcontent.IndexOf("(") + 1, leftrightcontent.IndexOf(")") - leftrightcontent.IndexOf("(") - 1);
                                                    leftslag = leftslag + vnutrperem[vnutriskobok] + ")";                                                    
                                                    bool leftslagvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, является ли первое слагаемое внутренней переменной
                                                    {
                                                        if (c == leftslag) { leftslagvp = true; };
                                                    }
                                                    if (leftslagvp == true)
                                                    {
                                                        leftrightcontentvalue = vnutrperem[leftslag];                                                        
                                                    }
                                                    string leftvalue = " ";
                                                    if ((leftrightcontentvalue.IndexOf("{") == 0))
                                                    {
                                                        leftvalue = "{" + "\"op\":\"+\",\"fO\":" + $"{leftrightcontentvalue},";
                                                        vnutrperem[leftcontent] = leftvalue;//изменено значение внутренней переменной                                                        
                                                    }
                                                    else
                                                    {
                                                        leftvalue = "{" + "\"op\":\"+\",\"fO\":\"" + $"{leftrightcontentvalue}\",";
                                                        vnutrperem[leftcontent] = leftvalue;//изменено значение внутренней переменной
                                                    }                                                    
                                                }                                                
                                                //Парсер второго слагаемого                                               
                                                if ((rightrightcontent.IndexOf("(") > 0) & (rightrightcontent.IndexOf(",") < 0))
                                                {
                                                    int leftbracket = rightrightcontent.IndexOf("(");
                                                    string rightrightcontentname = rightrightcontent.Substring(0, leftbracket);//Имя переменной справа от операции сложения                                                    
                                                    string rightslag = rightrightcontentname + "(";
                                                    string vnutriskobok = rightrightcontent.Substring(rightrightcontent.IndexOf("(") + 1, rightrightcontent.IndexOf(")") - rightrightcontent.IndexOf("(") - 1);
                                                    rightslag = rightslag + vnutrperem[vnutriskobok] + ")";                                         
                                                    bool rightslagvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, является ли второе слагаемое внутренней переменной
                                                    {
                                                        if (c == rightslag) { rightslagvp = true; };
                                                    }
                                                    if (rightslagvp == true)
                                                    {
                                                        rightrightcontentvalue = vnutrperem[rightslag];                                                        
                                                        vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":{rightrightcontentvalue}" + "}";                                                        
                                                    }                                                    
                                                }                                                
                                                if ((leftrightcontent.IndexOf("(") > 0) & (leftrightcontent.IndexOf(",") > 0))
                                                {
                                                    int leftrightbracket = leftrightcontent.IndexOf("(");
                                                    string leftrightcontentname = leftrightcontent.Substring(0, leftrightbracket);//имя переменной слева от операции сложения                                                    
                                                    string leftrightcontentvnp = leftrightcontentname + "(";
                                                    string vnutriskobok = leftrightcontent.Substring(leftrightcontent.IndexOf("(") + 1, leftrightcontent.IndexOf(")") - leftrightcontent.IndexOf("(") - 1);
                                                    string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                    foreach (string s in masindex)
                                                    {                                                        
                                                        foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов переменной слева от операции сложения и определяются их значения
                                                        {
                                                            if (c == s)
                                                            {
                                                                leftrightcontentvnp = leftrightcontentvnp + vnutrperem[s] + ",";                                                                
                                                            }
                                                        }
                                                    }
                                                    leftrightcontentvnp = leftrightcontentvnp.Substring(0, leftrightcontentvnp.Length - 1) + ")";                                                                                                       
                                                    bool rcv = false;
                                                    foreach (string c in vnutrperem.Keys)//определяется, является ли значение справа от знака присваивания внутренней переменной
                                                    {
                                                        if (c == leftrightcontentvnp)
                                                        {
                                                            rcv = true;
                                                        }
                                                    }
                                                    if (rcv == true)
                                                    {
                                                        leftrightcontentvalue = vnutrperem[leftrightcontentvnp];                                                        
                                                    }
                                                    string leftvalue = " ";
                                                    if ((leftrightcontentvalue.IndexOf("{") == 0))
                                                    {
                                                        leftvalue = "{" + "\"op\":\"+\",\"fO\":" + $"{leftrightcontentvalue},";
                                                        vnutrperem[leftcontent] = leftvalue;//изменено значение внутренней переменной                                                        
                                                    }
                                                    else
                                                    {
                                                        leftvalue = "{" + "\"op\":\"+\",\"fO\":\"" + $"{leftrightcontentvalue}\",";
                                                        vnutrperem[leftcontent] = leftvalue;//изменено значение внутренней переменной
                                                    }                                                    
                                                    if (rightrightcontentvp == true)
                                                    {
                                                        rightrightcontentvalue = vnutrperem[rightrightcontent];                                                        
                                                        vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":{rightrightcontentvalue}" + "}";                                                        
                                                    }
                                                }                                                
                                            }                                            
                                            if ((rightcontent.IndexOf("-") > 0))
                                            {
                                                int locationoperation = rightcontent.IndexOf("-");
                                                string leftrightcontent = rightcontent.Substring(0, locationoperation);                                                
                                                string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                                
                                                string leftrightcontentvalue = "";
                                                if ((leftrightcontent.IndexOf("(") > 0) & (leftrightcontent.IndexOf(",") < 0))
                                                {
                                                    int leftbracket = leftrightcontent.IndexOf("(");
                                                    string leftrightcontentname = leftrightcontent.Substring(0, leftbracket);//Имя переменной слева от операции вычитания                                                    
                                                    string leftsomnoj = leftrightcontentname + "(";
                                                    string vnutriskobok = leftrightcontent.Substring(leftrightcontent.IndexOf("(") + 1, leftrightcontent.IndexOf(")") - leftrightcontent.IndexOf("(") - 1);
                                                    leftsomnoj = leftsomnoj + vnutrperem[vnutriskobok] + ")";                                                    

                                                    bool leftrightcontentvpnew = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, является ли уменьшаемое внутренней переменной
                                                    {
                                                        if (c == leftsomnoj) { leftrightcontentvpnew = true; };
                                                    }
                                                    if (leftrightcontentvpnew == true)
                                                    {
                                                        leftrightcontentvalue = vnutrperem[leftsomnoj];                                                        
                                                    }
                                                    bool leftrightcontentop = false;
                                                    foreach (string c in outdata.Keys)//проверяется, является ли уменьшаемое выходной переменной
                                                    {
                                                        if (c == leftsomnoj) { leftrightcontentop = true; };
                                                    }
                                                    if (leftrightcontentop == true)
                                                    {
                                                        leftrightcontentvalue = outdata[leftsomnoj];                                                        
                                                    }
                                                    if ((leftrightcontentvalue.IndexOf("{") == 0))
                                                    {
                                                        string leftvalue = "{" + "\"op\":\"-\",\"fO\":" + $"{leftrightcontentvalue},";
                                                        vnutrperem[leftcontent] = leftvalue;//изменено значение внутренней переменной                                                        
                                                    }
                                                    else
                                                    {
                                                        string leftvalue = "{" + "\"op\":\"-\",\"fO\":\"" + $"{leftrightcontentvalue}\",";
                                                        vnutrperem[leftcontent] = leftvalue;//изменено значение внутренней переменной
                                                    }                                                    
                                                }                                                
                                                if ((leftrightcontent.IndexOf("(") > 0) & (leftrightcontent.IndexOf(",") > 0)) //уменьшаемое с несколькими индексами
                                                {
                                                    int leftbracket = leftrightcontent.IndexOf("(");
                                                    string leftrightcontentname = leftrightcontent.Substring(0, leftbracket);//имя переменной слева от операции вычитания
                                                    string leftsomnoj = leftrightcontentname + "(";
                                                    string vnutriskobok = leftrightcontent.Substring(leftrightcontent.IndexOf("(") + 1, leftrightcontent.IndexOf(")") - leftrightcontent.IndexOf("(") - 1);
                                                    string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                    foreach (string s in masindex)
                                                    {                                                        
                                                        foreach (string c in vnutrperem.Keys)//ищем внутренние переменные для индексов уменьшаемого и определяются их значения
                                                        {
                                                            if (c == s)
                                                            {
                                                                leftsomnoj = leftsomnoj + vnutrperem[s] + ",";                                                                
                                                            }
                                                        }
                                                    }
                                                    leftsomnoj = leftsomnoj.Substring(0, leftsomnoj.Length - 1) + ")";                                                    
                                                    bool leftsomnojvnutp = false;
                                                    foreach (string c in vnutrperem.Keys)//определяем, является ли уменьшаемое внутренней переменной
                                                    {
                                                        if (c == leftsomnoj)
                                                        {
                                                            leftsomnojvnutp = true;
                                                        }
                                                    }
                                                    string leftvalue = " ";
                                                    if (leftsomnojvnutp == true)
                                                    {
                                                        if (vnutrperem[leftsomnoj].IndexOf("{") < 0)
                                                        {
                                                            leftvalue = "{" + "\"op\":\"-\",\"fO\":\"" + $"{vnutrperem[leftsomnoj]}\",";                                                            
                                                        }
                                                        if (vnutrperem[leftsomnoj].IndexOf("{") >= 0)
                                                        {
                                                            leftvalue = "{" + "\"op\":\"-\",\"fO\":" + $"{vnutrperem[leftsomnoj]},";                                                            
                                                        }
                                                    }
                                                    if (leftsomnojvnutp == false)
                                                    {                                                        
                                                        leftvalue = "{" + "\"op\":\"-\",\"fO\":\"" + $"{leftsomnoj}\",";
                                                    }
                                                    vnutrperem[leftcontent] = leftvalue;//изменено значение внутренней переменной                                                    
                                                } 
                                                
                                                bool leftrightcontentvp = false;
                                                string rightvaluenew = " ";
                                                foreach (string c in vnutrperem.Keys)//проверяется, находится ли слева от операции вычитания внутренняя переменная
                                                {
                                                    if (c == leftrightcontent)
                                                    { leftrightcontentvp = true; };
                                                }                                                
                                                bool leftrightcontentinp= false;
                                                if (leftrightcontentvp == true)
                                                {
                                                    foreach (string c in indata)//проверяется, находится ли слева от операции вычитания внутренняя переменная,содержащая входную переменную
                                                    {
                                                        if ((vnutrperem[leftrightcontent].IndexOf(c)) >= 0)
                                                        { leftrightcontentinp = true; };
                                                    }
                                                    if (leftrightcontentinp == true)
                                                    {                                                        
                                                    }
                                                }                                                
                                                if ((leftrightcontentvp == true) & (leftrightcontentinp == true))
                                                {
                                                    if ((vnutrperem[leftrightcontent].IndexOf("{")) >= 0)
                                                    {
                                                        rightvaluenew = "{" + "\"op\":\"-\",\"fO\":" + $"{vnutrperem[leftrightcontent]},";
                                                    }
                                                    else
                                                    {
                                                        rightvaluenew = "{" + "\"op\":\"-\",\"fO\":" + $"\"{vnutrperem[leftrightcontent]}\",";
                                                    }                                                                                                        
                                                    vnutrperem[leftcontent] = rightvaluenew;                                                    
                                                }
                                                if ((leftrightcontentvp == true) & (leftrightcontentinp == false))
                                                {
                                                    leftrightcontentvalue = vnutrperem[leftrightcontent];                                                    
                                                }                                                
                                                //Парсер вычитаемого
                                                if ((rightrightcontent.IndexOf("(") < 0))
                                                {                                                    
                                                    if ((rightrightcontent.Substring(0, 1) == "0") || (rightrightcontent.Substring(0, 1) == "1") || (rightrightcontent.Substring(0, 1) == "2") || (rightrightcontent.Substring(0, 1) == "3") || (rightrightcontent.Substring(0, 1) == "4") || (rightrightcontent.Substring(0, 1) == "5") || (rightrightcontent.Substring(0, 1) == "6") ||
                                                       (rightrightcontent.Substring(0, 1) == "7") || (rightrightcontent.Substring(0, 1) == "8") || (rightrightcontent.Substring(0, 1) == "9"))
                                                    {                                                        
                                                        string rightvalue = ((Int32.Parse(leftrightcontentvalue)) - (Int32.Parse(rightrightcontent))).ToString();                                                        
                                                        vnutrperem[leftcontent] = rightvalue;                                                        
                                                    }
                                                    bool rightrightcontentvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, является ли вычитаемое внутренней переменной
                                                    {
                                                        if (c == rightrightcontent) { rightrightcontentvp = true; };
                                                    }                                                                                                           
                                                        string rightrightcontentvalue = " ";
                                                        if (rightrightcontentvp == true)
                                                        {
                                                            if ((vnutrperem[leftcontent].IndexOf("{")) >= 0)
                                                            {
                                                                rightrightcontentvalue = vnutrperem[rightrightcontent];
                                                                if ((rightrightcontentvalue.IndexOf("{")) >= 0)
                                                                {
                                                                    vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":{rightrightcontentvalue}" + "}";                                                                    
                                                                }
                                                                else
                                                                {
                                                                    vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":\"{rightrightcontentvalue}\"" + "}";                                                                    
                                                                }
                                                            }                                                                                                                    
                                                        }
                                                    }                                                
                                                if ((rightrightcontent.IndexOf("(") > 0) & (rightrightcontent.IndexOf(",") < 0))
                                                {
                                                    int leftbracket = rightrightcontent.IndexOf("(");
                                                    string rightrightcontentname = rightrightcontent.Substring(0, leftbracket);//имя переменной справа от операции вычитания
                                                    string rightsomnoj = rightrightcontentname + "(";
                                                    string vnutriskobok = rightrightcontent.Substring(rightrightcontent.IndexOf("(") + 1, rightrightcontent.IndexOf(")") - rightrightcontent.IndexOf("(") - 1);
                                                    rightsomnoj = rightsomnoj + vnutrperem[vnutriskobok] + ")";                                                    

                                                    bool rightrightcontentvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, является ли вычитаемое внутренней переменной
                                                    {
                                                        if (c == rightsomnoj) { rightrightcontentvp = true; };
                                                    }
                                                    if (rightrightcontentvp == true)
                                                    {
                                                        string rightrightcontentvalue = vnutrperem[rightsomnoj];                                                        
                                                        vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":{rightrightcontentvalue}" + "}";                                                        
                                                    }
                                                    bool rightrightcontentop = false;
                                                    foreach (string c in outdata.Keys)//проверяется, является ли вычитаемое выходной переменной
                                                    {
                                                        if (c == rightsomnoj) { rightrightcontentop = true; };
                                                    }
                                                    if (rightrightcontentop == true)
                                                    {
                                                        string rightrightcontentvalue = outdata[rightsomnoj];                                                        
                                                        vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":{rightrightcontentvalue}" + "}";                                                        
                                                    }
                                                }                                                                                                
                                                if ((rightrightcontent.IndexOf("(") > 0) & (rightrightcontent.IndexOf(",") > 0))
                                                {
                                                    int rightbracket = rightrightcontent.IndexOf("(");
                                                    string rightrightcontentname = rightrightcontent.Substring(0, rightbracket);//имя переменной справа от операции вычитания
                                                    string rightoperand = rightrightcontentname + "(";
                                                    string vnutriskobok = rightrightcontent.Substring(rightrightcontent.IndexOf("(") + 1, rightrightcontent.IndexOf(")") - rightrightcontent.IndexOf("(") - 1);                                                    
                                                    string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                    foreach (string s in masindex)
                                                    {                                                        
                                                        foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов вычитаемого и определяются их значения
                                                        {
                                                            if (c == s)
                                                            {
                                                                rightoperand = rightoperand + vnutrperem[s] + ",";                                                                
                                                            }
                                                        }
                                                    }
                                                    rightoperand = rightoperand.Substring(0, rightoperand.Length - 1) + ")";                                                    
                                                    bool rightrightcontentoutp = false;
                                                    foreach (string c in outdata.Keys)//проверяется, является ли вычитаемое выходной переменной
                                                    {
                                                        if (c == rightoperand) { rightrightcontentoutp = true; };
                                                    }
                                                    string rightrightcontentvalue = " ";
                                                    if (rightrightcontentoutp == true)
                                                    {
                                                        if ((vnutrperem[leftcontent].IndexOf("{")) >= 0)
                                                        {
                                                            rightrightcontentvalue = outdata[rightoperand];
                                                            if ((rightrightcontentvalue.IndexOf("{")) >= 0)
                                                            {
                                                                vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":{rightrightcontentvalue}" + "}";                                                                
                                                            }
                                                            else
                                                            {
                                                                vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":\"{rightrightcontentvalue}\"" + "}";                                                                
                                                            }
                                                        }
                                                    }
                                                }                                                                                                
                                            }                                            
                                            if ((rightcontent.IndexOf("*") > 0))
                                            {
                                                int locationoperation = rightcontent.IndexOf("*");
                                                string leftrightcontent = rightcontent.Substring(0, locationoperation);                                                
                                                string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                                
                                                if ((leftrightcontent.IndexOf("(") > 0) & (leftrightcontent.IndexOf(".") < 0)) //левый сомножитель с несколькими индексами                                                
                                                {
                                                    int leftbracket = leftrightcontent.IndexOf("(");
                                                    string leftrightcontentname = leftrightcontent.Substring(0, leftbracket);//имя переменной слева от операции умножения                                                    
                                                    string leftsomnoj = leftrightcontentname + "(";
                                                    string vnutriskobok = leftrightcontent.Substring(leftrightcontent.IndexOf("(") + 1, leftrightcontent.IndexOf(")") - leftrightcontent.IndexOf("(") - 1);
                                                    if (leftrightcontent.IndexOf(",") > 0) //левый сомножитель с несколькими индексами
                                                    {
                                                        string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                        foreach (string s in masindex)
                                                        {                                                            
                                                            foreach (string c in vnutrperem.Keys)//ищем внутренние переменные для индексов первого сомножителя и определяются их значения
                                                            {
                                                                if (c == s)
                                                                {
                                                                    leftsomnoj = leftsomnoj + vnutrperem[s] + ",";                                                                    
                                                                }
                                                            }
                                                        }
                                                        leftsomnoj = leftsomnoj.Substring(0, leftsomnoj.Length - 1) + ")";                                                        
                                                        bool leftsomnojvnutp = false;
                                                        foreach (string c in vnutrperem.Keys)//определяем, является ли левый сомножитель внутренней переменной
                                                        {
                                                            if (c == leftsomnoj)
                                                            {
                                                                leftsomnojvnutp = true;                                                               
                                                            }
                                                        }
                                                        string leftvalue = " ";
                                                        if (leftsomnojvnutp == true)
                                                        {
                                                            if (vnutrperem[leftsomnoj].IndexOf("{") < 0)
                                                            {
                                                                leftvalue = "{" + "\"op\":\"*\",\"fO\":\"" + $"{vnutrperem[leftsomnoj]}\",";                                                                
                                                            }                                                        
                                                            if (vnutrperem[leftsomnoj].IndexOf("{") >= 0)
                                                            {
                                                                leftvalue = "{" + "\"op\":\"*\",\"fO\":" + $"{vnutrperem[leftsomnoj]},";                                                                
                                                            }
                                                        }
                                                        if (leftsomnojvnutp == false)
                                                        {                                                            
                                                            leftvalue = "{" + "\"op\":\"*\",\"fO\":\"" + $"{leftsomnoj}\",";
                                                        }
                                                        vnutrperem[leftcontent] = leftvalue;//изменено значение внутренней переменной                                                        
                                                    }                                                    
                                                }                                                
                                                if ((leftrightcontent.IndexOf("(") > 0) & (leftrightcontent.IndexOf(",") < 0))
                                                {
                                                    int leftbracket = leftrightcontent.IndexOf("(");
                                                    string leftrightcontentname = leftrightcontent.Substring(0, leftbracket);//имя переменной слева от операции умножения                                                                                                        
                                                    string leftsomnoj = leftrightcontentname + "(";
                                                    string vnutriskobok = leftrightcontent.Substring(leftrightcontent.IndexOf("(") + 1, leftrightcontent.IndexOf(")") - leftrightcontent.IndexOf("(") - 1);                                                    
                                                    bool vnutriskobokvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, является ли индексом первого сомножителя внутренняя переменная
                                                    {
                                                        if (c == vnutriskobok)
                                                        {
                                                            vnutriskobokvp = true;
                                                        }
                                                    }
                                                    if (vnutriskobokvp == true)
                                                    {
                                                        leftsomnoj = leftsomnoj + vnutrperem[vnutriskobok] + ")";
                                                        foreach (string c in indata)
                                                        {
                                                            if (c == leftsomnoj)
                                                            {
                                                                vnutrperem[leftcontent] = "{" + "\"op\":\"*\",\"fO\":\"" + $"{leftsomnoj}\",";                                                                
                                                            }
                                                        }
                                                    }
                                                }                                                
                                                //Парсер второго сомножителя
                                                if ((rightrightcontent.IndexOf("(") > 0)) //второй сомножитель имеет хотя бы один индекс
                                                {
                                                    int leftbracket = rightrightcontent.IndexOf("(");
                                                    string rightrightcontentname = rightrightcontent.Substring(0, leftbracket);//имя переменной справа от операции умножения
                                                    string rightsomnoj = rightrightcontentname + "(";
                                                    string vnutriskobok = rightrightcontent.Substring(rightrightcontent.IndexOf("(") + 1, rightrightcontent.IndexOf(")") - rightrightcontent.IndexOf("(") - 1);                                                    
                                                    if (rightrightcontent.IndexOf(",") > 0) //второй сомножитель имеет несколько индексов
                                                    {
                                                        string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                        foreach (string s in masindex)
                                                        {                                                            
                                                            foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов правого сомножителя и определяются их значения
                                                            {
                                                                if (c == s)
                                                                {
                                                                    rightsomnoj = rightsomnoj + vnutrperem[s] + ",";                                                                    
                                                                }
                                                            }
                                                        }
                                                        rightsomnoj = rightsomnoj.Substring(0, rightsomnoj.Length - 1) + ")";                                                        
                                                    }
                                                    if (rightrightcontent.IndexOf(",") < 0) //второй сомножитель имеет один индекс
                                                    {
                                                        foreach (string c in vnutrperem.Keys)//ищется внутренняя переменная для индекса правого сомножителя и определяются ее значение
                                                        {
                                                            if (c == vnutriskobok)
                                                            {
                                                                rightsomnoj = rightsomnoj + vnutrperem[vnutriskobok];                                                                
                                                            }
                                                        }
                                                        rightsomnoj = rightsomnoj.Substring(0, rightsomnoj.Length) + ")";                                                        
                                                        bool outd = false;
                                                        foreach (string c in outdata.Keys)
                                                        {
                                                            if (c == rightsomnoj)
                                                            {
                                                                outd = true;
                                                                rightsomnoj = outdata[rightsomnoj];                                                                
                                                            }
                                                        }                                                                                                                
                                                        foreach (string c in indata)
                                                        {
                                                            if (c == rightsomnoj)
                                                            {                                                                
                                                                vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":\"{rightsomnoj}\"" + "}";                                                                
                                                            }
                                                        }                                                        
                                                    }                                                    
                                                    bool rightsomnojvnutp = false;
                                                    foreach (string c in vnutrperem.Keys) //проверяется, является ли второй сомножитель внутренней переменной
                                                    {
                                                        if (c == rightsomnoj)
                                                        {
                                                            rightsomnojvnutp = true;
                                                        }
                                                    }
                                                    if (rightsomnojvnutp == true) //второй сомножитель является внутренней переменной
                                                    {
                                                        if (vnutrperem[rightsomnoj].IndexOf("{") < 0)
                                                        {
                                                            vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":\"{vnutrperem[rightsomnoj]}\"" + "}";                                                            
                                                        }
                                                        if (vnutrperem[rightsomnoj].IndexOf("{") >= 0)
                                                        {
                                                            vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":{vnutrperem[rightsomnoj]}" + "}";                                                            
                                                        }
                                                    }                                                    
                                                    if (rightsomnojvnutp == false)
                                                    {                                                        
                                                        bool outd = false;
                                                        foreach (string c in outdata.Keys)
                                                        {
                                                            if (c == rightsomnoj)
                                                            {
                                                                outd = true;
                                                                rightsomnoj = outdata[rightsomnoj];                                                                
                                                            }
                                                        }                                                        
                                                        if ((rightsomnoj.IndexOf("{") == 0))
                                                        {
                                                            vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":{rightsomnoj}" + "}";
                                                        }
                                                        else
                                                        {
                                                            vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":\"{rightsomnoj}\"" + "}";
                                                        }                                                        
                                                    }                                                     
                                                }                                                
                                                string leftrightcontentvalue = " ";
                                                if ((leftrightcontent.IndexOf("(") < 0)) //левый сомножитель без индексов
                                                {
                                                    bool leftrightcontentvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, находится ли слева от операции умножения внутренняя переменная
                                                    {
                                                        if (c == leftrightcontent) { leftrightcontentvp = true; };
                                                    }
                                                    if (leftrightcontentvp == true)
                                                    {
                                                        leftrightcontentvalue = vnutrperem[leftrightcontent];                                                        
                                                    }                                                    
                                                    if ((leftrightcontent.Substring(0, 1) == "0") || (leftrightcontent.Substring(0, 1) == "1") || (leftrightcontent.Substring(0, 1) == "2") || (leftrightcontent.Substring(0, 1) == "3") || (leftrightcontent.Substring(0, 1) == "4") || (leftrightcontent.Substring(0, 1) == "5") || (leftrightcontent.Substring(0, 1) == "6") ||
                                                       (leftrightcontent.Substring(0, 1) == "7") || (leftrightcontent.Substring(0, 1) == "8") || (leftrightcontent.Substring(0, 1) == "9"))
                                                    {                                                        
                                                        leftrightcontentvalue = leftrightcontent;
                                                    }
                                                    bool leftrightcontentinp = false;
                                                    foreach (string c in indata)//проверяется, находится ли слева от операции умножения входная переменная
                                                    {
                                                        if (c == leftrightcontent) { leftrightcontentinp = true; };
                                                    }
                                                    if (leftrightcontentinp == true)
                                                    {
                                                        leftrightcontentvalue = leftrightcontent;                                                                                                                                                                       
                                                    }                                                    
                                                }
                                                //Парсер второго сомножителя
                                                if ((rightrightcontent.IndexOf("(") < 0)) //правый сомножитель без индексов
                                                {                                                    
                                                    if ((rightrightcontent.Substring(0, 1) == "0") || (rightrightcontent.Substring(0, 1) == "1") || (rightrightcontent.Substring(0, 1) == "2") || (rightrightcontent.Substring(0, 1) == "3") || (rightrightcontent.Substring(0, 1) == "4") || (rightrightcontent.Substring(0, 1) == "5") || (rightrightcontent.Substring(0, 1) == "6") ||
                                                       (rightrightcontent.Substring(0, 1) == "7") || (rightrightcontent.Substring(0, 1) == "8") || (rightrightcontent.Substring(0, 1) == "9"))
                                                    {                                                        
                                                        string rightvalue = ((Int32.Parse(leftrightcontentvalue)) * (Int32.Parse(rightrightcontent))).ToString();
                                                        vnutrperem[leftcontent] = rightvalue;                                                        
                                                    }                                                    
                                                    bool rightrightcontentvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, находится ли справа от операции умножения внутренняя переменная
                                                    {
                                                        if (c == rightrightcontent)
                                                        { rightrightcontentvp = true; };
                                                    }
                                                    if (rightrightcontentvp == true)
                                                    {
                                                        string rightvalue = "{" + "\"op\":\"*\",\"fO\":" + $"\"{leftrightcontentvalue}\"" + ",\"sO\":" + $"{vnutrperem[rightrightcontent]}" + "}";                                                        
                                                        vnutrperem[leftcontent] = rightvalue;                                                        
                                                    }
                                                    bool rightrightcontentinp = false;
                                                    foreach (string c in indata)//проверяется, находится ли справа от операции умножения входная переменная
                                                    {
                                                        if (c == rightrightcontent) { rightrightcontentinp = true; };
                                                    }
                                                    if (rightrightcontentinp == true)
                                                    {
                                                        string rightvalue = "{" + "\"op\":\"*\",\"fO\":" + $"\"{leftrightcontentvalue}\"" + ",\"sO\":" + $"\"{rightrightcontent}\"" + "}";                                                        
                                                        vnutrperem[leftcontent] = rightvalue;                                                                                                                                                                       
                                                    }                                                    
                                                }
                                            }                                            
                                            if ((rightcontent.IndexOf("/") > 0))
                                            {
                                                int locationoperation = rightcontent.IndexOf("/");
                                                string leftrightcontent = rightcontent.Substring(0, locationoperation);                                                
                                                string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                                
                                                string leftrightcontentvalue = "";
                                                if ((leftrightcontent.IndexOf("(") > 0) & (leftrightcontent.IndexOf(",") < 0))//делимое с одним индексом
                                                {                                                    
                                                    int leftbracket = leftrightcontent.IndexOf("(");
                                                    string leftrightcontentname = leftrightcontent.Substring(0, leftbracket);//имя переменной слева от операции деления                                                    
                                                    string leftsomnoj = leftrightcontentname + "(";
                                                    string vnutriskobok = leftrightcontent.Substring(leftrightcontent.IndexOf("(") + 1, leftrightcontent.IndexOf(")") - leftrightcontent.IndexOf("(") - 1);

                                                    leftsomnoj = leftsomnoj + vnutrperem[vnutriskobok] + ")";                                                    

                                                    bool leftrightcontentvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, является ли делимое внутренней переменной
                                                    {
                                                        if (c == leftsomnoj) { leftrightcontentvp = true; };
                                                    }
                                                    if (leftrightcontentvp == true)
                                                    {
                                                        leftrightcontentvalue = vnutrperem[leftsomnoj];                                                        
                                                    }
                                                    string leftvalue = "{" + "\"op\":\"/\",\"fO\":" + $"{leftrightcontentvalue},";
                                                    vnutrperem[leftcontent] = leftvalue;//изменено значение внутренней переменной                                                                                                        
                                                }                                                
                                                if ((leftrightcontent.IndexOf("(") > 0) & (leftrightcontent.IndexOf(",") > 0))//делимое с несколькими индексами
                                                {
                                                    int leftbracket = leftrightcontent.IndexOf("(");
                                                    string leftrightcontentname = leftrightcontent.Substring(0, leftbracket);//имя переменной слева от операции деления                                                    
                                                    string leftdel = leftrightcontentname + "(";
                                                    string vnutriskobok = leftrightcontent.Substring(leftrightcontent.IndexOf("(") + 1, leftrightcontent.IndexOf(")") - leftrightcontent.IndexOf("(") - 1);
                                                    string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                    foreach (string s in masindex)
                                                    {                                                        
                                                        foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов делимого и определяются их значения
                                                        {
                                                            if (c == s)
                                                            {
                                                                leftdel = leftdel + vnutrperem[s] + ",";                                                                
                                                            }
                                                        }
                                                    }
                                                    leftdel = leftdel.Substring(0, leftdel.Length - 1) + ")";                                                    
                                                    bool leftrightcontentvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, является ли делимое внутренней переменной
                                                    {
                                                        if (c == leftdel) { leftrightcontentvp = true; };
                                                    }
                                                    if (leftrightcontentvp == true)
                                                    {
                                                        leftrightcontentvalue = vnutrperem[leftdel];                                                        
                                                    }
                                                    else
                                                    {
                                                        leftrightcontentvalue = leftdel;                                                        
                                                    }
                                                    bool lrcvinp = false;
                                                    foreach (string c in indata)//проверяем: делимое является входной переменной?
                                                    {                                                                                          
                                                        if (c == leftrightcontentvalue)
                                                        {
                                                            lrcvinp = true;                                                            
                                                        }
                                                    }
                                                    if ((lrcvinp == false) & (leftrightcontentvalue.IndexOf("{") < 0))
                                                    {
                                                        vnutrperem[leftcontent] = leftrightcontentvalue;
                                                    }
                                                    string leftvalue = " ";
                                                    if (lrcvinp == true)
                                                    {
                                                        leftvalue = "{" + "\"op\":\"/\",\"fO\":\"" + $"{leftrightcontentvalue}\",";
                                                        vnutrperem[leftcontent] = leftvalue;//изменено значение внутренней переменной                                                        
                                                    }
                                                    if (leftrightcontentvalue.IndexOf("{") >= 0)
                                                    {
                                                        leftvalue = "{" + "\"op\":\"/\",\"fO\":" + $"{leftrightcontentvalue},";
                                                        vnutrperem[leftcontent] = leftvalue;//изменено значение внутренней переменной                                                        
                                                    }                                                    
                                                }                                                                                                
                                                if ((leftrightcontent.IndexOf("(") < 0))//делимое
                                                {
                                                    bool leftrightcontentinp = false;
                                                    foreach (string c in indata)//проверяется, находится ли слева от операции деления входная переменная
                                                    {
                                                        if (c == leftrightcontent) { leftrightcontentinp = true; };
                                                    }
                                                    if (leftrightcontentinp == true)
                                                    {
                                                        vnutrperem[leftcontent] = "{" + "\"op\":\"/\",\"fO\":" + $"\"{leftrightcontentvalue}\",";                                                        
                                                    }
                                                }                                                                                                
                                                //Парсер делителя
                                                if ((rightrightcontent.IndexOf("(") < 0))//делитель без индексов
                                                {                                                    
                                                    if ((rightrightcontent.Substring(0, 1) == "0") || (rightrightcontent.Substring(0, 1) == "1") || (rightrightcontent.Substring(0, 1) == "2") || (rightrightcontent.Substring(0, 1) == "3") || (rightrightcontent.Substring(0, 1) == "4") || (rightrightcontent.Substring(0, 1) == "5") || (rightrightcontent.Substring(0, 1) == "6") ||
                                                       (rightrightcontent.Substring(0, 1) == "7") || (rightrightcontent.Substring(0, 1) == "8") || (rightrightcontent.Substring(0, 1) == "9"))
                                                    {                                                        
                                                        string rightvalue = ((Int32.Parse(leftrightcontentvalue)) + (Int32.Parse(rightrightcontent))).ToString();
                                                        vnutrperem[leftcontent] = rightvalue;                                                        
                                                    }
                                                    bool rightrightcontentvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, является ли делитель внутренней переменной
                                                    {
                                                        if (c == rightrightcontent) { rightrightcontentvp = true; };
                                                    }
                                                    string rightrightcontentvalue = " ";
                                                    if (rightrightcontentvp == true)
                                                    {
                                                        if ((vnutrperem[leftcontent].IndexOf("{")) >= 0)
                                                        {
                                                            rightrightcontentvalue = vnutrperem[rightrightcontent];
                                                            if ((rightrightcontentvalue.IndexOf("{")) >= 0)
                                                            {
                                                                vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":{rightrightcontentvalue}" + "}";                                                                
                                                            }
                                                            else
                                                            {
                                                                vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":\"{rightrightcontentvalue}\"" + "}";                                                                
                                                            }
                                                        }                                                                                                     
                                                    }
                                                }
                                                if ((rightrightcontent.IndexOf("(") > 0))//делитель с индексами
                                                {
                                                    int leftbracket = rightrightcontent.IndexOf("(");
                                                    string rightrightcontentname = rightrightcontent.Substring(0, leftbracket);//имя переменной справа от операции деления                                                    
                                                    string rightsomnoj = rightrightcontentname + "(";
                                                    string vnutriskobok = rightrightcontent.Substring(rightrightcontent.IndexOf("(") + 1, rightrightcontent.IndexOf(")") - rightrightcontent.IndexOf("(") - 1);
                                                    if (rightrightcontent.IndexOf(",") > 0)//делитель с несколькими индексами
                                                    {
                                                        string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                        foreach (string s in masindex)
                                                        {                                                            
                                                            foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов делителя и определяются их значения
                                                            {
                                                                if (c == s)
                                                                {
                                                                    rightsomnoj = rightsomnoj + vnutrperem[s] + ",";                                                                    
                                                                }
                                                            }
                                                        }
                                                        rightsomnoj = rightsomnoj.Substring(0, rightsomnoj.Length - 1) + ")";                                                        
                                                    }
                                                    if (rightrightcontent.IndexOf(",") < 0)//делитель с одним индексом
                                                    {
                                                        foreach (string c in vnutrperem.Keys)//ищется индекс делителя и определяется его значение
                                                        {
                                                            if (c == vnutriskobok)
                                                            {
                                                                rightsomnoj = rightsomnoj + vnutrperem[vnutriskobok];                                                                
                                                            }
                                                        }
                                                        rightsomnoj = rightsomnoj.Substring(0, rightsomnoj.Length) + ")";                                                        
                                                    }                                                    
                                                    vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":\"{rightsomnoj}\"" + "}";                                                     
                                                }
                                            }                                            
                                            if ((rightcontent.IndexOf("abs(") == 0))
                                            {
                                                int locationoperation = rightcontent.IndexOf("abc");
                                                string rightrightcontent = rightcontent.Substring(locationoperation + 4, rightcontent.Length - locationoperation - 4);                                                
                                                string vnutriskobok = rightrightcontent.Substring(rightrightcontent.IndexOf("(") + 1, rightrightcontent.IndexOf(")") - rightrightcontent.IndexOf("(") - 1);                                                
                                                bool vnutriskobokvp = false;
                                                foreach (string c in vnutrperem.Keys)//определяется значение внутри скобок операции abs
                                                {
                                                    if (c == vnutriskobok)
                                                    {
                                                        vnutriskobokvp = true;
                                                    }
                                                }
                                                if ((vnutriskobokvp == true))
                                                {
                                                    string rightvalue = "{" + "\"op\":\"abs\",\"od\":" + $"{vnutrperem[vnutriskobok]}" + "}";
                                                    vnutrperem[leftcontent] = rightvalue;//добавление значения внутренней переменной                                                    
                                                }                                                   
                                            }                                            
                                            if ((rightcontent.IndexOf("sqrt(") == 0))
                                            {
                                                int locationoperation = rightcontent.IndexOf("sqrt");
                                                string rightrightcontent = rightcontent.Substring(locationoperation + 4, rightcontent.Length - locationoperation - 4);                                                                                                                                              
                                                string vnutriskobok = rightrightcontent.Substring(rightrightcontent.IndexOf("(") + 1, rightrightcontent.IndexOf(")") - rightrightcontent.IndexOf("(") - 1);                                                
                                                bool vnutriskobokvp = false;
                                                foreach (string c in vnutrperem.Keys)//определяется значение внутри скобок операции sqrt
                                                {
                                                    if (c == vnutriskobok)
                                                    {
                                                        vnutriskobokvp = true;
                                                    }
                                                }
                                                if ((vnutriskobokvp == true))
                                                {
                                                    string rightvalue = "{" + "\"op\":\"sqrt\",\"od\":" + $"{vnutrperem[vnutriskobok]}" + "}";
                                                    vnutrperem[leftcontent] = rightvalue;//запись значения внутренней переменной                                                    
                                                }
                                            }                                            
                                        }
                                        if (vnutp == false)
                                            {
                                            string rightcontent = content.Substring(assignmentcharacter + 1, content.Length - assignmentcharacter - 1);                                            
                                            if ((rightcontent.IndexOf("+") < 0) & (rightcontent.IndexOf("-") < 0) & (rightcontent.IndexOf("*") < 0) & (rightcontent.IndexOf("/") < 0) & (rightcontent.IndexOf("abs") < 0) & (rightcontent.IndexOf("sqrt") < 0))
                                            {                                                
                                                if ((rightcontent.Substring(0, 1) == "0") || (rightcontent.Substring(0, 1) == "1") || (rightcontent.Substring(0, 1) == "2") || (rightcontent.Substring(0, 1) == "3") || (rightcontent.Substring(0, 1) == "4") || (rightcontent.Substring(0, 1) == "5") || (rightcontent.Substring(0, 1) == "6") ||
                                                   (rightcontent.Substring(0, 1) == "7") || (rightcontent.Substring(0, 1) == "8") || (rightcontent.Substring(0, 1) == "9"))
                                                {
                                                    vnutrperem.Add(leftcontent, rightcontent);                                                                                                        
                                                }                                                
                                                bool vnutrpr = false; //справа внутренняя переменная без индекса?
                                                foreach (string c in vnutrperem.Keys)
                                                {
                                                    if (c == rightcontent)
                                                    {
                                                        vnutrpr = true;                                                        
                                                    }
                                                }
                                                if (vnutrpr == true)
                                                {
                                                    vnutrperem.Add(leftcontent, vnutrperem[rightcontent]);                                                    
                                                }                                                
                                                if ((rightcontent.IndexOf("(") > 0) & (rightcontent.IndexOf(",") < 0))//один индекс
                                                {
                                                    int rightbracket = rightcontent.IndexOf("(");
                                                    string rightcontentname = rightcontent.Substring(0, rightbracket);//имя переменной слева от скобки (
                                                    string rightotprisv = rightcontentname + "(";
                                                    string vnutriskobok = rightcontent.Substring(rightcontent.IndexOf("(") + 1, rightcontent.IndexOf(")") - rightcontent.IndexOf("(") - 1);
                                                    foreach (string c in vnutrperem.Keys)//ищется внутренняя переменная для индекса переменной справа от знака присваивания и определяется ее значение
                                                    {
                                                        if (c == vnutriskobok)
                                                        {
                                                            rightotprisv = rightotprisv + vnutrperem[vnutriskobok];                                                            
                                                        }
                                                    }
                                                    rightotprisv = rightotprisv.Substring(0, rightotprisv.Length) + ")";
                                                    bool vnpr = false;
                                                    foreach (string c in vnutrperem.Keys)//определяется, является ли справа от знака присваивания переменная с одним индексом внутренней переменной
                                                    {
                                                        if (c == rightotprisv)
                                                        {
                                                            vnpr = true;                                                            
                                                        }
                                                    }
                                                    bool outpr = false;
                                                    foreach (string c in outdata.Keys)//определяется, является ли справа от знака присваивания переменная с одним индексом выходной переменной
                                                    {
                                                        if (c == rightotprisv)
                                                        {
                                                            outpr = true;                                                            
                                                        }
                                                    }
                                                    string rightotprisvv = " ";
                                                    if (vnpr == true)
                                                    {
                                                        rightotprisvv = vnutrperem[rightotprisv];
                                                    }
                                                    if (outpr == true)
                                                    {
                                                        rightotprisvv = outdata[rightotprisv];
                                                    }
                                                    if ((rightotprisvv.Substring(0, 1) == "0") || (rightotprisvv.Substring(0, 1) == "1") || (rightotprisvv.Substring(0, 1) == "2") || (rightotprisvv.Substring(0, 1) == "3") || (rightotprisvv.Substring(0, 1) == "4") || (rightotprisvv.Substring(0, 1) == "5") || (rightotprisvv.Substring(0, 1) == "6") ||
                                                       (rightotprisvv.Substring(0, 1) == "7") || (rightotprisvv.Substring(0, 1) == "8") || (rightotprisvv.Substring(0, 1) == "9"))
                                                    {
                                                        rightcontent = rightotprisvv;
                                                    }
                                                    else
                                                    {
                                                        rightcontent = rightotprisv;
                                                    }
                                                    vnutrperem.Add(leftcontent, rightcontent);                                                    
                                                }                                                                                                
                                                if ((rightcontent.IndexOf("(") > 0) & (rightcontent.IndexOf(",") > 0))
                                                {
                                                    int leftbracket = rightcontent.IndexOf("(");
                                                    string rightcontentname = rightcontent.Substring(0, leftbracket);//имя переменной слева от скобки (                                                    
                                                    string rightotprisv = rightcontentname + "(";
                                                    string vnutriskobok = rightcontent.Substring(rightcontent.IndexOf("(") + 1, rightcontent.IndexOf(")") - rightcontent.IndexOf("(") - 1);
                                                        string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                        foreach (string s in masindex)
                                                        {                                                            
                                                            foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов переменной слева от знака присваивания и определяются их значения
                                                            {
                                                                if (c == s)
                                                                {
                                                                rightotprisv = rightotprisv + vnutrperem[s] + ",";                                                                    
                                                                }
                                                            }
                                                        }
                                                        rightotprisv = rightotprisv.Substring(0, rightotprisv.Length - 1) + ")";
                                                        rightcontent = rightotprisv;
                                                        bool rightcontentvnp = false;
                                                        foreach (string c in vnutrperem.Keys)//справа от знака присваивания внутренняя переменная?
                                                    {
                                                            if (c == rightcontent)
                                                            {
                                                                rightcontentvnp = true;                                                                
                                                            }
                                                        }
                                                    if (rightcontentvnp == true)
                                                    {   vnutrperem.Add(leftcontent, vnutrperem[rightcontent]);                                                        
                                                    }
                                                    else
                                                    {
                                                        vnutrperem.Add(leftcontent, rightcontent);                                                        
                                                    }
                                                }                                                
                                            }
                                            if ((rightcontent.IndexOf("+") > 0))
                                            {
                                                int locationoperation = rightcontent.IndexOf("+");
                                                string leftrightcontent = rightcontent.Substring(0, locationoperation);                                                
                                                string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                                
                                                bool leftvnutrp = false;
                                                bool rightvnutrp = false;
                                                if ((leftrightcontent.IndexOf("(") < 0) & (rightrightcontent.IndexOf("(") < 0))
                                                {                                         
                                                    foreach (string c in vnutrperem.Keys)
                                                    {
                                                        if (c == leftrightcontent)
                                                        {
                                                            leftvnutrp = true;                                                            
                                                        }
                                                    }                                                    
                                                    foreach (string c in vnutrperem.Keys)
                                                    {
                                                        if (c == rightrightcontent)
                                                        {
                                                            rightvnutrp = true;                                                            
                                                        }
                                                    }
                                                    if ((leftvnutrp == true) & (rightvnutrp == true))
                                                    {
                                                        if ((vnutrperem[leftrightcontent].Substring(0, 1) == "0") || (vnutrperem[leftrightcontent].Substring(0, 1) == "1") || (vnutrperem[leftrightcontent].Substring(0, 1) == "2") || (vnutrperem[leftrightcontent].Substring(0, 1) == "3") || (vnutrperem[leftrightcontent].Substring(0, 1) == "4") || (vnutrperem[leftrightcontent].Substring(0, 1) == "5") || (vnutrperem[leftrightcontent].Substring(0, 1) == "6") ||
                                                        (vnutrperem[leftrightcontent].Substring(0, 1) == "7") || (vnutrperem[leftrightcontent].Substring(0, 1) == "8") || (vnutrperem[leftrightcontent].Substring(0, 1) == "9"))
                                                        {                                                            
                                                            if ((vnutrperem[rightrightcontent].Substring(0, 1) == "0") || (vnutrperem[rightrightcontent].Substring(0, 1) == "1") || (vnutrperem[rightrightcontent].Substring(0, 1) == "2") || (vnutrperem[rightrightcontent].Substring(0, 1) == "3") || (vnutrperem[rightrightcontent].Substring(0, 1) == "4") || (vnutrperem[rightrightcontent].Substring(0, 1) == "5") || (vnutrperem[rightrightcontent].Substring(0, 1) == "6") ||
                                                            (vnutrperem[rightrightcontent].Substring(0, 1) == "7") || (vnutrperem[rightrightcontent].Substring(0, 1) == "8") || (vnutrperem[rightrightcontent].Substring(0, 1) == "9"))
                                                            {
                                                                string rightvalue = ((Int32.Parse(vnutrperem[leftrightcontent])) + (Int32.Parse(vnutrperem[rightrightcontent]))).ToString();
                                                                vnutrperem.Add(leftcontent,rightvalue);                                                                
                                                            }
                                                        }
                                                        if (vnutrperem[leftrightcontent].IndexOf("{") >= 0)
                                                        {
                                                            if (vnutrperem[rightrightcontent].IndexOf("{") >= 0)
                                                            {
                                                                string rightvalue = "{" + "\"op\":\"+\",\"fO\":" + $"{vnutrperem[leftrightcontent]}" + ",\"sO\":" + $"{vnutrperem[rightrightcontent]}" + "}";
                                                                vnutrperem.Add(leftcontent, rightvalue);                                                                
                                                            }
                                                        }
                                                    }
                                                }
                                            }                                           
                                            if ((rightcontent.IndexOf("-") > 0))
                                            {
                                                int locationoperation = rightcontent.IndexOf("-");
                                                string leftrightcontent = rightcontent.Substring(0, locationoperation);                                                
                                                string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                                
                                                if ((leftrightcontent.IndexOf("(") > 0))
                                                {
                                                    int leftbracket = leftrightcontent.IndexOf("(");
                                                    string leftrightcontentname = leftrightcontent.Substring(0, leftbracket);//имя переменной слева от операции вычитания
                                                    string leftoperand = leftrightcontentname + "(";
                                                    string vnutriskobok = leftrightcontent.Substring(leftrightcontent.IndexOf("(") + 1, leftrightcontent.IndexOf(")") - leftrightcontent.IndexOf("(") - 1);                                                    
                                                    string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                    foreach (string s in masindex)
                                                    {                                                        
                                                        foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов уменьшаемого и определяются их значения
                                                        {
                                                            if (c == s)
                                                            {
                                                                leftoperand = leftoperand + vnutrperem[s] + ",";                                                                
                                                            }
                                                        }
                                                    }
                                                    leftoperand = leftoperand.Substring(0, leftoperand.Length - 1) + ")";                                                    
                                                    string leftrightcontentvalue = " ";
                                                    bool leftrightcontentvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, является ли уменьшаемое внутренней переменной
                                                    {
                                                        if (c == leftoperand) { leftrightcontentvp = true; };
                                                    }
                                                    if (leftrightcontentvp == true)
                                                    {
                                                        leftrightcontentvalue = vnutrperem[leftoperand];                                                        
                                                    }
                                                    else
                                                    {
                                                        leftrightcontentvalue = leftoperand;                                                        
                                                    }
                                                    bool lrcvinp = false;
                                                    foreach (string c in indata)//проверяем: уменьшаемое является входной переменной?
                                                    {
                                                        if (c == leftrightcontentvalue)
                                                        {
                                                            lrcvinp = true;                                                            
                                                        }
                                                    }
                                                    if ((lrcvinp == false) & (leftrightcontentvalue.IndexOf("{") < 0))
                                                    {
                                                        vnutrperem.Add(leftcontent,leftrightcontentvalue);                                                        
                                                    }
                                                    string leftvalue = " ";
                                                    if (lrcvinp == true)
                                                    {
                                                        leftvalue = "{" + "\"op\":\"-\",\"fO\":\"" + $"{leftrightcontentvalue}\",";
                                                        vnutrperem.Add(leftcontent,leftvalue);//добавлено значение внутренней переменной                                                        
                                                    }
                                                    if (leftrightcontentvalue.IndexOf("{") >= 0)
                                                    {
                                                        leftvalue = "{" + "\"op\":\"-\",\"fO\":" + $"{leftrightcontentvalue},";
                                                        vnutrperem.Add(leftcontent,leftvalue);//добавлено значение внутренней переменной                                                        
                                                    }                                                                                                    
                                                }                                                
                                                if ((leftrightcontent.IndexOf("(") < 0))//уменьшаемое без индексов
                                                {
                                                    string leftrightcontentvalue = " ";
                                                    if ((leftrightcontent.Substring(0, 1) == "0") || (leftrightcontent.Substring(0, 1) == "1") || (leftrightcontent.Substring(0, 1) == "2") || (leftrightcontent.Substring(0, 1) == "3") || (leftrightcontent.Substring(0, 1) == "4") || (leftrightcontent.Substring(0, 1) == "5") || (leftrightcontent.Substring(0, 1) == "6") ||
                                                       (leftrightcontent.Substring(0, 1) == "7") || (leftrightcontent.Substring(0, 1) == "8") || (leftrightcontent.Substring(0, 1) == "9"))
                                                    {                                                        
                                                        leftrightcontentvalue = leftrightcontent;
                                                    }
                                                    bool leftrightcontentvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, является ли уменьшаемое внутренней переменной
                                                    {
                                                        if (c == leftrightcontent) { leftrightcontentvp = true; };
                                                    }
                                                    if (leftrightcontentvp == true)//уменьшаемое является внутренней переменной
                                                    {
                                                        leftrightcontentvalue = vnutrperem[leftrightcontent];                                                        
                                                    }
                                                    string leftvalue = " ";
                                                    if (leftrightcontentvalue.IndexOf("{") >= 0)
                                                    {
                                                        leftvalue = "{" + "\"op\":\"-\",\"fO\":" + $"{leftrightcontentvalue},";
                                                        vnutrperem.Add(leftcontent, leftvalue);//добавлено значение внутренней переменной                                                        
                                                    }
                                                    else
                                                    {
                                                        leftvalue = "{" + "\"op\":\"-\",\"fO\":" + $"\"{leftrightcontentvalue}\",";
                                                        vnutrperem.Add(leftcontent, leftvalue);//добавлено значение внутренней переменной                                                        
                                                    }
                                                }                                                                               
                                                if ((rightrightcontent.IndexOf("(") > 0))
                                                {
                                                    int rightbracket = rightrightcontent.IndexOf("(");
                                                    string rightrightcontentname = rightrightcontent.Substring(0, rightbracket);//имя переменной справа от операции вычитания                                                    
                                                    string rightoperand = rightrightcontentname + "(";
                                                    string vnutriskobok = rightrightcontent.Substring(rightrightcontent.IndexOf("(") + 1, rightrightcontent.IndexOf(")") - rightrightcontent.IndexOf("(") - 1);                                                    
                                                    string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                    foreach (string s in masindex)
                                                    {                                                        
                                                        foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов вычитаемого и определяются их значения
                                                        {
                                                            if (c == s)
                                                            {
                                                                rightoperand = rightoperand + vnutrperem[s] + ",";                                                                
                                                            }
                                                        }
                                                    }
                                                    rightoperand = rightoperand.Substring(0, rightoperand.Length - 1) + ")";                                                    
                                                    bool rightrightcontentoutp = false;
                                                    foreach (string c in outdata.Keys)//проверяется, является ли вычитаемое выходной переменной
                                                    {
                                                        if (c == rightoperand) { rightrightcontentoutp = true; };
                                                    }
                                                    string rightrightcontentvalue = " ";
                                                    if (rightrightcontentoutp == true)
                                                    {
                                                        if ((vnutrperem[leftcontent].IndexOf("{")) >= 0)
                                                        {
                                                            rightrightcontentvalue = outdata[rightoperand];
                                                            if ((rightrightcontentvalue.IndexOf("{")) >= 0)
                                                            {
                                                                vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":{rightrightcontentvalue}" + "}";                                                                
                                                            }
                                                            else
                                                            {
                                                                vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":\"{rightrightcontentvalue}\"" + "}";                                                                
                                                            }
                                                        }
                                                    }
                                                }                                                
                                                    if ((rightrightcontent.IndexOf("(") < 0))//вычитаемое без индексов
                                                {                                                    
                                                    bool rightrightcontentvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, является ли вычитаемое внутренней переменной
                                                    {
                                                        if (c == rightrightcontent) { rightrightcontentvp = true; };
                                                    }                                                        
                                                        string rightrightcontentvalue = " ";
                                                        if (rightrightcontentvp == true)
                                                        {
                                                            if ((vnutrperem[leftcontent].IndexOf("{")) >= 0)
                                                            {
                                                                rightrightcontentvalue = vnutrperem[rightrightcontent];
                                                                if ((rightrightcontentvalue.IndexOf("{")) >= 0)
                                                                {
                                                                    vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":{rightrightcontentvalue}" + "}";                                                                    
                                                                }
                                                                else
                                                                {
                                                                    vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":\"{rightrightcontentvalue}\"" + "}";                                                                    
                                                                }
                                                            }
                                                        }                                                    
                                                    bool rightrightcontentinp = false;
                                                    foreach (string c in indata)//проверяется, является ли вычитаемое входной переменной
                                                    {
                                                        if (c == rightrightcontent) { rightrightcontentinp = true; };
                                                    }                                                    
                                                    if (rightrightcontentinp == true)
                                                    {                                                        
                                                                vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":\"{rightrightcontent}\"" + "}";                                                                                                                            
                                                    }                                                    
                                                }
                                            }                                            
                                            if ((rightcontent.IndexOf("-") == 0))//операция унарный минус
                                            {
                                                int locationoperation = rightcontent.IndexOf("-");
                                                string leftrightcontent = rightcontent.Substring(0, locationoperation);                                                
                                                string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                                 
                                                if ((rightrightcontent.IndexOf("(") < 0))//вычитаемое не содержит индексов
                                                {
                                                    bool rightrightcontentinp = false;
                                                    foreach (string c in indata)//проверяется, находится ли справа от операции унарного минуса входная переменная
                                                    {
                                                        if (c == rightrightcontent)
                                                        { rightrightcontentinp = true; };
                                                    }
                                                    if (rightrightcontentinp == true)
                                                    {                                                       
                                                        string value = "{" + "\"op\":\"-\",\"od\":" + $"\"{rightrightcontent}\"" + "}";
                                                        vnutrperem.Add(leftcontent, value);                                                        
                                                    }
                                                }
                                            }                                            
                                            if ((rightcontent.IndexOf("*") > 0))
                                            {
                                                int locationoperation = rightcontent.IndexOf("*");
                                                string leftrightcontent = rightcontent.Substring(0, locationoperation);                                                
                                                string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                                
                                                if ((leftrightcontent.IndexOf("(") > 0) & (leftrightcontent.IndexOf(",") > 0))//добавила проверку наличия запятой
                                                {
                                                    int leftbracket = leftrightcontent.IndexOf("(");
                                                    string leftrightcontentname = leftrightcontent.Substring(0, leftbracket);//имя переменной слева от операции умножения                                                    
                                                    string leftsomnoj = leftrightcontentname + "(";
                                                    string vnutriskobok = leftrightcontent.Substring(leftrightcontent.IndexOf("(") + 1, leftrightcontent.IndexOf(")") - leftrightcontent.IndexOf("(") - 1);                                                    
                                                    string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                    foreach (string s in masindex)
                                                    {                                                        
                                                        foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов первого сомножителя и определяются их значения
                                                        {
                                                            if (c == s)
                                                            {
                                                                leftsomnoj = leftsomnoj + vnutrperem[s] + ",";                                                                
                                                            }
                                                        }
                                                    }
                                                    leftsomnoj = leftsomnoj.Substring(0, leftsomnoj.Length - 1) + ")";                                                    
                                                    string leftvalue = "{" + "\"op\":\"*\",\"fO\":\"" + $"{leftsomnoj}\",";
                                                    vnutrperem.Add(leftcontent, leftvalue);//добавление значения внутренней переменной                                                                                                        
                                                }                                                
                                                if ((leftrightcontent.IndexOf("(") > 0) & (leftrightcontent.IndexOf(",") < 0))
                                                {
                                                    int leftbracket = leftrightcontent.IndexOf("(");
                                                    string leftrightcontentname = leftrightcontent.Substring(0, leftbracket);//имя переменной слева от операции умножения                                                                                                        
                                                    string leftsomnoj = leftrightcontentname + "(";
                                                    string vnutriskobok = leftrightcontent.Substring(leftrightcontent.IndexOf("(") + 1, leftrightcontent.IndexOf(")") - leftrightcontent.IndexOf("(") - 1);                                                    
                                                    bool vnutriskobokvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, является ли индексом первого сомножителя внутренняя переменная
                                                    {
                                                        if (c == vnutriskobok)
                                                        {
                                                            vnutriskobokvp = true;                                                            
                                                        }
                                                    }
                                                    if (vnutriskobokvp == true)
                                                    {
                                                        leftsomnoj = leftsomnoj + vnutrperem[vnutriskobok] + ")";
                                                        string leftvalue = "{" + "\"op\":\"*\",\"fO\":\"" + $"{leftsomnoj}\",";
                                                        vnutrperem.Add(leftcontent, leftvalue);//добавление значения внутренней переменной                                                        
                                                    }
                                                }
                                                
                                                //Парсер второго сомножителя
                                                if ((rightrightcontent.IndexOf("(") > 0))
                                                {
                                                    int leftbracket = rightrightcontent.IndexOf("(");
                                                    string rightrightcontentname = rightrightcontent.Substring(0, leftbracket);//имя переменной справа от операции умножения                                                    
                                                    string rightsomnoj = rightrightcontentname + "(";
                                                    string vnutriskobok = rightrightcontent.Substring(rightrightcontent.IndexOf("(") + 1, rightrightcontent.IndexOf(")") - rightrightcontent.IndexOf("(") - 1);
                                                    if (rightrightcontent.IndexOf(",") > 0)
                                                    {
                                                        string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                                        foreach (string s in masindex)
                                                        {                                                            
                                                            foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов правого сомножителя и определяются их значения
                                                            {
                                                                if (c == s)
                                                                {
                                                                    rightsomnoj = rightsomnoj + vnutrperem[s] + ",";                                                                    
                                                                }
                                                            }
                                                        }
                                                        rightsomnoj = rightsomnoj.Substring(0, rightsomnoj.Length - 1) + ")";                                                        
                                                    }
                                                    if (rightrightcontent.IndexOf(",") < 0)
                                                    {
                                                        foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов правого сомножителя и определяются их значения
                                                        {
                                                            if (c == vnutriskobok)
                                                            {
                                                                rightsomnoj = rightsomnoj + vnutrperem[vnutriskobok];                                                                
                                                            }
                                                        }

                                                        rightsomnoj = rightsomnoj + ")";                                                        
                                                        bool outd = false;
                                                        foreach (string c in outdata.Keys)//является ли правый сомножитель выходной переменной?
                                                        {
                                                            if (c == rightsomnoj)
                                                            {
                                                                outd = true;
                                                                rightsomnoj = outdata[rightsomnoj];                                                                
                                                            }
                                                        }
                                                    }                                                    
                                                    bool outdd = false;
                                                    foreach (string c in outdata.Keys)
                                                    {
                                                        if (c == rightsomnoj)
                                                        {
                                                            outdd = true;
                                                            rightsomnoj = outdata[rightsomnoj];                                                            
                                                        }
                                                    }                                                    
                                                    if (rightsomnoj.IndexOf("{") >= 0)
                                                    {
                                                        vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":{rightsomnoj}" + "}";
                                                    }
                                                    else 
                                                    { 
                                                        vnutrperem[leftcontent] = vnutrperem[leftcontent] + $"\"sO\":\"{rightsomnoj}\"" + "}";
                                                    }                                                     
                                                }                                                
                                                string leftrightcontentvalue = " ";
                                                if ((leftrightcontent.IndexOf("(") < 0))
                                                {
                                                    bool leftrightcontentvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, находится ли слева от операции умножения внутренняя переменная
                                                    {
                                                        if (c == leftrightcontent) { leftrightcontentvp = true; };
                                                    }
                                                    if (leftrightcontentvp == true)
                                                    {
                                                        leftrightcontentvalue = vnutrperem[leftrightcontent];                                                        
                                                    }                                                    
                                                    bool leftrightcontentinp = false;
                                                    foreach (string c in indata)//проверяется, находится ли слева от операции умножения входная переменная
                                                    {
                                                        if (c == leftrightcontent) { leftrightcontentinp = true; };
                                                    }
                                                    if (leftrightcontentinp == true)
                                                    {
                                                        leftrightcontentvalue = leftrightcontent;                                                        
                                                    }
                                                    if ((leftrightcontent.Substring(0, 1) == "0") || (leftrightcontent.Substring(0, 1) == "1") || (leftrightcontent.Substring(0, 1) == "2") || (leftrightcontent.Substring(0, 1) == "3") || (leftrightcontent.Substring(0, 1) == "4") || (leftrightcontent.Substring(0, 1) == "5") || (leftrightcontent.Substring(0, 1) == "6") ||
                                                       (leftrightcontent.Substring(0, 1) == "7") || (leftrightcontent.Substring(0, 1) == "8") || (leftrightcontent.Substring(0, 1) == "9"))
                                                    {                                                        
                                                        leftrightcontentvalue = leftrightcontent;                                                    
                                                    }                                                    
                                                }
                                                //Парсер второго сомножителя

                                                if ((rightrightcontent.IndexOf("(") < 0))
                                                {                                                    
                                                    if ((rightrightcontent.Substring(0, 1) == "0") || (rightrightcontent.Substring(0, 1) == "1") || (rightrightcontent.Substring(0, 1) == "2") || (rightrightcontent.Substring(0, 1) == "3") || (rightrightcontent.Substring(0, 1) == "4") || (rightrightcontent.Substring(0, 1) == "5") || (rightrightcontent.Substring(0, 1) == "6") ||
                                                       (rightrightcontent.Substring(0, 1) == "7") || (rightrightcontent.Substring(0, 1) == "8") || (rightrightcontent.Substring(0, 1) == "9"))
                                                    {                                                        
                                                        string rightvalue = ((Int32.Parse(leftrightcontentvalue)) * (Int32.Parse(rightrightcontent))).ToString();
                                                        vnutrperem.Add(leftcontent, rightvalue);                                                        
                                                    }                                                    
                                                    bool rightrightcontentinp = false;
                                                    foreach (string c in indata)//проверяется, находится ли справа от операции умножения входная переменная
                                                    {
                                                        if (c == rightrightcontent) { rightrightcontentinp = true; };
                                                    }
                                                    if (rightrightcontentinp == true)
                                                    {
                                                        string rightvalue = "{" + "\"op\":\"*\",\"fO\":" + $"\"{leftrightcontentvalue}\"" + ",\"sO\":" + $"\"{rightrightcontent}\"" + "}";
                                                        vnutrperem.Add(leftcontent, rightvalue);                                                        
                                                    }
                                                    bool rightrightcontentvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, находится ли справа от операции умножения внутренняя переменная
                                                    {
                                                        if (c == rightrightcontent)
                                                        { rightrightcontentvp = true; };
                                                    }
                                                    if (rightrightcontentvp == true)
                                                    {
                                                        string rightvalue = "{" + "\"op\":\"*\",\"fO\":" + $"\"{leftrightcontentvalue}\"" + ",\"sO\":" + $"{vnutrperem[rightrightcontent]}" + "}";
                                                        vnutrperem.Add(leftcontent,rightvalue);                                                        
                                                    }                                                    
                                                }
                                            }                                            
                                            if ((rightcontent.IndexOf("/") > 0))
                                            {
                                                int locationoperation = rightcontent.IndexOf("/");
                                                string leftrightcontent = rightcontent.Substring(0, locationoperation);                                                
                                                string rightrightcontent = rightcontent.Substring(locationoperation + 1, rightcontent.Length - locationoperation - 1);                                                
                                                string leftrightcontentvalue = " ";
                                                if ((leftrightcontent.IndexOf("(") < 0))//делимое
                                                {
                                                    bool leftrightcontentinp = false;
                                                    foreach (string c in indata)//проверяется, находится ли слева от операции деления входная переменная
                                                    {
                                                        if (c == leftrightcontent) { leftrightcontentinp = true; };
                                                    }
                                                    if (leftrightcontentinp == true)
                                                    {
                                                        leftrightcontentvalue = leftrightcontent;                                                        
                                                    }
                                                }
                                                if ((rightrightcontent.IndexOf("(") < 0))//делитель
                                                {                                                    
                                                    bool rightrightcontentvp = false;
                                                    foreach (string c in vnutrperem.Keys)//проверяется, находится ли справа от операции деления внутренняя переменная
                                                    {
                                                        if (c == rightrightcontent)
                                                        { rightrightcontentvp = true; };
                                                    }
                                                    if (rightrightcontentvp == true)
                                                    {
                                                        string rightvalue = "{" + "\"op\":\"/\",\"fO\":" + $"\"{leftrightcontentvalue}\"" + ",\"sO\":" + $"{vnutrperem[rightrightcontent]}" + "}";
                                                        vnutrperem.Add(leftcontent, rightvalue);                                                        
                                                    }
                                                }
                                            }                                            
                                            if ((rightcontent.IndexOf("abs(") == 0))
                                            {
                                                int locationoperation = rightcontent.IndexOf("abc");
                                                string rightrightcontent = rightcontent.Substring(locationoperation + 4, rightcontent.Length - locationoperation - 4);                                                                                                                                              
                                                                                                
                                                string vnutriskobok = rightrightcontent.Substring(rightrightcontent.IndexOf("(") + 1, rightrightcontent.IndexOf(")") - rightrightcontent.IndexOf("(") - 1);                                                
                                                bool vnutriskobokvp = false;                                                    
                                                foreach (string c in vnutrperem.Keys)//определяется значение внутри скобок операции abs
                                                {
                                                           if (c == vnutriskobok)
                                                           {
                                                              vnutriskobokvp = true;
                                                           }
                                                }
                                                if ((vnutriskobokvp == true))
                                                {
                                                    string rightvalue = "{" + "\"op\":\"abs\",\"od\":" + $"{vnutrperem[vnutriskobok]}" + "}";
                                                    vnutrperem.Add(leftcontent, rightvalue);//добавление значения внутренней переменной                                                    
                                                }                                                                                        
                                            }                                            
                                            if ((rightcontent.IndexOf("sqrt(") == 0))
                                            {
                                                int locationoperation = rightcontent.IndexOf("sqrt");
                                                string rightrightcontent = rightcontent.Substring(locationoperation + 4, rightcontent.Length - locationoperation - 4);                                                                                                                                              
                                                string vnutriskobok = rightrightcontent.Substring(rightrightcontent.IndexOf("(") + 1, rightrightcontent.IndexOf(")") - rightrightcontent.IndexOf("(") - 1);                                                
                                                bool vnutriskobokvp = false;
                                                foreach (string c in vnutrperem.Keys)//определяется значение внутри скобок операции abs
                                                {
                                                    if (c == vnutriskobok)
                                                    {
                                                        vnutriskobokvp = true;
                                                    }
                                                }
                                                if ((vnutriskobokvp == true))
                                                {
                                                    string rightvalue = "{" + "\"op\":\"sqrt\",\"od\":" + $"{vnutrperem[vnutriskobok]}" + "}";
                                                    vnutrperem.Add(leftcontent, rightvalue);//добавление значения внутренней переменной                                                    
                                                }
                                            }                                            
                                        }
                                    }                                    
                                }
                                foreach (var ed in jResults.Edges)
                                {
                                    if (ed.From == ver.Id)
                                    {                                        
                                        nextb = ed.To;                                        
                                    }
                                }
                            }
                        if (ver.Type == 3)
                        {                            
                            string content3 = ver.Content;
                            string leftcontent3 = " ";
                            string rightcontent3 = " ";
                            string leftcontent3v = " ";
                            string rightcontent3v = " ";
                                if (content3.IndexOf("<=") > 0)//для сравнения используется знак <=
                                {
                                    int assignmentcharacter3 = content3.IndexOf("<=");
                                    leftcontent3 = content3.Substring(0, assignmentcharacter3);                                    
                                    rightcontent3 = content3.Substring(assignmentcharacter3 + 2, content3.Length - assignmentcharacter3 - 2);                                    
                                }
                                if ((content3.IndexOf("<") > 0) & (content3.IndexOf("=") < 0))//для сравнения используется знак <
                                {
                                    int assignmentcharacter3 = content3.IndexOf("<");
                                    leftcontent3 = content3.Substring(0, assignmentcharacter3);                                    
                                    rightcontent3 = content3.Substring(assignmentcharacter3 + 1, content3.Length - assignmentcharacter3 - 1);                                    
                                }                                
                                if (content3.IndexOf(">=") > 0)//для сравнения используется знак >=
                                {
                                    int assignmentcharacter3 = content3.IndexOf(">=");
                                    leftcontent3 = content3.Substring(0, assignmentcharacter3);                                    
                                    rightcontent3 = content3.Substring(assignmentcharacter3 + 2, content3.Length - assignmentcharacter3 - 2);                                    
                                }
                                if ((content3.IndexOf(">") > 0) & (content3.IndexOf("=") < 0))//для сравнения используется знак >
                                {
                                    int assignmentcharacter3 = content3.IndexOf(">");
                                    leftcontent3 = content3.Substring(0, assignmentcharacter3);                                    
                                    rightcontent3 = content3.Substring(assignmentcharacter3 + 1, content3.Length - assignmentcharacter3 - 1);                                    
                                }                                
                                if (content3.IndexOf("==") > 0)//для сравнения используется знак ==
                                {
                                    int assignmentcharacter3 = content3.IndexOf("=");
                                    leftcontent3 = content3.Substring(0, assignmentcharacter3);                                    
                                    rightcontent3 = content3.Substring(assignmentcharacter3 + 2, content3.Length - assignmentcharacter3 - 2);                                    
                                }
                                if (content3.IndexOf("!=") > 0)//для сравнения используется знак !=
                                {
                                    int assignmentcharacter3 = content3.IndexOf("!=");
                                    leftcontent3 = content3.Substring(0, assignmentcharacter3);                                    
                                    rightcontent3 = content3.Substring(assignmentcharacter3 + 2, content3.Length - assignmentcharacter3 - 2);                                    
                                }

                            bool vnupl = false; //проверяем: слева от знака сравнения внутренняя переменная без индексов?
                            foreach (string c in vnutrperem.Keys)
                            {
                                if (c == leftcontent3)
                                {
                                    vnupl = true;
                                    leftcontent3v = vnutrperem[leftcontent3];                                    
                                }
                            }                                
                                bool vnupli = false;
                                bool outpli = false;
                                bool inplii = false;
                                if (leftcontent3.IndexOf("(") > 0) //определение переменной слева от знака сравнения                                
                                {
                                    int leftbracket = leftcontent3.IndexOf("(");
                                    string leftcontentname = leftcontent3.Substring(0, leftbracket);//имя переменной слева от скобки (                                    
                                    string leftotzsr = leftcontentname + "(";
                                    string vnutriskobok = leftcontent3.Substring(leftcontent3.IndexOf("(") + 1, leftcontent3.IndexOf(")") - leftcontent3.IndexOf("(") - 1);                                    
                                    if (leftcontent3.IndexOf(",") < 0)//один индекс
                                    {
                                        foreach (string c in vnutrperem.Keys)//ищется внутренняя переменная для индекса переменной слева от знака сравнения и определяется ее значение
                                        {
                                            if (c == vnutriskobok)
                                            {
                                                leftotzsr = leftotzsr + vnutrperem[vnutriskobok] + ")";                                               
                                            }
                                        }
                                        vnupli = false; //проверяем: слева от знака сравнения внутренняя переменная с одним индексом?
                                        foreach (string c in vnutrperem.Keys)
                                        {
                                            if (c == leftotzsr)
                                            {
                                                vnupli = true;
                                                leftcontent3v = vnutrperem[leftotzsr];                                                
                                            }
                                        }
                                        outpli = false; //проверяем: слева от знака сравнения выходная переменная с одним индексом?
                                        foreach (string c in outdata.Keys)
                                        {
                                            if (c == leftotzsr)
                                            {
                                                outpli = true;
                                                leftcontent3v = outdata[leftotzsr];                                                
                                            }
                                        }
                                    }                                                                       
                                    if (leftcontent3.IndexOf(",") > 0)//несколько индексов
                                    {
                                        string[] masindex = vnutriskobok.Split(new char[] { ',' });
                                        foreach (string s in masindex)
                                        {                                            
                                            foreach (string c in vnutrperem.Keys)//ищутся внутренние переменные для индексов переменной слева от знака сравнения и определяются их значения
                                            {
                                                if (c == s)
                                                {
                                                    leftotzsr = leftotzsr + vnutrperem[s] + ",";                                                    
                                                }
                                            }
                                        }
                                        leftotzsr = leftotzsr.Substring(0, leftotzsr.Length - 1) + ")";
                                        bool vnuplii = false; //проверяем: слева от знака сравнения внутренняя переменная с несколькими индексами?
                                        foreach (string c in vnutrperem.Keys)
                                        {
                                            if (c == leftotzsr)
                                            {
                                                vnuplii = true;
                                                leftcontent3v = vnutrperem[leftotzsr];                                                
                                            }
                                        }                                        
                                        inplii = false; //проверяем: слева от знака сравнения входная переменная с несколькими индексами?
                                        foreach (string c in indata)
                                        {
                                            if (c == leftotzsr)
                                            {
                                                inplii = true;
                                                leftcontent3v = leftotzsr;                                                
                                            }
                                        }                                        
                                    }
                                }
                                    
                            bool outpl = false; //проверяем: слева от знака сравнения выходная переменная без индексов?
                            foreach (string c in outdata.Keys)
                            {
                                if (c == leftcontent3)
                                {
                                    outpl = true;
                                    leftcontent3v = outdata[leftcontent3];                                    
                                }
                            }
                            /*bool inpl = false; //проверяем: слева от знака сравнения входная переменная?
                            foreach (string c in indata)
                            {
                                if (c == leftcontent3)
                                {
                                    inpl = true;                                    
                                }
                            }*/
                            bool parrr = false; //проверяем: справа от знака сравнения параметр размерности?
                            foreach (string c in paramrazm.Keys)
                            {
                                if (c == rightcontent3)
                                {
                                    parrr = true;                                    
                                }
                            }
                                bool inpri = false;
                                bool vnupri = false;
                                bool outpri = false;
                                if ((rightcontent3.IndexOf("(") > 0) & (rightcontent3.IndexOf(",") < 0)) //анализируем правую часть в условии, когда есть скобки, но в них один индекс
                                {
                                    string rightc = rightcontent3.Substring(rightcontent3.IndexOf("(") + 1, rightcontent3.IndexOf(")") - rightcontent3.IndexOf("(") - 1);                                                                         
                                    foreach (string c in indata)//проверяем: справа от знака сравнения входная переменная с одним индексом?
                                    {
                                        if (c == (rightcontent3.Substring(0, rightcontent3.IndexOf("(")) + "(" + vnutrperem[rightc] + ")"))
                                        {
                                            inpri = true;
                                            rightcontent3v = rightcontent3.Substring(0, rightcontent3.IndexOf("(")) + "(" + vnutrperem[rightc] + ")";                                            
                                        }
                                    }                                   
                                    foreach (string c in vnutrperem.Keys)//проверяем: справа от знака сравнения внутренняя переменная c индексом?
                                    {
                                        if (c == (rightcontent3.Substring(0, rightcontent3.IndexOf("(")) + "(" + vnutrperem[rightc] + ")"))
                                        {
                                            vnupri = true;
                                            rightcontent3v = vnutrperem[rightcontent3.Substring(0, rightcontent3.IndexOf("(")) + "(" + vnutrperem[rightc] + ")"];                                            
                                        }
                                    }                                                                       
                                    foreach (string c in outdata.Keys)//проверяем: справа от знака сравнения выходная переменная c индексом?
                                    {
                                        if (c == (rightcontent3.Substring(0, rightcontent3.IndexOf("(")) + "(" + vnutrperem[rightc] + ")"))
                                        {
                                            outpri = true;
                                            rightcontent3v = outdata[rightcontent3.Substring(0, rightcontent3.IndexOf("(")) + "(" + vnutrperem[rightc] + ")"];                                            
                                        }
                                    }                                    
                                }
                                bool vnupr = false; //проверяем: справа от знака сравнения внутренняя переменная без индекса?
                                foreach (string c in vnutrperem.Keys)
                                {
                                    if (c == rightcontent3)
                                    {
                                        vnupr = true;
                                        rightcontent3v = vnutrperem[rightcontent3];                                        
                                    }
                                }
                                /*bool outpr = false; //проверяем: справа от знака сравнения выходная переменная без индекса?
                                foreach (string c in outdata.Keys)
                                {
                                    if (c == rightcontent3)
                                    {
                                        outpr = true;                                        
                                    }
                                }*/
                                bool inpr = false;
                                foreach (string c in indata)//проверяем: справа от знака сравнения входная переменная без индекса?
                                {                                                                       
                                    if (c == rightcontent3)
                                    {
                                        inpr = true;
                                        rightcontent3v = rightcontent3;                                        
                                    }
                                }
                                bool numberr = false; //справа от знака сравнения число
                                if ((rightcontent3.Substring(0, 1) == "0") || (rightcontent3.Substring(0, 1) == "1") || (rightcontent3.Substring(0, 1) == "2") || (rightcontent3.Substring(0, 1) == "3") || (rightcontent3.Substring(0, 1) == "4") || (rightcontent3.Substring(0, 1) == "5") || (rightcontent3.Substring(0, 1) == "6") ||
                                    (rightcontent3.Substring(0, 1) == "7") || (rightcontent3.Substring(0, 1) == "8") || (rightcontent3.Substring(0, 1) == "9"))
                                {
                                    numberr = true;
                                    rightcontent3v = rightcontent3;                                    
                                }
                                bool uslovie = false;
                                if ((vnupl == true) & (parrr == true) & (content3.IndexOf("<=") > 0))
                                {
                                    if ((Int32.Parse(vnutrperem[leftcontent3]) <= paramrazm[rightcontent3])) uslovie = true;
                                }
                                if ((vnupl == true) & (parrr == true) & (content3.IndexOf("<") > 0))
                                {
                                    if (content3.IndexOf("=") < 0)
                                    {
                                        if ((Int32.Parse(vnutrperem[leftcontent3]) < paramrazm[rightcontent3])) uslovie = true;                                        
                                    }
                                }
                                if ((vnupl == true) & (vnupr == true) & (content3.IndexOf("<") > 0))
                                {
                                    if (content3.IndexOf("=") < 0)
                                    {
                                        if ((Int32.Parse(vnutrperem[leftcontent3]) < Int32.Parse(vnutrperem[rightcontent3]))) uslovie = true;                                        
                                    }
                                }                                
                                if ((vnupl == true) & (vnupr == true) & (content3.IndexOf(">") > 0))
                                {
                                    if (content3.IndexOf("=") < 0)
                                    {
                                        if ((Int32.Parse(vnutrperem[leftcontent3]) > Int32.Parse(vnutrperem[rightcontent3]))) uslovie = true;                                        
                                    }
                                }
                                if ((vnupl == true) & (vnupr == true) & (content3.IndexOf(">=") > 0))
                                {
                                    if ((Int32.Parse(vnutrperem[leftcontent3]) >= Int32.Parse(vnutrperem[rightcontent3]))) uslovie = true;                                    
                                }
                                
                                if ((vnupl == true) & (vnupr == true) & (content3.IndexOf("!=") > 0))
                                {
                                    if ((Int32.Parse(vnutrperem[leftcontent3]) != Int32.Parse(vnutrperem[rightcontent3]))) uslovie = true;                                    
                                }
                                if ((vnupl == true) & (vnupri == true) & (content3.IndexOf("==") > 0))
                                {
                                    
                                    if ((Int32.Parse(vnutrperem[leftcontent3]) == Int32.Parse(rightcontent3v))) uslovie = true;                                                                                                            
                                }
                                if ((vnupl == true) & (numberr == true) & (content3.IndexOf("<") > 0))
                                {
                                    if (content3.IndexOf("=") < 0)
                                    {
                                        if ((Int32.Parse(vnutrperem[leftcontent3]) < Int32.Parse(rightcontent3))) uslovie = true;                                        
                                    }
                                }                                
                                if ((vnupli == true) & (numberr == true) & (content3.IndexOf("==") > 0))
                                {
                                        if (Int32.Parse(leftcontent3v) == Int32.Parse(rightcontent3v)) uslovie = true;                                                                         
                                }                                
                                if ((vnupl == true) & (rightcontent3 == "iterations") & (content3.IndexOf("<=") > 0))
                                {                                    
                                    if ((Int32.Parse(vnutrperem[leftcontent3]) <= iterations)) uslovie = true;                                    
                                }                                
                                bool lc3v = false;
                                foreach (string c in indata)//проверяем: содержится ли входная переменная в leftcontent3v?
                                {                                    
                                    if (leftcontent3v.IndexOf(c) >= 0)
                                    {                                        
                                        lc3v = true;                                                                                
                                    }
                                }
                                bool rc3v = false;
                                foreach (string c in indata)//проверяем: содержится ли входная переменная в rightcontent3v?
                                {                                    
                                    if (rightcontent3v.IndexOf(c) >= 0)
                                    {
                                        rc3v = true;                                        
                                    }
                                }                                                                
                                if (((lc3v == true) || (rc3v == true)) & (rightcontent3 != "iterations"))//для логического Q-терма
                                {                                                                    
                                    if (way.Count <= numberinway)
                                    {
                                       numberinway = numberinway + 1;
                                        way[numberinway] = ver.Id + "," + "1";
                                        uslovie = true;
                                    }
                                    if (numberinway < way.Count)
                                    {
                                        numberinway = numberinway + 1;
                                        if (way[numberinway].IndexOf(",1") > 0) uslovie = true;
                                        if (way[numberinway].IndexOf(",0") > 0) uslovie = false;
                                    }
                                    if ((content3.IndexOf("<") > 0) & (content3.IndexOf("=") < 0) & (uslovie == true) & (rightcontent3 == "e"))//нужно запрограммировать и другие операции сравнения
                                    {                                        
                                        if (logicalqterm == " ") { logicalqterm = "{\"op\":\"<\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}"; }
                                        else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"<\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}}"; }                                        
                                    }
                                    if ((content3.IndexOf("<") > 0) & (content3.IndexOf("=") < 0) & (uslovie == true) & (rightcontent3 != "e"))//нужно запрограммировать и другие операции сравнения
                                    {                                        
                                        if ((leftcontent3v.IndexOf("{") < 0) & (rightcontent3v.IndexOf("{") < 0))
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"<\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"<\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}}"; }
                                        }
                                        if ((leftcontent3v.IndexOf("{") < 0) & (rightcontent3v.IndexOf("{") >= 0))
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"<\",\"fO\":\"" + leftcontent3v + "\",\"sO\":" + rightcontent3v + "}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"<\",\"fO\":\"" + leftcontent3v + "\",\"sO\":" + rightcontent3v + "}}"; }
                                        }
                                        if ((leftcontent3v.IndexOf("{") >= 0) & (rightcontent3v.IndexOf("{") < 0))
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"<\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"<\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}}"; }
                                        }
                                        if ((leftcontent3v.IndexOf("{") >= 0) & (rightcontent3v.IndexOf("{") >= 0))
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"<\",\"fO\":" + leftcontent3v + ",\"sO\":" + rightcontent3v + "}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"<\",\"fO\":" + leftcontent3v + ",\"sO\":" + rightcontent3v + "}}"; }
                                        }                                        
                                    }                                    
                                    if ((content3.IndexOf("==") > 0) & (uslovie == true) & (rightcontent3 != "e"))
                                    {                                        
                                        if (leftcontent3v.IndexOf("{") < 0)
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"==\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"==\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}}"; }                                          
                                        }
                                        if (leftcontent3v.IndexOf("{") >= 0)
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"==\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"==\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}}"; }
                                        }                                        
                                    }
                                    if ((content3.IndexOf(">") > 0) & (content3.IndexOf("=") < 0) & (uslovie == true) & (rightcontent3 != "e"))
                                    {                                        
                                        if (leftcontent3v.IndexOf("{") < 0)
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\">\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\">\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}}"; }
                                        }
                                        if (leftcontent3v.IndexOf("{") >= 0)
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\">\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\">\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}}"; }
                                        }                                        
                                    }                                    
                                    if ((content3.IndexOf("!") > 0) & (content3.IndexOf("=") > 0) & (uslovie == true) & (rightcontent3 != "e"))
                                    {                                        
                                        if (leftcontent3v.IndexOf("{") < 0)
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"!=\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"!=\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}}"; }                                            
                                        }
                                        if (leftcontent3v.IndexOf("{") >= 0)
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"!=\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"!=\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}}"; }                                            
                                        }
                                    }
                                    if ((content3.IndexOf("<") > 0) & (content3.IndexOf("=") < 0) & (uslovie == false) & (rightcontent3 != "e"))
                                    {                                        
                                        if ((leftcontent3v.IndexOf("{") < 0) & (rightcontent3v.IndexOf("{") < 0))
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\">=\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\">=\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}}"; }
                                        }
                                        if ((leftcontent3v.IndexOf("{") < 0) & (rightcontent3v.IndexOf("{") >= 0))
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\">=\",\"fO\":\"" + leftcontent3v + "\",\"sO\":" + rightcontent3v + "}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\">=\",\"fO\":\"" + leftcontent3v + "\",\"sO\":" + rightcontent3v + "}}"; }
                                        }
                                        if ((leftcontent3v.IndexOf("{") >= 0) & (rightcontent3v.IndexOf("{") < 0))
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\">=\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\">=\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}}"; }
                                        }
                                        if ((leftcontent3v.IndexOf("{") >= 0) & (rightcontent3v.IndexOf("{") >= 0))
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\">=\",\"fO\":" + leftcontent3v + ",\"sO\":" + rightcontent3v + "}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\">=\",\"fO\":" + leftcontent3v + ",\"sO\":" + rightcontent3v + "}}"; }
                                        }                                        
                                    }                                    
                                    if ((content3.IndexOf("==") > 0) & (uslovie == false) & (rightcontent3 != "e"))
                                    {                                        
                                        if (leftcontent3v.IndexOf("{") < 0)
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"!=\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"!=\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}}"; }
                                        }
                                        if (leftcontent3v.IndexOf("{") >= 0)
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"!=\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"!=\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}}"; }
                                        }                                        
                                    }
                                    if ((content3.IndexOf(">") > 0) & (content3.IndexOf("=") < 0) & (uslovie == false) & (rightcontent3 != "e"))//нужно запрограммировать и другие операции сравнения
                                    {                                        
                                        if (leftcontent3v.IndexOf("{") < 0)
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"<=\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"<=\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}}"; }
                                        }
                                        if (leftcontent3v.IndexOf("{") >= 0)
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"<=\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"<=\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}}"; }
                                        }                                        
                                    }                                    
                                    if ((content3.IndexOf("!") > 0) & (content3.IndexOf("=") > 0) & (uslovie == false) & (rightcontent3 != "e"))//нужно запрограммировать и другие операции сравнения
                                    {                                        
                                        if (leftcontent3v.IndexOf("{") < 0)
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"==\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"==\",\"fO\":\"" + leftcontent3v + "\",\"sO\":\"" + rightcontent3v + "\"}}"; }                                            
                                        }
                                        if (leftcontent3v.IndexOf("{") >= 0)
                                        {
                                            if (logicalqterm == " ") { logicalqterm = "{\"op\":\"==\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}"; }
                                            else { logicalqterm = "{\"op\":\"&\",\"fO\":" + logicalqterm + ",\"sO\":" + "{\"op\":\"==\",\"fO\":" + leftcontent3v + ",\"sO\":\"" + rightcontent3v + "\"}}"; }                                            
                                        }
                                   }                                    
                                }
                                
                            foreach (var ed in jResults.Edges)
                            {
                                if ((ed.From == ver.Id) & (ed.Type == 1) & (uslovie == true))
                                {                                    
                                    nextb = ed.To;                                    
                                }
                                if ((ed.From == ver.Id) & (ed.Type == 0) & (uslovie == false))
                                {                                    
                                    nextb = ed.To;                                    
                                }
                            }
                        }
                            if (!((ver.Type == 2) || (ver.Type == 3)))
                            {                                                            
                                foreach (var ed in jResults.Edges)
                            {
                                if (ed.From == ver.Id)
                                {                                    
                                    nextb = ed.To;                                    
                                }
                            }
                        }
                    }

                }
            }             
             string writePath = @"Qdeterminant.txt";//вывод Q-детерминанта в файл
             try
             {
                 using (StreamWriter sw = new StreamWriter(writePath, true, System.Text.Encoding.Default))
                 {
                        if (vnutrperem["empout"].IndexOf("1") >= 0) //данные для Q-детерминанта будут выведены в файл
                        {
                            if ((iterations == 0))

                            {
                                foreach (string c in outdata.Keys)
                                {                                    
                                    sw.WriteLine($"{c}" + "=" + $"{logicalqterm}" + ";" + $"{outdata[c]}");
                                }
                            }
                            if ((iterations > 0) & (way[numberinway].IndexOf(",1") > 0))

                            {                                
                                foreach (string c in outdata.Keys)
                                {                                    
                                    sw.WriteLine($"{c}" + "=" + $"{logicalqterm}" + ";" + $"{outdata[c]}");
                                }
                            }
                        }
                 }
             }
             catch (Exception e)
             {
                 Console.WriteLine(e.Message);
             }
             return countie5;

                }                             
            }
        }
    }
    