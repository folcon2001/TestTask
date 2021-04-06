using NLog;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;

namespace Countwords
{
    class Program
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            string adr, pathcont, pathres;


            Console.Title = "Ввод информации";
            Console.WriteLine("\nВведите адрес сайта в формате: http://xxxxx.xx \n");
            adr = Console.ReadLine();



            if (adr.Length < 10 || adr.Substring(0,7)!="http://" || !adr.Contains("."))
            {
                do
                {
                    Console.Title = "Ошибка ввода";
                    Console.WriteLine("Не верный формат ввода адреса!!! Попробуйте еще раз.");
                    adr = Console.ReadLine();

                } while (adr.Length < 10 || adr.Substring(0, 7) != "http://" || !adr.Contains(".") );
            }


            // путь  сохранения файла содержимого сайта
            pathcont = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\contains.txt";

            // путь сохранения файла с результатом подсчета уникальных элементов
            pathres = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\result.txt";

            logger.Trace("Адрес сайта получен, пути для сохранения файлов определены");

            Download zagruz = new Download();
            zagruz.Web(adr, pathcont);

            Sorting otbor = new Sorting();
            otbor.Count(pathcont, pathres);

            Baza import = new Baza();
            import.Import(adr, pathres);

            logger.Trace("Программа успешно завершена.");
        }
    }

    class Download
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        /* Метод получает адрес сайта и путь к файлу для сохранения.
         * Скачивает содержимое сайта и сохраняет в файл.  */

        public void Web(string adr, string path)
        {
            try
            {
                WebClient sait = new WebClient();

                Console.WriteLine("\nПодключаемся к  {0} ...", adr);


                Stream ptk = sait.OpenRead(adr);
                StreamReader strd = new StreamReader(ptk);

                Console.WriteLine("\nПодключение успешно выполнено!\n");

                File.WriteAllText(path, strd.ReadToEnd());

                ptk.Close();

                logger.Trace("Веб страница загружена и сохранена в файл");

                Console.WriteLine("\nВеб страница загружена!\n");
            }

            catch (Exception er)
            {
                Console.Title = "Ошибка ввода";
                Console.WriteLine("Ошибка подключения! {0}", er.Message);
                logger.Error("Ошибка подключения к сайту");
            }
        }
    }

    class Sorting
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        /* Метод получает путь к файлу с данными и путь к файлу для сохранения результата.
         * Разделяет данные на слова. Подсчитывает кол-во уникальных слов
         * И сохраняет результат в файл*/

        public void Count(string pathc, string pathr)
        {

            string txt, itog = "", num = "";

            int n = 0;

            txt = File.ReadAllText(pathc).ToUpper();

            logger.Trace("Данные считаны из файла");

            Console.WriteLine("\nИдет обработка данных.\n");

            string[] subs = txt.Split(new char[] { ' ', '{', '}', '(', ')', '?', '!', '\\', '<', '-', ';', ':', '/', ',', '"', '=', '>', '<', '+', '.', '&', '%', '_', '\'', '|', '[', ']', '$', '*', '#', '@', '~', '^', '«', '»', '—', '–', '−', '…', '°', '\n', '\r', '\'', '\t' }, StringSplitOptions.RemoveEmptyEntries);


            //убираем из массива "слова" содержащие не только буквы

            for (int i = 0; i < subs.Length; i++)
            {
                for (int j = 0; j < subs[i].Length; j++)
                {
                    if (!char.IsLetter(subs[i][j]))
                    {
                        n = 1;
                        break;
                    }


                }
                if (n == 0)
                    num += subs[i] + " ";

                if (i % 1000 == 0)
                    Console.Write(".");

                n = 0;
            }

            logger.Trace("Из массива убраны слова содержащие не только буквы");

            string[] obch = num.Split();

            string[] izb = obch.Distinct().ToArray();

            for (int i = 0; i < izb.Length; i++)
            {
                for (int j = 0; j < obch.Length; j++)
                {
                    if (izb[i] != "" && izb[i] == obch[j])
                        n++;

                }

                if (n != 0)
                    izb[i] = izb[i] + " " + n;

                n = 0;
            }

            Array.Sort(izb);

            logger.Trace("Подсчет уникальных слов выполнен");

            Console.WriteLine("\n");

            foreach (string s in izb)
            {
                Console.WriteLine(s + "\n");
                if (s != "")
                    itog += s + "\n";
            }

            File.WriteAllText(pathr, itog);

            Console.WriteLine("\nДанные обработаны!\n");

            logger.Trace("Результат обработки сохранен в файле");
        }
    }

    class Baza
    {
        public static Logger logger = LogManager.GetCurrentClassLogger();

        /* Метод получает адрес сайта и путь к файлу с данными.
         * Создает в базе данных таблицу с названием сайта.
         * Содержимое файла импортирует в базу.
         * Путь подключения к базе данных прописан в App.config */

        public void Import(string name, string path)
        {

            string txt, adrs, a;
            int poz;

            // создание названия для таблицы в БД из адреса сайта
            adrs = name.Substring(7);
            poz = adrs.IndexOf('.');
            a = adrs.Substring(0, poz);

            SqlConnection myConn = new SqlConnection(ConfigurationManager.ConnectionStrings["Baza"].ConnectionString);
           
            Console.WriteLine("Проверка входа");

            myConn.Open();

            if (myConn.State == ConnectionState.Open)
            {
                Console.WriteLine("Подключение к базе данных выполнено успешно!");
                logger.Trace("Подключение к базе данных выполнено успешно");
            }

            SqlCommand myCommand = new SqlCommand("CREATE DATABASE MyDatabase", myConn);

            string query = "CREATE TABLE " + a + "(slovo NVARCHAR(40), povtor INT)";

            SqlCommand sqlNewTab = new SqlCommand(query, myConn);
            try
            {
                sqlNewTab.ExecuteNonQuery();

                Console.WriteLine($"\nТаблица создана c именем \"{a}\"");
                logger.Trace("Создана новая таблица с названием сайта");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            StreamReader file = new StreamReader(path);

            Console.WriteLine("\nИмпортируем данные в базу данных.\n");

            while ((txt = file.ReadLine()) != null)
            {
                if (txt != "")
                {
                    string[] para = txt.Split();

                    query = "INSERT INTO " + a + " (slovo, povtor) VALUES ('" + para[0] + "','" + Int32.Parse(para[1]) + "')";
                    SqlCommand sqlNewString = new SqlCommand(query, myConn);
                    sqlNewString.ExecuteNonQuery();
                }
            }

            file.Close();

            myConn.Close();

            logger.Trace("Данные импортированы в базу данных");

            Console.WriteLine("\nДанные успешно импортированы в базу данных!\n");

        }
    }
}